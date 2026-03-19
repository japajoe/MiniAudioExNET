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
using System.Runtime.InteropServices;
using MiniAudioEx.Native;
using static MiniAudioEx.Native.MiniAudio;

namespace MiniAudioEx.Core
{
    public sealed class AudioClip : IDisposable
    {
        private ma_sound_ptr sound;
        private string filePath;
        private IntPtr dataHandle;
        private UInt64 dataLength;
        private bool streamFromDisk;
        private UInt64 pcmLength;
        private UInt64 hashCode;
        public ma_sound_ptr Sound => sound;

        /// <summary>
        /// Gets the length of the audio clip in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 LengthSamples => pcmLength;
        public UInt64 HashCode => hashCode;

        public AudioClip()
        {
            sound = new ma_sound_ptr(true);
            dataHandle = IntPtr.Zero;
            dataLength = 0;
            streamFromDisk = false;
            pcmLength = 0;
            hashCode = 0;
        }

        public AudioClip(string filePath, bool streamFromDisk = true)
        {
            AudioContext context = AudioContext.GetCurrent();

            if(context == null)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because AudioContext is null");
            }

            if(!System.IO.File.Exists(filePath))
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because the file does not exist");
            }

            this.filePath = filePath;
            this.streamFromDisk = streamFromDisk;
            dataHandle = IntPtr.Zero;
            dataLength = 0;
            
            sound = new ma_sound_ptr(true);

            ma_sound_flags flags = streamFromDisk ? ma_sound_flags.stream : ma_sound_flags.decode;
            ma_result result = ma_result.success;
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                result = ma_sound_init_from_file_w(context.Engine, filePath, flags, default, default, sound);
            else
                result = ma_sound_init_from_file(context.Engine, filePath, flags, default, default, sound);

            if (result != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip: " + result);
            }

            ma_sound_get_length_in_pcm_frames(sound, out pcmLength);

            hashCode = (UInt64)filePath.GetHashCode();
        }

        public AudioClip(byte[] data)
        {
            AudioContext context = AudioContext.GetCurrent();

            if(context == null)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because AudioContext is null");
            }

            if(data == null)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because the given data is null");
            }

            sound = new ma_sound_ptr(true);
            streamFromDisk = false;
            dataHandle = Marshal.AllocHGlobal(data.Length);
            dataLength = (UInt64)data.Length;

            if(dataHandle == IntPtr.Zero)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because data could not be allocated");
            }

            Marshal.Copy(data, 0, dataHandle, data.Length);

            ma_sound_flags flags = ma_sound_flags.decode;
            ma_result result = ma_sound_init_from_memory(context.Engine, dataHandle, dataLength, flags, default, default, sound);

            if (result != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip: " + result);
            }

            ma_sound_get_length_in_pcm_frames(sound, out pcmLength);

            hashCode = GetHashCode(data, data.Length);
        }

        internal bool CopyTo(AudioClip other, ma_sound_group_ptr group = default)
        {
            AudioContext context = AudioContext.GetCurrent();

            if(context == null)
            {
                Console.WriteLine("Failed to Copy AudioClip because AudioContext is null");
                return false;
            }

            if(sound.pointer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to Copy AudioClip because sound is null");
                return false;
            }
            
            if(other.sound.pointer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to Copy AudioClip because other.sound is null");
                return false;
            }

            ma_sound_stop(other.sound);
            ma_sound_uninit(other.sound);

            other.streamFromDisk = streamFromDisk;
            other.dataLength = dataLength;
            other.filePath = filePath;
            other.pcmLength = pcmLength;
            other.hashCode = hashCode;

            ma_sound_flags flags = streamFromDisk ? ma_sound_flags.stream : ma_sound_flags.decode;
            ma_result result = ma_result.success;

            if(flags == ma_sound_flags.stream)
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    result = ma_sound_init_from_file_w(context.Engine, filePath, flags, group, default, other.sound);
                else
                    result = ma_sound_init_from_file(context.Engine, filePath, flags, group, default, other.sound);
            }
            else
            {
                if(dataHandle == IntPtr.Zero)
                    result = ma_sound_init_copy(context.Engine, sound, flags, group, other.sound);
                else
                    result = ma_sound_init_from_memory(context.Engine, dataHandle, dataLength, flags, group, default, other.sound);
            }
            
            return result == ma_result.success;
        }

        public void Dispose()
        {
            if(sound.pointer != IntPtr.Zero)
            {
                ma_sound_uninit(sound);
                sound.Free();
            }

            if(dataHandle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(dataHandle);
                dataHandle = IntPtr.Zero;
                dataLength = 0;
            }
        }

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