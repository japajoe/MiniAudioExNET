using System;
using System.Threading;
using MiniAudioEx;

namespace MiniAudioExDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            
            AudioManager.Initialize();

            AudioSource source = new AudioSource();
            AudioClip clip = new AudioClip("some_music.mp3");
            source.Play(clip);

            //If your program has a main loop, you should call AudioManager.Update from there
            while(true)
            {
                AudioManager.Update();
                Thread.Sleep(10);
            }
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            AudioManager.Deinitialize();
        }
    }
}