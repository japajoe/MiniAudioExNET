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
    /// Experimental class to play audio (mp3) from an internet stream.
    /// </summary>
    public unsafe sealed class AudioStream : IDisposable
    {
        private ma_device_ptr device;
        private ma_decoder_ptr decoder;
        private ma_decoder_read_proc decoderReadProc;
        private ma_decoder_seek_proc decoderSeekProc;
        private ma_device_data_proc deviceDataProc;
        private bool isAudioInitialized;
        private Socket socket;
        private Thread connectionThread;
        private AtomicBool isRunning = new AtomicBool();
        private RingBuffer audioBuffer = new RingBuffer(1024 * 128);
        private float volume;

        public float Volume
        {
            get => volume;
            set
            {
                if(value < 0.0f)
                    value = 0.0f;
                volume = value;
            }
        }

        private class AtomicBool
        {
            private int value = 0;

            public bool Load()
            {
                return Interlocked.CompareExchange(ref value, 0, 0) == 1;
            }

            public void Store(bool value)
            {
                Interlocked.Exchange(ref this.value, value ? 1 : 0);
            }
        }

        private class RingBuffer
        {
            private byte[] buffer;
            private int readIndex;
            private int writeIndex;
            private int count;
            private object lockObject = new object();

            public int Count 
            { 
                get 
                { 
                    lock (lockObject) 
                    {
                        return count; 
                    }
                } 
            }

            public RingBuffer(int size)
            {
                buffer = new byte[size];
            }

            public void Write(byte[] data, int offset, int length)
            {
                lock (lockObject)
                {
                    for (int i = 0; i < length; i++)
                    {
                        if (count == buffer.Length)
                        {
                            break;
                        }

                        buffer[writeIndex] = data[offset + i];
                        writeIndex = (writeIndex + 1) % buffer.Length;
                        count++;
                    }
                }
            }

            public int Read(IntPtr destination, int length)
            {
                lock (lockObject)
                {
                    byte *pDestination = (byte*)destination;
                    int bytesToRead = Math.Min(length, count);
                    for (int i = 0; i < bytesToRead; i++)
                    {
                        pDestination[i] = buffer[readIndex];
                        readIndex = (readIndex + 1) % buffer.Length;
                        count--;
                    }
                    return bytesToRead;
                }
            }
        }

        public AudioStream()
        {
            isRunning = new AtomicBool();
            audioBuffer = new RingBuffer(1024 * 128);
            decoder = new ma_decoder_ptr(true);
            device = new ma_device_ptr(true);
            volume = 1.0f;
        }

        public void Play(string url)
        {
            if (isRunning.Load())
                return;

            isRunning.Store(true);
            connectionThread = new Thread(() =>
            {
                Connect(url);
            });
            connectionThread.IsBackground = true;
            connectionThread.Start();
        }

        public void Dispose()
        {
            isRunning.Store(false);
            CloseConnection();

            if (device.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_device_stop(device);
                MiniAudio.ma_device_uninit(device);
                device.Free();
            }

            if (decoder.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_decoder_uninit(decoder);
                decoder.Free();
            }

            isAudioInitialized = false;

            if (connectionThread != null && connectionThread.IsAlive)
            {
                connectionThread.Join();
                connectionThread = null;
            }
        }

        private void CloseConnection()
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

                byte[] requestBytes = Encoding.ASCII.GetBytes(request);
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
                    
                    if (!isAudioInitialized && audioBuffer.Count > 32768)
                    {
                        InitializeAudio();
                    }
                }
            }
            catch (SocketException)
            {
                isRunning.Store(false);
            }
        }

        private void InitializeAudio()
        {
            decoderReadProc = OnDecoderRead;
            decoderSeekProc = OnDecoderSeek;
            deviceDataProc = OnDeviceData;

            var decoderConfig = MiniAudio.ma_decoder_config_init(ma_format.f32, 2, 44100);
            
            ma_result result = MiniAudio.ma_decoder_init(decoderReadProc, decoderSeekProc, IntPtr.Zero, ref decoderConfig, decoder);

            if(result != ma_result.success)
            {
                Console.WriteLine("Failed to create decoder: " + result);
                return;
            }

            ma_device_config deviceConfig = MiniAudio.ma_device_config_init(ma_device_type.playback);
            ma_decoder *pDecoder = decoder.Get();
            deviceConfig.playback.format = pDecoder->outputFormat;
            deviceConfig.playback.channels = pDecoder->outputChannels;
            deviceConfig.sampleRate = pDecoder->outputSampleRate;
            deviceConfig.SetDataCallback(deviceDataProc);
            deviceConfig.pUserData = IntPtr.Zero;

            result = MiniAudio.ma_device_init(new ma_context_ptr(IntPtr.Zero), ref deviceConfig, device);

            if(result != ma_result.success)
            {
                Console.WriteLine("Failed to create device");
                MiniAudio.ma_decoder_uninit(decoder);
                return;
            }

            MiniAudio.ma_device_start(device);
            isAudioInitialized = true;
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
            // if the number of bytes actually read is less than the number of bytes requested (bytesToRead), miniaudio will treat it as
            // if the end of the file has been reached and it will stop decoding
            int actualRead = audioBuffer.Read(pBufferOut, (int)bytesToRead.ToUInt32());
            
            pBytesRead = actualRead;

            if(actualRead > 0)
                return ma_result.success;
            else if(actualRead == 0)
                return ma_result.at_end;
            else
                return ma_result.error;
        }

        private ma_result OnDecoderSeek(ma_decoder_ptr pDecoder, Int64 byteOffset, ma_seek_origin origin) 
        {
            
            return ma_result.success;
        }

		private unsafe void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
		{
            ma_device* device = pDevice.Get();

            if (device == null)
                return;

            if(MiniAudio.ma_decoder_read_pcm_frames(decoder, pOutput, frameCount, IntPtr.Zero) == ma_result.success)
            {
                if(volume == 1.0f)
                    return;
                
                Int32 totalSamples = (Int32)(frameCount * decoder.Get()->outputChannels);
                NativeArray<float> data = new NativeArray<float>(pOutput, totalSamples);

                for(int i = 0; i < data.Length; i++)
                    data[i] *= volume;
            }
		}
    }
}