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
// Copyright 2026 W.M.R Jap-A-Joe

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
using MiniAudioEx.Native;

namespace MiniAudioEx.Core
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

            if (MiniAudio.ma_context_init(null, context) != ma_result.success)
            {
                context.Free();
                throw new Exception("Can not obtain devices, failed to create audio context");
            }

            if (MiniAudio.ma_context_get_devices(context, out ma_device_info[] playbackDevices, out ma_device_info[] captureDevices) != ma_result.success)
            {
                MiniAudio.ma_context_uninit(context);
                context.Free();
                throw new Exception("Failed to get devices");
            }

            MiniAudio.ma_context_uninit(context);
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