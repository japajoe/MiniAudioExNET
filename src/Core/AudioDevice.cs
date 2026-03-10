using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core
{
    public sealed class AudioDevice
    {
        public ma_device_info info;

        public static AudioDevice[] GetDevices()
        {
            ma_context_ptr context = new ma_context_ptr(true);

            if (MiniAudio.ma_context_init(null, context) != ma_result.success)
            {
                context.Free();
                throw new Exception("Can not obtain devices, failed to create audio context");
            }

            if (MiniAudio.ma_context_get_devices(context, out ma_device_info[] ppPlaybackDeviceInfos, out ma_device_info[] ppCaptureDeviceInfos) != ma_result.success)
            {
                context.Free();
                throw new Exception("Failed to get devices");
            }

            context.Free();

            AudioDevice[] devices = new AudioDevice[ppPlaybackDeviceInfos.Length];

            for(int i = 0; i < ppPlaybackDeviceInfos.Length; i++)
            {
                devices[i] = new AudioDevice();
                devices[i].info = ppPlaybackDeviceInfos[i];
            }

            return devices;
        }
    }
}