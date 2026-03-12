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
using System.IO;
using System.Net.Security;

namespace MiniAudioEx.Core
{
    public delegate void AudioStreamConnectedEvent(Dictionary<string,string> headers);
    public delegate void AudioStreamDisconnectedEvent(string reason);
    public delegate void AudioStreamMetadataEvent(string metadata);

    /// <summary>
    /// Experimental class for playing (ICY) internet streams.
    /// </summary>
    public sealed class AudioStream : IDisposable
    {
        /// <summary>
        /// Triggered when response headers are received. Is called from the network thread.
        /// </summary>   
        public event AudioStreamConnectedEvent Connected;
        /// <summary>
        /// Triggered when an unexpected disconnection happens. Is called from the network thread.
        /// </summary>
        public event AudioStreamDisconnectedEvent Disconnected;
        /// <summary>
        /// Triggered when a new song starts. Is called from the network thread.
        /// </summary>
        public event AudioStreamMetadataEvent MetadataReceived;

        private ma_device_ptr device;
        private ma_decoder_ptr decoder;
        private ma_decoder_read_proc decoderReadProc;
        private ma_decoder_seek_proc decoderSeekProc;
        private ma_device_data_proc deviceDataProc;
        private Socket socket;
        private Stream networkStream;
        private Thread connectionThread;
        private AtomicBool isRunning;
        private RingBuffer audioBuffer;
        private AudioDevice playbackDevice;
        private bool audioInitialized;
        private bool isBuffering;
        private float volume;
        private int metaInterval;
        private int bytesUntilMeta;
        private const int bufferCapacity = 1024 * 128;
        private const int bufferThreshold = 1024 * 64; 

        public float Volume
        {
            get => volume;
            set => volume = Math.Max(value, 0.0f);
        }

        public AudioStream(AudioDevice playbackDevice = null)
        {
            this.playbackDevice = playbackDevice;
            isRunning = new AtomicBool();
            audioBuffer = new RingBuffer(bufferCapacity);
            decoder = new ma_decoder_ptr(true);
            device = new ma_device_ptr(true);
            decoderReadProc = OnDecoderRead;
            decoderSeekProc = OnDecoderSeek;
            deviceDataProc = OnDeviceData;
            volume = 1.0f;
            audioInitialized = false;
            isBuffering = true;
            metaInterval = 0;
            bytesUntilMeta = 0;
        }

        public void Play(string url)
        {
            if (isRunning.Load())
            {
                Stop();
            }

            audioBuffer.Reset();
            audioInitialized = false;
            isBuffering = true;
            metaInterval = 0;
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
            if (networkStream != null)
            {
                try
                {
                    networkStream.Close();
                }
                catch
                {
                }
                networkStream.Dispose();
                networkStream = null;
            }

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
            string disconnectReason = "Unknown";
            bool wasUnexpected = false;

            try
            {
                Uri uri = new Uri(url);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                int port = uri.Port;
                if (port == -1)
                {
                    port = uri.Scheme == "https" ? 443 : 80;
                }

                socket.Connect(uri.Host, port);

                NetworkStream rawStream = new NetworkStream(socket, true);

                if (uri.Scheme == "https")
                {
                    SslStream sslStream = new SslStream(rawStream, false);
                    sslStream.AuthenticateAsClient(uri.Host);
                    networkStream = sslStream;
                }
                else
                {
                    networkStream = rawStream;
                }

                string request = "GET " + uri.PathAndQuery + " HTTP/1.1\r\n" +
                                 "Host: " + uri.Host + "\r\n" +
                                 "User-Agent: MiniAudioEx/1.0\r\n" +
                                 "Icy-MetaData: 1\r\n" +
                                 "Connection: close\r\n\r\n";

                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                networkStream.Write(requestBytes, 0, requestBytes.Length);
                networkStream.Flush();

                Receive();
            }
            catch (Exception ex)
            {
                if (isRunning.Load())
                {
                    disconnectReason = ex.Message;
                    wasUnexpected = true;
                }
            }
            finally
            {
                bool expectedStop = !isRunning.Load();
                isRunning.Store(false);

                if (wasUnexpected || (!expectedStop))
                {
                    Disconnected?.Invoke(disconnectReason);
                }
            }
        }

