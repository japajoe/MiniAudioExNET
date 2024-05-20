using System;
using System.IO;

namespace MiniAudioExNET.Utilities
{
    /// <summary>
    /// A helper class to write PCM data to a 16 bit wave file.
    /// </summary>
    public static unsafe class WaveFileWriter
    {
        public static void Save(string outputFilePath, float[] data, Int32 channels, Int32 sampleRate)
        {
            Int32 bytesToWrite = data.Length * sizeof(Int16);
            byte[] header = new byte[44];

            const Int32 bitDepth = 16;

            Int32 chunkId = 1179011410;           //"RIFF
            Int32 chunkSize = 44 + (data.Length * sizeof(Int16)) - 8; //File size - 8
            Int32 format = 1163280727;            //"WAVE"
            Int32 subChunk1Id = 544501094;        //"fmt "
            Int32 subChunk1Size = 16;
            Int16 audioFormat = 1;
            Int16 numChannels = (Int16)channels;
            Int32 byteRate = sampleRate * numChannels * bitDepth / 8;
            Int16 blockAlign = (Int16)(numChannels * bitDepth / 8);
            Int16 bitsPerSample = bitDepth;
            Int32 subChunk2Id = 1635017060;       //"data"
            Int32 subChunk2Size = bytesToWrite;

            fixed(byte *pHeader = &header[0])
            {            
                WriteInt32(chunkId, pHeader, 0);
                WriteInt32(chunkSize, pHeader, 4);
                WriteInt32(format, pHeader, 8);
                WriteInt32(subChunk1Id, pHeader, 12);
                WriteInt32(subChunk1Size, pHeader, 16);
                WriteInt16(audioFormat, pHeader, 20);
                WriteInt16(numChannels, pHeader, 22);
                WriteInt32(sampleRate, pHeader, 24);
                WriteInt32(byteRate, pHeader, 28);
                WriteInt16(blockAlign, pHeader, 32);
                WriteInt16(bitsPerSample, pHeader, 34);
                WriteInt32(subChunk2Id, pHeader, 36);
                WriteInt32(subChunk2Size, pHeader, 40);
            }

            using(FileStream stream = new FileStream(outputFilePath, FileMode.Create))
            {
                stream.Write(header, 0, header.Length);

                byte[] buffer = new byte[4096];
                Int32 bytesRemaining = bytesToWrite;
                Int32 index = 0;

                fixed(byte *pBuffer = &buffer[0])
                {
                    while(bytesRemaining > 0)
                    {
                        Int32 bytesAvailable = Math.Min(buffer.Length, bytesRemaining);
                        Int32 increment = sizeof(Int16);

                        for(int i = 0; i < bytesAvailable; i+=increment)
                        {
                            WriteInt16((Int16)(data[index] * Int16.MaxValue), pBuffer, i);
                            index++;
                        }

                        stream.Write(buffer, 0, bytesAvailable);
                        bytesRemaining -= bytesAvailable;
                    }
                }
            }
        }

        private static void WriteInt16(Int16 value, byte *buffer, Int32 offset)
        {
            byte* src = (byte*)&value;
            Buffer.MemoryCopy(src, (buffer+offset), sizeof(Int16), sizeof(Int16));
        }

        private static void WriteInt32(Int32 value, byte *buffer, Int32 offset)
        {
            byte* src = (byte*)&value;
            Buffer.MemoryCopy(src, (buffer+offset), sizeof(Int32), sizeof(Int32));
        }
    }
}