//Using the 'End' callback to play audio from a playlist.

using System.Collections.Generic;
using MiniAudioExNET;

namespace MiniAudioExNETExamples
{
    class Example4
    {
        static AudioSource source;
        static List<AudioClip> audioClips;
        static int currentClip = 0;

        static void Main(string[] args)
        {
            AudioApp application = new AudioApp(44100, 2);
            application.Loaded += OnLoaded;
            application.Run();
        }

        static void OnLoaded()
        {
            source = new AudioSource();
            source.End += OnPlaybackEnded;

            audioClips = new List<AudioClip>();            
            audioClips.Add(new AudioClip("track_1.mp3"));
            audioClips.Add(new AudioClip("track_2.mp3"));
            audioClips.Add(new AudioClip("track_3.mp3"));
            audioClips.Add(new AudioClip("track_4.mp3"));
            audioClips.Add(new AudioClip("track_5.mp3"));

            source.Play(audioClips[currentClip]);
        }

        static void OnPlaybackEnded()
        {
            currentClip++;
            if(currentClip >= audioClips.Count)
                currentClip = 0;
            source.Play(audioClips[currentClip]);
        }
    }
}