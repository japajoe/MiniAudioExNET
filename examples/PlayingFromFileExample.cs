//Playing audio from a file on disk.
using MiniAudioEx.Core.StandardAPI;

namespace MiniAudioExExamples
{
	public class PlayingFromFileExample
	{
		private AudioApp application;
		private AudioSource source;
		private AudioClip clip;

		public PlayingFromFileExample()
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
			clip = new AudioClip("some_sound.mp3");
			source.Play(clip);
		}
	}
}