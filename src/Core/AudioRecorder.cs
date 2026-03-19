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
using MiniAudioEx.Utilities;
using static MiniAudioEx.Native.MiniAudio;

namespace MiniAudioEx.Core
{
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
                
                return ma_device_is_started(device) > 0;
            }
        }

        public AudioRecorder(UInt32 sampleRate, UInt32 channels)
        {
            this.sampleRate = (UInt32)Math.Max(sampleRate, 1);
            this.channels = (UInt32)Math.Max(channels, 1);

            context = new ma_context_ptr(true);

            ma_context_config contextConfig = ma_context_config_init();
            UInt32 deviceCount = 0;
            ma_backend nullBackend = ma_backend.nill;
            ma_backend*[] backendLists = { null, &nullBackend };
            
            foreach (var backendList in backendLists)
            {
                ma_result result = ma_context_init(backendList, 1, &contextConfig, context);

                if(result != ma_result.success)
                {
                    context.Free();
                    throw new Exception("Failed to initialize the audio capture context: " + result);
                }

                result = ma_context_get_devices(context, null, null, null, &deviceCount);

                if(result != ma_result.success)
                {
                    ma_context_uninit(context);
                    context.Free();
                    throw new Exception("Failed to get audio capture devices: " + result);
                }

                if(deviceCount > 0)
                    break;

                if (backendList == null)
                    Console.WriteLine("No audio capture devices available on the system");

                ma_context_uninit(context);
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
                if(ma_device_is_started(device) > 0)
                {
                    Stop();
                    OnDestroy();
                }
            }

            ma_device_stop(device);
            ma_device_uninit(device);
            ma_context_uninit(context);
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

            ma_device_config captureDeviceConfig = ma_device_config_init(ma_device_type.capture);
            
            fixed(ma_device_id *deviceId = &captureDevice.id)
            {
                captureDeviceConfig.capture.pDeviceID = new ma_device_id_ptr(new IntPtr(deviceId));
                captureDeviceConfig.capture.channels = channels;
                captureDeviceConfig.capture.format = ma_format.f32;
                captureDeviceConfig.sampleRate = sampleRate;
                captureDeviceConfig.SetDataCallback(dataProc);

                ma_result result = ma_device_init(context, ref captureDeviceConfig, device);

                if(result != ma_result.success)
                {
                    Console.WriteLine("Failed to initialize device: " + result);
                    ma_context_uninit(context);
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
            if(OnStart())
                return ma_device_start(device) == ma_result.success;
            return false;
        }

        public void Stop()
        {
            OnStop();
            ma_device_stop(device);
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