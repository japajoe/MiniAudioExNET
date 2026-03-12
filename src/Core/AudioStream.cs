// This software is available as a choice of the following licenses. Choose
// whichever you prefer.

// ===============================================================================
// ALTERNATIVE 1 - Public Domain (www.unlicense.org)
// ===============================================================================
// This is free and unencumbered software released into the public domain.

// Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
// software, either in source code form or as a compiled binary, for any purpose,
// commercial or non-commercial, and by any means.

// In jurisdictions that recognize copyright laws, the author or authors of this
// software dedicate any and all copyright interest in the software to the public
// domain. We make this dedication for the benefit of the public at large and to
// the detriment of our heirs and successors. We intend this dedication to be an
// overt act of relinquishment in perpetuity of all present and future rights to
// this software under copyright law.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// For more information, please refer to <http://unlicense.org/>

// ===============================================================================
// ALTERNATIVE 2 - MIT No Attribution
// ===============================================================================
// Copyright 2026 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using MiniAudioEx.Native;
using MiniAudioEx.Utilities;

namespace MiniAudioEx.Core
{
    /// <summary>
    /// Experimental class for playing (ICY) internet streams.
    /// </summary>
    public unsafe sealed class AudioStream : IDisposable
    {
        private ma_device_ptr device;
        private ma_decoder_ptr decoder;
        private ma_decoder_read_proc decoderReadProc;
        private ma_decoder_seek_proc decoderSeekProc;
        private ma_device_data_proc deviceDataProc;
        private Socket socket;
        private Thread connectionThread;
        private AtomicBool isRunning;
        private RingBuffer audioBuffer;
        private AudioDevice playbackDevice;
        private bool audioInitialized;
        private float volume;

        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if (value < 0.0f)
                {
                    value = 0.0f;
                }
                volume = value;
            }
        }

        public AudioStream(AudioDevice playbackDevice = null)
        {
            this.playbackDevice = playbackDevice;
            isRunning = new AtomicBool();
            audioBuffer = new RingBuffer(1024 * 128);
            decoder = new ma_decoder_ptr(true);
            device = new ma_device_ptr(true);
            decoderReadProc = OnDecoderRead;
            decoderSeekProc = OnDecoderSeek;
            deviceDataProc = OnDeviceData;
            volume = 1.0f;
            audioInitialized = false;
        }

        public void Play(string url)
        {
            if (isRunning.Load())
            {
                Stop();
            }

            audioBuffer.Reset();
            audioInitialized = false;
            isRunning.Store(true);

            connectionThread = new Thread(() =>
            {
                Connect(url);
            });

            connectionThread.IsBackground = true;
            connectionThread.Start();
        }

        public void Stop()
        {
            isRunning.Store(false);
            
            Disconnect();
            UninitializeAudio();

            if (connectionThread != null)
            {
                if (connectionThread.IsAlive)
                {
                    connectionThread.Join(500);
                }
                connectionThread = null;
            }
        }

        public void Dispose()
        {
            Stop();

            if (device.pointer != IntPtr.Zero)
            {
                device.Free();
            }

            if (decoder.pointer != IntPtr.Zero)
            {
                decoder.Free();
            }
        }

        private void Disconnect()
        {
            if (socket != null)
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch
                {
                }
                finally
                {
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                }
            }
        }

        private void UninitializeAudio()
        {
            if (audioInitialized)
            {
                MiniAudio.ma_device_stop(device);
                MiniAudio.ma_device_uninit(device);
                MiniAudio.ma_decoder_uninit(decoder);
                audioInitialized = false;
            }
        }

        private void Connect(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                int port = uri.Port == -1 ? 80 : uri.Port;
                socket.Connect(uri.Host, port);

                string request = "GET " + uri.PathAndQuery + " HTTP/1.1\r\n" +
                                 "Host: " + uri.Host + "\r\n" +
                                 "User-Agent: MiniAudioEx/1.0\r\n" +
                                 "Icy-MetaData: 0\r\n" +
                                 "Connection: close\r\n\r\n";

                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                socket.Send(requestBytes);

                Receive();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection error: " + ex.Message);
                isRunning.Store(false);
            }
        }

        private void Receive()
        {
            byte[] buffer = new byte[4096];
            bool headerSkipped = false;
            List<byte> headerCollector = new List<byte>();

            try
            {
                while (isRunning.Load() && socket != null && socket.Connected)
                {
                    int bytesRead = socket.Receive(buffer);
                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    if (!headerSkipped)
                    {
                        int payloadOffset = FindHeaderEnd(buffer, bytesRead, headerCollector);
                        if (payloadOffset != -1)
                        {
                            if (!ValidateHeader(headerCollector))
                            {
                                isRunning.Store(false);
                                break;
                            }

                            string header = Encoding.UTF8.GetString(headerCollector.ToArray());
                            Console.WriteLine(header);

                            headerSkipped = true;
                            int remainingBytes = bytesRead - payloadOffset;
                            if (remainingBytes > 0)
                            {
                                ProcessAudioData(buffer, payloadOffset, remainingBytes);
                            }
                        }
                        continue;
                    }

                    ProcessAudioData(buffer, 0, bytesRead);

                    if (!audioInitialized && audioBuffer.Count > 32768)
                    {
                        if (!InitializeAudio())
                        {
                            break;
                        }
                    }
                }
            }
            catch (SocketException)
            {
                isRunning.Store(false);
            }
            finally
            {
                isRunning.Store(false);
            }
        }

        private bool ValidateHeader(List<byte> headerBytes)
        {
            string headerText = Encoding.UTF8.GetString(headerBytes.ToArray());
            string[] lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            if (lines.Length == 0)
            {
                return false;
            }

            // Check for HTTP 200 OK
            if (!lines[0].Contains("200"))
            {
                Console.WriteLine("Server returned error: " + lines[0]);
                return false;
            }

            bool isAudio = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Contains("audio/") || line.Contains("application/octet-stream"))
                    {
                        isAudio = true;
                    }
                }
            }

            return isAudio;
        }

        private bool InitializeAudio()
        {
            ma_decoder_config decoderConfig = MiniAudio.ma_decoder_config_init(ma_format.f32, 0, 0);

            ma_result result = MiniAudio.ma_decoder_init(decoderReadProc, decoderSeekProc, IntPtr.Zero, ref decoderConfig, decoder);

            if (result != ma_result.success)
            {
                Console.WriteLine("Failed to create decoder: " + result);
                return false;
            }

            ma_device_config deviceConfig = MiniAudio.ma_device_config_init(ma_device_type.playback);
            deviceConfig.playback.format = decoder.Get()->outputFormat;
            deviceConfig.playback.channels = decoder.Get()->outputChannels;
            deviceConfig.sampleRate = decoder.Get()->outputSampleRate;
            deviceConfig.SetDataCallback(deviceDataProc);

            if (playbackDevice != null)
            {
                fixed(ma_device_id *deviceId = &playbackDevice.info.id)
                {
                    deviceConfig.playback.pDeviceID = new ma_device_id_ptr(new IntPtr(deviceId));
                    result = MiniAudio.ma_device_init(new ma_context_ptr(IntPtr.Zero), ref deviceConfig, device);
                }
            }
            else
            {
                result = MiniAudio.ma_device_init(new ma_context_ptr(IntPtr.Zero), ref deviceConfig, device);
            }

            if (result != ma_result.success)
            {
                Console.WriteLine("Failed to create device");
                MiniAudio.ma_decoder_uninit(decoder);
                return false;
            }

            audioInitialized = true;
            MiniAudio.ma_device_start(device);
            return true;
        }

        private int FindHeaderEnd(byte[] buffer, int length, List<byte> collector)
        {
            for (int i = 0; i < length; i++)
            {
                collector.Add(buffer[i]);
                if (collector.Count >= 4)
                {
                    int last = collector.Count;
                    if (collector[last - 4] == 0x0D && collector[last - 3] == 0x0A &&
                        collector[last - 2] == 0x0D && collector[last - 1] == 0x0A)
                    {
                        return i + 1;
                    }
                }
            }
            return -1;
        }

        private void ProcessAudioData(byte[] buffer, int offset, int count)
        {
            audioBuffer.Write(buffer, offset, count);
        }

        private ma_result OnDecoderRead(ma_decoder_ptr pDecoder, IntPtr pBufferOut, size_t bytesToRead, out size_t pBytesRead)
        {
            int actualRead = audioBuffer.Read(pBufferOut, (int)bytesToRead.ToUInt32());

            pBytesRead = actualRead;

            if (actualRead > 0)
            {
                return ma_result.success;
            }
            
            if (isRunning.Load())
            {
                return ma_result.success;
            }

            return ma_result.at_end;
        }

        private ma_result OnDecoderSeek(ma_decoder_ptr pDecoder, Int64 byteOffset, ma_seek_origin origin)
        {
            return ma_result.success;
        }

        private void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
        {
            ma_device* devicePtr = pDevice.Get();

            if (devicePtr == null)
            {
                return;
            }

            if (MiniAudio.ma_decoder_read_pcm_frames(decoder, pOutput, frameCount, IntPtr.Zero) == ma_result.success)
            {
                if (volume == 1.0f)
                {
                    return;
                }

                int totalSamples = (int)(frameCount * decoder.Get()->outputChannels);
                NativeArray<float> data = new NativeArray<float>(pOutput, totalSamples);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] *= volume;
                }
            }
        }
    }
}