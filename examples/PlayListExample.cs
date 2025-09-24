//Using the 'End' callback to play audio from a playlist.
using System.Collections.Generic;
using MiniAudioEx.Core.StandardAPI;

namespace MiniAudioExExamples
{
    public class PlayListExample
    {
		private AudioApp application;
        private AudioSource source;
        private List<AudioClip> audioClips;
        private int currentClip = 0;

        public PlayListExample()
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
            source.End += OnPlaybackEnded;

            audioClips = new List<AudioClip>();
            audioClips.Add(new AudioClip("sound1.mp3"));
            audioClips.Add(new AudioClip("sound2.ogg"));
            audioClips.Add(new AudioClip("sound3.flac"));

            source.Play(audioClips[currentClip]);
        }

        private void OnPlaybackEnded()
        {
            currentClip++;
            if (currentClip >= audioClips.Count)
                currentClip = 0;
            source.Play(audioClips[currentClip]);
        }
    }
}