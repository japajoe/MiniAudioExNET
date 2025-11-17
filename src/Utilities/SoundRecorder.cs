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
// Copyright 2025 W.M.R Jap-A-Joe

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
using System.Diagnostics;
using MiniAudioEx.Core.AdvancedAPI;
using MiniAudioEx.DSP.Effects;
using MiniAudioEx.Native;

namespace MiniAudioEx.Utilities
{
	public unsafe sealed class SoundRecorder : IDisposable
	{
		private ma_context_ptr context;
		private ma_device_ptr captureDevice;
		private ma_device_data_proc callback;
		private AudioRecorder recorder;

		public SoundRecorder()
		{
			context = new ma_context_ptr(true);
			captureDevice = new ma_device_ptr(true);
			callback = OnDeviceData;

			recorder = new AudioRecorder();

			var contextConfig = MiniAudioNative.ma_context_config_init();
			UInt32 deviceCount = 0;
			ma_backend nullBackend = ma_backend.nill;

			ma_backend*[] backendLists = {
				null,
				&nullBackend
			};

			foreach (var backendList in backendLists)
			{
				// We can set backendCount to 1 since it is ignored when backends is set to nullptr
				if (MiniAudioNative.ma_context_init(backendList, 1, &contextConfig, context) != ma_result.success)
				{
					Console.WriteLine("Failed to initialize the audio capture context");
					context.Free();
					return;
				}

				// Count the capture devices
				if (MiniAudioNative.ma_context_get_devices(context, null, null, null, &deviceCount) != ma_result.success)
				{
					Console.WriteLine("Failed to get audio capture devices");
					context.Free();
					return;
				}

				// Check if there are audio capture devices available on the system
				if (deviceCount > 0)
					break;

				// Warn if no devices were found using the default backend list
				if (backendList == null)
					Console.WriteLine("No audio capture devices available on the system");

				// Clean up the context if we didn't find any devices
				MiniAudioNative.ma_context_uninit(context);
			}

			// If the NULL audio backend also doesn't provide a device we give up
			if (deviceCount == 0)
			{
				context.Free();
				return;
			}

			if (context.Get()->backend == ma_backend.nill)
				Console.WriteLine("Using NULL audio backend for capture");
		}

		public bool Initialize()
		{
			if (context.pointer == IntPtr.Zero)
				return false;

			var device = GetAvailableDevice();

			Console.WriteLine(device.GetName());

			var captureDeviceConfig = MiniAudioNative.ma_device_config_init(ma_device_type.capture);
			captureDeviceConfig.capture.pDeviceID = device.pDeviceId;
			captureDeviceConfig.capture.channels = 2;
			captureDeviceConfig.capture.format = ma_format.f32;
			captureDeviceConfig.sampleRate = 44100;
			//captureDeviceConfig.pUserData = context.pointer;
			captureDeviceConfig.pUserData = IntPtr.Zero;
			captureDeviceConfig.SetDataCallback(callback);

			if (MiniAudioNative.ma_device_init(context, ref captureDeviceConfig, captureDevice) != ma_result.success)
			{
				Console.WriteLine("Failed to initialize the audio capture device");
				return false;
			}


			return false;
		}

		public void Dispose()
		{
			Stop();
			MiniAudioNative.ma_device_uninit(captureDevice);
			MiniAudioNative.ma_context_uninit(context);
			captureDevice.Free();
			context.Free();
		}

		public void Stop()
		{
			recorder.Stop();
			MiniAudioNative.ma_device_stop(captureDevice);
		}

		public void Start()
		{
			if (MiniAudioNative.ma_device_is_started(captureDevice) > 0)
				return;

			recorder.Start();

			MiniAudioNative.ma_device_start(captureDevice);
		}

		private MaDeviceInfo GetAvailableDevice()
		{
			MaContext context = new MaContext();
			context.Initialize();
			var device = context.GetDefaultCaptureDevice();
			context.Dispose();
			return device;

		}

		private ma_device_info[] GetAvailableDevices()
		{
			ma_device_info[] deviceList = null;

			// Create the context
			ma_context_ptr pContext = new ma_context_ptr(true);

			var contextConfig = MiniAudioNative.ma_context_config_init();

			if (MiniAudioNative.ma_context_init(null, 0, &contextConfig, pContext) != ma_result.success)
			{
				pContext.Free();
				Console.WriteLine("Failed to initialize the audio context");
				return deviceList;
			}

			// Enumerate the capture devices
			ma_device_info* deviceInfos = null;
			UInt32 deviceCount = 0;

			if (MiniAudioNative.ma_context_get_devices(pContext, null, null, &deviceInfos, &deviceCount) != ma_result.success)
			{
				Console.WriteLine("Failed to get audio capture devices");
				MiniAudioNative.ma_context_uninit(pContext);
				pContext.Free();
				return deviceList;
			}

			if(deviceCount > 0)
				deviceList = new ma_device_info[deviceCount];

			for (int i = 0; i < deviceCount; i++)
			{
				deviceList[i] = deviceInfos[i];
			}
			
			MiniAudioNative.ma_context_uninit(pContext);
			pContext.Free();
			return deviceList;
		}

		private void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
		{
			if (pDevice.pointer != captureDevice.pointer)
				return;

			const int channels = 2;
			int length = (int)(frameCount * channels);
			//NativeArray<short> buffer = new NativeArray<short>(pInput, length);
			NativeArray<float> buffer = new NativeArray<float>(pInput, length);

			uint frameCountOut = 0;
			recorder.OnProcess(buffer, frameCount, buffer, ref frameCountOut, 2);
		}
	}
}