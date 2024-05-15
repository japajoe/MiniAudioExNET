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
using System.Runtime.InteropServices;

namespace MiniAudioExNET
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_sound_load_proc(IntPtr pUserData, IntPtr pSound);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_sound_end_proc(IntPtr pUserData, IntPtr pSound);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_sound_process_proc(IntPtr pUserData, IntPtr pSound, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_waveform_proc(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels);

    public enum ma_result
    {
        MA_SUCCESS                        =  0,
        MA_ERROR                          = -1,  /* A generic error. */
        MA_INVALID_ARGS                   = -2,
        MA_INVALID_OPERATION              = -3,
        MA_OUT_OF_MEMORY                  = -4,
        MA_OUT_OF_RANGE                   = -5,
        MA_ACCESS_DENIED                  = -6,
        MA_DOES_NOT_EXIST                 = -7,
        MA_ALREADY_EXISTS                 = -8,
        MA_TOO_MANY_OPEN_FILES            = -9,
        MA_INVALID_FILE                   = -10,
        MA_TOO_BIG                        = -11,
        MA_PATH_TOO_LONG                  = -12,
        MA_NAME_TOO_LONG                  = -13,
        MA_NOT_DIRECTORY                  = -14,
        MA_IS_DIRECTORY                   = -15,
        MA_DIRECTORY_NOT_EMPTY            = -16,
        MA_AT_END                         = -17,
        MA_NO_SPACE                       = -18,
        MA_BUSY                           = -19,
        MA_IO_ERROR                       = -20,
        MA_INTERRUPT                      = -21,
        MA_UNAVAILABLE                    = -22,
        MA_ALREADY_IN_USE                 = -23,
        MA_BAD_ADDRESS                    = -24,
        MA_BAD_SEEK                       = -25,
        MA_BAD_PIPE                       = -26,
        MA_DEADLOCK                       = -27,
        MA_TOO_MANY_LINKS                 = -28,
        MA_NOT_IMPLEMENTED                = -29,
        MA_NO_MESSAGE                     = -30,
        MA_BAD_MESSAGE                    = -31,
        MA_NO_DATA_AVAILABLE              = -32,
        MA_INVALID_DATA                   = -33,
        MA_TIMEOUT                        = -34,
        MA_NO_NETWORK                     = -35,
        MA_NOT_UNIQUE                     = -36,
        MA_NOT_SOCKET                     = -37,
        MA_NO_ADDRESS                     = -38,
        MA_BAD_PROTOCOL                   = -39,
        MA_PROTOCOL_UNAVAILABLE           = -40,
        MA_PROTOCOL_NOT_SUPPORTED         = -41,
        MA_PROTOCOL_FAMILY_NOT_SUPPORTED  = -42,
        MA_ADDRESS_FAMILY_NOT_SUPPORTED   = -43,
        MA_SOCKET_NOT_SUPPORTED           = -44,
        MA_CONNECTION_RESET               = -45,
        MA_ALREADY_CONNECTED              = -46,
        MA_NOT_CONNECTED                  = -47,
        MA_CONNECTION_REFUSED             = -48,
        MA_NO_HOST                        = -49,
        MA_IN_PROGRESS                    = -50,
        MA_CANCELLED                      = -51,
        MA_MEMORY_ALREADY_MAPPED          = -52,

        /* General non-standard errors. */
        MA_CRC_MISMATCH                   = -100,

        /* General miniaudio-specific errors. */
        MA_FORMAT_NOT_SUPPORTED           = -200,
        MA_DEVICE_TYPE_NOT_SUPPORTED      = -201,
        MA_SHARE_MODE_NOT_SUPPORTED       = -202,
        MA_NO_BACKEND                     = -203,
        MA_NO_DEVICE                      = -204,
        MA_API_NOT_FOUND                  = -205,
        MA_INVALID_DEVICE_CONFIG          = -206,
        MA_LOOP                           = -207,
        MA_BACKEND_NOT_ENABLED            = -208,

        /* State errors. */
        MA_DEVICE_NOT_INITIALIZED         = -300,
        MA_DEVICE_ALREADY_INITIALIZED     = -301,
        MA_DEVICE_NOT_STARTED             = -302,
        MA_DEVICE_NOT_STOPPED             = -303,

        /* Operation errors. */
        MA_FAILED_TO_INIT_BACKEND         = -400,
        MA_FAILED_TO_OPEN_BACKEND_DEVICE  = -401,
        MA_FAILED_TO_START_BACKEND_DEVICE = -402,
        MA_FAILED_TO_STOP_BACKEND_DEVICE  = -403
    }

    public enum ma_attenuation_model
    {
        ma_attenuation_model_none,          /* No distance attenuation and no spatialization. */
        ma_attenuation_model_inverse,       /* Equivalent to OpenAL's AL_INVERSE_DISTANCE_CLAMPED. */
        ma_attenuation_model_linear,        /* Linear attenuation. Equivalent to OpenAL's AL_LINEAR_DISTANCE_CLAMPED. */
        ma_attenuation_model_exponential    /* Exponential attenuation. Equivalent to OpenAL's AL_EXPONENT_DISTANCE_CLAMPED. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_device_info
    {
        public IntPtr pName;
        public UInt32 index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_context_config
    {
        public ma_ex_device_info deviceInfo;
        public UInt32 sampleRate;
        public byte channels;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_audio_source_callbacks
    {
        public IntPtr pUserData;
        public ma_sound_end_proc endCallback;
        public ma_sound_load_proc loadCallback;
        public ma_sound_process_proc processCallback;
        public ma_waveform_proc waveformCallback;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_vec3f
    {
        public float x;
        public float y;
        public float z;

        public ma_vec3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }
        }

        /// <summary>
        /// Gets the squared length of the vector.
        /// </summary>
        public float LengthSquared
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }

        /// <summary>
        /// Returns a vector with components set to zero.
        /// </summary>
        public static Vector3f Zero
        {
            get
            {
                return new Vector3f(0, 0, 0);
            }
        }

        /// <summary>
        /// Returns a vector with all components set to one.
        /// </summary>
        public static Vector3f One
        {
            get
            {
                return new Vector3f(1, 1, 1);
            }
        }

        /// <summary>
        /// Gets the unit vector along the X-axis.
        /// </summary>
        public static Vector3f UnitX
        {
            get
            {
                return new Vector3f(1, 0, 0);
            }
        }

        /// <summary>
        /// Gets the unit vector along the Y-axis.
        /// </summary>
        public static Vector3f UnitY
        {
            get
            {
                return new Vector3f(0, 1, 0);
            }
        }

        /// <summary>
        /// Gets the unit vector along the Z-axis.
        /// </summary>
        public static Vector3f UnitZ
        {
            get
            {
                return new Vector3f(0, 0, 1);
            }
        }

        /// <summary>
        /// Constructs a new Vector3f with the specified components.
        /// </summary>
        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        public void Normalize()
        {
            float scale = 1.0f / Length;
            x *= scale;
            y *= scale;
            z *= scale;
        }

        /// <summary>
        /// Returns the normalized vector of the input vector.
        /// </summary>
        public static Vector3f Normalize(Vector3f v)
        {
            float scale = 1.0f / v.Length;
            v.x = v.x *= scale;
            v.y = v.y *= scale;
            v.z = v.z *= scale;
            return v;
        }        

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        public static float Distance(Vector3f a, Vector3f b)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float dz = b.z - a.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        public static float DistanceSquared(Vector3f a, Vector3f b)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        public static float Dot(Vector3f a, Vector3f b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        public static Vector3f Cross(Vector3f a, Vector3f b)
        {
            float x = a.y * b.z - a.z * b.y;
            float y = a.z * b.x - a.x * b.z;
            float z = a.x * b.y - a.y * b.x;
            return new Vector3f(x, y, z);
        }

        /// <summary>
        /// Performs linear interpolation between two vectors.
        /// </summary>
        public static Vector3f Lerp(Vector3f a, Vector3f b, float t)
        {
            float x = a.x + (b.x - a.x) * t;
            float y = a.y + (b.y - a.y) * t;
            float z = a.z + (b.z - a.z) * t;
            return new Vector3f(x, y, z);
        }

        // Helper method for clamping values between min and max.
        private static float Clamp(float n, float min, float max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        /// <summary>
        /// Calculates the angle between two vectors in radians.
        /// </summary>
        public static float Angle(Vector3f a, Vector3f b)
        {
            float temp = Dot(a, b);
            return (float)Math.Acos(Clamp(temp / (a.Length * b.Length), -1.0f, 1.0f));
        }

        public static Vector3f operator +(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3f operator -(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3f operator -(Vector3f a)
        {
            return new Vector3f(-a.x, -a.y, -a.z);
        }

        public static Vector3f operator *(Vector3f a, float scalar)
        {
            return new Vector3f(a.x * scalar, a.y * scalar, a.z * scalar);
        }

        public static Vector3f operator *(float scalar, Vector3f a)
        {
            return a * scalar;
        }

        public static Vector3f operator /(Vector3f a, float scalar)
        {
            a.x /= scalar;
            a.y /= scalar;
            a.z /= scalar;
            return a;
        }

        public static bool operator ==(Vector3f a, Vector3f b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Vector3f a, Vector3f b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3f))
                return false;

            Vector3f other = (Vector3f)obj;
            return this == other;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }

        public override int GetHashCode()
        {
            int hash = 42;
            hash = hash ^ x.GetHashCode();
            hash = hash ^ y.GetHashCode();
            hash = hash ^ z.GetHashCode();
            return hash;
        }
    }

    public enum AttenuationModel
    {
        None,
        Inverse,
        Linear,
        Exponential
    }

    public sealed class DeviceInfo
    {
        private string name;
        private UInt32 index;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public UInt32 Index
        {
            get
            {
                return index;
            }
        }

        public DeviceInfo(IntPtr pName, UInt32 index)
        {
            if(pName != IntPtr.Zero)
                name = Marshal.PtrToStringAnsi(pName);
            else
                name = string.Empty;

            this.index = index;
        }
    }

    public static class Library
    {
        private const string LIB_MINIAUDIO_EX = "miniaudioex";

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_playback_devices_get(out UInt32 count);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_playback_devices_free(IntPtr pDeviceInfo, UInt32 count);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_ex_context_config ma_ex_context_config_init(UInt32 sampleRate, byte channels, ref ma_ex_device_info pDeviceInfo);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_context_init(ref ma_ex_context_config config);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_context_uninit(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_context_set_master_volume(IntPtr context, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_ex_context_get_master_volume(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_audio_source_init(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_uninit(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_callbacks(IntPtr source, ma_ex_audio_source_callbacks callbacks);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_ex_audio_source_play(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_ex_audio_source_play_from_file(IntPtr source, string filePath, UInt32 streamFromDisk);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_ex_audio_source_play_from_memory(IntPtr source, IntPtr data, UInt64 dataSize);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_stop(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_apply_settings(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_volume(IntPtr source, float value);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_ex_audio_source_get_volume(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_pitch(IntPtr source, float value);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_ex_audio_source_get_pitch(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_pcm_position(IntPtr source, UInt64 position);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 ma_ex_audio_source_get_pcm_position(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 ma_ex_audio_source_get_pcm_length(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_loop(IntPtr source, UInt32 loop);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 ma_ex_audio_source_get_loop(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_position(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_get_position(IntPtr source, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_direction(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_get_direction(IntPtr source, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_velocity(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_get_velocity(IntPtr source, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_spatialization(IntPtr source, UInt32 enabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 ma_ex_audio_source_get_spatialization(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_attenuation_model(IntPtr source, ma_attenuation_model model);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_attenuation_model ma_ex_audio_source_get_attenuation_model(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_doppler_factor(IntPtr source, float factor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_ex_audio_source_get_doppler_factor(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_min_distance(IntPtr source, float distance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_ex_audio_source_get_min_distance(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_max_distance(IntPtr source, float distance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_ex_audio_source_get_max_distance(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 ma_ex_audio_source_get_is_playing(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_audio_listener_init(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_uninit(IntPtr listener);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_spatialization(IntPtr listener, UInt32 enabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 ma_ex_audio_listener_get_spatialization(IntPtr listener);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_position(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_get_position(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_direction(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_get_direction(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_velocity(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_get_velocity(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_world_up(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_get_world_up(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_cone(IntPtr listener, float innerAngleInRadians, float outerAngleInRadians, float outerGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_get_cone(IntPtr listener, out float innerAngleInRadians, out float outerAngleInRadians, out float outerGain);
    }
}