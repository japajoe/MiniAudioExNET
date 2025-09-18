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
	public sealed class MaSound : IDisposable
	{
		private struct Settings
		{
			public bool loop;
			public bool spatialization;
			public float volume;
			public float pan;
			public ma_pan_mode panMode;
			public float pitch;
			public float dopplerFactor;
			public ma_vec3f position;
			public ma_vec3f direction;
			public ma_vec3f velocity;
			public ma_attenuation_model attenuationModel;
		}

		private ma_sound_ptr handle;
		private MaEngine engine;
		private Settings settings;
		private bool isSoundLoaded;

		public ma_sound_ptr Handle
		{
			get => handle;
		}

		public bool IsPlaying
		{
			get => MiniAudioNative.ma_sound_is_playing(handle) > 0;
		}

		public bool Loop
		{
			get
			{
				return settings.loop;
			}
			set
			{
				settings.loop = value;
				MiniAudioNative.ma_sound_set_looping(handle, value ? (uint)1 : 0);
			}
		}

		public bool Spatialization
		{
			get
			{
				return settings.spatialization;
			}
			set
			{
				settings.spatialization = value;
				MiniAudioNative.ma_sound_set_spatialization_enabled(handle, value ? (uint)1 : 0);
			}
		}

		public UInt64 Cursor
		{
			get
			{
				return MiniAudioNative.ma_sound_get_time_in_pcm_frames(handle);
			}
			set
			{
				MiniAudioNative.ma_sound_seek_to_pcm_frame(handle, value);
			}
		}

		public float Volume
		{
			get
			{
				return settings.volume;
			}
			set
			{
				settings.volume = value;
				MiniAudioNative.ma_sound_set_volume(handle, value);
			}
		}

		public float Pan
		{
			get
			{
				return settings.pan;
			}
			set
			{
				settings.pan = value;
				MiniAudioNative.ma_sound_set_pan(handle, value);
			}
		}

		public ma_pan_mode PanMode
		{
			get
			{
				return settings.panMode;
			}
			set
			{
				settings.panMode = value;
				MiniAudioNative.ma_sound_set_pan_mode(handle, value);
			}
		}

		public float Pitch
		{
			get
			{
				return settings.pitch;
			}
			set
			{
				settings.pitch = value;
				MiniAudioNative.ma_sound_set_pitch(handle, value);
			}
		}

		public float DopplerFactor
		{
			get
			{
				return settings.dopplerFactor;
			}
			set
			{
				settings.dopplerFactor = value;
				MiniAudioNative.ma_sound_set_doppler_factor(handle, value);
			}
		}

		public ma_vec3f Position
		{
			get
			{
				return settings.position;
			}
			set
			{
				settings.position = value;
				MiniAudioNative.ma_sound_set_position(handle, value.x, value.y, value.z);
			}
		}

		public ma_vec3f Direction
		{
			get
			{
				return settings.direction;
			}
			set
			{
				settings.direction = value;
				MiniAudioNative.ma_sound_set_direction(handle, value.x, value.y, value.z);
			}
		}

		public ma_vec3f Velocity
		{
			get
			{
				return settings.velocity;
			}
			set
			{
				settings.velocity = value;
				MiniAudioNative.ma_sound_set_velocity(handle, value.x, value.y, value.z);
			}
		}

		public ma_attenuation_model AttenuationModel
		{
			get
			{
				return settings.attenuationModel;
			}
			set
			{
				settings.attenuationModel = value;
				MiniAudioNative.ma_sound_set_attenuation_model(handle, value);
			}
		}

		public MaSound(MaEngine engine)
		{
			isSoundLoaded = false;

			if (engine.Handle.pointer == IntPtr.Zero)
				throw new ArgumentException("engine isn't initialized");

			this.engine = engine;

			handle = new ma_sound_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			settings.loop = false;
			settings.spatialization = false;
			settings.volume = 1.0f;
			settings.pan = 0.0f;
			settings.panMode = ma_pan_mode.balance;
			settings.pitch = 1.0f;
			settings.dopplerFactor = 1.0f;
			settings.position = new ma_vec3f(0.0f, 0.0f, 0.0f);
			settings.direction = new ma_vec3f(0.0f, 0.0f, -1.0f);
			settings.velocity = new ma_vec3f(0.0f, 0.0f, 0.0f);
			settings.attenuationModel = ma_attenuation_model.inverse;
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				if (isSoundLoaded)
				{
					MiniAudioNative.ma_sound_stop(handle);
					MiniAudioNative.ma_sound_uninit(handle);
				}
				handle.Free();
				engine = null;
				isSoundLoaded = false;
			}
		}

		public bool LoadFromFile(string filePath, bool streamFromDisk, MaSoundGroup group = null)
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero)
				return false;

			Unload();

			ma_sound_flags flags = ma_sound_flags.DECODE;

			if (streamFromDisk)
				flags |= ma_sound_flags.STREAM;

			ma_sound_group_ptr pGroup = group == null ? default : group.Handle;

			ma_result result = MiniAudioNative.ma_sound_init_from_file(engine.Handle, filePath, flags, pGroup, default, handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Console.WriteLine("Failed to initialize MaSound");
				return false;
			}

			ApplySettings();
			isSoundLoaded = true;

			return true;
		}

		public bool LoadFromMemory(IntPtr data, UInt64 dataSize, MaSoundGroup group = null)
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero)
				return false;

			Unload();

			ma_sound_flags flags = ma_sound_flags.DECODE;

			ma_sound_group_ptr pGroup = group == null ? default : group.Handle;

			ma_result result = MiniAudioNative.ma_sound_init_from_memory(engine.Handle, data, dataSize, flags, pGroup, default, handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Console.WriteLine("Failed to initialize MaSound");
				return false;
			}

			ApplySettings();
			isSoundLoaded = true;

			return true;
		}

		public bool LoadFromCallback(UInt32 channels, UInt32 sampleRate, ma_procedural_sound_proc callback, IntPtr userData, MaSoundGroup group = null)
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero)
				return false;

			Unload();

			ma_sound_flags flags = (ma_sound_flags)0;

			ma_sound_group_ptr pGroup = group == null ? default : group.Handle;

			ma_procedural_sound_config config = MiniAudioNative.ma_procedural_sound_config_init(ma_format.f32, channels, sampleRate, callback, userData);

			ma_result result = MiniAudioNative.ma_sound_init_from_callback(engine.Handle, ref config, flags, pGroup, default, handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Console.WriteLine("Failed to initialize MaSound");
				return false;
			}

			ApplySettings();
			isSoundLoaded = true;

			return true;
		}

		public void Unload()
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero || !isSoundLoaded)
				return;
			MiniAudioNative.ma_sound_stop(handle);
			MiniAudioNative.ma_sound_uninit(handle);
			isSoundLoaded = false;
		}

		public void Play()
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero)
				return;
			MiniAudioNative.ma_sound_start(handle);
		}

		public void Stop()
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero)
				return;
			MiniAudioNative.ma_sound_stop(handle);
		}

		public void SetNotificationsUserData(IntPtr userData)
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero || !isSoundLoaded)
				return;
			MiniAudioNative.ma_sound_set_notifications_userdata(handle, userData);			
		}

		public void SetLoadNotificationCallback(ma_sound_load_proc callback)
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero || !isSoundLoaded)
				return;
			MiniAudioNative.ma_sound_set_load_notification_callback(handle, callback);
		}

		public void SetEndNotificationCallback(ma_sound_end_proc callback)
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero || !isSoundLoaded)
				return;
			MiniAudioNative.ma_sound_set_end_notification_callback(handle, callback);
		}

		public void SetProcessNotificationCallback(ma_sound_process_proc callback)
		{
			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero || !isSoundLoaded)
				return;
			MiniAudioNative.ma_sound_set_process_notification_callback(handle, callback);
		}

		private void ApplySettings()
		{
			MiniAudioNative.ma_sound_set_looping(handle, settings.loop ? (uint)1 : 0);
			MiniAudioNative.ma_sound_set_spatialization_enabled(handle, settings.spatialization ? (uint)1 : 0);
			MiniAudioNative.ma_sound_set_volume(handle, settings.volume);
			MiniAudioNative.ma_sound_set_pan(handle, settings.pan);
			MiniAudioNative.ma_sound_set_pan_mode(handle, settings.panMode);
			MiniAudioNative.ma_sound_set_pitch(handle, settings.pitch);
			MiniAudioNative.ma_sound_set_doppler_factor(handle, settings.dopplerFactor);
			MiniAudioNative.ma_sound_set_position(handle, settings.position.x, settings.position.y, settings.position.z);
			MiniAudioNative.ma_sound_set_direction(handle, settings.direction.x, settings.direction.y, settings.direction.z);
			MiniAudioNative.ma_sound_set_velocity(handle, settings.velocity.x, settings.velocity.y, settings.velocity.z);
			MiniAudioNative.ma_sound_set_attenuation_model(handle, settings.attenuationModel);
		}
	}
}