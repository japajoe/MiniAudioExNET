using System;
using System.IO;

namespace MiniAudioExNET.DSP
{
    /// <summary>
    /// Simple wave file reader for 16 bit wave files.
    /// </summary>
    public sealed class WaveFileReader
    {
        private Int32 chunkId;
        private Int32 chunkSize;
        private Int32 format;
        private Int32 subChunk1Id;
        private Int32 subChunk1Size;
        private Int16 audioFormat;
        private Int16 channels;
        private Int32 sampleRate;
        private Int32 byteRate;
        private Int16 blockAlign;
        private Int16 bitsPerSample;
        private Int32 subChunk2Id;
        private Int32 subChunk2Size;
        private byte[] data;

        public Int32 Channels
        {
            get
            {
                return channels;
            }
        }

        public Int32 SampleRate
        {
            get
            {
                return sampleRate;
            }
        }

        public byte[] Data
        {
            get
            {
                return data;
            }
        }

        public WaveFileReader(string filepath)
        {
            if(File.Exists(filepath))
            {
                data = File.ReadAllBytes(filepath);

                chunkId = BitConverter.ToInt32(data, 0);
                chunkSize = BitConverter.ToInt32(data, 4);
                format = BitConverter.ToInt32(data, 8);
                subChunk1Id = BitConverter.ToInt32(data, 12);
                subChunk1Size = BitConverter.ToInt32(data, 16);
                audioFormat = BitConverter.ToInt16(data, 20);
                channels = BitConverter.ToInt16(data, 22);
                sampleRate = BitConverter.ToInt32(data, 24);
                byteRate = BitConverter.ToInt32(data, 28);
                blockAlign = BitConverter.ToInt16(data, 32);
                bitsPerSample = BitConverter.ToInt16(data, 34);
                subChunk2Id = BitConverter.ToInt32(data, 36);
                subChunk2Size = BitConverter.ToInt32(data, 40);
            }
        }
    }
}