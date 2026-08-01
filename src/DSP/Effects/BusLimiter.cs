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

namespace MiniAudioEx.DSP.Effects
{
    /// <summary>
    /// A channel-linked peak limiter for interleaved 32-bit floating-point PCM data.
    /// </summary>
    /// <remarks>
    /// Process one bus from one audio thread. Look-ahead delays the signal by the
    /// configured number of frames. Call <see cref="Reset"/> before reusing the
    /// limiter for a new stream.
    /// </remarks>
    /// <example>
    /// Call the limiter after mixing the engine output:
    /// <code>
    /// MiniAudioNative.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
    /// limiter.Process(pOutput, frameCount);
    /// </code>
    /// </example>
    public sealed class BusLimiter
    {
        private readonly int channels;
        private readonly int lookAheadFrames;
        private readonly float ceiling;
        private readonly float releaseCoefficient;
        private readonly float[] delayBuffer;

        private int delayOffset;
        private int holdFramesRemaining;
        private bool isAttacking;
        private float attackTarget;
        private float attackStep;
        private float gain;

        /// <summary>Gets the sample rate this limiter was created for.</summary>
        public UInt32 SampleRate { get; }

        /// <summary>Gets the number of interleaved channels this limiter processes.</summary>
        public UInt32 Channels => (UInt32)channels;

        /// <summary>Gets the configured look-ahead duration in milliseconds.</summary>
        public float LookAheadMilliseconds { get; }

        /// <summary>Gets the actual look-ahead duration in PCM frames.</summary>
        public Int32 LookAheadFrames => lookAheadFrames;

        /// <summary>Gets the output ceiling in decibels relative to full scale.</summary>
        public float CeilingDB { get; }

        /// <summary>Gets the release time in milliseconds.</summary>
        public float ReleaseMilliseconds { get; }

