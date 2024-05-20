using System;
using System.IO;
using MiniAudioExNET.Core;
using MiniAudioExNET.Compatibility;

namespace MiniAudioExNET.Utilities
{
    /// <summary>
    /// Helper class to convert encoded audio files (WAV/MP3/FLAC) into PCM data.
    /// </summary>
    public sealed class AudioDecoder
    {
        private UInt32 channels;
        private UInt32 sampleRate;
        private float[] data;

        public Int32 Channels
        {
            get
            {
                return (Int32)channels;
            }
        }

        public Int32 SampleRate
        {
            get
            {
                return (Int32)sampleRate;
            }
        }

        public float[] Data
        {
            get
            {
                return data;
            }
        }

        /// <summary>
        /// Initializes this object and on success sets the data.
        /// </summary>
        /// <param name="filepath">The file path to the file to decode.</param>
        /// <param name="desiredChannels">The desired number of channels. Leave at 0 to let the decoder decide.</param>
        /// <param name="desiredSampleRate">The desired sample rate. Leave at 0 to let the decoder decide.</param>
        public AudioDecoder(string filepath, UInt32 desiredChannels = 0, UInt32 desiredSampleRate = 0)
        {
            if(File.Exists(filepath))
            {
                IntPtr pResult = Library.ma_ex_decode_file(filepath, out UInt64 dataLength, out channels, out sampleRate, desiredChannels, desiredSampleRate);

                if(pResult != IntPtr.Zero)
                {
                    data = new float[dataLength];

                    unsafe
                    {
                        Span<float> pData = new Span<float>(pResult.ToPointer(), (int)dataLength);

                        for(int i = 0; i < pData.Length; i++)
                        {
                            data[i] = pData[i];
                        }
                    }

                    Library.ma_ex_free(pResult);
                }
                else
                {
                    Console.WriteLine("Failed to decode file: unsupported format");
                }
            }
            else
            {
                Console.WriteLine("Failed to decode file because the file was not found: " + filepath);
            }
        }
    }
}