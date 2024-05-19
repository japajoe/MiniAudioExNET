//A more advanced example of generating a sine wave, applying a tremolo effect to it, 
//and have it play in 3D space.

using System;
using System.Collections.Generic;
using MiniAudioExNET;
using MiniAudioExNET.DSP;
using MiniAudioExNET.Compatibility; //Needed for Span<T> compatibility on netstandard 2.0

namespace MiniAudioExNETExamples
{
    class Example5
    {
        static AudioSource source;
        static AudioListener listener;
        static SineGenerator sineGenerator;
        static TremoloEffect tremoloEffect;
        static double timer;

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
            source.DopplerFactor = 0.1f;
            source.Position = new Vector3f(0, 0, 0);
            source.MinDistance = 1.0f;
            source.MaxDistance = 200.0f;
            source.AttenuationModel = AttenuationModel.Exponential;
            //Simply set Spatial to false to disable any 3D effects on the source
            source.Spatial = true;

            listener = new AudioListener();
            listener.Position = new Vector3f(0, 0, 0);

            sineGenerator = new SineGenerator(440);
            tremoloEffect = new TremoloEffect(8);

            source.AddGenerator(sineGenerator);
            source.AddEffect(tremoloEffect);

            source.Play();
        }

        static void OnUpdate(float deltaTime)
        {
            float direction = (float)Math.Sin(timer * 0.5);
            source.Position = new Vector3f(100, 0, 0) * direction;
            source.Velocity = source.GetCalculatedVelocity();
            timer += deltaTime;
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

        public void OnDestroy() {}
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

        public void OnDestroy() {}
    }
}