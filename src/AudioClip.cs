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
using System.Runtime.InteropServices;

namespace MiniAudioEx
{
    /// <summary>
    /// Represents audio data that can be played back or streamed by an AudioSource. Supported file types are WAV/MP3/FlAC.
    /// </summary>
    public sealed class AudioClip : IDisposable
    {
        private string filePath;
        private string name;
        private IntPtr handle;
        private UInt64 dataSize;
        private UInt64 hashCode;
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
        /// The name of this AudioClip. If the filepath constructor is used it will contain the filepath, otherwise the string is empty.
        /// </summary>
        /// <value></value>
        public string Name
        {
            get => name;
            set => name = value;
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
        /// Gets the hash code used to identify the data of this AudioClip. Only applicable if the 'byte[] data' overload is used.
        /// </summary>
        /// <value></value>
        public UInt64 Hash
        {
            get => hashCode;
        }

        /// <summary>
        /// If the constructor with 'byte[] data' overload is used this will contain the size of the data in number of bytes.
        /// </summary>
        /// <value></value>
        public UInt64 DataSize
        {
            get
            {
                if(handle != IntPtr.Zero)
                {
                    return dataSize;
                }
                return 0;
            }
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from a file on disk. The file must be in an encoded format.
        /// </summary>
        /// <param name="filePath">The filepath of the encoded audio file (WAV/MP3/FLAC)</param>
        /// <param name="streamFromDisk">If true, streams data from disk rather than loading the entire file into memory for playback. Typically you'd stream from disk if a sound is more than just a couple of seconds long.</param>
        public AudioClip(string filePath, bool streamFromDisk = true)
        {
            if(!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException("Can't create AudioClip because the file does not exist: " + filePath);

            this.filePath = filePath;
            this.name = filePath;
            this.streamFromDisk = streamFromDisk;
            this.handle = IntPtr.Zero;
            this.hashCode = 0;
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from memory. The data must be in an encoded format.
        /// </summary>
        /// <param name="data">Must be encoded audio data (either WAV/MP3/WAV)</param>
        /// <param name="isUnique">If true, then this clip will not use shared memory. If true, this clip will reuse existing memory if possible.</param>
        public AudioClip(byte[] data, bool isUnique = false)
        {
            if(data == null)
                throw new System.ArgumentException("Can't create AudioClip because the data is null");

            this.filePath = string.Empty;
            this.name = string.Empty;
            this.streamFromDisk = false;
            this.dataSize = (UInt64)data.Length;

            if(isUnique)
                this.hashCode = (UInt64)data.GetHashCode();
            else
                this.hashCode = GetHashCode(data, data.Length);

            if(AudioContext.GetAudioClipHandle(hashCode, out IntPtr existingHandle))
            {
                handle = existingHandle;
            }
            else
            {
                handle = Marshal.AllocHGlobal(data.Length);

                if(handle != IntPtr.Zero)
                {            
                    Marshal.Copy(data, 0, handle, data.Length);
                    AudioContext.Add(this);
                }
            }
        }

        public void Dispose()
        {
            AudioContext.Remove(this);
        }

        /// <summary>
        /// This methods creates a hash of the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private UInt64 GetHashCode(byte[] data, int size)
        {
            UInt64 hash = 0;

            for(int i = 0; i < size; i++) 
            {
                hash = data[i] + (hash << 6) + (hash << 16) - hash;
            }

            return hash;            
        }
    }
}