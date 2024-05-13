using System;

namespace MiniAudioExNET
{
    /// <summary>
    /// This class represents a point in the 3D space where audio is perceived or heard.
    /// </summary>
    public sealed class AudioListener : IDisposable
    {
        private IntPtr handle;
        private Vector3f previousPosition;

        /// <summary>
        /// A handle to the native ma_audio_listener instance.
        /// </summary>
        /// <value></value>
        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        /// <summary>
        /// If true, then spatialization is enabled for this listener.
        /// </summary>
        /// <value></value>
        public bool Enabled
        {
            get
            {
                return Library.ma_ex_audio_listener_get_spatialization(handle) > 0;
            }
            set
            {
                Library.ma_ex_audio_listener_set_spatialization(handle, value ? (uint)1 : 0);
            }
        }

        /// <summary>
        /// The position of the listener.
        /// </summary>
        /// <value></value>
        public Vector3f Position
        {
            get
            {
                float x, y, z;
                Library.ma_ex_audio_listener_get_position(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                Library.ma_ex_audio_listener_get_position(handle, out previousPosition.x, out previousPosition.y, out previousPosition.z);
                Library.ma_ex_audio_listener_set_position(handle, value.x, value.y, value.z);
            }
        }

        /// <summary>
        /// The direction that the listener is facing.
        /// </summary>
        /// <value></value>
        public Vector3f Direction
        {
            get
            {
                float x, y, z;
                Library.ma_ex_audio_listener_get_direction(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                Library.ma_ex_audio_listener_set_direction(handle, value.x, value.y, value.z);
            }
        }

        /// <summary>
        /// The velocity of the listener.
        /// </summary>
        /// <value></value>
        public Vector3f Velocity
        {
            get
            {
                float x, y, z;
                Library.ma_ex_audio_listener_get_velocity(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                Library.ma_ex_audio_listener_set_velocity(handle, value.x, value.y, value.z);
            }
        }

        /// <summary>
        /// The up direction of the world. By default this is 0,1,0.
        /// </summary>
        /// <value></value>
        public Vector3f WorldUp
        {
            get
            {
                float x, y, z;
                Library.ma_ex_audio_listener_get_world_up(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                Library.ma_ex_audio_listener_set_world_up(handle, value.x, value.y, value.z);
            }
        }

        public AudioListener()
        {
            handle = Library.ma_ex_audio_listener_init(MiniAudioEx.AudioContext);

            if(handle != IntPtr.Zero)
            {
                previousPosition = new Vector3f(0, 0, 0);
                MiniAudioEx.Add(this);
            }
        }

        internal void Destroy()
        {
            if(handle != IntPtr.Zero)
            {
                Library.ma_ex_audio_listener_uninit(handle);
                handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Calculates the velocity based on the current position and the previous position.
        /// </summary>
        /// <returns></returns>
        public Vector3f GetCalculatedVelocity()
        {
            float deltaTime = MiniAudioEx.DeltaTime;
            Vector3f currentPosition = Position;
            float dx = currentPosition.x - previousPosition.x;
            float dy = currentPosition.y - previousPosition.y;
            float dz = currentPosition.z - previousPosition.z;
            return new Vector3f(dx / deltaTime, dy / deltaTime, dz / deltaTime);
        }

        public void Dispose()
        {
            MiniAudioEx.Remove(this);
        }
    }
}