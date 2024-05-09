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

# Platforms
MiniAudio was designed to work on every major platform, however I do not have a Mac so I can not build a library for Mac OS. As a result only Windows and Linux libraries are currently available.

