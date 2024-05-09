using System;
using System.Threading;

namespace MiniAudioExNET
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            MiniAudioEx.Initialize(48000, 2);
            
            AudioSource source = new AudioSource();
            source.End += OnAudioEnd;
            source.Load += OnAudioLoad;

            AudioClip clip = new AudioClip("some_audio.mp3", true);
            source.Play(clip);
            
            while(true)
            {
                MiniAudioEx.Update();
                Thread.Sleep(10);
            }            
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            MiniAudioEx.Deinitialize();
        }

        private static void OnAudioLoad(AudioSource source)
        {
            Console.WriteLine("Audio loaded");
        }

        private static void OnAudioEnd(AudioSource source)
        {
            Console.WriteLine("Audio ended");
        }
    }
}