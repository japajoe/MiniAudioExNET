// This software is available as a choice of the following licenses. Choose
// whichever you prefer.

// ===============================================================================
// ALTERNATIVE 1 - Public Domain (www.unlicense.org)
// ===============================================================================
// This is free and unencumbered software released into the public domain.

// Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
// software, either in source code form or as a compiled binary, for any purpose,
// commercial or non-commercial, and by any means.

// In jurisdictions that recognize copyright laws, the author or authors of this
// software dedicate any and all copyright interest in the software to the public
// domain. We make this dedication for the benefit of the public at large and to
// the detriment of our heirs and successors. We intend this dedication to be an
// overt act of relinquishment in perpetuity of all present and future rights to
// this software under copyright law.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// For more information, please refer to <http://unlicense.org/>

// ===============================================================================
// ALTERNATIVE 2 - MIT No Attribution
// ===============================================================================
// Copyright 2024 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MiniAudioExNET
{
    /// <summary>
    /// This class is responsible for managing the audio context.
    /// </summary>
    public static class MiniAudioEx
    {
        private static IntPtr audioContext;
        private static List<AudioSource> audioSources = new List<AudioSource>();
        private static List<AudioClip> audioClips = new List<AudioClip>();
        private static List<AudioListener> audioListeners = new List<AudioListener>();
        private static UInt32 sampleRate;
        private static DateTime lastUpdateTime;
        private static float deltaTime;

        internal static IntPtr AudioContext
        {
            get
            {
                return audioContext;
            }
        }

        /// <summary>
        /// Gets the chosen sample rate.
        /// </summary>
        /// <value></value>
        public static Int32 SampleRate
        {
            get
            {
                return (int)sampleRate;
            }
        }

        /// <summary>
        /// Controls the master volume.
        /// </summary>
        /// <value></value>
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

        /// <summary>
        /// The elapsed time since last call to 'Update'.
        /// </summary>
        /// <value></value>
        public static float DeltaTime
        {
            get
            {
                return deltaTime;
            }
        }

        /// <summary>
        /// Initializes MiniAudioEx. Call this once at the start of your application. The 'deviceInfo' parameter can be left null.
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="channels"></param>
        /// <param name="deviceInfo"></param>
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

            lastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// Deinitializes MiniAudioEx. Call this before closing the application.
        /// </summary>
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

        /// <summary>
        /// Used to calculate delta time and move messages from the audio thread to the main thread. Call this method from within your main thread loop.
        /// </summary>
        public static void Update()
        {
            if(audioContext == IntPtr.Zero)
                return;

            DateTime currentTime = DateTime.Now;
            TimeSpan dt = currentTime - lastUpdateTime;
            deltaTime = (float)dt.TotalSeconds;

            for(int i = 0; i < audioSources.Count; i++)
            {
                audioSources[i].Update();
            }

            lastUpdateTime = currentTime;
        }

        /// <summary>
        /// Gets an array of available playback devices.
        /// </summary>
        /// <returns></returns>
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