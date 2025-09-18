//An example of how to procedurally generate sound with the 'Read' callback.

using System;
using MiniAudioEx.Core.StandardAPI;

namespace MiniAudioExExamples
{
    class Example2
    {
        static AudioSource source;
        static long timeCounter = 0;

        static void Main(string[] args)
        {
            AudioApp application = new AudioApp(44100, 2);
            application.Loaded += OnLoaded;
            application.Run();
        }

        static void OnLoaded()
        {
            source = new AudioSource();
            source.Read += OnAudioRead;
            source.Play();
        }

        static void OnAudioRead(AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0.0f;
            float frequency = 440.0f;

            for(int i = 0; i < framesOut.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * timeCounter / 44100);
                framesOut[i] = sample;
                if(channels == 2)
                    framesOut[i+1] = sample;
                timeCounter++;
            }
        }
    }
}