# MiniAudioEx
A .NET wrapper for MiniAudioEx. MiniAudioEx is a a modified version of MiniAudio, see [this repository](https://github.com/japajoe/miniaudioex). The goal of MiniAudioEx is to make it easy to add audio playback to .NET applications. I've tried several libraries in the past and none of them could offer all of the things I was looking for:

- Easy to set up and interact with
- Spatial audio
- Cross platform
- A permissive license

This library ticks all these boxes. There are some (in my opinion) minor things missing such as more audio format decoders, but at least 3 widely used formats are supported which is sufficient for my needs. If you would like to have support for more formats, then please make your request [here](https://github.com/mackron/miniaudio).

# Installation
```
dotnet add package JAJ.Packages.MiniAudioEx --version 1.1.0
```

# Features
- Playback of various audio formats such as WAV/MP3/FLAC.
- Stream audio from disk or from memory.
- Callbacks for effects processing and generating audio.
- Spatial properties like doppler effect, pitching, distance attenuation and panning.

# General pointers
- Don't manually dispose an AudioClip/AudioSource/AudioListener. These get cleaned up after calling MiniAudioEx.Deinitialize.
- Reuse audio clips. If you have loaded an AudioClip from memory, then the library allocates memory that the garbage collector doesn't free. All memory is freed after calling MiniAudioEx.Deinitialize. It is perfectly fine to reuse audio clips across multiple audio sources, so you don't have to load multiple clips with the same sound. A good strategy is to store your audio clips in an array or a list for the lifetime of your application.

# Platforms
MiniAudio was designed to work on every major platform, however I do not have a Mac so I can not build a library for Mac OS. As a result only Windows and Linux libraries are currently available.

