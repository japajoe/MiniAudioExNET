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
        private UInt32 index;
        private Vector3f previousPosition;

        /// <summary>
        /// If true, then spatialization is enabled for this listener.
        /// </summary>
        /// <value></value>
        public bool Enabled
        {
            get => MiniAudio.ma_engine_listener_is_enabled(context.Engine, index) > 0;
            set => MiniAudio.ma_engine_listener_set_enabled(context.Engine, index, value ? (UInt32)1 : 0);
        }

        /// <summary>
        /// Gets or sets the position of the listener.
        /// </summary>
        /// <value></value>
        public Vector3f Position
        {
            get
            {
                ma_vec3f position = MiniAudio.ma_engine_listener_get_position(context.Engine, index);
                return new Vector3f(position.x, position.y, position.z);
            }
            set
            {
                var previous = MiniAudio.ma_engine_listener_get_position(context.Engine, index);
                previousPosition.x = previous.x;
                previousPosition.y = previous.y;
                previousPosition.z = previous.z;
                MiniAudio.ma_engine_listener_set_position(context.Engine, 0, value.x, value.y, value.z);
            }
        }


        /// <summary>
        /// Gets or sets the direction that the listener is facing.
        /// </summary>
        /// <value></value>
        public Vector3f Direction
        {
            get
            {
                ma_vec3f direction = MiniAudio.ma_engine_listener_get_direction(context.Engine, index);
                return new Vector3f(direction.x, direction.y, direction.z);
            }
            set => MiniAudio.ma_engine_listener_set_direction(context.Engine, index, value.x, value.y, value.z);
        }


        /// <summary>
        /// Gets or sets the velocity of the listener.
        /// </summary>
        /// <value></value>
        public Vector3f Velocity
        {
            get
            {
                ma_vec3f velocity = MiniAudio.ma_engine_listener_get_velocity(context.Engine, index);
                return new Vector3f(velocity.x, velocity.y, velocity.z);
            }
            set => MiniAudio.ma_engine_listener_set_velocity(context.Engine, index, value.x, value.y, value.z);
        }

        /// <summary>
        /// Gets the the velocity based on the current position and the previous position.
        /// </summary>
        /// <returns></returns>
        public Vector3f CurrentVelocity
        {
            get
            {
                if(context == null)
                    return new Vector3f(0, 0, 0);
                float deltaTime = context.DeltaTime;
                Vector3f currentPosition = Position;
                float dx = currentPosition.x - previousPosition.x;
                float dy = currentPosition.y - previousPosition.y;
                float dz = currentPosition.z - previousPosition.z;
                return new Vector3f(dx / deltaTime, dy / deltaTime, dz / deltaTime);
            }
        }

        /// <summary>
        /// Gets or sets the up direction of the world. By default this is 0,1,0.
        /// </summary>
        /// <value></value>
        public Vector3f WorldUp
        {
            get
            {
                ma_vec3f worldUp = MiniAudio.ma_engine_listener_get_world_up(context.Engine, index);
                return new Vector3f(worldUp.x, worldUp.y, worldUp.z);
            }
            set => MiniAudio.ma_engine_listener_set_world_up(context.Engine, index, value.x, value.y, value.z);
        }

        public AudioListener(UInt32 index = 0)
        {
            context = AudioContext.GetCurrent();

            if(context == null)
                throw new Exception("Failed to initialize AudioListener because there is no current AudioContext");

            if(index >= MiniAudio.MA_ENGINE_MAX_LISTENERS)
                throw new Exception("Listener index should be less than " + MiniAudio.MA_ENGINE_MAX_LISTENERS);

            this.index = index;

            Position = new Vector3f(0, 0, 0);
            Direction = new Vector3f(0, 0, -1);
            WorldUp = new Vector3f(0, 1, 0);
            Enabled = true;
            previousPosition  = new Vector3f(0, 0, 0);
        }
    }
}