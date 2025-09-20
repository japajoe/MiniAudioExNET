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
	public sealed class MaEngineListener : IDisposable
	{
		private MaEngine engine;
		private UInt32 index;

		public MaEngineListener()
		{
			engine = null;
			index = 0;
		}

		public void Dispose()
		{
			if (engine != null)
			{
				if (engine.Handle.pointer != IntPtr.Zero)
				{
					MiniAudioNative.ma_engine_listener_set_enabled(engine.Handle, index, 0);
					engine = null;
				}
			}
		}

		public ma_result Initialize(MaEngine engine, UInt32 index)
		{
			if (engine == null)
				return ma_result.invalid_args;

			if (engine.Handle.pointer == IntPtr.Zero)
				return ma_result.error;

			this.engine = engine;
			this.index = index;

			const float coneInnerAngleInRadians = (float)(2 * Math.PI);
			const float coneOuterAngleInRadians = (float)(2 * Math.PI);
			const float coneOuterGain = 0.0f;

			MiniAudioNative.ma_engine_listener_set_position(engine.Handle, index, 0, 0, 0);
			MiniAudioNative.ma_engine_listener_set_direction(engine.Handle, index, 0, 0, -1);
			MiniAudioNative.ma_engine_listener_set_velocity(engine.Handle, index, 0, 0, 0);
			MiniAudioNative.ma_engine_listener_set_world_up(engine.Handle, index, 0, 1, 0);
			MiniAudioNative.ma_engine_listener_set_cone(engine.Handle, index, coneInnerAngleInRadians, coneOuterAngleInRadians, coneOuterGain);
			MiniAudioNative.ma_engine_listener_set_enabled(engine.Handle, index, 1);

			return ma_result.success;
		}

		public bool IsEnabled()
		{
			return MiniAudioNative.ma_engine_listener_is_enabled(engine.Handle, index) > 0;
		}

		public void SetEnabled(bool enabled)
		{
			MiniAudioNative.ma_engine_listener_set_enabled(engine.Handle, index, enabled ? (UInt32)1 : 0);
		}

		public ma_vec3f GetPosition()
		{
			return MiniAudioNative.ma_engine_listener_get_position(engine.Handle, index);
		}

		public void SetPosition(ma_vec3f position)
		{
			MiniAudioNative.ma_engine_listener_set_position(engine.Handle, index, position.x, position.y, position.z);
		}

		public ma_vec3f GetDirection()
		{
			return MiniAudioNative.ma_engine_listener_get_direction(engine.Handle, index);
		}

		public void SetDirection(ma_vec3f direction)
		{
			MiniAudioNative.ma_engine_listener_set_direction(engine.Handle, index, direction.x, direction.y, direction.z);
		}

		public ma_vec3f GetVelocity()
		{
			return MiniAudioNative.ma_engine_listener_get_velocity(engine.Handle, index);
		}

		public void SetVelocity(ma_vec3f velocity)
		{
			MiniAudioNative.ma_engine_listener_set_velocity(engine.Handle, index, velocity.x, velocity.y, velocity.z);
		}

		public ma_vec3f GetWorldUp()
		{
			return MiniAudioNative.ma_engine_listener_get_world_up(engine.Handle, index);
		}

		public void SetWorldUp(ma_vec3f worldUp)
		{
			MiniAudioNative.ma_engine_listener_set_world_up(engine.Handle, index, worldUp.x, worldUp.y, worldUp.z);
		}

		public void GetCone(out float innerAngleInRadians, out float outerAngleInRadians, out float outerGain)
		{
			MiniAudioNative.ma_engine_listener_get_cone(engine.Handle, index, out innerAngleInRadians, out outerAngleInRadians, out outerGain);
		}

		public void SetCone(float innerAngleInRadians, float outerAngleInRadians, float outerGain)
		{
			MiniAudioNative.ma_engine_listener_set_world_up(engine.Handle, index, innerAngleInRadians, outerAngleInRadians, outerGain);
		}
	}
}