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
using System.Runtime.InteropServices;

namespace MiniAudioExNET
{
    /// <summary>
    /// Represents audio data that can be played back or streamed by an AudioSource. Supported file types are WAV/MP3/FlAC.
    /// </summary>
    public sealed class AudioClip : IDisposable
    {
        private string filePath;
        private byte[] data;
        private IntPtr handle;
        private bool streamFromDisk;

        /// <summary>
        /// If the constructor with 'string filePath' overloaded is used this will contain the file path, or string.Empty otherwise.
        /// </summary>
        /// <value></value>
        public string FilePath
        {
            get => filePath;
        }

        /// <summary>
        /// If true, data will be streamed from disk. This is useful when a sound is longer than just a couple of seconds. If data is loaded from memory, this property has no effect.
        /// </summary>
        /// <value></value>
        public bool StreamFromDisk
        {
            get => streamFromDisk;
        }

        /// <summary>
        /// If the constructor with 'byte[] data' overload is used this will contain a pointer to the allocated memory of the data. Do not manually free!
        /// </summary>
        /// <value></value>
        public IntPtr Handle
        {
            get => handle;
        }

        /// <summary>
        /// If the constructor with 'byte[] data' overload is used this will contain the size of the data in number of bytes.
        /// </summary>
        /// <value></value>
        public ulong DataSize
        {
            get
            {
                if(data != null)
                {
                    return (ulong)data.Length;
                }
                return 0;
            }
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from a file on disk. The file must be in an encoded format.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="streamFromDisk"></param>
        public AudioClip(string filePath, bool streamFromDisk = true)
        {
            this.filePath = filePath;
            this.streamFromDisk = streamFromDisk;
            this.handle = IntPtr.Zero;
            MiniAudioEx.Add(this);
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from memory. The data must be in an encoded format.
        /// </summary>
        /// <param name="data"></param>
        public AudioClip(byte[] data)
        {
            this.filePath = string.Empty;
            this.data = data;
            this.streamFromDisk = false;
            this.handle = Marshal.AllocHGlobal(data.Length);

            if(handle != IntPtr.Zero)
            {            
                unsafe
                {
                    byte *ptr = (byte*)handle.ToPointer();
                    for(int i = 0; i < data.Length; i++)
                        ptr[i] = data[i];
                }
                MiniAudioEx.Add(this);
            }
            
        }

        internal void Destroy()
        {
            if(handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            MiniAudioEx.Remove(this);
        }
    }
}