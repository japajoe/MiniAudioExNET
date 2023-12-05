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
            
            AudioManager.Initialize(44100, 2);

            AudioSource source = new AudioSource();
            source.read += OnAudioRead;
            source.Play();

            //If your program has a main loop, you should call AudioManager.Update from there
            while(true)
            {
                AudioManager.Update();
                Thread.Sleep(10);
            }
        }

        private static ulong timer = 0;
        private static float frequency = 440.0f;

        private static void OnAudioRead(Span<float> data, int channels)
        {
            float sample = 0;

            for(int i = 0; i < data.Length; i += channels)
            {
                sample = MathF.Sin(2 * MathF.PI * frequency * timer / AudioManager.SampleRate);
                data[i] = sample;
                if(channels == 2)
                    data[i+1] = sample;
                timer++;
            }
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            AudioManager.Deinitialize();
        }
    }
}