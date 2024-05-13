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

        public Wavetable(int length, IWavetable calculator)
        {
            this.data = new float[length];
            this.length = length;

            for (int i = 0; i < length; i++)
            {
                data[i] = calculator.GetValue((float)i / length);
            }            
        }

        public Wavetable(int length, Func<float, float> fillFunc)
        {
            this.data = new float[length];
            this.length = length;

            for (int i = 0; i < length; i++)
            {
                data[i] = fillFunc((float)i / length);
            }
        }

        public float GetValue(long time, float frequency, float sampleRate)
        {
            return GetValue(frequency, (float)time, sampleRate);
        }

        private float GetValue(float frequency, float time, float sampleRate)
        {
            float phase = time * frequency / sampleRate;
            index = (int)(phase * length);
            float t = phase * length - index;

            float value1 = data[index % length];
            float value2 = data[(index + 1) % length];
            return Interpolate(value1, value2, t);
        }

        private float Interpolate(float value1, float value2, float t)
        {
            return value1 + (value2 - value1) * t;
        }
    }

    public sealed class SawtoothCalculator : IWavetable
    {
        public float GetValue(float phase)
        {
            double tp = phase / (2 * Math.PI);
            double result = 2 * (tp - Math.Floor(0.5 + tp));
            return (float)result;
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
            double w = 1.0f;
            double t = 2 * Math.PI * phase;
            double wt = w * t;
            double f = wt / (2.0f * Math.PI); //frequency

            return (float)Math.Abs(4 * (f - Math.Floor(f + 0.5f))) - 1.0f;
        }
    }
}