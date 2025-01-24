# MiniAudioExNET
A .NET wrapper for MiniAudioEx. MiniAudioEx is a modified version of MiniAudio, see [this repository](https://github.com/japajoe/miniaudioex). The goal of MiniAudioExNET is to make it easy to add audio playback to .NET applications. I've tried several libraries in the past and none of them could offer all of the things I was looking for:

- Easy to set up and interact with.
- Spatial audio.
- Cross platform.
- A permissive license.

This library ticks all these boxes. There are some (in my opinion) minor things missing such as more audio format decoders, but at least 3 widely used formats are supported which is sufficient for my needs. If you would like to have support for more formats, then please make your request [here](https://github.com/mackron/miniaudio).

# Features
- Playback and decoding of various audio formats such as WAV/MP3/FLAC.
- Stream audio from disk or from memory.
- Callbacks for effects processing and generating audio.
- Spatial properties like doppler effect, pitching, distance attenuation and panning.
- Utilities for audio generation.

# Platforms
- Windows x86_64
- Windows x86
- Linux x86_64
- Linux ARM64
- Linux ARM32
- OSX x86_64
- OSX ARM64

# Installation
```
dotnet add package JAJ.Packages.MiniAudioEx --version 1.7.8
```

# Changes in 1.7.8
- Attempt to select default device if no device info was given at initialization.
- Expose `IsDefault` property in `DeviceInfo`.

# Changes in 1.7.7
- Throw exception in `AudioClip` constructor if file or data does not exist.
- Remove unsafe block from `AudioClip` constructor and use Marshal.Copy instead.
- Add `Name` property to `AudioClip` class.
- Check sample rate in `Filter` constructor.

# Changes in 1.7.6
- Add native library for Windows x86.

# Changes in 1.7.5
- Add native libraries for Linux (ARM32 and ARM64).
- Fix possible bug that prevents the native library from loading on OSX x86_64.

# Changes in 1.7.4
- Add native library for OSX x64 (ARM).

# Changes in 1.7.3
- Add native library for OSX x64 (Intel).

# Changes in 1.7.2
- Add global device data processing callback.

# Changes in 1.7.1
- Reuse memory if multiple audio clips use the same data.

# Changes in 1.7.0
- Renamed `MiniAudioExNET` namespace to `MiniAudioEx`.
- Renamed `MiniAudioEx` class to `AudioContext`.
- Got rid of `Span<T>` in favor of `AudioBuffer<T>`.

# General gotchas
- Reuse audio clips. If you have loaded an AudioClip from memory, then the library allocates memory that the garbage collector doesn't free. All memory is freed after calling AudioContext.Deinitialize. It is perfectly fine to reuse audio clips across multiple audio sources, so you don't have to load multiple clips with the same sound. A good strategy is to store your audio clips in an array or a list for the lifetime of your application.
- Call AudioContext.Update from your main thread loop. This method calculates a delta time, and is responsible for moving messages from the audio thread to the main thread. If not called (regularly), the `End` callback will never be able to run.
- The `Process` and `Read` event run on a separate thread as well. You should not call any MiniAudioEx API functions from these callbacks.
- It can happen that I change things over the course of time. The [examples](https://github.com/japajoe/MiniAudioExNET/tree/master/examples) always reflect the use of the API as of the latest available Nuget package so please refer to them if things are different.

# Basic Example
Initializing MiniAudioEx and playing audio from a file on disk.
```cs
using System;
using System.Threading;
using MiniAudioEx;

namespace MiniAudioExExample
{
    class Program
    {
        static readonly uint SAMPLE_RATE = 44100;
        static readonly uint CHANNELS = 2;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            AudioContext.Initialize(SAMPLE_RATE, CHANNELS);
            
            AudioSource source = new AudioSource();

            AudioClip clip = new AudioClip("some_audio.mp3");
            source.Play(clip);
            
            while(true)
            {
                AudioContext.Update();
                Thread.Sleep(10);
            }
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            AudioContext.Deinitialize();
        }
    }
}
```
See links below for more examples. These use the `AudioApp` class which is suitable for simple console based applications.

[Playing audio from a file on disk](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example1.cs)

[Using the 'Read' callback to generate sound](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example2.cs)

[Basic spatial audio](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example3.cs)

[Playing audio from a playlist](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example4.cs)

[Generating sound, applying effects and spatial audio](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example5.cs)

[FM synthesis](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example6.cs)