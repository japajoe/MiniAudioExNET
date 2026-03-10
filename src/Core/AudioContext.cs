using System;
using System.Collections.Generic;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core
{   
    public delegate void LogEventHandler(UInt32 level, string message);

    public sealed class AudioContext : IDisposable
    {
        public event LogEventHandler Log;
        
		private ma_log_ptr log;
        private ma_context_ptr context;
        private ma_resource_manager_ptr resourceManager;
        private ma_device_ptr device;
        private ma_engine_ptr engine;
		private ma_log_callback_proc onLog;
        private ma_device_data_proc deviceDataProc;
        private UInt32 channels;
        private UInt32 sampleRate;
        private UInt32 periodSizeInFrames;
        private AudioDevice audioDevice;
        private List<AudioSource> sources;
        private static AudioContext current;

        public ma_context_ptr Context => context;
        public ma_engine_ptr Engine => engine;
        public UInt32 Channels => channels;
        public UInt32 SampleRate => sampleRate;

        public AudioContext(UInt32 sampleRate = 44100, UInt32 channels = 2, UInt32 periodSizeInFrames = 2048, AudioDevice audioDevice = null)
        {
            this.sampleRate = sampleRate;
            this.channels = channels;
            this.periodSizeInFrames = periodSizeInFrames;
            this.audioDevice = audioDevice;
            sources = new List<AudioSource>();

            log = new ma_log_ptr(true);
            engine = new ma_engine_ptr(true);
            context = new ma_context_ptr(true);
            device = new ma_device_ptr(true);
            resourceManager = new ma_resource_manager_ptr(true);
            deviceDataProc = OnDeviceData;
        }

        public void Create()
        {
            if (MiniAudio.ma_log_init(log) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize log");
            }

			ma_log_callback callback = new ma_log_callback();
            onLog = OnLog;
			callback.SetLogCallback(onLog);
			
            if (MiniAudio.ma_log_register_callback(log, callback) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize log callback");
            }

            ma_context_config contextConfig = MiniAudio.ma_context_config_init();
            contextConfig.pLog = log;

            if (MiniAudio.ma_context_init(null, ref contextConfig, context) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to create context");
            }

            ma_device_config deviceConfig = MiniAudio.ma_device_config_init(ma_device_type.playback);
            deviceConfig.playback.format = ma_format.f32;
            deviceConfig.playback.channels = channels;
            deviceConfig.sampleRate = sampleRate;
            deviceConfig.periodSizeInFrames = periodSizeInFrames;
            deviceConfig.SetDataCallback(deviceDataProc);

            if(audioDevice == null)
            {
                if (MiniAudio.ma_context_get_devices(context, out ma_device_info_ex[] ppPlaybackDeviceInfos, out ma_device_info_ex[] ppCaptureDeviceInfos) != ma_result.success)
                {
                    Dispose();
                    throw new Exception("Failed to get devices");
                }

                if (ppPlaybackDeviceInfos?.Length > 0)
                {
                    for (int i = 0; i < ppPlaybackDeviceInfos.Length; i++)
                    {
                        if (ppPlaybackDeviceInfos[i].deviceInfo.isDefault > 0)
                        {
                            deviceConfig.playback.pDeviceID = ppPlaybackDeviceInfos[i].pDeviceId;
                            break;
                        }
                    }
                }
            }
            else
            {
                deviceConfig.playback.pDeviceID = audioDevice.info.pDeviceId;
            }

            if (MiniAudio.ma_device_init(context, ref deviceConfig, device) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize device");
            }

            ma_decoding_backend_vtable_ptr[] vtables = {
                MiniAudio.ma_libvorbis_get_decoding_backend_ptr()
            };

            ma_resource_manager_config resourceManagerConfig = MiniAudio.ma_resource_manager_config_init();
            resourceManagerConfig.SetCustomDecodingBackendVTables(vtables);

            if (MiniAudio.ma_resource_manager_init(ref resourceManagerConfig, resourceManager) != ma_result.success)
            {
                resourceManagerConfig.FreeCustomDecodingBackendVTables();
                Dispose();
                throw new Exception("Failed to initialize ma_resource_manager");
            }

            resourceManagerConfig.FreeCustomDecodingBackendVTables();

            ma_engine_config engineConfig = MiniAudio.ma_engine_config_init();
            engineConfig.listenerCount = MiniAudio.MA_ENGINE_MAX_LISTENERS;
            engineConfig.pDevice = device;
            engineConfig.pResourceManager = resourceManager;

            if (MiniAudio.ma_engine_init(ref engineConfig, engine) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize ma_engine");
            }

            unsafe
            {
                device.Get()->pUserData = engine.pointer;
            }

            if (MiniAudio.ma_device_start(device) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to start ma_device");
            }
        }

        internal void Add(AudioSource source)
        {
            for(int i = 0; i < sources.Count; i++)
            {
                if(sources[i].GetHashCode() == source.GetHashCode())
                    return;
            }

            sources.Add(source);
        }

        internal void Remove(AudioSource source)
        {
            int index = -1;

            for(int i = 0; i < sources.Count; i++)
            {
                if(sources[i].GetHashCode() == source.GetHashCode())
                {
                    index = i;
                    break;
                }
            }

            if(index >= 0)
                sources.RemoveAt(index);
        }

        public void Dispose()
        {
			MiniAudio.ma_engine_uninit(engine);
			MiniAudio.ma_device_uninit(device);
			MiniAudio.ma_context_uninit(context);
			MiniAudio.ma_resource_manager_uninit(resourceManager);
            MiniAudio.ma_log_uninit(log);

			engine.Free();
			context.Free();
			device.Free();
			resourceManager.Free();
            log.Free();

            sources.Clear();
        }

        public void Update()
        {
            for(int i = 0; i < sources.Count; i++)
                sources[i].Update();
        }

		private unsafe void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
		{
            ma_device* device = pDevice.Get();

            if (device == null)
                return;

            ma_engine_ptr pEngine = new ma_engine_ptr(device->pUserData);

            MiniAudio.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
		}

		private void OnLog(IntPtr pUserData, UInt32 level, IntPtr pMessage)
		{
			string message = MarshalHelper.PtrToStringUTF8(pMessage);
			Log?.Invoke(level, message);
		}

        public void MakeCurrent()
        {
            current = this;
        }

        public static AudioContext GetCurrent()
        {
            return current;
        }
    }

    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;

        public Vector3f()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}