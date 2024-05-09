# MiniAudioEx
A simplified .NET wrapper for MiniAudio. This uses a modified version of MiniAudio, see [this repository](https://github.com/japajoe/miniaudioex/tree/dev).

# Features
- Playback of various audio formats such as WAV/MP3/FLAC.
- Stream audio from disk or from memory.
- Callbacks for adding effects and generating audio.
- Spatial properties like doppler effect/pitch, distance attenuation and panning.

# Platforms
MiniAudio was designed to work on every major platform, however I do not have a Mac so I can not build a library for Mac OS. As a result only Windows and Linux libraries are currently available.

# Building
This is a development repository which does not include any native libraries for miniaudioex. To build for your specific platform [check here](https://github.com/japajoe/miniaudioex/tree/dev). Note that since this is a development branch, it is not compatible with the native library of the master branch.
