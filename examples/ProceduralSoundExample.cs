//An example of how to procedurally generate sound with the 'Read' callback.
using System;
using MiniAudioEx.Core.StandardAPI;
using MiniAudioEx.Native;

namespace MiniAudioExExamples
{
    public class ProceduralSoundExample
    {
		private AudioApp application;
        private AudioSource source;
        private long timeCounter = 0;

		public ProceduralSoundExample()
        {
            application = new AudioApp(44100, 2);
            application.Loaded += OnLoaded;
        }

		public void Run()
		{
            application.Run();			
		}

        private void OnLoaded()
		{
			source = new AudioSource();
			source.Read += OnAudioRead;
			source.Play();
		}

        private void OnAudioRead(NativeArray<float> framesOut, ulong frameCount, int channels)
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