# MiniAudioExNET
A .NET wrapper for MiniAudioEx. MiniAudioEx is a a modified version of MiniAudio, see [this repository](https://github.com/japajoe/miniaudioex). The goal of MiniAudioExNET is to make it easy to add audio playback to .NET applications. I've tried several libraries in the past and none of them could offer all of the things I was looking for:

- Easy to set up and interact with.
- Spatial audio.
- Cross platform.
- A permissive license.

This library ticks all these boxes. There are some (in my opinion) minor things missing such as more audio format decoders, but at least 3 widely used formats are supported which is sufficient for my needs. If you would like to have support for more formats, then please make your request [here](https://github.com/mackron/miniaudio).

# Features
- Playback of various audio formats such as WAV/MP3/FLAC.
- Stream audio from disk or from memory.
- Callbacks for effects processing and generating audio.
- Spatial properties like doppler effect, pitching, distance attenuation and panning.
- Utilities for audio generation.

# Platforms
MiniAudio was designed to work on every major platform, however I do not have a Mac so I can not build a library for Mac OS. As a result only Windows and Linux libraries are currently available.

# Installation
```
dotnet add package JAJ.Packages.MiniAudioEx --version 1.5.4
```

# General gotchas
- Reuse audio clips. If you have loaded an AudioClip from memory, then the library allocates memory that the garbage collector doesn't free. All memory is freed after calling MiniAudioEx.Deinitialize. It is perfectly fine to reuse audio clips across multiple audio sources, so you don't have to load multiple clips with the same sound. A good strategy is to store your audio clips in an array or a list for the lifetime of your application.
- Call MiniAudioEx.Update from your main thread loop. This method calculates a delta time, and is responsible for moving messages from the audio thread to the main thread. If not called (regularly), the `End` callback will never be able to run.
- The `Process` and `Read` event run on a separate thread as well. You should not call any MiniAudioEx API functions from these callbacks.

# Example 1
Playing audio from a file on disk.
```cs
using System;
using System.Threading;
using MiniAudioExNET;

namespace MiniAudioExExample
{
    class Program
    {
        static readonly uint SAMPLE_RATE = 44100;
        static readonly uint CHANNELS = 2;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            MiniAudioEx.Initialize(SAMPLE_RATE, CHANNELS);
            
            AudioSource source = new AudioSource();

            AudioClip clip = new AudioClip("some_audio.mp3");
            source.Play(clip);
            
            while(true)
            {
                MiniAudioEx.Update();
                Thread.Sleep(10);
            }
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            MiniAudioEx.Deinitialize();
        }
    }
}
```
# Example 2
An example of how to procedurally generate sound with the `Read` callback.
```cs
using System;
using System.Threading;
using MiniAudioExNET;

namespace MiniAudioExExample
{
    class Program
    {
        static readonly uint SAMPLE_RATE = 44100;
        static readonly uint CHANNELS = 2;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            MiniAudioEx.Initialize(SAMPLE_RATE, CHANNELS);
            
            AudioSource source = new AudioSource();
            source.Read += OnAudioRead;
            source.Play();
            
            while(true)
            {
                MiniAudioEx.Update();
                Thread.Sleep(10);
            }
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            MiniAudioEx.Deinitialize();
        }

        static long timeCounter = 0;

        static void OnAudioRead(Span<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0.0f;
            float frequency = 440.0f;

            for(int i = 0; i < framesOut.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * timeCounter / SAMPLE_RATE);
                framesOut[i] = sample;
                if(channels == 2)
                    framesOut[i+1] = sample;
                timeCounter++;
            }
        }
    }
}
```
# Example 3
A minimal example of spatial audio.
```cs
using System;
using System.Threading;
using MiniAudioExNET;

namespace MiniAudioExExample
{
    class Program
    {
        static readonly uint SAMPLE_RATE = 44100;
        static readonly uint CHANNELS = 2;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            MiniAudioEx.Initialize(SAMPLE_RATE, CHANNELS);

            AudioListener listener = new AudioListener();
            listener.Position = new Vector3f(0, 0, 0);
            
            AudioSource source = new AudioSource();

            AudioClip clip = new AudioClip("some_audio.mp3", true);
            source.Loop = true;
            source.Spatial = true;
            source.Position = new Vector3f(10, 0, 0);
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
    }
}
```
# Example 4
This shows how you can use the `End` callback to play audio from a playlist.
```cs
using System;
using System.Threading;
using System.Collections.Generic;
using MiniAudioExNET;

namespace MiniAudioExExample
{
    class Program
    {
        static readonly uint SAMPLE_RATE = 44100;
        static readonly uint CHANNELS = 2;
        static AudioSource source;
        static List<AudioClip> audioClips;
        static int currentClip = 0;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            MiniAudioEx.Initialize(SAMPLE_RATE, CHANNELS);

            audioClips = new List<AudioClip>();            
            audioClips.Add(new AudioClip("track_1.mp3"));
            audioClips.Add(new AudioClip("track_2.mp3"));
            audioClips.Add(new AudioClip("track_3.mp3"));
            audioClips.Add(new AudioClip("track_4.mp3"));
            audioClips.Add(new AudioClip("track_5.mp3"));

            source = new AudioSource();
            
            //End callback will not trigger if source has Loop set to true
            //Will also not trigger if you don't call MiniAudioEx.Update
            source.End += OnPlaybackEnded;

            source.Play(audioClips[currentClip]);
            
            while(true)
            {
                MiniAudioEx.Update();
                Thread.Sleep(10);
            }
        }

        static void OnPlaybackEnded()
        {
            currentClip++;
            if(currentClip >= audioClips.Count)
                currentClip = 0;
            source.Play(audioClips[currentClip]);
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            MiniAudioEx.Deinitialize();
        }
    }
}
```
# Example 5
A more advanced example of generating a sine wave, applying a tremolo effect to it, and have it play in 3D space.
```csharp
using System;
using System.Threading;
using MiniAudioExNET;

namespace MiniAudioExExample
{
    class Program
    {
        static readonly uint SAMPLE_RATE = 44100;
        static readonly uint CHANNELS = 2;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            MiniAudioEx.Initialize(SAMPLE_RATE, CHANNELS);

            AudioListener listener = new AudioListener();
            listener.Position = new Vector3f(0, 0, 0);
            
            AudioSource source = new AudioSource();

            var sineGenerator = new SineGenerator(440);
            var tremoloEffect = new TremoloEffect(8);

            source.AddGenerator(sineGenerator);
            source.AddEffect(tremoloEffect);
            source.DopplerFactor = 0.1f;
            source.Position = new Vector3f(0, 0, 0);
            source.MinDistance = 1.0f;
            source.MaxDistance = 200.0f;
            source.AttenuationModel = AttenuationModel.Exponential;
            //Simply set Spatial to false to disable any 3D effects on the source
            source.Spatial = true;
            source.Play();

            double timer = 0;
            
            while(true)
            {
                MiniAudioEx.Update();

                float direction = (float)Math.Sin(timer * 0.5);
                source.Position = new Vector3f(100, 0, 0) * direction;
                source.Velocity = source.GetCalculatedVelocity();

                timer += MiniAudioEx.DeltaTime;

                Thread.Sleep(10);
            }            
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            MiniAudioEx.Deinitialize();
        }
    }

    public class TremoloEffect : IAudioEffect
    {
        private long tickTimer;
        private float frequency;

        public TremoloEffect(float frequency)
        {
            this.frequency = frequency;
            tickTimer = 0;
        }

        public void OnProcess(Span<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;
            for(int i = 0; i < framesOut.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * tickTimer / MiniAudioEx.SampleRate);
                framesOut[i] *= sample;
                if(channels == 2)
                    framesOut[i+1] *= sample;
                tickTimer++;
            }
        }
    }

    public class SineGenerator : IAudioGenerator
    {
        private long tickTimer;
        private float frequency;

        public SineGenerator(float frequency)
        {
            this.frequency = frequency;
            tickTimer = 0;
        }

        public void OnGenerate(Span<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;
            for(int i = 0; i < framesOut.Length; i+=channels)
            {
                sample = (float)Math.Sin(2 * Math.PI * frequency * tickTimer / MiniAudioEx.SampleRate);
                framesOut[i] = sample;
                if(channels == 2)
                    framesOut[i+1] = sample;
                tickTimer++;
            }
        }
    }
}
```
# Example 6
An example of FM synthesis.
```csharp
using System;
using System.Threading;
using MiniAudioExNET;
using MiniAudioExNET.Synthesis;

namespace MiniAudioExExample
{
    class Example
    {
        static readonly uint SAMPLE_RATE = 44100;
        static readonly uint CHANNELS = 2;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            MiniAudioEx.Initialize(SAMPLE_RATE, CHANNELS);
            
            AudioSource source = new AudioSource();

            var generator = new FMGenerator(WaveType.Sine, 110.0f, 1.0f);
            generator.AddModulator(WaveType.Sine, 55, 1.0f);
            generator.AddModulator(WaveType.Sine, 22, 0.5f);
            source.AddGenerator(generator);
            source.Play();

            while(true)
            {
                MiniAudioEx.Update();

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if(generator.Carrier.Operator.Frequency < 2000.0f)
                            generator.Carrier.Operator.Frequency += 1.0f;
                    }

                    if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if(generator.Carrier.Operator.Frequency > 20.0f)
                            generator.Carrier.Operator.Frequency -= 1.0f;
                    }
                }                

                Thread.Sleep(10);
            }            
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            MiniAudioEx.Deinitialize();
        }
    }
}
```