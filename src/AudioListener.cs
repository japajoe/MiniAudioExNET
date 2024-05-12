using System;

namespace MiniAudioExNET
{
    public sealed class AudioListener : IDisposable
    {
        private IntPtr handle;

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

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
                Library.ma_ex_audio_listener_set_position(handle, value.x, value.y, value.z);
            }
        }

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

        public void Dispose()
        {
            MiniAudioEx.Remove(this);
        }
    }
}