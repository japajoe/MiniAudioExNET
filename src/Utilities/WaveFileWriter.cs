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
// Copyright 2024 W.M.R Jap-A-Joe

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
using System.IO;

namespace MiniAudioEx.Utilities
{
    /// <summary>
    /// A helper class to write PCM data to a 16 bit wave file.
    /// </summary>
    public static unsafe class WaveFileWriter
    {
        public static void Save(string outputFilePath, float[] data, Int32 channels, Int32 sampleRate)
        {
            Int32 payloadSize = data.Length * sizeof(Int16);
            
            const int headerSize = 44;
            byte[] header = new byte[headerSize];

            const Int32 bitDepth = 16;

            Int32 chunkId = 1179011410;                     //"RIFF
            Int32 chunkSize = headerSize + payloadSize - 8; //File size - 8
            Int32 format = 1163280727;                      //"WAVE"
            Int32 subChunk1Id = 544501094;                  //"fmt "
            Int32 subChunk1Size = 16;
            Int16 audioFormat = 1;
            Int16 numChannels = (Int16)channels;
            Int32 byteRate = sampleRate * numChannels * bitDepth / 8;
            Int16 blockAlign = (Int16)(numChannels * bitDepth / 8);
            Int16 bitsPerSample = bitDepth;
            Int32 subChunk2Id = 1635017060;                 //"data"
            Int32 subChunk2Size = payloadSize;

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
                Int32 bytesRemaining = payloadSize;
                Int32 index = 0;

                fixed(byte *pBuffer = &buffer[0])
                {
                    while(bytesRemaining > 0)
                    {
                        Int32 bytesToWrite = Math.Min(buffer.Length, bytesRemaining);
                        Int32 increment = sizeof(Int16);

                        for(int i = 0; i < bytesToWrite; i+=increment)
                        {
                            WriteInt16((Int16)(data[index] * Int16.MaxValue), pBuffer, i);
                            index++;
                        }

                        stream.Write(buffer, 0, bytesToWrite);
                        bytesRemaining -= bytesToWrite;
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