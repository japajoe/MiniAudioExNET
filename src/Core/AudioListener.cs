using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core
{
    public sealed class AudioListener
    {
        private AudioContext context;

        public AudioListener()
        {
            context = AudioContext.GetCurrent();

            if(context == null)
                throw new Exception("Failed to initialize AudioListener because there is no current AudioContext");

            Position = new Vector3f(0, 0, 0);
            Direction = new Vector3f(0, 0, -1);
            WorldUp = new Vector3f(0, 1, 0);
            Enabled = true;
        }

        public bool Enabled
        {
            get => MiniAudio.ma_engine_listener_is_enabled(context.Engine, 0) > 0;
            set => MiniAudio.ma_engine_listener_set_enabled(context.Engine, 0, value ? (UInt32)1 : 0);
        }

        public Vector3f Position
        {
            get
            {
                ma_vec3f position = MiniAudio.ma_engine_listener_get_position(context.Engine, 0);
                return new Vector3f(position.x, position.y, position.z);
            }
            set => MiniAudio.ma_engine_listener_set_position(context.Engine, 0, value.x, value.y, value.z);
        }

        public Vector3f Direction
        {
            get
            {
                ma_vec3f direction = MiniAudio.ma_engine_listener_get_direction(context.Engine, 0);
                return new Vector3f(direction.x, direction.y, direction.z);
            }
            set => MiniAudio.ma_engine_listener_set_direction(context.Engine, 0, value.x, value.y, value.z);
        }

        public Vector3f Velocity
        {
            get
            {
                ma_vec3f velocity = MiniAudio.ma_engine_listener_get_velocity(context.Engine, 0);
                return new Vector3f(velocity.x, velocity.y, velocity.z);
            }
            set => MiniAudio.ma_engine_listener_set_velocity(context.Engine, 0, value.x, value.y, value.z);
        }

        public Vector3f WorldUp
        {
            get
            {
                ma_vec3f worldUp = MiniAudio.ma_engine_listener_get_world_up(context.Engine, 0);
                return new Vector3f(worldUp.x, worldUp.y, worldUp.z);
            }
            set => MiniAudio.ma_engine_listener_set_world_up(context.Engine, 0, value.x, value.y, value.z);
        }
    }
}