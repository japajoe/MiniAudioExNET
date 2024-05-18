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

// Originally Created by Nigel Redmon on 12/18/12.
// EarLevel Engineering: earlevel.com
// C# Port 2024 W.M.R Jap-A-Joe

using System;

namespace MiniAudioExNET.DSP
{
    public class ADSR
    {
        public double a;
        public double d;
        public double s;
        public double r;

        public ADSR(double a, double d, double s, double r)
        {
            this.a = a * MiniAudioEx.SampleRate;
            this.d = d * MiniAudioEx.SampleRate;
            this.s = s * MiniAudioEx.SampleRate;
            this.r = r * MiniAudioEx.SampleRate;
        }
    }

    public sealed class Envelope
    {
        public enum EnvelopeState
        {
            Idle = 0,
            Attack,
            Decay,
            Sustain,
            Release
        }

        private EnvelopeState state;
        private double output;
        private double attackRate;
        private double decayRate;
        private double releaseRate;
        private double attackCoef;
        private double decayCoef;
        private double releaseCoef;
        private double sustainLevel;
        private double targetRatioA;
        private double targetRatioDR;
        private double attackBase;
        private double decayBase;
        private double releaseBase;

        public double Attack
        {
            get
            {
                return attackRate;
            }
            set
            {
                SetAttackRate(value);
            }
        }

        public double Decay
        {
            get
            {
                return decayRate;
            }
            set
            {
                SetDecayRate(value);
            }
        }

        public double Sustain
        {
            get
            {
                return sustainLevel;
            }
            set
            {
                SetSustainLevel(value);
            }
        }

        public double Release
        {
            get
            {
                return releaseRate;
            }
            set
            {
                SetReleaseRate(value);
            }
        }

        public EnvelopeState State
        {
            get
            {
                return state;
            }
        }

        public double Output
        {
            get
            {
                return output;
            }
        }

        public Envelope()
        {
            Reset();

            SetAttackRate(0);
            SetDecayRate(0);
            SetReleaseRate(0);
            SetSustainLevel(1.0);
            SetTargetRatioA(0.3f);
            SetTargetRatioDR(0.0001);

            state = EnvelopeState.Attack;
        }

        public Envelope(ADSR config)
        {
            Reset();

            SetAttackRate(config.a);
            SetDecayRate(config.d);
            SetReleaseRate(config.r);
            SetSustainLevel(config.s);
            SetTargetRatioA(0.3f);
            SetTargetRatioDR(0.0001);

            state = EnvelopeState.Attack;
        }

        public Envelope(double attackRate, double decayRate, double sustainLevel, double releaseRate)
        {
            Reset();

            SetAttackRate(attackRate);
            SetDecayRate(decayRate);
            SetReleaseRate(releaseRate);
            SetSustainLevel(sustainLevel);
            SetTargetRatioA(0.3f);
            SetTargetRatioDR(0.0001);

            state = EnvelopeState.Attack;
        }

        public float Process()
        {
            switch (state) 
            {
                case EnvelopeState.Idle:
                {
                    break;
                }
                case EnvelopeState.Attack:
                {
                    output = attackBase + output * attackCoef;
                    if (output >= 1.0) {
                        output = 1.0;
                        state = EnvelopeState.Decay;
                    }
                    break;
                }
                case EnvelopeState.Decay:
                {
                    output = decayBase + output * decayCoef;
                    if (output <= sustainLevel) {
                        output = sustainLevel;
                        state = EnvelopeState.Sustain;
                    }
                    break;
                }
                case EnvelopeState.Sustain:
                {
                    break;
                }
                case EnvelopeState.Release:
                {
                    output = releaseBase + output * releaseCoef;
                    if (output <= 0.0) {
                        output = 0.0;
                        state = EnvelopeState.Idle;
                    }
                    break;
                }
            }
            return (float)output;
        }

        public void SetGate(bool enabled)
        {
            if (enabled)
                state = EnvelopeState.Attack;
            else if (state != EnvelopeState.Idle)
                state = EnvelopeState.Release;
        }

        public void Reset()
        {
            state = EnvelopeState.Idle;
            output = 0.0;
        }

        private void SetAttackRate(double rate)
        {
            attackRate = rate;
            attackCoef = CalcCoef(rate, targetRatioA);
            attackBase = (1.0 + targetRatioA) * (1.0 - attackCoef);
        }

        private void SetDecayRate(double rate)
        {
            decayRate = rate;
            decayCoef = CalcCoef(rate, targetRatioDR);
            decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
        }

        private void SetReleaseRate(double rate)
        {
            releaseRate = rate;
            releaseCoef = CalcCoef(rate, targetRatioDR);
            releaseBase = -targetRatioDR * (1.0 - releaseCoef);
        }

        private double CalcCoef(double rate, double targetRatio)
        {
            return (rate <= 0) ? 0.0 : Math.Exp(-Math.Log((1.0 + targetRatio) / targetRatio) / rate);
        }

        private void SetSustainLevel(double level)
        {
            sustainLevel = level;
            decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
        }

        private void SetTargetRatioA(double targetRatio)
        {
            if (targetRatio < 0.000000001)
                targetRatio = 0.000000001;  // -180 dB
            targetRatioA = targetRatio;
            attackCoef = CalcCoef(attackRate, targetRatioA);
            attackBase = (1.0 + targetRatioA) * (1.0 - attackCoef);
        }

        private void SetTargetRatioDR(double targetRatio)
        {
            if (targetRatio < 0.000000001)
                targetRatio = 0.000000001;  // -180 dB
            targetRatioDR = targetRatio;
            decayCoef = CalcCoef(decayRate, targetRatioDR);
            releaseCoef = CalcCoef(releaseRate, targetRatioDR);
            decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
            releaseBase = -targetRatioDR * (1.0 - releaseCoef);
        }
    }
}