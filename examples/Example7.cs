//An example of using the native miniaudio API.
//You need to compile this with AllowUnsafeBlocks

using System;
using MiniAudioEx.Native;

namespace MiniAudioExExamples
{
    class Example7
    {
        private static ma_engine_ptr pEngine;
        private static ma_context_ptr pContext;
        private static ma_device_ptr pDevice;
        private static ma_resource_manager_ptr pResourceManager;
        private static ma_device_data_proc deviceDataProc;
        private static ma_sound_ptr pSound;

        public static void Main(string[] args)
        {
            pEngine = new ma_engine_ptr(true);
            pContext = new ma_context_ptr(true);
            pDevice = new ma_device_ptr(true);
            pResourceManager = new ma_resource_manager_ptr(true);
            pSound = new ma_sound_ptr(true);
            deviceDataProc = OnDeviceData;

            if (MiniAudioNative.ma_context_init(null, pContext) != ma_result.success)
            {
                Console.WriteLine("Failed to create context");
                Dispose();
                return;
            }

            ma_device_config deviceConfig = MiniAudioNative.ma_device_config_init(ma_device_type.playback);
            deviceConfig.playback.format = ma_format.f32;
            deviceConfig.playback.channels = 2;
            deviceConfig.sampleRate = 44100;
            deviceConfig.periodSizeInFrames = 2048;
            deviceConfig.SetDataCallback(deviceDataProc);

            if (MiniAudioNative.ma_context_get_devices(pContext, out ma_device_info_ex[] ppPlaybackDeviceInfos, out ma_device_info_ex[] ppCaptureDeviceInfos) != ma_result.success)
            {
                Console.WriteLine("Failed to get devices");
                Dispose();
                return;
            }

            if (ppPlaybackDeviceInfos?.Length > 0)
            {
                for (int i = 0; i < ppPlaybackDeviceInfos.Length; i++)
                {
                    if (ppPlaybackDeviceInfos[i].deviceInfo.isDefault > 0)
                    {
                        deviceConfig.playback.pDeviceID = ppPlaybackDeviceInfos[i].pDeviceId;
                        Console.WriteLine("Selected default device: " + ppPlaybackDeviceInfos[i].deviceInfo.GetName());
                        break;
                    }
                }
            }

            if (MiniAudioNative.ma_device_init(pContext, ref deviceConfig, pDevice) != ma_result.success)
            {
                Console.WriteLine("Failed to initialize device");
                Dispose();
                return;
            }

            ma_decoding_backend_vtable_ptr[] vtables = {
                MiniAudioNative.ma_libvorbis_get_decoding_backend_ptr()
            };

            ma_resource_manager_config resourceManagerConfig = MiniAudioNative.ma_resource_manager_config_init();
            resourceManagerConfig.SetCustomDecodingBackendVTables(vtables);

            if (MiniAudioNative.ma_resource_manager_init(ref resourceManagerConfig, pResourceManager) != ma_result.success)
            {
                resourceManagerConfig.FreeCustomDecodingBackendVTables();
                Console.WriteLine("Failed to initialize ma_resource_manager");
                Dispose();
                return;
            }

            resourceManagerConfig.FreeCustomDecodingBackendVTables();

            ma_engine_config engineConfig = MiniAudioNative.ma_engine_config_init();
            engineConfig.listenerCount = MiniAudioNative.MA_ENGINE_MAX_LISTENERS;
            engineConfig.pDevice = pDevice;
            engineConfig.pResourceManager = pResourceManager;

            if (MiniAudioNative.ma_engine_init(ref engineConfig, pEngine) != ma_result.success)
            {
                Console.WriteLine("Failed to initialize ma_engine");
                Dispose();
                return;
            }

            unsafe
            {
                ma_device* device = (ma_device*)pDevice.pointer;
                device->pUserData = pEngine.pointer;
            }

            if (MiniAudioNative.ma_device_start(pDevice) != ma_result.success)
            {
                Console.WriteLine("Failed to start ma_device");
                Dispose();
                return;
            }

            if (MiniAudioNative.ma_sound_init_from_file(pEngine, "some_file.ogg", ma_sound_flags.stream, default, default, pSound) != ma_result.success)
            {
                Console.WriteLine("Failed to initialize sound");
                Dispose();
                return;
            }

            MiniAudioNative.ma_sound_set_looping(pSound, 1);
            MiniAudioNative.ma_sound_start(pSound);

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();

            MiniAudioNative.ma_sound_stop(pSound);

            Dispose();
        }

        private static void Dispose()
        {
            MiniAudioNative.ma_sound_uninit(pSound);
            MiniAudioNative.ma_engine_uninit(pEngine);
            MiniAudioNative.ma_device_uninit(pDevice);
            MiniAudioNative.ma_context_uninit(pContext);
            MiniAudioNative.ma_resource_manager_uninit(pResourceManager);

            pEngine.Free();
            pContext.Free();
            pDevice.Free();
            pResourceManager.Free();
            pSound.Free();
        }

        private static unsafe void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
        {
            ma_device* device = pDevice.Get();

            if (device == null)
                return;

            ma_engine_ptr pEngine = new ma_engine_ptr(device->pUserData);

            MiniAudioNative.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
        }
    }
}