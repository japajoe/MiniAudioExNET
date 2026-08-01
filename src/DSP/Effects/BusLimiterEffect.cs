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
using MiniAudioEx.Native;

namespace MiniAudioEx.DSP.Effects
{
    /// <summary>
    /// A channel-linked peak limiter effect for interleaved 32-bit floating-point PCM data.
    /// </summary>
    /// <remarks>
    /// Process one effect instance from one audio thread. Look-ahead delays the
    /// signal by the configured number of frames. <see cref="OnDestroy"/> resets
    /// delayed samples and gain reduction.
    /// </remarks>
    /// <example>
    /// Add the limiter to an audio source's effect chain:
    /// <code>
    /// BusLimiterEffect limiter = new BusLimiterEffect(sampleRate, channels);
    /// source.AddEffect(limiter);
    /// </code>
    /// </example>
    public sealed class BusLimiterEffect : IAudioEffect
    {
        private readonly BusLimiter limiter;

        /// <summary>Gets the sample rate this limiter was created for.</summary>
        public UInt32 SampleRate => limiter.SampleRate;

        /// <summary>Gets the number of interleaved channels this limiter processes.</summary>
        public UInt32 Channels => limiter.Channels;

        /// <summary>Gets the configured look-ahead duration in milliseconds.</summary>
        public float LookAheadMilliseconds => limiter.LookAheadMilliseconds;

        /// <summary>Gets the actual look-ahead duration in PCM frames.</summary>
        public Int32 LookAheadFrames => limiter.LookAheadFrames;

        /// <summary>Gets the output ceiling in decibels relative to full scale.</summary>
        public float CeilingDB => limiter.CeilingDB;

        /// <summary>Gets the release time in milliseconds.</summary>
        public float ReleaseMilliseconds => limiter.ReleaseMilliseconds;

        /// <summary>
        /// Creates a bus limiter effect for interleaved 32-bit floating-point PCM data.
        /// </summary>
        /// <param name="sampleRate">The PCM sample rate.</param>
        /// <param name="channels">The number of interleaved channels.</param>
        /// <param name="lookAheadMilliseconds">Look-ahead in milliseconds. Use 0 for zero latency.</param>
        /// <param name="ceilingDB">The maximum sample peak in dBFS. Must not be greater than 0.</param>
        /// <param name="releaseMilliseconds">The gain recovery time constant in milliseconds.</param>
        public BusLimiterEffect(
            UInt32 sampleRate,
            UInt32 channels,
            float lookAheadMilliseconds = 5.0f,
            float ceilingDB = -1.0f,
            float releaseMilliseconds = 100.0f)
        {
            limiter = new BusLimiter(
                sampleRate,
                channels,
                lookAheadMilliseconds,
                ceilingDB,
                releaseMilliseconds);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">
        /// The processing channel count does not match <see cref="Channels"/>, or
        /// the output buffer does not have enough frames.
        /// </exception>
        public void OnProcess(
            NativeArray<float> framesIn,
            UInt32 frameCountIn,
            NativeArray<float> framesOut,
            ref UInt32 frameCountOut,
            UInt32 channels)
        {
            if (channels != limiter.Channels)
                throw new ArgumentException("The processing channel count must match the limiter channel count.", nameof(channels));
            if (frameCountIn > frameCountOut)
                throw new ArgumentException("The output buffer does not have enough frames.", nameof(frameCountOut));

            framesIn.CopyTo(framesOut);
            limiter.Process(framesOut.Pointer, frameCountIn);
            frameCountOut = frameCountIn;
        }

        /// <inheritdoc />
        public void OnDestroy()
        {
            limiter.Reset();
        }
    }
}
