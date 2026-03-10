using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core
{
    /// <summary>
    /// This class represents a point in the 3D space where audio is perceived or heard.
    /// </summary>
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

        /// <summary>
        /// If true, then spatialization is enabled for this listener.
        /// </summary>
        /// <value></value>
        public bool Enabled
        {
            get => MiniAudio.ma_engine_listener_is_enabled(context.Engine, 0) > 0;
            set => MiniAudio.ma_engine_listener_set_enabled(context.Engine, 0, value ? (UInt32)1 : 0);
        }

        /// <summary>
        /// Gets or sets the position of the listener.
        /// </summary>
        /// <value></value>
        public Vector3f Position
        {
            get
            {
                ma_vec3f position = MiniAudio.ma_engine_listener_get_position(context.Engine, 0);
                return new Vector3f(position.x, position.y, position.z);
            }
            set => MiniAudio.ma_engine_listener_set_position(context.Engine, 0, value.x, value.y, value.z);
        }


        /// <summary>
        /// Gets or sets the direction that the listener is facing.
        /// </summary>
        /// <value></value>
        public Vector3f Direction
        {
            get
            {
                ma_vec3f direction = MiniAudio.ma_engine_listener_get_direction(context.Engine, 0);
                return new Vector3f(direction.x, direction.y, direction.z);
            }
            set => MiniAudio.ma_engine_listener_set_direction(context.Engine, 0, value.x, value.y, value.z);
        }


        /// <summary>
        /// Gets or sets the velocity of the listener.
        /// </summary>
        /// <value></value>
        public Vector3f Velocity
        {
            get
            {
                ma_vec3f velocity = MiniAudio.ma_engine_listener_get_velocity(context.Engine, 0);
                return new Vector3f(velocity.x, velocity.y, velocity.z);
            }
            set => MiniAudio.ma_engine_listener_set_velocity(context.Engine, 0, value.x, value.y, value.z);
        }

        /// <summary>
        /// Gets or sets the up direction of the world. By default this is 0,1,0.
        /// </summary>
        /// <value></value>
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