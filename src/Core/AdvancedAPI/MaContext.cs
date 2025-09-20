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
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public sealed class MaContext : IDisposable
	{
		private ma_context_ptr handle;
		private ma_enum_devices_callback_proc enumDevicesCallback;

		public ma_context_ptr Handle
		{
			get => handle;
		}

		public MaContext()
		{
			handle = new ma_context_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_context_uninit(handle);
				handle.Free();
			}
		}

		public ma_context_config GetConfig()
		{
			return MiniAudioNative.ma_context_config_init();
		}

		public ma_result Initialize()
		{
			ma_context_config config = MiniAudioNative.ma_context_config_init();
			return Initialize(null, config);
		}

		public ma_result Initialize(ma_backend[] backends, ma_context_config config)
		{
			return MiniAudioNative.ma_context_init(backends, ref config, handle);
		}

		public ma_log_ptr GetLog()
		{
			return MiniAudioNative.ma_context_get_log(handle);
		}

		public MaDeviceInfo GetDefaultPlaybackDevice()
		{
			if (GetDevices(out MaDeviceInfo[] playbackDevices, out _))
			{
				if (playbackDevices == null)
					return default;

				for (int i = 0; i < playbackDevices.Length; i++)
				{
					if (playbackDevices[i].deviceInfo.isDefault > 0)
					{
						return playbackDevices[i];
					}
				}
			}

			return default;
		}

		public MaDeviceInfo GetDefaultCaptureDevice()
		{
			if (GetDevices(out _, out MaDeviceInfo[] captureDevices))
			{
				if (captureDevices == null)
					return default;

				for (int i = 0; i < captureDevices.Length; i++)
				{
					if (captureDevices[i].deviceInfo.isDefault > 0)
					{
						return captureDevices[i];
					}
				}
			}

			return default;
		}

		public bool GetDevices(out MaDeviceInfo[] playbackDevices, out MaDeviceInfo[] captureDevices)
		{
			playbackDevices = null;
			captureDevices = null;

			ma_result result = MiniAudioNative.ma_context_get_devices(handle, out ma_device_info_ex[] ppPlaybackDeviceInfos, out ma_device_info_ex[] ppCaptureDeviceInfos);

			if (result != ma_result.success)
				return false;

			if (ppPlaybackDeviceInfos?.Length > 0)
			{
				playbackDevices = new MaDeviceInfo[ppPlaybackDeviceInfos.Length];

				for (int i = 0; i < ppPlaybackDeviceInfos.Length; i++)
				{
					playbackDevices[i] = new MaDeviceInfo();
					playbackDevices[i].deviceInfo = ppPlaybackDeviceInfos[i].deviceInfo;
					playbackDevices[i].pDeviceId = ppPlaybackDeviceInfos[i].pDeviceId;
				}
			}

			if (ppCaptureDeviceInfos?.Length > 0)
			{
				captureDevices = new MaDeviceInfo[ppCaptureDeviceInfos.Length];

				for (int i = 0; i < ppCaptureDeviceInfos.Length; i++)
				{
					captureDevices[i] = new MaDeviceInfo();
					captureDevices[i].deviceInfo = ppCaptureDeviceInfos[i].deviceInfo;
					captureDevices[i].pDeviceId = ppCaptureDeviceInfos[i].pDeviceId;
				}
			}

			return true;
		}

		public ma_result EnumerateDevices(ma_enum_devices_callback_proc callback, IntPtr pUserData)
		{
			enumDevicesCallback = callback;
			return MiniAudioNative.ma_context_enumerate_devices(handle, enumDevicesCallback, pUserData);
		}

		public ma_result GetDeviceInfo(ma_device_type deviceType, ma_device_id_ptr pDeviceID, out ma_device_info pDeviceInfo)
		{
			return MiniAudioNative.ma_context_get_device_info(handle, deviceType, pDeviceID, out pDeviceInfo);
		}

		public bool IsLoopBackSupported()
		{
			return MiniAudioNative.ma_context_is_loopback_supported(handle) > 0;
		}
	}
}