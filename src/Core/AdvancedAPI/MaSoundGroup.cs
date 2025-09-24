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
	public sealed class MaSoundGroup : MaNode, IDisposable
	{
		private ma_sound_group_ptr handle;

		public ma_sound_group_ptr Handle
		{
			get => handle;
		}

		public MaSoundGroup() : base()
		{
			handle = new ma_sound_group_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			nodeHandle = new ma_node_ptr(handle.pointer);
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_sound_group_uninit(handle);
				handle.Free();
			}
			nodeHandle.pointer = IntPtr.Zero;
		}

		public ma_result Initialize(MaEngine engine)
		{
			return Initialize(engine, (ma_sound_flags)0, null);
		}

		public ma_result Initialize(MaEngine engine, ma_sound_flags flags, MaSoundGroup parentGroup = null)
		{
			if (engine == null)
				return ma_result.invalid_args;

			if (engine.Handle.pointer == IntPtr.Zero)
				return ma_result.invalid_args;

			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			ma_sound_group_ptr parent = parentGroup == null ? default : parentGroup.handle;
			return MiniAudioNative.ma_sound_group_init(engine.Handle, flags, parent, handle);
		}

		public ma_result Start()
		{
			return MiniAudioNative.ma_sound_group_start(handle);
		}

		public ma_result Stop()
		{
			return MiniAudioNative.ma_sound_group_stop(handle);
		}

		public bool IsPlaying()
		{
			return MiniAudioNative.ma_sound_group_is_playing(handle) > 0;
		}

		public bool IsSpatialializationEnabled()
		{
			return MiniAudioNative.ma_sound_group_is_spatialization_enabled(handle) > 0;
		}

		public void SetSpatialializationEnabled(bool enabled)
		{
			MiniAudioNative.ma_sound_group_set_spatialization_enabled(handle, enabled ? (uint)1 : 0);
		}

		public float GetVolume()
		{
			return MiniAudioNative.ma_sound_group_get_volume(handle);
		}

		public void SetVolume(float volume)
		{
			MiniAudioNative.ma_sound_group_set_volume(handle, volume);
		}

		public float GetPan()
		{
			return MiniAudioNative.ma_sound_group_get_pan(handle);
		}

		public void SetPan(float pan)
		{
			MiniAudioNative.ma_sound_group_set_pan(handle, pan);
		}

		public ma_pan_mode GetPanMode()
		{
			return MiniAudioNative.ma_sound_group_get_pan_mode(handle);
		}

		public void SetPanMode(ma_pan_mode panMode)
		{
			MiniAudioNative.ma_sound_group_set_pan_mode(handle, panMode);
		}

		public float GetPitch()
		{
			return MiniAudioNative.ma_sound_group_get_pitch(handle);
		}

		public void SetPitch(float pitch)
		{
			MiniAudioNative.ma_sound_group_set_pitch(handle, pitch);
		}

		public float GetyDopplerFactor()
		{
			return MiniAudioNative.ma_sound_group_get_doppler_factor(handle);
		}

		public void SetDopplerFactor(float dopplerFactor)
		{
			MiniAudioNative.ma_sound_group_set_doppler_factor(handle, dopplerFactor);
		}

		public ma_vec3f GetPosition()
		{
			return MiniAudioNative.ma_sound_group_get_position(handle);
		}

		public void SetPosition(ma_vec3f position)
		{
			MiniAudioNative.ma_sound_group_set_position(handle, position.x, position.y, position.z);
		}

		public ma_vec3f GetDirection()
		{
			return MiniAudioNative.ma_sound_group_get_direction(handle);
		}

		public void SetDirection(ma_vec3f direction)
		{
			MiniAudioNative.ma_sound_group_set_direction(handle, direction.x, direction.y, direction.z);
		}

		public ma_vec3f GetVelocity()
		{
			return MiniAudioNative.ma_sound_group_get_velocity(handle);
		}

		public void SetVelocity(ma_vec3f velocity)
		{
			MiniAudioNative.ma_sound_group_set_velocity(handle, velocity.x, velocity.y, velocity.z);
		}

		public ma_attenuation_model GetAttenuationModel()
		{
			return MiniAudioNative.ma_sound_group_get_attenuation_model(handle);
		}

		public void SetAttenuationModel(ma_attenuation_model attenuationModel)
		{
			MiniAudioNative.ma_sound_group_set_attenuation_model(handle, attenuationModel);
		}

		public ma_result SetEndCallback(ma_sound_end_proc callback, IntPtr pUserData)
		{
			return MiniAudioNative.ma_sound_set_end_callback(new ma_sound_ptr(handle.pointer), callback, pUserData);
		}
	}
}