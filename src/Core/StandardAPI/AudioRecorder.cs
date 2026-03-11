using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.StandardAPI
{
    /// <summary>
    /// An abstract class to capture audio from either a microphone or a playback device.
    /// </summary>
    public unsafe abstract class AudioRecorder : IDisposable
    {
        private ma_context_ptr context;
        private ma_device_ptr device;
        private ma_device_data_proc dataProc;
        private ma_device_info captureDevice;
        protected readonly UInt32 sampleRate;
        protected readonly UInt32 channels;

        /// <summary>
        /// Indicates whether recording is currently active or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                if(device.pointer == IntPtr.Zero)
                    return false;
                
                return MiniAudioNative.ma_device_is_started(device) > 0;
            }
        }

        public AudioRecorder(UInt32 sampleRate, UInt32 channels)
        {
            this.sampleRate = (UInt32)Math.Max(sampleRate, 1);
            this.channels = (UInt32)Math.Max(channels, 1);

            context = new ma_context_ptr(true);

            ma_context_config contextConfig = MiniAudioNative.ma_context_config_init();
            UInt32 deviceCount = 0;
            ma_backend nullBackend = ma_backend.nill;
            ma_backend*[] backendLists = { null, &nullBackend };
            
            foreach (var backendList in backendLists)
            {
                ma_result result = MiniAudioNative.ma_context_init(backendList, 1, &contextConfig, context);

                if(result != ma_result.success)
                {
                    context.Free();
                    throw new Exception("Failed to initialize the audio capture context: " + result);
                }

                result = MiniAudioNative.ma_context_get_devices(context, null, null, null, &deviceCount);

                if(result != ma_result.success)
                {
                    context.Free();
                    throw new Exception("Failed to get audio capture devices: " + result);
                }

                if(deviceCount > 0)
                    break;

                if (backendList == null)
                    Console.WriteLine("No audio capture devices available on the system");

                MiniAudioNative.ma_context_uninit(context);
            }

            if (deviceCount == 0)
            {
                context.Free();
                throw new Exception("No capture devices found");
            }

            if (context.Get()->backend == ma_backend.nill)
                Console.WriteLine("Using NULL audio backend for capture");
        }

        public void Dispose()
        {
            if(device.pointer != IntPtr.Zero)
            {
                if(MiniAudioNative.ma_device_is_started(device) > 0)
                {
                    Stop();
                    OnDestroy();
                }
            }

            MiniAudioNative.ma_device_stop(device);
            MiniAudioNative.ma_device_uninit(device);
            MiniAudioNative.ma_context_uninit(context);
            device.Free();
            context.Free();
        }

        protected bool Initialize()
        {
            if(context.pointer == IntPtr.Zero)
            {
                Console.WriteLine("Context is null");
                return false;
            }

            if(device.pointer != IntPtr.Zero)
            {
                Console.WriteLine("Already initialized");
                return true;
            }

            device = new ma_device_ptr(true);

            dataProc = OnDeviceData;

            ma_device_config captureDeviceConfig = MiniAudioNative.ma_device_config_init(ma_device_type.capture);
            
            fixed(ma_device_id *deviceId = &captureDevice.id)
            {
                captureDeviceConfig.capture.pDeviceID = new ma_device_id_ptr(new IntPtr(deviceId));
                captureDeviceConfig.capture.channels = channels;
                captureDeviceConfig.capture.format = ma_format.f32;
                captureDeviceConfig.sampleRate = sampleRate;
                captureDeviceConfig.SetDataCallback(dataProc);

                ma_result result = MiniAudioNative.ma_device_init(context, ref captureDeviceConfig, device);

                if(result != ma_result.success)
                {
                    Console.WriteLine("Failed to initialize device: " + result);
                    MiniAudioNative.ma_context_uninit(context);
                    context.Free();
                    device.Free();
                    return false;
                }
            }

            return true;
        }

        public AudioDevice[] GetAvailableDevices()
        {
            return AudioDevice.GetDevices(AudioDeviceType.Capture);
        }

        public void SetDevice(AudioDevice captureDevice)
        {
            this.captureDevice = captureDevice.info;
        }

        public bool Start()
        {
            if(device.pointer == IntPtr.Zero)
                return false;
            if(OnStart())
                return MiniAudioNative.ma_device_start(device) == ma_result.success;
            return false;
        }

        public void Stop()
        {
            if(device.pointer == IntPtr.Zero)
                return;
            OnStop();
            MiniAudioNative.ma_device_stop(device);
        }

        protected virtual bool OnStart()
        {
            return false;
        }

        protected virtual void OnStop()
        {
            
        }

        protected virtual void OnDestroy()
        {
            
        }

        protected virtual void OnProcess(NativeArray<float> data, UInt32 frameCount)
        {
            
        }

        private void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
        {
            Int32 length = (Int32)(frameCount * channels);
            NativeArray<float> data = new NativeArray<float>(pInput, length);
            OnProcess(data, frameCount);
        }
    }
}