# MiniAudioEx
A simplified .NET wrapper for MiniAudio. This uses a modified version of MiniAudio, see [this repository](https://github.com/japajoe/miniaudioex).

# Installation
```
dotnet add package JAJ.Packages.MiniAudioEx --version 1.0.0
```

# Features
- Playback of various audio formats such as WAV/MP3/FLAC.
- Stream audio from disk or from memory.
- Callbacks for adding effects and generating audio.
- Spatial properties like doppler effect/pitch, distance attenuation and panning.

# Platforms
MiniAudio was designed to work on every major platform, however I do not have a Mac so I can not build a library for Mac OS. As a result only Windows and Linux libraries are currently available.