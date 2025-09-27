//A more advanced example of generating a sine wave, applying a tremolo effect to it, 
//and have it play in 3D space.

using System;
using MiniAudioEx.Core.StandardAPI;
using MiniAudioEx.DSP.Generators;
using MiniAudioEx.Native;

namespace MiniAudioExExamples
{
    public class AdvancedProceduralSoundExample
    {
		private AudioApp application;
        private AudioSource source;
        private AudioListener listener;
        private SineGenerator sineGenerator;
        private TremoloEffect tremoloEffect;
        private double timer;

        public AdvancedProceduralSoundExample()
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
			source.DopplerFactor = 0.1f;
			source.Position = new Vector3f(0, 0, 0);
			source.MinDistance = 1.0f;
			source.MaxDistance = 200.0f;
			source.AttenuationModel = AttenuationModel.Exponential;
			//Simply set Spatial to false to disable any 3D effects on the source
			source.Spatial = true;

			listener = new AudioListener();
			listener.Position = new Vector3f(0, 0, 3);

			sineGenerator = new SineGenerator(440);
			tremoloEffect = new TremoloEffect(8);

			source.AddGenerator(sineGenerator);
			source.AddEffect(tremoloEffect);

			source.Play();
		}

        private void OnUpdate(float deltaTime)
        {
            float direction = (float)Math.Sin(timer * 0.5);
            source.Position = new Vector3f(20, 0, 0) * direction;
            source.Velocity = source.GetCalculatedVelocity();
            timer += deltaTime;
        }
    }

    public class TremoloEffect : IAudioEffect
    {
		private WaveCalculator calculator;
        private long tickTimer;
        private float frequency;

		public TremoloEffect(float frequency)
		{
			this.frequency = frequency;
			tickTimer = 0;
			calculator = new WaveCalculator(WaveType.Sine);
        }

        public void OnProcess(NativeArray<float> framesIn, uint frameCountIn, NativeArray<float> framesOut, ref uint frameCountOut, uint channels)
        {
            float sample = 0;
			float phase = 0;

            for (int i = 0; i < framesOut.Length; i += (int)channels)
			{
				phase = (float)(2 * Math.PI * frequency * tickTimer / AudioContext.SampleRate);
				sample = calculator.GetValue(phase);
				framesOut[i] = framesIn[i] * sample;
				if (channels == 2)
					framesOut[i + 1] = framesIn[i + 1] * sample;
				tickTimer++;
			}
        }

        public void OnDestroy() { }
	}

    public class SineGenerator : IAudioGenerator
    {
		private WaveCalculator calculator;
        private long tickTimer;
        private float frequency;

		public SineGenerator(float frequency)
		{
			this.frequency = frequency;
			tickTimer = 0;
			calculator = new WaveCalculator(WaveType.Sine);
		}

        public void OnGenerate(NativeArray<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;
			float phase = 0;
            for (int i = 0; i < framesOut.Length; i += channels)
			{
				phase = (float)(2 * Math.PI * frequency * tickTimer / AudioContext.SampleRate);
				sample = calculator.GetValue(phase);
				framesOut[i] = sample;
				if (channels == 2)
					framesOut[i + 1] = sample;
				tickTimer++;
			}
        }

        public void OnDestroy() {}
	}
}