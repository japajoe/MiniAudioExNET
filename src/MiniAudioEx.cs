using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MiniAudioExNET
{
    public static class MiniAudioEx
    {
        private static IntPtr audioContext;
        private static List<AudioSource> audioSources = new List<AudioSource>();
        private static List<AudioClip> audioClips = new List<AudioClip>();
        private static List<AudioListener> audioListeners = new List<AudioListener>();
        private static UInt32 sampleRate;

        internal static IntPtr AudioContext
        {
            get
            {
                return audioContext;
            }
        }

        public static UInt32 SampleRate
        {
            get
            {
                return sampleRate;
            }
        }

        public static float MasterVolume
        {
            get
            {
                return Library.ma_ex_context_get_master_volume(audioContext);
            }
            set
            {
                Library.ma_ex_context_set_master_volume(audioContext, value);
            }
        }

        public static void Initialize(UInt32 sampleRate, UInt32 channels, DeviceInfo deviceInfo = null)
        {
            if(audioContext != IntPtr.Zero)
                return;

            ma_ex_device_info pDeviceInfo = new ma_ex_device_info();
            pDeviceInfo.index = deviceInfo == null ? 0 : deviceInfo.Index;
            pDeviceInfo.pName = IntPtr.Zero;

            MiniAudioEx.sampleRate = sampleRate;

            ma_ex_context_config contextConfig = Library.ma_ex_context_config_init(sampleRate, (byte)channels, ref pDeviceInfo);

            audioContext = Library.ma_ex_context_init(ref contextConfig);
        }

        public static void Deinitialize()
        {
            if(audioContext == IntPtr.Zero)
                return;

            for(int i = 0; i < audioSources.Count; i++)
                audioSources[i].Destroy();

            audioSources.Clear();

            for(int i = 0; i < audioClips.Count; i++)
                audioClips[i].Destroy();
            
            audioClips.Clear();

            for(int i = 0; i < audioListeners.Count; i++)
                audioListeners[i].Destroy();

            audioListeners.Clear();

            Library.ma_ex_context_uninit(audioContext);
            audioContext = IntPtr.Zero;
        }

        public static void Update()
        {
            if(audioContext == IntPtr.Zero)
                return;

            for(int i = 0; i < audioSources.Count; i++)
            {
                audioSources[i].Update();
            }
        }

        public static DeviceInfo[] GetDevices()
        {
            IntPtr pDevices = Library.ma_ex_playback_devices_get(out UInt32 count);

            if(pDevices == IntPtr.Zero)
                return null;

            if(count == 0)
            {
                Library.ma_ex_playback_devices_free(pDevices, count);
                return null;
            }
            
            DeviceInfo[] devices = new DeviceInfo[count];

            for (UInt32 i = 0; i < count; i++)
            {
                IntPtr elementPtr = IntPtr.Add(pDevices, (int)i * Marshal.SizeOf<ma_ex_device_info>());
                ma_ex_device_info deviceInfo = Marshal.PtrToStructure<ma_ex_device_info>(elementPtr);
                devices[i] = new DeviceInfo(deviceInfo.pName, deviceInfo.index);
            }

            Library.ma_ex_playback_devices_free(pDevices, count);
            
            return devices;
        }

        internal static void Add(AudioSource source)
        {
            int hashcode = source.GetHashCode();

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].GetHashCode() == hashcode)
                    return;
            }

            audioSources.Add(source);
        }

        internal static void Add(AudioClip clip)
        {
            int hashcode = clip.GetHashCode();

            for(int i = 0; i < audioClips.Count; i++)
            {
                if(audioClips[i].GetHashCode() == hashcode)
                    return;
            }

            audioClips.Add(clip);
        }

        internal static void Add(AudioListener listener)
        {
            int hashcode = audioListeners.GetHashCode();

            for(int i = 0; i < audioListeners.Count; i++)
            {
                if(audioListeners[i].GetHashCode() == hashcode)
                    return;
            }

            audioListeners.Add(listener);
        }        

        internal static void Remove(AudioSource source)
        {
            int hashcode = source.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioSources[index].Destroy();
                audioSources.RemoveAt(index);
            }
        }

        internal static void Remove(AudioClip clip)
        {
            int hashcode = clip.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioClips.Count; i++)
            {
                if(audioClips[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioClips[index].Destroy();
                audioClips.RemoveAt(index);
            }
        }

        internal static void Remove(AudioListener listener)
        {
            int hashcode = listener.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioListeners.Count; i++)
            {
                if(audioListeners[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioListeners[index].Destroy();
                audioListeners.RemoveAt(index);
            }
        }
    }
}