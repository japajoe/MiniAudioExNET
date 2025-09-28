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
dotnet add package JAJ.Packages.MiniAudioEx --version 2.6.3
```

# Changes in 2.6.3
- Added `DelayEffect`.
- Added `PhaserEffect`.
- Added `AudioSource.GetOutputBuffer` and `AudioContext.GetOutputBuffer` methods.
- Moved generators and effects into separate namespaces.

# Changes in 2.6.2
- Fixed bug with stopping/continuing an audio source.
- Added `DistortionEffect`

# Changes in 2.6.1
- Manual polling to see if playback of an `AudioSource` ended.

# Changes in 2.6.0
- Fixed serious bug that prevented to play new sounds with an `AudioSource`.
- Added `MaDataSource`.
- Added `MaProceduralDataSource`.
- Added `MaEffectNode`.
- Added `Reverb` and `ReverbEffect`.
- Renamed `MaSound.Play` to `MaSound.Start`.
- Renamed `AudioBuffer` to `NativeArray`.
- Changed signature of `AudioProcessEvent`.
- Changed signature of the `Process` method in `IAudioEffect`.
- Examples updated.

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

# Example of using the Standard API
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

# Example of using the Advanced API
Note that you must compile with AllowUnsafeBlocks.
```cs
using System;
using MiniAudioEx.Native;
using MiniAudioEx.Core.AdvancedAPI;

namespace MiniAudioExExample
{
	class Program
	{
		static unsafe void Main(string[] args)
		{
			MaLog log = new MaLog();
			log.Message += OnLog;

			if (log.Initialize() != ma_result.success)
			{
				Console.WriteLine("Failed to initialize log");
				log.Dispose();
				return;
			}

			MaContext context = new MaContext();
			ma_context_config contextConfig = context.GetConfig();
			contextConfig.pLog = log.Handle;

			if (context.Initialize(null, contextConfig) != ma_result.success)
			{
				Console.WriteLine("Failed to initialize context");
				context.Dispose();
				log.Dispose();
				return;
			}

			ma_decoding_backend_vtable_ptr[] vtables = {
				MiniAudioNative.ma_libvorbis_get_decoding_backend_ptr()
			};

			MaResourceManager resourceManager = new MaResourceManager();
			ma_resource_manager_config resourceManagerConfig = resourceManager.GetConfig();
			resourceManagerConfig.SetCustomDecodingBackendVTables(vtables);

			if (resourceManager.Initialize(resourceManagerConfig) != ma_result.success)
			{
				Console.WriteLine("Failed to initialize resource manager");
				resourceManager.Dispose();
				context.Dispose();
				log.Dispose();
				resourceManagerConfig.FreeCustomDecodingBackendVTables();
				return;
			}

			resourceManagerConfig.FreeCustomDecodingBackendVTables();

			MaDeviceInfo deviceInfo = context.GetDefaultPlaybackDevice();
			ma_device_data_proc deviceDataCallback = OnDeviceData;
			MaDevice device = new MaDevice();

			ma_device_config deviceConfig = device.GetConfig(ma_device_type.playback);
			deviceConfig.sampleRate = 44100;
			deviceConfig.periodSizeInFrames = 2048;
			deviceConfig.playback.format = ma_format.f32;
			deviceConfig.playback.channels = 2;
			deviceConfig.playback.pDeviceID = deviceInfo.pDeviceId;
			deviceConfig.SetDataCallback(deviceDataCallback);

			if (device.Initialize(deviceConfig) != ma_result.success)
			{
				Console.WriteLine("Failed to initialize device");
				device.Dispose();
				resourceManager.Dispose();
				context.Dispose();
				log.Dispose();
				return;
			}

			MaEngine engine = new MaEngine();
			ma_engine_config engineConfig = engine.GetConfig();
			engineConfig.pDevice = device.Handle;
			engineConfig.pResourceManager = resourceManager.Handle;

			if (engine.Initialize(engineConfig) != ma_result.success)
			{
				Console.WriteLine("Failed to initialize engine");
				engine.Dispose();
				device.Dispose();
				resourceManager.Dispose();
				context.Dispose();
				log.Dispose();
				return;
			}

			MaSound sound = new MaSound();

			if (sound.InitializeFromFile(engine, "some_file.ogg", ma_sound_flags.stream | ma_sound_flags.decode) != ma_result.success)
			{
				Console.WriteLine("Failed to initialize sound");
				sound.Dispose();
				engine.Dispose();
				device.Dispose();
				resourceManager.Dispose();
				context.Dispose();
				log.Dispose();
				return;
			}

			sound.SetLooping(true);

			// Set the user data before starting the device, so the device callback can use it.
            device.Handle.Get()->pUserData = engine.Handle.pointer;

			device.Start();

			sound.Start();

			Console.WriteLine("Press enter to exit");
			Console.ReadLine();

			sound.Stop();
			device.Stop();

			// Dispose in the reverse order of initialization.
			sound.Dispose();
			engine.Dispose();
			resourceManager.Dispose();
			device.Dispose();
			context.Dispose();
			log.Dispose();
		}

		private static unsafe void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
		{
			ma_device* device = pDevice.Get();

			if (device == null)
				return;

			ma_engine_ptr pEngine = new ma_engine_ptr(device->pUserData);

			MiniAudioNative.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
		}

		private static void OnLog(UInt32 level, string message)
		{
			Console.Write("Log [{0}] {1}", level, message);
		}
	}
}
```

See links below for more examples. Most of these use the `AudioApp` class which is suitable for simple console based applications.

[Playing audio from a file on disk](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/PlayingFromFileExample.cs)

[Using the 'Read' callback to generate sound](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/ProceduralSoundExample.cs)

[Basic spatial audio](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/SpatialAudioExample.cs)

[Playing audio from a playlist](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/PlayListExample.cs)

[Generating sound, applying effects and spatial audio](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/AdvancedProceduralSoundExample.cs)

[FM synthesis](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/FMSynthesisExample.cs)

[Using the native miniaudio API.](https://github.com/japajoe/MiniAudioExNET/tree/master/examples/NativeAPIExample.cs)