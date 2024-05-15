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

namespace MiniAudioExNET.Synthesis
{
    public interface IWavetable
    {
        float GetValue(float phase);
    }

    public sealed class Wavetable
    {
        private readonly float[] data;
        private readonly int length;
        private int index;

        public Wavetable(IWavetable calculator, int length)
        {
            this.data = new float[length];
            this.length = length;

            for (int i = 0; i < length; i++)
            {
                data[i] = calculator.GetValue((float)i / length);
            }            
        }

        public Wavetable(float[] data)
        {
            this.data = data;
            this.length = data.Length;
        }

        public float GetValue(long time, float frequency, float sampleRate)
        {
            return GetValue(frequency, (float)time, sampleRate);
        }

        /// <summary>
        /// Helper method to calculate how many samples are needed for a single period of a signal by given frequency and sample rate.
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public static int CalculateSampleLengthForFrequency(float frequency, float sampleRate)
        {
            return (int)Math.Ceiling(sampleRate / sampleRate);
        }

        private float GetValue(float frequency, float time, float sampleRate)
        {
            float phase = time * frequency / sampleRate;
            index = (int)(phase * length);
            float t = phase * length - index;

            int i1 = index % length;
            int i2 = (index+1) % length;

            if(i1 < 0 || i2 < 0)
                return 0.0f;

            float value1 = data[i1];
            float value2 = data[i2];
            return Interpolate(value1, value2, t);
        }

        private float Interpolate(float value1, float value2, float t)
        {
            return value1 + (value2 - value1) * t;
        }
    }

    public sealed class SawCalculator : IWavetable
    {
        public float GetValue(float phase)
        {
            return 2 * (phase - 0.5f);
        }
    }

    public sealed class SineCalculator : IWavetable
    {
        public float GetValue(float phase)
        {
            return (float)System.Math.Sin(2 * Math.PI * phase);
        }
    }

    public sealed class SquareCalculator : IWavetable
    {
        public float GetValue(float phase)
        {
            return (float)Math.Sign(System.Math.Sin(2 * Math.PI * phase));
        }
    }

    public sealed class TriangleCalculator : IWavetable
    {
        public float GetValue(float phase)
        {
            return 2 * (float)Math.Abs(2 * (phase - 0.5)) - 1;
        }
    }
}