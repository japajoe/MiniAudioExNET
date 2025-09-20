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
	public sealed class MaDevice : IDisposable
	{
		private ma_device_ptr handle;

		public ma_device_ptr Handle
		{
			get => handle;
		}

		public MaDevice()
		{
			handle = new ma_device_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_device_uninit(handle);
				handle.Free();
			}
		}

		public ma_device_config GetConfig(ma_device_type deviceType)
		{
			return MiniAudioNative.ma_device_config_init(deviceType);
		}

		public ma_result Initialize(ma_device_config config)
		{
			return Initialize(null, config);
		}

		public ma_result Initialize(MaContext context, ma_device_config config)
		{
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			return MiniAudioNative.ma_device_init(context != null ? context.Handle : default, ref config, handle);
		}

		public ma_context_ptr GetContext()
		{
			return MiniAudioNative.ma_device_get_context(handle);
		}

		public ma_result Start()
		{
			return MiniAudioNative.ma_device_start(handle);
		}

		public ma_result Stop()
		{
			return MiniAudioNative.ma_device_stop(handle);
		}

		public bool IsStarted()
		{
			return MiniAudioNative.ma_device_is_started(handle) > 0;
		}

		public ma_device_state GetState()
		{
			return MiniAudioNative.ma_device_get_state(handle);
		}

		public ma_result GetMasterVolume(out float volume)
		{
			return MiniAudioNative.ma_device_get_master_volume(handle, out volume);
		}

		public ma_result SetMasterVolume(float volume)
		{
			return MiniAudioNative.ma_device_set_master_volume(handle, volume);
		}

		public ma_result GetMasterVolumeDB(out float gainDB)
		{
			return MiniAudioNative.ma_device_get_master_volume_db(handle, out gainDB);
		}

		public ma_result SetMasterVolumeDB(float gainDB)
		{
			return MiniAudioNative.ma_device_set_master_volume_db(handle, gainDB);
		}
	}
}