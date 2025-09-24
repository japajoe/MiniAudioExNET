//A minimal example of spatial audio.
using System;
using MiniAudioEx.Core.StandardAPI;

namespace MiniAudioExExamples
{
	public class SpatialAudioExample
	{
		private AudioApp application;
		private AudioSource source;
		private AudioListener listener;
		private AudioClip clip;
		private float timer;

		public SpatialAudioExample()
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
			source.Loop = true;
			source.Spatial = true;
			source.Position = new Vector3f(10, 0, 0);

			listener = new AudioListener();

			clip = new AudioClip("some_sound.mp3");

			source.Play(clip);

			timer = 0;
		}

		private void OnUpdate(float deltaTime)
		{
			float posX = (float)Math.Sin(timer) * 10.0f;

			source.Position = new Vector3f(posX, 0, 0);

			timer += deltaTime;
		}
    }
}