using System;
using System.Collections.Generic;
using System.Threading;
using MiniAudioEx;

namespace MiniAudioExDemo
{
    class Program
    {
        private static List<AudioClip> clips = new List<AudioClip>();
        private static int currentClip = 0;
        private static AudioSource source;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            
            AudioManager.Initialize(44100, 2);

            source = new AudioSource();

            //Note: if IsLooping is set on an audio source, this callback is never triggered
            source.playbackEnded += OnPlaybackEnded;
            
            clips.Add(new AudioClip("Track_1.mp3"));
            clips.Add(new AudioClip("Track_2.mp3"));
            clips.Add(new AudioClip("Track_3.mp3"));
            clips.Add(new AudioClip("Track_4.mp3"));
            clips.Add(new AudioClip("Track_5.mp3"));

            source.Play(clips[currentClip]);

            //If your program has a main loop, you should call AudioManager.Update from there
            while(true)
            {
                AudioManager.Update();
                Thread.Sleep(10);
            }
        }

        private static void OnPlaybackEnded()
        {
            currentClip++;
            if(currentClip >= clips.Count)
                currentClip = 0;
            source.Play(clips[currentClip]);
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            AudioManager.Deinitialize();
        }
    }
}