        /// <summary>
        /// Creates a bus limiter for interleaved 32-bit floating-point PCM data.
        /// </summary>
        /// <param name="sampleRate">The PCM sample rate.</param>
        /// <param name="channels">The number of interleaved channels.</param>
        /// <param name="lookAheadMilliseconds">Look-ahead in milliseconds. Use 0 for zero latency.</param>
        /// <param name="ceilingDB">The maximum sample peak in dBFS. Must not be greater than 0.</param>
        /// <param name="releaseMilliseconds">The gain recovery time constant in milliseconds.</param>
        public BusLimiter(
            UInt32 sampleRate,
            UInt32 channels,
            float lookAheadMilliseconds = 5.0f,
            float ceilingDB = -1.0f,
            float releaseMilliseconds = 100.0f)
        {
            if (sampleRate == 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            if (channels == 0 || channels > Int32.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(channels));
            if (float.IsNaN(lookAheadMilliseconds) || float.IsInfinity(lookAheadMilliseconds) || lookAheadMilliseconds < 0.0f)
                throw new ArgumentOutOfRangeException(nameof(lookAheadMilliseconds));
            if (float.IsNaN(ceilingDB) || float.IsInfinity(ceilingDB) || ceilingDB > 0.0f)
                throw new ArgumentOutOfRangeException(nameof(ceilingDB));
            if (float.IsNaN(releaseMilliseconds) || float.IsInfinity(releaseMilliseconds) || releaseMilliseconds <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(releaseMilliseconds));

            double lookAheadFrameCount = Math.Ceiling((double)sampleRate * lookAheadMilliseconds / 1000.0);
            if (lookAheadFrameCount >= Int32.MaxValue || lookAheadFrameCount * channels > Int32.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(lookAheadMilliseconds));

            SampleRate = sampleRate;
            this.channels = (int)channels;
            LookAheadMilliseconds = lookAheadMilliseconds;
            lookAheadFrames = (int)lookAheadFrameCount;
            CeilingDB = ceilingDB;
            ceiling = (float)Math.Pow(10.0, ceilingDB / 20.0);
            ReleaseMilliseconds = releaseMilliseconds;
            releaseCoefficient = (float)Math.Exp(-1.0 / ((double)sampleRate * releaseMilliseconds / 1000.0));
            delayBuffer = new float[lookAheadFrames * this.channels];

            Reset();
        }

        /// <summary>
        /// Limits an interleaved 32-bit floating-point PCM buffer in place.
        /// </summary>
        /// <param name="pFrames">A pointer to the first sample in the buffer.</param>
        /// <param name="frameCount">The number of PCM frames in the buffer.</param>
        public unsafe void Process(IntPtr pFrames, UInt32 frameCount)
        {
            if (pFrames == IntPtr.Zero || frameCount == 0)
                return;

            float* pFrame = (float*)pFrames;

            fixed (float* pDelayBuffer = delayBuffer)
            {
                for (UInt32 frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    float inputPeak = GetPeak(pFrame);
                    float requiredGain = inputPeak > ceiling ? ceiling / inputPeak : 1.0f;

                    PrepareGain(requiredGain);

                    if (isAttacking)
                    {
                        gain -= attackStep;
                        if (gain <= attackTarget)
                        {
                            gain = attackTarget;
                            isAttacking = false;
                        }
                    }

                    if (lookAheadFrames == 0)
                    {
                        LimitFrame(pFrame, pFrame, requiredGain);
                    }
                    else
                    {
                        float* pDelayedFrame = pDelayBuffer + delayOffset;
                        float delayedPeak = GetPeak(pDelayedFrame);
                        float maximumGain = delayedPeak > ceiling ? ceiling / delayedPeak : 1.0f;
                        LimitFrame(pFrame, pDelayedFrame, maximumGain);

                        delayOffset += channels;
                        if (delayOffset >= delayBuffer.Length)
                            delayOffset = 0;
                    }

                    if (holdFramesRemaining > 0)
                        holdFramesRemaining--;

                    if (!isAttacking && holdFramesRemaining == 0 && gain < 1.0f)
                        gain = 1.0f - (1.0f - gain) * releaseCoefficient;

                    pFrame += channels;
                }
            }
        }

        /// <summary>Clears delayed samples and resets gain reduction.</summary>
        public void Reset()
        {
            Array.Clear(delayBuffer, 0, delayBuffer.Length);
            delayOffset = 0;
            holdFramesRemaining = 0;
            isAttacking = false;
            attackTarget = 1.0f;
            attackStep = 0.0f;
            gain = 1.0f;
        }

        private unsafe float GetPeak(float* pFrame)
        {
            float peak = 0.0f;
            for (int channel = 0; channel < channels; channel++)
            {
                float sample = Math.Abs(pFrame[channel]);
                if (sample > peak)
                    peak = sample;
            }
            return peak;
        }

        private void PrepareGain(float requiredGain)
        {
            if (requiredGain >= 1.0f)
                return;

            int holdFrames = lookAheadFrames + 1;
            if (holdFrames > holdFramesRemaining)
                holdFramesRemaining = holdFrames;

            if (lookAheadFrames == 0)
            {
                if (requiredGain < gain)
                    gain = requiredGain;
                return;
            }

            if (requiredGain >= gain)
                return;

            float requiredStep = (gain - requiredGain) / lookAheadFrames;
            if (!isAttacking)
            {
                isAttacking = true;
                attackTarget = requiredGain;
                attackStep = requiredStep;
                return;
            }

            if (requiredGain < attackTarget)
                attackTarget = requiredGain;
            if (requiredStep > attackStep)
                attackStep = requiredStep;
        }

        private unsafe void LimitFrame(float* pDestination, float* pSource, float maximumGain)
        {
            if (gain > maximumGain)
                gain = maximumGain;

            if (isAttacking && gain <= attackTarget)
            {
                isAttacking = false;
            }

            for (int channel = 0; channel < channels; channel++)
            {
                float input = pSource[channel];
                float output = input * gain;

                if (pSource != pDestination)
                    pSource[channel] = pDestination[channel];

                if (output > ceiling)
                    output = ceiling;
                else if (output < -ceiling)
                    output = -ceiling;

                pDestination[channel] = output;
            }
        }
    }
}
