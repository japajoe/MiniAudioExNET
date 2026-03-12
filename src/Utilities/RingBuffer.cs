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

namespace MiniAudioEx.Utilities
{
    public class RingBuffer
    {
        private byte[] buffer;
        private int readIndex;
        private int writeIndex;
        private int count;
        private int mask;
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
            // Ensure size is a power of two for bitwise masking
            int actualSize = 1;
            while (actualSize < size)
            {
                actualSize <<= 1;
            }

            buffer = new byte[actualSize];
            mask = actualSize - 1;
        }

        public void Reset()
        {
            lock (lockObject)
            {
                readIndex = 0;
                writeIndex = 0;
                count = 0;
            }
        }

        public void Write(byte[] data, int offset, int length)
        {
            lock (lockObject)
            {
                int freeSpace = buffer.Length - count;
                int bytesToWrite = Math.Min(length, freeSpace);

                if (bytesToWrite <= 0)
                {
                    return;
                }

                int firstPart = Math.Min(bytesToWrite, buffer.Length - writeIndex);
                Buffer.BlockCopy(data, offset, buffer, writeIndex, firstPart);

                int secondPart = bytesToWrite - firstPart;
                if (secondPart > 0)
                {
                    Buffer.BlockCopy(data, offset + firstPart, buffer, 0, secondPart);
                }

                writeIndex = (writeIndex + bytesToWrite) & mask;
                count += bytesToWrite;
            }
        }

        public int Read(IntPtr destination, int length)
        {
            lock (lockObject)
            {
                int bytesToRead = Math.Min(length, count);

                if (bytesToRead <= 0)
                {
                    return 0;
                }

                int firstPart = Math.Min(bytesToRead, buffer.Length - readIndex);
                System.Runtime.InteropServices.Marshal.Copy(buffer, readIndex, destination, firstPart);

                int secondPart = bytesToRead - firstPart;
                if (secondPart > 0)
                {
                    IntPtr secondDest = new IntPtr(destination.ToInt64() + firstPart);
                    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, secondDest, secondPart);
                }

                readIndex = (readIndex + bytesToRead) & mask;
                count -= bytesToRead;

                return bytesToRead;
            }
        }
    }
}