//An example of FM synthesis.

using System;
using MiniAudioExNET;
using MiniAudioExNET.DSP;

namespace MiniAudioExNETExamples
{
    class Example6
    {
        static AudioSource source;
        static FMGenerator fmGenerator;

        static void Main(string[] args)
        {
            AudioApp application = new AudioApp(44100, 2);
            application.Loaded += OnLoaded;
            application.Update += OnUpdate;
            application.Run();
        }

        static void OnLoaded()
        {
            source = new AudioSource();
            
            fmGenerator = new FMGenerator(WaveType.Sine, 110.0f, 1.0f);
            fmGenerator.AddOperator(WaveType.Sine, 55, 1.0f);
            fmGenerator.AddOperator(WaveType.Sine, 22, 0.5f);
            
            source.AddGenerator(fmGenerator);

            source.Play();
        }

        static void OnUpdate(float deltaTime)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if(fmGenerator.Carrier.Frequency < 2000.0f)
                        fmGenerator.Carrier.Frequency += 1.0f;
                }

                if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if(fmGenerator.Carrier.Frequency > 20.0f)
                        fmGenerator.Carrier.Frequency -= 1.0f;
                }
            }
        }
    }

    public class TremoloEffect : IAudioEffect
    {
        private long tickTimer;
        private float frequency;

        public TremoloEffect(float frequency)
        {
            this.frequency = frequency;
            tickTimer = 0;
        }

        public void OnProcess(Span<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;
            for(int i = 0; i < framesOut.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * tickTimer / MiniAudioEx.SampleRate);
                framesOut[i] *= sample;
                if(channels == 2)
                    framesOut[i+1] *= sample;
                tickTimer++;
            }
        }
    }

    public class SineGenerator : IAudioGenerator
    {
        private long tickTimer;
        private float frequency;

        public SineGenerator(float frequency)
        {
            this.frequency = frequency;
            tickTimer = 0;
        }

        public void OnGenerate(Span<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;
            for(int i = 0; i < framesOut.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * tickTimer / MiniAudioEx.SampleRate);
                framesOut[i] = sample;
                if(channels == 2)
                    framesOut[i+1] = sample;
                tickTimer++;
            }
        }
    }
}