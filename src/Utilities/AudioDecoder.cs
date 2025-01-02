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
using MiniAudioEx.Core;

namespace MiniAudioEx.Utilities
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
                        AudioBuffer<float> pData = new AudioBuffer<float>(pResult.ToPointer(), (int)dataLength);

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