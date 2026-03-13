using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.StandardAPI
{
    public enum AudioDeviceType
    {
        Capture,
        Playback    
    }

    public sealed class AudioDevice
    {
        public ma_device_info info;
        public bool IsDefault => info.isDefault > 0;
        public string Name => info.GetName();

        public static AudioDevice[] GetDevices(AudioDeviceType deviceType)
        {
            ma_context_ptr context = new ma_context_ptr(true);

            if (MiniAudioNative.ma_context_init(null, context) != ma_result.success)
            {
                context.Free();
                throw new Exception("Can not obtain devices, failed to create audio context");
            }

            if (MiniAudioNative.ma_context_get_devices(context, out ma_device_info[] playbackDevices, out ma_device_info[] captureDevices) != ma_result.success)
            {
                MiniAudioNative.ma_context_uninit(context);
                context.Free();
                throw new Exception("Failed to get devices");
            }

            MiniAudioNative.ma_context_uninit(context);
            context.Free();

            AudioDevice[] devices = null;

            if(deviceType == AudioDeviceType.Capture)
            {
                devices = new AudioDevice[captureDevices.Length];
                for(int i = 0; i < captureDevices.Length; i++)
                {
                    devices[i] = new AudioDevice();
                    devices[i].info = captureDevices[i];
                }                
            }
            else
            {
                devices = new AudioDevice[playbackDevices.Length];
                for(int i = 0; i < playbackDevices.Length; i++)
                {
                    devices[i] = new AudioDevice();
                    devices[i].info = playbackDevices[i];
                }
            }

            return devices;
        }
    }
}