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
	public sealed class MaSoundGroup : IDisposable
	{
		private ma_sound_group_ptr handle;

		public ma_sound_group_ptr Handle
		{
			get => handle;
		}

		public bool IsPlaying
		{
			get => MiniAudioNative.ma_sound_group_is_playing(handle) > 0;
		}

		public bool Spatial
		{
			get => MiniAudioNative.ma_sound_group_is_spatialization_enabled(handle) > 0;
			set => MiniAudioNative.ma_sound_group_set_spatialization_enabled(handle, value ? (uint)1 : 0);
		}

		public float Volume
		{
			get => MiniAudioNative.ma_sound_group_get_volume(handle);
			set => MiniAudioNative.ma_sound_group_set_volume(handle, value);
		}

		public float Pan
		{
			get => MiniAudioNative.ma_sound_group_get_pan(handle);
			set => MiniAudioNative.ma_sound_group_set_pan(handle, value);
		}

		public ma_pan_mode PanMode
		{
			get => MiniAudioNative.ma_sound_group_get_pan_mode(handle);
			set => MiniAudioNative.ma_sound_group_set_pan_mode(handle, value);
		}

		public float Pitch
		{
			get => MiniAudioNative.ma_sound_group_get_pitch(handle);
			set => MiniAudioNative.ma_sound_group_set_pitch(handle, value);
		}

		public float DopplerFactor
		{
			get => MiniAudioNative.ma_sound_group_get_doppler_factor(handle);
			set => MiniAudioNative.ma_sound_group_set_doppler_factor(handle, value);
		}

		public ma_vec3f Position
		{
			get => MiniAudioNative.ma_sound_group_get_position(handle);
			set => MiniAudioNative.ma_sound_group_set_position(handle, value.x, value.y, value.z);
		}

		public ma_vec3f Direction
		{
			get => MiniAudioNative.ma_sound_group_get_direction(handle);
			set => MiniAudioNative.ma_sound_group_set_direction(handle, value.x, value.y, value.z);
		}

		public ma_vec3f Velocity
		{
			get => MiniAudioNative.ma_sound_group_get_velocity(handle);
			set => MiniAudioNative.ma_sound_group_set_velocity(handle, value.x, value.y, value.z);
		}

		public ma_attenuation_model AttenuationModel
		{
			get => MiniAudioNative.ma_sound_group_get_attenuation_model(handle);
			set => MiniAudioNative.ma_sound_group_set_attenuation_model(handle, value);
		}

		public MaSoundGroup(MaEngine engine)
		{
			if (engine.Handle.pointer == IntPtr.Zero)
				throw new ArgumentException("engine isn't initialized");

			handle = new ma_sound_group_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			ma_sound_flags flags = (ma_sound_flags)0;

			ma_result result = MiniAudioNative.ma_sound_group_init(engine.Handle, flags, default, handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Dispose();
				throw new Exception("Failed to initialize MaSoundGroup");
			}
		}

		public MaSoundGroup(MaEngine engine, ma_sound_flags flags, MaSoundGroup parentGroup = null)
		{
			if (engine.Handle.pointer == IntPtr.Zero)
				throw new ArgumentException("engine isn't initialized");

			handle = new ma_sound_group_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			if (parentGroup != null)
			{
				if (parentGroup.handle.pointer == IntPtr.Zero)
				{
					Dispose();
					throw new ArgumentException("parentGroup isn't initialized");
				}
			}
			
			ma_sound_group_ptr parent = parentGroup == null ? default : parentGroup.handle;
			ma_result result = MiniAudioNative.ma_sound_group_init(engine.Handle, flags, parent, handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Dispose();
				throw new Exception("Failed to initialize MaSoundGroup");
			}
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_sound_group_uninit(handle);
				handle.Free();
			}
		}

		public void Play()
		{
			if (handle.pointer == IntPtr.Zero)
				return;
			MiniAudioNative.ma_sound_group_start(handle);
		}

		public void Stop()
		{
			if (handle.pointer == IntPtr.Zero)
				return;
			MiniAudioNative.ma_sound_group_stop(handle);
		}

		public void SetNotificationsUserData(IntPtr userData)
		{
			if (handle.pointer == IntPtr.Zero)
				return;
			ma_sound_ptr s = new ma_sound_ptr(handle.pointer);
			MiniAudioNative.ma_sound_set_notifications_userdata(s, userData);
		}

		public void SetLoadNotificationCallback(ma_sound_load_proc callback)
		{
			if (handle.pointer == IntPtr.Zero)
				return;
			ma_sound_ptr s = new ma_sound_ptr(handle.pointer);
			MiniAudioNative.ma_sound_set_load_notification_callback(s, callback);
		}

		public void SetEndNotificationCallback(ma_sound_end_proc callback)
		{
			if (handle.pointer == IntPtr.Zero)
				return;
			ma_sound_ptr s = new ma_sound_ptr(handle.pointer);
			MiniAudioNative.ma_sound_set_end_notification_callback(s, callback);
		}

		public void SetProcessNotificationCallback(ma_sound_process_proc callback)
		{
			if (handle.pointer == IntPtr.Zero)
				return;
			ma_sound_ptr s = new ma_sound_ptr(handle.pointer);
			MiniAudioNative.ma_sound_set_process_notification_callback(s, callback);
		}
	}
}