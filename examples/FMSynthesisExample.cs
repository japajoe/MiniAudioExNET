//An example of FM synthesis.
using System;
using MiniAudioEx.Core.StandardAPI;
using MiniAudioEx.DSP;

namespace MiniAudioExExamples
{
    public class FMSynthesisExample
    {
		private AudioApp application;
        private AudioSource source;
        private FMGenerator fmGenerator;

        public FMSynthesisExample()
        {
            application = new AudioApp(44100, 2);
            application.Loaded += OnLoaded;
            application.Update += OnUpdate;
        }

		public void Run()
		{
            application.Run();
		}

        private void OnLoaded()
		{
			source = new AudioSource();

			fmGenerator = new FMGenerator(WaveType.Sine, 110.0f, 1.0f);
			fmGenerator.AddOperator(WaveType.Sine, 55, 1.0f);
			fmGenerator.AddOperator(WaveType.Sine, 22, 0.5f);

			source.AddGenerator(fmGenerator);

			source.Play();
		}

        private void OnUpdate(float deltaTime)
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
}