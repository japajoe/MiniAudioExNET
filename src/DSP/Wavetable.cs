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

namespace MiniAudioEx.DSP
{
    public interface IWaveCalculator
    {
        float GetValue(float phase);
    }

    public sealed class Wavetable
    {
        private readonly float[] data;
        private readonly int length;
        private int index;
        private float phase;
        private float phaseIncrement;
        private readonly float TAU = (float)(2 * Math.PI);

        public Wavetable(IWaveCalculator calculator, int length)
        {
            this.data = new float[length];
            this.length = length;
            this.phase = 0;
            this.phaseIncrement = 0;

            float phaseIncrement = (float)((2 * Math.PI) / length);

            for (int i = 0; i < length; i++)
            {
                data[i] = calculator.GetValue(i * phaseIncrement);
            }
        }

        public Wavetable(float[] data)
        {
            this.data = data;
            this.length = data.Length;
            this.phase = 0;
            this.phaseIncrement = 0;

            //To do:
            //If signal is non periodic (discrete then phase is between 0 and data.Length)
            //Phase increment is calculated like: data.Length * frequency / sampleRate
        }

        /// <summary>
        /// Gets a sample by the given frequency and sample rate.
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public float GetValue(float frequency, float sampleRate)
        {
            float phase = this.phase > 0.0f ? (this.phase / TAU) : 0.0f;

            this.phaseIncrement = TAU * frequency / sampleRate;
            this.phase += this.phaseIncrement;
            this.phase %= TAU;

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

        /// <summary>
        /// Gets a sample by the given phase term.
        /// </summary>
        /// <param name="phase">Phase must be between 0 and 2 * PI</param>
        /// <returns></returns>
        public float GetValueAtPhase(float phase)
        {
            phase = phase > 0.0f ? (phase / TAU) : 0.0f;

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
}