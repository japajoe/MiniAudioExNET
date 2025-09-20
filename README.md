# MiniAudioExNET
A .NET wrapper for MiniAudioEx. MiniAudioEx is a modified version of MiniAudio, see [this repository](https://github.com/japajoe/miniaudioex). The goal of MiniAudioExNET is to make it easy to add audio playback to .NET applications. I've tried several libraries in the past and none of them could offer all of the things I was looking for:

- Easy to set up and interact with.
- Spatial audio.
- Cross platform.
- A permissive license.

# Features
- Playback and decoding of various audio formats such as WAV/MP3/FLAC/OGG.
- Stream audio from disk or from memory.
- Callbacks for effects processing and generating audio.
- Spatial properties like doppler effect, pitching, distance attenuation and panning.
- Utilities for audio generation.
- Exposes many native miniaudio API calls.

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
dotnet add package JAJ.Packages.MiniAudioEx --version 2.5.1
```

# Changes in 2.5.1
- Made AdvancedAPI types more reflective of their native counterparts.
- Removed redundant prefixes in native enum types.

# Changes in 2.5.0
- Added low level API for directly using miniaudio functionality.
- Added some managed types that use the low level API (MaEngine/MaContext/MaSound etc).
- Added 2 new namespaces: `MiniAudioEx.Core.StandardAPI` and `MiniAudioEx.Core.AdvancedAPI`.
- Moved AudioContext/AudioSource/AudioClip and other related types into `MiniAudioEx.Core.StandardAPI`.
- Updated the examples.

# Changes in 2.4.0
- New AudioSource implementation that allows playing multiple sounds simultaneously. With this comes a new method called `PlayOneShot` which is suitable in scenarios where you need to rapidly play sounds (for example think of gun shots) without having to stop an already playing sound and thus cutting it off. An additional benefit is that all these sounds are processed in the same FX chain.

# Changes in 2.3.0
- Update to miniaudio 0.11.23.
- Implemented playing ogg files from memory.
- Support for older Linux versions (x86_64/ARM32/ARM64 architectures starting at Ubuntu 16.04).

# Changes in 2.2.2
- Update native libraries.

# Changes in 2.2.1
- Expose more device properties.

# Changes in 2.2.0
- Add support for ogg file format (experimental).
- Support for ogg on Linux ARM might not work when using glibc < 2.34.
- Restructured `AudioDecoder` and added `DecodeFromMemory` method.

# Changes in 2.1.0
- Renamed MiniAudioEx.csproj to MiniAudioExNET.csproj.

# Changes in 2.0.1
- Attempt to fix bug where default device isn't selected.

# Changes in 2.0.0
- Upgrade to miniaudio 0.11.22.

# Changes in 1.8.0
- Fix for no symbols on Windows x86 and x86_64.

# Changes in 1.7.9
- Add `Pointer` property to `AudioBuffer`.

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
using MiniAudioEx.Core.StandardAPI;

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
See links below for more examples. Most of these use the `AudioApp` class which is suitable for simple console based applications.

[Playing audio from a file on disk](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example1.cs)

[Using the 'Read' callback to generate sound](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example2.cs)

[Basic spatial audio](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example3.cs)

[Playing audio from a playlist](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example4.cs)

[Generating sound, applying effects and spatial audio](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example5.cs)

[FM synthesis](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example6.cs)

[Using the native miniaudio API.](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example7.cs)

[Using the advanced API.](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/Example8.cs)