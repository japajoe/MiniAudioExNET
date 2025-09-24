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
// Copyright 2025 W.M.R Jap-A-Joe

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
using MiniAudioEx.Core.StandardAPI;
using MiniAudioEx.Native;

namespace MiniAudioEx.DSP
{
    public sealed class AudioRecorder : IAudioEffect
    {
        private enum State
        {
            Idle,
            WriteHeader,
            WriteData,
            CloseFile
        }

        private string currentFileName;
        private byte[] outputBuffer;
        private UInt64 bytesWritten;
        private FileStream stream;
        private State state;
        private readonly object stateLock = new object();

        public AudioRecorder()
        {
            outputBuffer = new byte[4096];
            bytesWritten = 0;
            state = State.Idle;
        }

        public void Start()
        {
            if(GetState() != State.Idle) 
                return;

            SetState(State.WriteHeader);
        }

        public void Stop()
        {
            CloseFile();
        }

		public void OnProcess(NativeArray<float> framesIn, UInt32 frameCountIn, NativeArray<float> framesOut, ref UInt32 frameCountOut, UInt32 channels)
		{
            WriteHeader();
            WriteData(framesOut, frameCountIn);
		}

        public void OnDestroy()
        {
            CloseFile();
        }

        private State GetState()
        {
            lock (stateLock)
            {
                return state;
            }
        }

        private void SetState(State newState)
        {
            lock (stateLock)
            {
                state = newState;
            }
        }

        private void WriteHeader()
        {
            if(GetState() != State.WriteHeader)
                return;

            if(bytesWritten > 0)
                return;

            if(stream != null)
                return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Recording");
            Console.ForegroundColor = ConsoleColor.White;

            currentFileName = "res/recordings/" + DateTime.Now.Ticks.ToString() + ".wav";

            byte[] header = new byte[44];

            const Int32 bitDepth = 16;

            Int32 chunkId = 1179011410;           //"RIFF
            Int32 chunkSize = 0;
            Int32 format = 1163280727;            //"WAVE"
            Int32 subChunk1Id = 544501094;        //"fmt "
            Int32 subChunk1Size = 16;
            Int16 audioFormat = 1;
            Int16 numChannels = (Int16)AudioContext.Channels;
            Int32 sampleRate = AudioContext.SampleRate;
            Int32 byteRate = sampleRate * numChannels * bitDepth / 8;
            Int16 blockAlign = (Int16)(numChannels * bitDepth / 8);
            Int16 bitsPerSample = bitDepth;
            Int32 subChunk2Id = 1635017060;       //"data"
            Int32 subChunk2Size = 0;
            
            WriteInt32(chunkId, header, 0);
            WriteInt32(chunkSize, header, 4);
            WriteInt32(format, header, 8);
            WriteInt32(subChunk1Id, header, 12);
            WriteInt32(subChunk1Size, header, 16);
            WriteInt16(audioFormat, header, 20);
            WriteInt16(numChannels, header, 22);
            WriteInt32(sampleRate, header, 24);
            WriteInt32(byteRate, header, 28);
            WriteInt16(blockAlign, header, 32);
            WriteInt16(bitsPerSample, header, 34);
            WriteInt32(subChunk2Id, header, 36);
            WriteInt32(subChunk2Size, header, 40);

            stream = new FileStream(currentFileName, FileMode.CreateNew);
            stream.Write(header, 0, header.Length);

            bytesWritten = 0;

            SetState(State.WriteData);
        }

        private void WriteData(NativeArray<float> framesOut, UInt32 frameCount)
        {
            if(GetState() != State.WriteData)
                return;

            if(stream == null)
                return;

            UInt32 byteSize = (UInt32)(framesOut.Length * sizeof(short));

            if(byteSize == 0)
                return;

            if(outputBuffer.Length < byteSize)
                outputBuffer = new byte[byteSize];

            Int32 index = 0;

            unsafe
            {
                fixed(byte *pOutputBuffer = &outputBuffer[0])
                {
                    Int16 *pBuffer = (Int16*)pOutputBuffer;
                    for(Int32 i = 0; i < framesOut.Length; i++)
                    {
                        pBuffer[index] = (short)(framesOut[i] * short.MaxValue);
                        index++;
                    }
                }
            }

            stream.Write(outputBuffer, 0, (Int32)byteSize);

            bytesWritten += byteSize;
        }

        private void CloseFile()
        {
            if(stream != null)
            {
                if(bytesWritten > 0)
                {
                    const Int32 headerSize = 44;
                    Int32 chunkSize = headerSize + (Int32)(bytesWritten - 8);//file size - 8;
                    Int32 subChunk2Size = (Int32)bytesWritten;

                    WriteInt32(chunkSize, outputBuffer, 0);
                    stream.Seek(4, SeekOrigin.Begin);
                    stream.Write(outputBuffer, 0, sizeof(Int32));
                    
                    WriteInt32(subChunk2Size, outputBuffer, 0);
                    stream.Seek(40, SeekOrigin.Begin);
                    stream.Write(outputBuffer, 0, sizeof(Int32));

                    bytesWritten = 0;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Recording stopped");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                stream.Dispose();
                stream = null;
            }

            SetState(State.Idle);
        }

        private unsafe void WriteInt16(Int16 value, byte[] buffer, Int32 offset)
        {
            fixed(byte *dst = &buffer[offset])
            {
                byte* src = (byte*)&value;
                Buffer.MemoryCopy(src, dst, sizeof(Int16), sizeof(Int16));
            }
        }

        private unsafe void WriteInt32(Int32 value, byte[] buffer, Int32 offset)
        {
            fixed(byte *dst = &buffer[offset])
            {
                byte* src = (byte*)&value;
                Buffer.MemoryCopy(src, dst, sizeof(Int32), sizeof(Int32));
            }
        }

        private unsafe void WriteFloat(float value, byte[] buffer, Int32 offset)
        {
            fixed(byte *dst = &buffer[offset])
            {
                byte* src = (byte*)&value;
                Buffer.MemoryCopy(src, dst, sizeof(float), sizeof(float));
            }
        }
	}
}