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
// Copyright 2023 W.M.R Jap-A-Joe

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

namespace MiniAudioEx
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_device_data_proc(ref ma_device pDevice, IntPtr pOutput, IntPtr pInput, uint frameCount);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_ex_sound_loaded_proc(IntPtr pUserData, IntPtr pSound);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_sound_end_proc(IntPtr pUserData, IntPtr pSound);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_engine_node_dsp_proc(IntPtr pEngineNode, IntPtr pFramesOut, IntPtr pFramesIn, ulong frameCount, int channels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_waveform_custom_proc(IntPtr pWaveform, IntPtr pFramesOut, ulong frameCount, int channels);

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_device
    {
        public IntPtr pContext;
        public ma_device_type type;
        public uint sampleRate;
        public int state;               /* The state of the device is variable and can change at any time on any thread. Must be used atomically. */
        public IntPtr onData;                 /* Set once at initialization time and should not be changed after. */
        public IntPtr onNotification; /* Set once at initialization time and should not be changed after. */
        public IntPtr onStop;                        /* DEPRECATED. Use the notification callback instead. Set once at initialization time and should not be changed after. */
        public IntPtr pUserData;                            /* Application defined data. */
        // More fields here that we don't necessarily need
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_context_config
    {
        public uint sampleRate;
        public uint channels;
        public ma_format format;
        public ma_device_data_proc dataProc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_context
    {
        public IntPtr device;
        public IntPtr engine;
        public uint sampleRate;
        public byte channels;
        public ma_format format;
        public ma_device_data_proc dataProc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_audio_source_callbacks
    {
        public ma_ex_sound_loaded_proc soundLoadedProc;
        public ma_sound_end_proc soundEndedProc;
        public ma_engine_node_dsp_proc dspProc;
        public ma_waveform_custom_proc waveformProc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_audio_source_config
    {
        public IntPtr context;
        public ma_ex_audio_source_callbacks callbacks;
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

    public enum ma_attenuation_model
    {
        ma_attenuation_model_none,          /* No distance attenuation and no spatialization. */
        ma_attenuation_model_inverse,       /* Equivalent to OpenAL's AL_INVERSE_DISTANCE_CLAMPED. */
        ma_attenuation_model_linear,        /* Linear attenuation. Equivalent to OpenAL's AL_LINEAR_DISTANCE_CLAMPED. */
        ma_attenuation_model_exponential    /* Exponential attenuation. Equivalent to OpenAL's AL_EXPONENT_DISTANCE_CLAMPED. */
    }

    public enum ma_device_type
    {
        ma_device_type_playback = 1,
        ma_device_type_capture  = 2,
        ma_device_type_duplex   = ma_device_type_playback | ma_device_type_capture, /* 3 */
        ma_device_type_loopback = 4
    }

    public enum ma_format
    {
        /*
        I like to keep these explicitly defined because they're used as a key into a lookup table. When items are
        added to this, make sure there are no gaps and that they're added to the lookup table in ma_get_bytes_per_sample().
        */
        ma_format_unknown = 0,     /* Mainly used for indicating an error, but also used as the default for the output format for decoders. */
        ma_format_u8      = 1,
        ma_format_s16     = 2,     /* Seems to be the most widely supported format. */
        ma_format_s24     = 3,     /* Tightly packed. 3 bytes per sample. */
        ma_format_s32     = 4,
        ma_format_f32     = 5,
        ma_format_count
    }

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

    public static class MiniAudio
    {
        private const string LIB_MINIAUDIO_EX = "miniaudioex";

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_ex_context_config ma_ex_context_config_init(uint sampleRate, uint channels, ma_format format, ma_device_data_proc dataProc);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_context_init(ref ma_ex_context_config config);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_context_uninit(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_ex_audio_source_config ma_ex_audio_source_config_init(IntPtr context, ma_ex_audio_source_callbacks callbacks);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_audio_source_init(ref ma_ex_audio_source_config config);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_uninit(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_ex_audio_source_play(IntPtr source, string filePath, byte streamFromDisk);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_ex_audio_source_play_from_waveform_proc(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_stop(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_pcm_position(IntPtr source, ulong frameIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_pcm_start_position(IntPtr source, ulong frameIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_ex_audio_source_get_pcm_length(IntPtr source, out ulong length);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_loop(IntPtr source, uint loop);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_volume(IntPtr source, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_pitch(IntPtr source, float pitch);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_position(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_direction(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_velocity(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_spatialization(IntPtr source, uint enabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_attenuation_model(IntPtr source, ma_attenuation_model model);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_doppler_factor(IntPtr source, float factor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_min_distance(IntPtr source, float distance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_set_max_distance(IntPtr source, float distance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ma_ex_audio_source_get_is_playing(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_audio_listener_init(IntPtr context);
        
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_uninit(IntPtr listener);
        
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_spatialization(IntPtr listener, uint enabled);
        
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_position(IntPtr listener, float x, float y, float z);
        
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_direction(IntPtr listener, float x, float y, float z);
        
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_velocity(IntPtr listener, float x, float y, float z);
        
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_world_up(IntPtr listener, float x, float y, float z);
        
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_listener_set_cone(IntPtr listener, float innerAngleInRadians, float outerAngleInRadians, float outerGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_read_pcm_frames(IntPtr pEngine, IntPtr pFramesOut, ulong frameCount, out ulong pFramesRead);
    }
}