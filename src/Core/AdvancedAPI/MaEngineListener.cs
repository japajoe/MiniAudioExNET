using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public sealed class MaEngineListener : IDisposable
	{
		private MaEngine engine;
		private UInt32 index;

		public bool Enabled
		{
			get => MiniAudioNative.ma_engine_listener_is_enabled(engine.Handle, index) > 0;
			set => MiniAudioNative.ma_engine_listener_set_enabled(engine.Handle, index, value ? (UInt32)1 : 0);
		}

		public ma_vec3f Position
		{
			get => MiniAudioNative.ma_engine_listener_get_position(engine.Handle, index);
			set => MiniAudioNative.ma_engine_listener_set_position(engine.Handle, index, value.x, value.y, value.z);
		}

		public ma_vec3f Direction
		{
			get => MiniAudioNative.ma_engine_listener_get_direction(engine.Handle, index);
			set => MiniAudioNative.ma_engine_listener_set_direction(engine.Handle, index, value.x, value.y, value.z);
		}

		public ma_vec3f Velocity
		{
			get => MiniAudioNative.ma_engine_listener_get_velocity(engine.Handle, index);
			set => MiniAudioNative.ma_engine_listener_set_velocity(engine.Handle, index, value.x, value.y, value.z);
		}

		public ma_vec3f WorldUp
		{
			get => MiniAudioNative.ma_engine_listener_get_world_up(engine.Handle, index);
			set => MiniAudioNative.ma_engine_listener_set_world_up(engine.Handle, index, value.x, value.y, value.z);
		}

		public ConeSettings Cone
		{
			get
			{
				ConeSettings c = new ConeSettings();
				MiniAudioNative.ma_engine_listener_get_cone(engine.Handle, index, out c.innerAngleInRadians, out c.outerAngleInRadians, out c.outerGain);
				return c;
			}
			set
			{
				MiniAudioNative.ma_engine_listener_set_world_up(engine.Handle, index, value.innerAngleInRadians, value.outerAngleInRadians, value.outerGain);
			}
		}

		public MaEngineListener(MaEngine engine, UInt32 index)
		{
			if (engine.Handle.pointer == IntPtr.Zero)
				throw new ArgumentException("engine isn't initialized");

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

		public struct ConeSettings
		{
			public float innerAngleInRadians;
			public float outerAngleInRadians;
			public float outerGain;
		}
	}
}