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
using MiniAudioEx.Core.StandardAPI;

namespace MiniAudioEx.DSP
{
    public enum WaveType
    {
        Sine,
        Square,
        Triangle,
        Saw
    }

    public sealed class Oscillator
    {
        private delegate float WaveFunction(float phase);
        private WaveFunction waveFunc;
        private WaveType type;
        private float frequency;
        private float amplitude;
        private float phase;
        private float phaseIncrement;
        private static readonly float TAU = (float)(2 * Math.PI);

        public WaveType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                SetWaveFunction();
            }
        }

        public float Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
                SetPhaseIncrement();
            }
        }

        public float Amplitude
        {
            get
            {
                return amplitude;
            }
            set
            {
                amplitude = value;
            }
        }

        public float Phase
        {
            get
            {
                return phase;
            }
            set
            {
                phase = value;
            }
        }

        public Oscillator(WaveType type, float frequency, float amplitude)
        {
            this.type = type;
            this.phase = 0.0f;
            this.frequency = frequency;
            this.amplitude = amplitude;
            SetPhaseIncrement();
            SetWaveFunction();
        }

        /// <summary>
        /// Resets the phase.
        /// </summary>
        public void Reset()
        {
            phase = 0;
        }

        public float GetValue()
        {
            float result = waveFunc(phase);
            phase += phaseIncrement;
            phase %= TAU;
            return result * amplitude;
        }

        /// <summary>
        /// Gets a sample by the given phase term instead of using the phase stored by this instance.
        /// </summary>
        /// <param name="phase">Phase must be between 0 and 2 * PI</param>
        /// <returns></returns>
        public float GetValueAtPhase(float phase)
        {
            return waveFunc(phase) * amplitude;
        }

        /// <summary>
        /// Modulates the generated sample by the given phase term.
        /// </summary>
        /// <param name="phase">Phase must be between 0 and 2 * PI</param>
        /// <returns></returns>
        public float GetModulatedValue(float phase)
        {
            float result = waveFunc(this.phase + phase);
            this.phase += phaseIncrement;
            this.phase %= TAU;
            return result * amplitude;
        }

        private void SetWaveFunction()
        {
            switch(type)
            {
                case WaveType.Saw:
                    waveFunc = GetSawSample;
                    break;
                case WaveType.Sine:
                    waveFunc = GetSineSample;
                    break;
                case WaveType.Square:
                    waveFunc = GetSquareSample;
                    break;
                case WaveType.Triangle:
                    waveFunc = GetTriangleSample;
                    break;
            }
        }

        private void SetPhaseIncrement()
        {
            phaseIncrement = TAU * frequency / AudioContext.SampleRate;
        }

        public static float GetSawSample(float phase) 
        {
            phase = phase / TAU;
            return 2.0f * phase - 1.0f;
        }

        public static float GetSineSample(float phase) 
        {
            return (float)Math.Sin(phase);
        }

        public static float GetSquareSample(float phase) 
        {
            return (float)Math.Sign(Math.Sin(phase));
        }

        public static float GetTriangleSample(float phase) 
        {
            phase = phase / TAU;
            return (float)(2 * Math.Abs(2 * (phase - 0.5)) - 1);
        }
    }
}