        private void Receive()
        {
            byte[] buffer = new byte[4096];
            bool headerSkipped = false;
            List<byte> headerCollector = new List<byte>();

            try
            {
                while (isRunning.Load() && networkStream != null)
                {
                    int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
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

                            headerSkipped = true;
                            int remainingBytes = bytesRead - payloadOffset;
                            if (remainingBytes > 0)
                            {
                                byte[] remainingData = new byte[remainingBytes];
                                Buffer.BlockCopy(buffer, payloadOffset, remainingData, 0, remainingBytes);
                                ProcessStreamData(remainingData, remainingBytes);
                            }
                        }
                        continue;
                    }

                    ProcessStreamData(buffer, bytesRead);

                    if (!audioInitialized && audioBuffer.Count >= bufferThreshold)
                    {
                        if (!InitializeAudio())
                        {
                            break;
                        }
                    }
                    else if (isBuffering && audioBuffer.Count >= bufferThreshold)
                    {
                        isBuffering = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (isRunning.Load())
                {
                    if (!(ex is IOException || ex is ObjectDisposedException || ex is SocketException))
                    {
                        Console.WriteLine("Receive error: " + ex.Message);
                    }
                }
            }
            finally
            {
                isRunning.Store(false);
            }
        }

        private void ProcessStreamData(byte[] data, int length)
        {
            if (metaInterval <= 0)
            {
                audioBuffer.Write(data, 0, length);
                return;
            }

            int offset = 0;
            while (offset < length)
            {
                if (bytesUntilMeta > 0)
                {
                    int toWrite = Math.Min(bytesUntilMeta, length - offset);
                    audioBuffer.Write(data, offset, toWrite);
                    bytesUntilMeta -= toWrite;
                    offset += toWrite;
                }
                else
                {
                    // Read meta length byte
                    int lengthByte = data[offset];
                    offset++;

                    if (lengthByte > 0)
                    {
                        int metaLength = lengthByte * 16;
                        byte[] metaBuffer = new byte[metaLength];
                        int metaRead = 0;

                        // Metadata might span across network packets
                        while (metaRead < metaLength)
                        {
                            if (offset < length)
                            {
                                metaBuffer[metaRead++] = data[offset++];
                            }
                            else
                            {
                                int r = networkStream.Read(metaBuffer, metaRead, metaLength - metaRead);
                                if (r <= 0)
                                {
                                    break;
                                }
                                metaRead += r;
                            }
                        }

                        string metaString = Encoding.UTF8.GetString(metaBuffer).TrimEnd('\0');
                        MetadataReceived?.Invoke(metaString);
                    }

                    bytesUntilMeta = metaInterval;
                }
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

            if (!lines[0].Contains("200"))
            {
                Console.WriteLine("Server returned error: " + lines[0]);
                return false;
            }

            bool isAudio = false;

            Dictionary<string,string> responseHeaders = new Dictionary<string, string>();

            foreach (string line in lines)
            {
                int separatorIndex = line.IndexOf(':');
                if (separatorIndex != -1)
                {
                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();
                    responseHeaders[key] = value;

                    if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value.Contains("audio/") || value.Contains("application/octet-stream"))
                        {
                            isAudio = true;
                        }
                    }

                    if (key.Equals("icy-metaint", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int interval))
                        {
                            metaInterval = interval;
                            bytesUntilMeta = metaInterval;
                        }
                    }
                }
            }

            Connected?.Invoke(responseHeaders);

            return isAudio;
        }

        private unsafe bool InitializeAudio()
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

        private ma_result OnDecoderRead(ma_decoder_ptr pDecoder, IntPtr pBufferOut, size_t bytesToRead, out size_t pBytesRead)
        {
            if (isBuffering && audioInitialized)
            {
                pBytesRead = 0;
                return ma_result.success;
            }

            int actualRead = audioBuffer.Read(pBufferOut, (int)bytesToRead.ToUInt32());
            pBytesRead = actualRead;

            if (actualRead > 0)
            {
                return ma_result.success;
            }
            
            if (isRunning.Load())
            {
                if (audioInitialized)
                {
                    isBuffering = true;
                }
                return ma_result.success;
            }

            return ma_result.at_end;
        }

        private ma_result OnDecoderSeek(ma_decoder_ptr pDecoder, Int64 byteOffset, ma_seek_origin origin)
        {
            return ma_result.success;
        }

        private unsafe void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
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