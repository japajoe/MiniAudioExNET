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
// Copyright 2024 W.M.R Jap-A-Joe

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