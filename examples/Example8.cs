//An example of using the advanced API.

using System;
using MiniAudioEx.Native;
using MiniAudioEx.Core.AdvancedAPI;

namespace MiniAudioExExamples
{
	class Example8
	{
		static void Main(string[] args)
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
			unsafe
			{
				device.Handle.Get()->pUserData = engine.Handle.pointer;
			}

			device.Start();

			sound.Play();

			Console.WriteLine("Press enter to exit");
			Console.ReadLine();

			sound.Stop();
			device.Stop();

			// Dispose in the reverse order of initialization
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