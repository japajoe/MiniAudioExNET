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
	public sealed class MaSound : MaNode, IDisposable
	{
		private ma_sound_ptr handle;
		private bool isSoundLoaded;

		public ma_sound_ptr Handle
		{
			get => handle;
		}

		public MaSound() : base()
		{
			isSoundLoaded = false;

			handle = new ma_sound_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			nodeHandle = new ma_node_ptr(handle.pointer);
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
				isSoundLoaded = false;
			}
			nodeHandle.pointer = IntPtr.Zero;
		}

		public ma_result InitializeFromFile(MaEngine engine, string filePath, ma_sound_flags flags, MaSoundGroup group = null)
		{
			ma_result result = IsInitialized(engine);

			if (result != ma_result.success)
				return result;

			Unload();

			ma_sound_group_ptr pGroup = group == null ? default : group.Handle;

			result = MiniAudioNative.ma_sound_init_from_file(engine.Handle, filePath, flags, pGroup, default, handle);

			if (result != ma_result.success)
				return result;

			isSoundLoaded = true;

			return result;
		}

		public ma_result InitializeFromMemory(MaEngine engine, IntPtr data, UInt64 dataSize, ma_sound_flags flags, MaSoundGroup group = null)
		{
			ma_result result = IsInitialized(engine);

			if (result != ma_result.success)
				return result;

			Unload();

			ma_sound_group_ptr pGroup = group == null ? default : group.Handle;

			result = MiniAudioNative.ma_sound_init_from_memory(engine.Handle, data, dataSize, flags, pGroup, default, handle);

			if (result != ma_result.success)
				return result;

			isSoundLoaded = true;

			return result;
		}

		public ma_result InitializeFromCallback(MaEngine engine, UInt32 channels, UInt32 sampleRate, ma_procedural_data_source_proc callback, IntPtr userData, MaSoundGroup group = null)
		{
			ma_result result = IsInitialized(engine);

			if (result != ma_result.success)
				return result;

			Unload();

			ma_sound_flags flags = (ma_sound_flags)0;

			ma_sound_group_ptr pGroup = group == null ? default : group.Handle;

			ma_procedural_data_source_config config = MiniAudioNative.ma_procedural_data_source_config_init(ma_format.f32, channels, sampleRate, callback, userData);

			result = MiniAudioNative.ma_sound_init_from_callback(engine.Handle, ref config, flags, pGroup, default, handle);

			if (result != ma_result.success)
				return result;

			isSoundLoaded = true;

			return result;
		}

		public ma_result InitializeFromDataSource(MaEngine engine, MaDataSource dataSource, ma_sound_flags flags, MaSoundGroup group = null)
		{
			if (dataSource == null)
				return ma_result.invalid_args;

			if (dataSource.DataSourceHandle.pointer == IntPtr.Zero)
				return ma_result.invalid_args;

			return InitializeFromDataSource(engine, dataSource.DataSourceHandle, flags, group);
		}

		public ma_result InitializeFromDataSource(MaEngine engine, ma_data_source_ptr pDataSource, ma_sound_flags flags, MaSoundGroup group = null)
		{
			ma_result result = IsInitialized(engine);

			if (result != ma_result.success)
				return result;

			Unload();

			ma_sound_group_ptr pGroup = group == null ? default : group.Handle;

			result = MiniAudioNative.ma_sound_init_from_data_source(engine.Handle, pDataSource, flags, pGroup, handle);

			if (result != ma_result.success)
				return result;

			isSoundLoaded = true;

			return result;
		}

		public ma_result Start()
		{
			return MiniAudioNative.ma_sound_start(handle);
		}

		public ma_result Stop()
		{
			return MiniAudioNative.ma_sound_stop(handle);
		}

		public bool IsPlaying()
		{
			return MiniAudioNative.ma_sound_is_playing(handle) > 0;
		}

		public bool IsLooping()
		{
			return MiniAudioNative.ma_sound_is_looping(handle) > 0;
		}

		public void SetLooping(bool loop)
		{
			MiniAudioNative.ma_sound_set_looping(handle, loop ? (uint)1 : 0);
		}

		public bool IsSpatializationEnabled()
		{
			return MiniAudioNative.ma_sound_is_spatialization_enabled(handle) > 0;
		}

		public void SetSpatializationEnabled(bool enabled)
		{
			MiniAudioNative.ma_sound_set_spatialization_enabled(handle, enabled ? (uint)1 : 0);
		}

		public UInt64 GetTimeInPCMFrames()
		{
			return MiniAudioNative.ma_sound_get_time_in_pcm_frames(handle);
		}

		public void SeekToPCMFrame(UInt64 frame)
		{
			MiniAudioNative.ma_sound_seek_to_pcm_frame(handle, frame);
		}

		public float GetVolume()
		{
			return MiniAudioNative.ma_sound_get_volume(handle);
		}

		public void SetVolume(float volume)
		{
			MiniAudioNative.ma_sound_set_volume(handle, volume);
		}

		public float GetPan()
		{
			return MiniAudioNative.ma_sound_get_pan(handle);
		}

		public void SetPan(float pan)
		{
			MiniAudioNative.ma_sound_set_pan(handle, pan);
		}

		public ma_pan_mode GetPanMode()
		{
			return MiniAudioNative.ma_sound_get_pan_mode(handle);
		}

		public void SetPanMode(ma_pan_mode panMode)
		{
			MiniAudioNative.ma_sound_set_pan_mode(handle, panMode);
		}

		public float GetPitch()
		{
			return MiniAudioNative.ma_sound_get_pitch(handle);
		}

		public void SetPitch(float pitch)
		{
			MiniAudioNative.ma_sound_set_pitch(handle, pitch);
		}

		public float GetDopplerFactor()
		{
			return MiniAudioNative.ma_sound_get_doppler_factor(handle);
		}

		public void SetDopplerFactor(float dopplerFactor)
		{
			MiniAudioNative.ma_sound_set_doppler_factor(handle, dopplerFactor);
		}

		public ma_vec3f GetPosition()
		{
			return MiniAudioNative.ma_sound_get_position(handle);
		}

		public void SetPosition(ma_vec3f position)
		{
			MiniAudioNative.ma_sound_set_position(handle, position.x, position.y, position.z);
		}

		public ma_vec3f GetDirection()
		{
			return MiniAudioNative.ma_sound_get_direction(handle);
		}

		public void SetDirection(ma_vec3f direction)
		{
			MiniAudioNative.ma_sound_set_direction(handle, direction.x, direction.y, direction.z);
		}

		public ma_vec3f GetVelocity()
		{
			return MiniAudioNative.ma_sound_get_velocity(handle);
		}

		public void SetVelocity(ma_vec3f velocity)
		{
			MiniAudioNative.ma_sound_set_velocity(handle, velocity.x, velocity.y, velocity.z);
		}

		public ma_attenuation_model GetAttenuationModel()
		{
			return MiniAudioNative.ma_sound_get_attenuation_model(handle);
		}

		public void SetAttenuationModel(ma_attenuation_model attenuationModel)
		{
			MiniAudioNative.ma_sound_set_attenuation_model(handle, attenuationModel);
		}

		public ma_result SetEndCallback(ma_sound_end_proc callback, IntPtr pUserData)
		{
			return MiniAudioNative.ma_sound_set_end_callback(handle, callback, pUserData);
		}

		private ma_result IsInitialized(MaEngine engine)
		{
			if (engine == null)
				return ma_result.invalid_args;

			if (engine.Handle.pointer == IntPtr.Zero)
				return ma_result.invalid_args;

			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero)
				return ma_result.error;

			return ma_result.success;
		}

		private void Unload()
		{
			if (handle.pointer == IntPtr.Zero || !isSoundLoaded)
				return;
			MiniAudioNative.ma_sound_stop(handle);
			MiniAudioNative.ma_sound_uninit(handle);
			isSoundLoaded = false;
		}
	}
}