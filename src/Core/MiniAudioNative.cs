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
using System.Runtime.InteropServices;

namespace MiniAudioEx.Core
{
    using size_t = UInt64;
    using ma_channel = Byte;
    using ma_bool8 = Byte;
    using ma_bool32 = UInt32;
    using ma_uint16 = UInt16;
    using ma_int32 = UInt32;
    using ma_uint32 = UInt32;
    using ma_int64 = Int64;
    using ma_uint64 = UInt64;
    using ma_handle = IntPtr;
    using ma_vfs_file = IntPtr;

    // [StructLayout(LayoutKind.Sequential)]
    // public unsafe struct ma_bool32
    // {
    //     private int _value;
    //     public ma_bool32(int v) { _value = v; }
    //     public static implicit operator ma_bool32(bool b) => new ma_bool32(b ? 1 : 0);
    //     public static implicit operator bool(ma_bool32 b) => b._value != 0;
    //     public int ToInt32() => _value;
    // }

    // [StructLayout(LayoutKind.Sequential)]
    // public unsafe struct ma_bool8
    // {
    //     private byte _value;
    //     public ma_bool8(byte v) { _value = v; }
    //     public static implicit operator ma_bool8(bool b) => new ma_bool8((byte)(b ? 1 : 0));
    //     public static implicit operator bool(ma_bool8 b) => b._value != 0;
    //     public byte ToByte() => _value;
    // }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_sound_load_proc(IntPtr pUserData, ma_sound_ptr pSound);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_sound_end_proc(IntPtr pUserData, ma_sound_ptr pSound);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_sound_process_proc(IntPtr pUserData, ma_sound_ptr pSound, IntPtr pFramesOut, ma_uint64 frameCount, ma_uint32 channels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_procedural_sound_proc(IntPtr pUserData, IntPtr pFramesOut, ma_uint64 frameCount, ma_uint32 channels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_device_data_proc(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, ma_uint32 frameCount);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_device_notification_proc(ma_device_notification_ptr pNotification);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_stop_proc(ma_device_ptr pDevice);  /* DEPRECATED. Use ma_device_notification_proc instead. */

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ma_bool32 ma_enum_devices_callback_proc(ma_context_ptr pContext, ma_device_type deviceType, ma_device_info pInfo, IntPtr pUserData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_engine_process_proc(IntPtr pUserData, IntPtr pFramesOut, ma_uint64 frameCount);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ma_result ma_decoder_read_proc(ma_decoder_ptr pDecoder, IntPtr pBufferOut, size_t bytesToRead, ref size_t pBytesRead);         /* Returns the number of bytes read. */

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ma_result ma_decoder_seek_proc(ma_decoder_ptr pDecoder, ma_int64 byteOffset, ma_seek_origin origin);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ma_result ma_decoder_tell_proc(ma_decoder_ptr pDecoder, ref ma_int64 pCursor);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ma_data_source_ptr ma_data_source_get_next_proc(ma_data_source_ptr pDataSource);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ma_log_callback_proc(IntPtr pUserData, ma_uint32 level, IntPtr pMessage);

    public enum ma_result
    {
        MA_SUCCESS = 0,
        MA_ERROR = -1,  /* A generic error. */
        MA_INVALID_ARGS = -2,
        MA_INVALID_OPERATION = -3,
        MA_OUT_OF_MEMORY = -4,
        MA_OUT_OF_RANGE = -5,
        MA_ACCESS_DENIED = -6,
        MA_DOES_NOT_EXIST = -7,
        MA_ALREADY_EXISTS = -8,
        MA_TOO_MANY_OPEN_FILES = -9,
        MA_INVALID_FILE = -10,
        MA_TOO_BIG = -11,
        MA_PATH_TOO_LONG = -12,
        MA_NAME_TOO_LONG = -13,
        MA_NOT_DIRECTORY = -14,
        MA_IS_DIRECTORY = -15,
        MA_DIRECTORY_NOT_EMPTY = -16,
        MA_AT_END = -17,
        MA_NO_SPACE = -18,
        MA_BUSY = -19,
        MA_IO_ERROR = -20,
        MA_INTERRUPT = -21,
        MA_UNAVAILABLE = -22,
        MA_ALREADY_IN_USE = -23,
        MA_BAD_ADDRESS = -24,
        MA_BAD_SEEK = -25,
        MA_BAD_PIPE = -26,
        MA_DEADLOCK = -27,
        MA_TOO_MANY_LINKS = -28,
        MA_NOT_IMPLEMENTED = -29,
        MA_NO_MESSAGE = -30,
        MA_BAD_MESSAGE = -31,
        MA_NO_DATA_AVAILABLE = -32,
        MA_INVALID_DATA = -33,
        MA_TIMEOUT = -34,
        MA_NO_NETWORK = -35,
        MA_NOT_UNIQUE = -36,
        MA_NOT_SOCKET = -37,
        MA_NO_ADDRESS = -38,
        MA_BAD_PROTOCOL = -39,
        MA_PROTOCOL_UNAVAILABLE = -40,
        MA_PROTOCOL_NOT_SUPPORTED = -41,
        MA_PROTOCOL_FAMILY_NOT_SUPPORTED = -42,
        MA_ADDRESS_FAMILY_NOT_SUPPORTED = -43,
        MA_SOCKET_NOT_SUPPORTED = -44,
        MA_CONNECTION_RESET = -45,
        MA_ALREADY_CONNECTED = -46,
        MA_NOT_CONNECTED = -47,
        MA_CONNECTION_REFUSED = -48,
        MA_NO_HOST = -49,
        MA_IN_PROGRESS = -50,
        MA_CANCELLED = -51,
        MA_MEMORY_ALREADY_MAPPED = -52,

        /* General non-standard errors. */
        MA_CRC_MISMATCH = -100,

        /* General miniaudio-specific errors. */
        MA_FORMAT_NOT_SUPPORTED = -200,
        MA_DEVICE_TYPE_NOT_SUPPORTED = -201,
        MA_SHARE_MODE_NOT_SUPPORTED = -202,
        MA_NO_BACKEND = -203,
        MA_NO_DEVICE = -204,
        MA_API_NOT_FOUND = -205,
        MA_INVALID_DEVICE_CONFIG = -206,
        MA_LOOP = -207,
        MA_BACKEND_NOT_ENABLED = -208,

        /* State errors. */
        MA_DEVICE_NOT_INITIALIZED = -300,
        MA_DEVICE_ALREADY_INITIALIZED = -301,
        MA_DEVICE_NOT_STARTED = -302,
        MA_DEVICE_NOT_STOPPED = -303,

        /* Operation errors. */
        MA_FAILED_TO_INIT_BACKEND = -400,
        MA_FAILED_TO_OPEN_BACKEND_DEVICE = -401,
        MA_FAILED_TO_START_BACKEND_DEVICE = -402,
        MA_FAILED_TO_STOP_BACKEND_DEVICE = -403
    }

    public enum ma_seek_origin
    {
        ma_seek_origin_start,
        ma_seek_origin_current,
        ma_seek_origin_end  /* Not used by decoders. */
    }

    public enum ma_performance_profile
    {
        ma_performance_profile_low_latency = 0,
        ma_performance_profile_conservative
    }

    public enum ma_channel_mix_mode
    {
        ma_channel_mix_mode_rectangular = 0,   /* Simple averaging based on the plane(s) the channel is sitting on. */
        ma_channel_mix_mode_simple,            /* Drop excess channels; zeroed out extra channels. */
        ma_channel_mix_mode_custom_weights,    /* Use custom weights specified in ma_channel_converter_config. */
        ma_channel_mix_mode_default = ma_channel_mix_mode_rectangular
    }

    public enum ma_wasapi_usage
    {
        ma_wasapi_usage_default = 0,
        ma_wasapi_usage_games,
        ma_wasapi_usage_pro_audio,
    }

    public enum ma_opensl_stream_type
    {
        ma_opensl_stream_type_default = 0,              /* Leaves the stream type unset. */
        ma_opensl_stream_type_voice,                    /* SL_ANDROID_STREAM_VOICE */
        ma_opensl_stream_type_system,                   /* SL_ANDROID_STREAM_SYSTEM */
        ma_opensl_stream_type_ring,                     /* SL_ANDROID_STREAM_RING */
        ma_opensl_stream_type_media,                    /* SL_ANDROID_STREAM_MEDIA */
        ma_opensl_stream_type_alarm,                    /* SL_ANDROID_STREAM_ALARM */
        ma_opensl_stream_type_notification              /* SL_ANDROID_STREAM_NOTIFICATION */
    }

    public enum ma_opensl_recording_preset
    {
        ma_opensl_recording_preset_default = 0,         /* Leaves the input preset unset. */
        ma_opensl_recording_preset_generic,             /* SL_ANDROID_RECORDING_PRESET_GENERIC */
        ma_opensl_recording_preset_camcorder,           /* SL_ANDROID_RECORDING_PRESET_CAMCORDER */
        ma_opensl_recording_preset_voice_recognition,   /* SL_ANDROID_RECORDING_PRESET_VOICE_RECOGNITION */
        ma_opensl_recording_preset_voice_communication, /* SL_ANDROID_RECORDING_PRESET_VOICE_COMMUNICATION */
        ma_opensl_recording_preset_voice_unprocessed    /* SL_ANDROID_RECORDING_PRESET_UNPROCESSED */
    }

    public enum ma_aaudio_usage
    {
        ma_aaudio_usage_default = 0,                    /* Leaves the usage type unset. */
        ma_aaudio_usage_media,                          /* AAUDIO_USAGE_MEDIA */
        ma_aaudio_usage_voice_communication,            /* AAUDIO_USAGE_VOICE_COMMUNICATION */
        ma_aaudio_usage_voice_communication_signalling, /* AAUDIO_USAGE_VOICE_COMMUNICATION_SIGNALLING */
        ma_aaudio_usage_alarm,                          /* AAUDIO_USAGE_ALARM */
        ma_aaudio_usage_notification,                   /* AAUDIO_USAGE_NOTIFICATION */
        ma_aaudio_usage_notification_ringtone,          /* AAUDIO_USAGE_NOTIFICATION_RINGTONE */
        ma_aaudio_usage_notification_event,             /* AAUDIO_USAGE_NOTIFICATION_EVENT */
        ma_aaudio_usage_assistance_accessibility,       /* AAUDIO_USAGE_ASSISTANCE_ACCESSIBILITY */
        ma_aaudio_usage_assistance_navigation_guidance, /* AAUDIO_USAGE_ASSISTANCE_NAVIGATION_GUIDANCE */
        ma_aaudio_usage_assistance_sonification,        /* AAUDIO_USAGE_ASSISTANCE_SONIFICATION */
        ma_aaudio_usage_game,                           /* AAUDIO_USAGE_GAME */
        ma_aaudio_usage_assitant,                       /* AAUDIO_USAGE_ASSISTANT */
        ma_aaudio_usage_emergency,                      /* AAUDIO_SYSTEM_USAGE_EMERGENCY */
        ma_aaudio_usage_safety,                         /* AAUDIO_SYSTEM_USAGE_SAFETY */
        ma_aaudio_usage_vehicle_status,                 /* AAUDIO_SYSTEM_USAGE_VEHICLE_STATUS */
        ma_aaudio_usage_announcement                    /* AAUDIO_SYSTEM_USAGE_ANNOUNCEMENT */
    }

    public enum ma_aaudio_content_type
    {
        ma_aaudio_content_type_default = 0,             /* Leaves the content type unset. */
        ma_aaudio_content_type_speech,                  /* AAUDIO_CONTENT_TYPE_SPEECH */
        ma_aaudio_content_type_music,                   /* AAUDIO_CONTENT_TYPE_MUSIC */
        ma_aaudio_content_type_movie,                   /* AAUDIO_CONTENT_TYPE_MOVIE */
        ma_aaudio_content_type_sonification             /* AAUDIO_CONTENT_TYPE_SONIFICATION */
    }

    public enum ma_aaudio_input_preset
    {
        ma_aaudio_input_preset_default = 0,             /* Leaves the input preset unset. */
        ma_aaudio_input_preset_generic,                 /* AAUDIO_INPUT_PRESET_GENERIC */
        ma_aaudio_input_preset_camcorder,               /* AAUDIO_INPUT_PRESET_CAMCORDER */
        ma_aaudio_input_preset_voice_recognition,       /* AAUDIO_INPUT_PRESET_VOICE_RECOGNITION */
        ma_aaudio_input_preset_voice_communication,     /* AAUDIO_INPUT_PRESET_VOICE_COMMUNICATION */
        ma_aaudio_input_preset_unprocessed,             /* AAUDIO_INPUT_PRESET_UNPROCESSED */
        ma_aaudio_input_preset_voice_performance        /* AAUDIO_INPUT_PRESET_VOICE_PERFORMANCE */
    }

    public enum ma_aaudio_allowed_capture_policy
    {
        ma_aaudio_allow_capture_default = 0,            /* Leaves the allowed capture policy unset. */
        ma_aaudio_allow_capture_by_all,                 /* AAUDIO_ALLOW_CAPTURE_BY_ALL */
        ma_aaudio_allow_capture_by_system,              /* AAUDIO_ALLOW_CAPTURE_BY_SYSTEM */
        ma_aaudio_allow_capture_by_none                 /* AAUDIO_ALLOW_CAPTURE_BY_NONE */
    }

    public enum ma_resample_algorithm
    {
        ma_resample_algorithm_linear = 0,    /* Fastest, lowest quality. Optional low-pass filtering. Default. */
        ma_resample_algorithm_custom,
    }

    public enum ma_share_mode
    {
        ma_share_mode_shared = 0,
        ma_share_mode_exclusive
    }

    public enum ma_attenuation_model
    {
        ma_attenuation_model_none,          /* No distance attenuation and no spatialization. */
        ma_attenuation_model_inverse,       /* Equivalent to OpenAL's AL_INVERSE_DISTANCE_CLAMPED. */
        ma_attenuation_model_linear,        /* Linear attenuation. Equivalent to OpenAL's AL_LINEAR_DISTANCE_CLAMPED. */
        ma_attenuation_model_exponential    /* Exponential attenuation. Equivalent to OpenAL's AL_EXPONENT_DISTANCE_CLAMPED. */
    }

    /* Backend enums must be in priority order. */
    public enum ma_backend
    {
        ma_backend_wasapi,
        ma_backend_dsound,
        ma_backend_winmm,
        ma_backend_coreaudio,
        ma_backend_sndio,
        ma_backend_audio4,
        ma_backend_oss,
        ma_backend_pulseaudio,
        ma_backend_alsa,
        ma_backend_jack,
        ma_backend_aaudio,
        ma_backend_opensl,
        ma_backend_webaudio,
        ma_backend_custom,  /* <-- Custom backend, with callbacks defined by the context config. */
        ma_backend_null     /* <-- Must always be the last item. Lowest priority, and used as the terminator for backend enumeration. */
    }

    public enum ma_format
    {
        /*
        I like to keep these explicitly defined because they're used as a key into a lookup table. When items are
        added to this, make sure there are no gaps and that they're added to the lookup table in ma_get_bytes_per_sample().
        */
        ma_format_unknown = 0,     /* Mainly used for indicating an error, but also used as the default for the output format for decoders. */
        ma_format_u8 = 1,
        ma_format_s16 = 2,     /* Seems to be the most widely supported format. */
        ma_format_s24 = 3,     /* Tightly packed. 3 bytes per sample. */
        ma_format_s32 = 4,
        ma_format_f32 = 5,
        ma_format_count
    }

    public enum ma_pan_mode
    {
        ma_pan_mode_balance = 0,    /* Does not blend one side with the other. Technically just a balance. Compatible with other popular audio engines and therefore the default. */
        ma_pan_mode_pan             /* A true pan. The sound from one side will "move" to the other side and blend with it. */
    }

    public enum ma_positioning
    {
        ma_positioning_absolute,
        ma_positioning_relative
    }

    public enum ma_allocation_type
    {
        ma_allocation_type_async_notification,
        ma_allocation_type_context,
        ma_allocation_type_decoder,
        ma_allocation_type_device,
        ma_allocation_type_device_id,
        ma_allocation_type_device_notification,
        ma_allocation_type_engine,
        ma_allocation_type_fence,
        ma_allocation_type_log,
        ma_allocation_type_node,
        ma_allocation_type_node_graph,
        ma_allocation_type_resource_manager,
        ma_allocation_type_sound,
        ma_allocation_type_sound_group,
        ma_allocation_type_vfs
    }

    public enum ma_device_type
    {
        ma_device_type_playback = 1,
        ma_device_type_capture = 2,
        ma_device_type_duplex = ma_device_type_playback | ma_device_type_capture, /* 3 */
        ma_device_type_loopback = 4
    }

    public enum ma_mono_expansion_mode
    {
        ma_mono_expansion_mode_duplicate = 0,   /* The default. */
        ma_mono_expansion_mode_average,         /* Average the mono channel across all channels. */
        ma_mono_expansion_mode_stereo_only,     /* Duplicate to the left and right channels only and ignore the others. */
        ma_mono_expansion_mode_default = ma_mono_expansion_mode_duplicate
    }

    public enum ma_thread_priority
    {
        ma_thread_priority_idle = -5,
        ma_thread_priority_lowest = -4,
        ma_thread_priority_low = -3,
        ma_thread_priority_normal = -2,
        ma_thread_priority_high = -1,
        ma_thread_priority_highest = 0,
        ma_thread_priority_realtime = 1,
        ma_thread_priority_default = 0
    }

    public enum ma_ios_session_category
    {
        ma_ios_session_category_default = 0,        /* AVAudioSessionCategoryPlayAndRecord. */
        ma_ios_session_category_none,               /* Leave the session category unchanged. */
        ma_ios_session_category_ambient,            /* AVAudioSessionCategoryAmbient */
        ma_ios_session_category_solo_ambient,       /* AVAudioSessionCategorySoloAmbient */
        ma_ios_session_category_playback,           /* AVAudioSessionCategoryPlayback */
        ma_ios_session_category_record,             /* AVAudioSessionCategoryRecord */
        ma_ios_session_category_play_and_record,    /* AVAudioSessionCategoryPlayAndRecord */
        ma_ios_session_category_multi_route         /* AVAudioSessionCategoryMultiRoute */
    }

    public enum ma_dither_mode
    {
        ma_dither_mode_none = 0,
        ma_dither_mode_rectangle,
        ma_dither_mode_triangle
    }

    public enum ma_encoding_format
    {
        ma_encoding_format_unknown = 0,
        ma_encoding_format_wav,
        ma_encoding_format_flac,
        ma_encoding_format_mp3,
        ma_encoding_format_vorbis
    }

    public enum ma_data_converter_execution_path
    {
        ma_data_converter_execution_path_passthrough,       /* No conversion. */
        ma_data_converter_execution_path_format_only,       /* Only format conversion. */
        ma_data_converter_execution_path_channels_only,     /* Only channel conversion. */
        ma_data_converter_execution_path_resample_only,     /* Only resampling. */
        ma_data_converter_execution_path_resample_first,    /* All conversions, but resample as the first step. */
        ma_data_converter_execution_path_channels_first     /* All conversions, but channels as the first step. */
    }

    public enum ma_channel_conversion_path
    {
        ma_channel_conversion_path_unknown,
        ma_channel_conversion_path_passthrough,
        ma_channel_conversion_path_mono_out,    /* Converting to mono. */
        ma_channel_conversion_path_mono_in,     /* Converting from mono. */
        ma_channel_conversion_path_shuffle,     /* Simple shuffle. Will use this when all channels are present in both input and output channel maps, but just in a different order. */
        ma_channel_conversion_path_weights      /* Blended based on weights. */
    }

    public enum ma_device_state
    {
        ma_device_state_uninitialized = 0,
        ma_device_state_stopped       = 1,  /* The device's default state after initialization. */
        ma_device_state_started       = 2,  /* The device is started and is requesting and/or delivering audio data. */
        ma_device_state_starting      = 3,  /* Transitioning from a stopped state to started. */
        ma_device_state_stopping      = 4   /* Transitioning from a started state to stopped. */
    }

    [Flags]
    public enum ma_sound_flags
    {
        MA_SOUND_FLAG_STREAM = 0x00000001,   /* MA_RESOURCE_MANAGER_DATA_SOURCE_FLAG_STREAM */
        MA_SOUND_FLAG_DECODE = 0x00000002,   /* MA_RESOURCE_MANAGER_DATA_SOURCE_FLAG_DECODE */
        MA_SOUND_FLAG_ASYNC = 0x00000004,   /* MA_RESOURCE_MANAGER_DATA_SOURCE_FLAG_ASYNC */
        MA_SOUND_FLAG_WAIT_INIT = 0x00000008,   /* MA_RESOURCE_MANAGER_DATA_SOURCE_FLAG_WAIT_INIT */
        MA_SOUND_FLAG_UNKNOWN_LENGTH = 0x00000010,   /* MA_RESOURCE_MANAGER_DATA_SOURCE_FLAG_UNKNOWN_LENGTH */
        MA_SOUND_FLAG_LOOPING = 0x00000020,   /* MA_RESOURCE_MANAGER_DATA_SOURCE_FLAG_LOOPING */

        /* ma_sound specific flags. */
        MA_SOUND_FLAG_NO_DEFAULT_ATTACHMENT = 0x00001000,   /* Do not attach to the endpoint by default. Useful for when setting up nodes in a complex graph system. */
        MA_SOUND_FLAG_NO_PITCH = 0x00002000,   /* Disable pitch shifting with ma_sound_set_pitch() and ma_sound_group_set_pitch(). This is an optimization. */
        MA_SOUND_FLAG_NO_SPATIALIZATION = 0x00004000    /* Disable spatialization. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_allocation_callbacks
    {
        public IntPtr pUserData;
        public delegate* unmanaged[Cdecl]<size_t, IntPtr, IntPtr> onMalloc;
        public delegate* unmanaged[Cdecl]<IntPtr, size_t, IntPtr, IntPtr> onRealloc;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFree;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device_descriptor
    {
        public ma_device_id_ptr pDeviceID;
        public ma_share_mode shareMode;
        public ma_format format;
        public ma_uint32 channels;
        public ma_uint32 sampleRate;
        private fixed ma_channel channelMap[MiniAudioNative.MA_MAX_CHANNELS];
        public ma_uint32 periodSizeInFrames;
        public ma_uint32 periodSizeInMilliseconds;
        public ma_uint32 periodCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_vec3f
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

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_node_ptr
    {
        public IntPtr pointer;

        public ma_node_ptr()
        {

        }

        public ma_node_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_sound_ptr
    {
        public IntPtr pointer;

        public ma_sound_ptr()
        {

        }

        public ma_sound_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_sound_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_sound);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_sound_group_ptr
    {
        public IntPtr pointer;

        public ma_sound_group_ptr()
        {

        }

        public ma_sound_group_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_sound_group_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_sound_group);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_node_graph_ptr
    {
        public IntPtr pointer;

        public ma_node_graph_ptr()
        {

        }

        public ma_node_graph_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_node_graph_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_node_graph);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_decoder_ptr
    {
        public IntPtr pointer;

        public ma_decoder_ptr()
        {

        }

        public ma_decoder_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_decoder_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_decoder);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }

        public ma_decoder Get()
        {
            if (pointer == IntPtr.Zero)
                return default;

            return Marshal.PtrToStructure<ma_decoder>(pointer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_data_source_ptr
    {
        public IntPtr pointer;

        public ma_data_source_ptr()
        {

        }

        public ma_data_source_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_engine_ptr
    {
        public IntPtr pointer;

        public ma_engine_ptr()
        {

        }

        public ma_engine_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_engine_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_engine);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_context_ptr
    {
        public IntPtr pointer;

        public ma_context_ptr()
        {

        }

        public ma_context_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_context_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_context);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }

        // public ma_context Get()
        // {
        //     retu
        // }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device_ptr
    {
        public IntPtr pointer;

        public ma_device_ptr()
        {

        }

        public ma_device_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_device_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_device);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }

        public ma_device Get()
        {
            if (pointer == IntPtr.Zero)
                return default;
            return Marshal.PtrToStructure<ma_device>(pointer);
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device_id_ptr
    {
        public IntPtr pointer;

        public ma_device_id_ptr()
        {

        }

        public ma_device_id_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_device_id Get()
        {
            if (pointer == IntPtr.Zero)
                return default;
            return Marshal.PtrToStructure<ma_device_id>(pointer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device_notification_ptr
    {
        public IntPtr pointer;

        public ma_device_notification_ptr()
        {

        }

        public ma_device_notification_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_resource_manager_ptr
    {
        public IntPtr pointer;

        public ma_resource_manager_ptr()
        {

        }

        public ma_resource_manager_ptr(IntPtr handle)
        {
            pointer = handle;
        }

        public ma_resource_manager_ptr(bool allocate)
        {
            if (allocate)
                Allocate();
        }

        public bool Allocate()
        {
            pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.ma_allocation_type_resource_manager);
            return pointer != IntPtr.Zero;
        }

        public void Free()
        {
            if (pointer != IntPtr.Zero)
            {
                MiniAudioNative.ma_deallocate_type(pointer);
                pointer = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_log_ptr
    {
        public IntPtr pointer;

        public ma_log_ptr()
        {

        }

        public ma_log_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_vfs_ptr
    {
        public IntPtr pointer;

        public ma_vfs_ptr()
        {

        }

        public ma_vfs_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_fence_ptr
    {
        public IntPtr pointer;

        public ma_fence_ptr()
        {

        }

        public ma_fence_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_async_notification_ptr
    {
        public IntPtr pointer;

        public ma_async_notification_ptr()
        {

        }

        public ma_async_notification_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_resampling_backend_vtable_ptr
    {
        public IntPtr pointer;

        public ma_resampling_backend_vtable_ptr()
        {

        }

        public ma_resampling_backend_vtable_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_lpf1_ptr
    {
        public IntPtr pointer;

        public ma_lpf1_ptr()
        {

        }

        public ma_lpf1_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_lpf2_ptr
    {
        public IntPtr pointer;

        public ma_lpf2_ptr()
        {

        }

        public ma_lpf2_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_biquad_coefficient_ptr
    {
        public IntPtr pointer;

        public ma_biquad_coefficient_ptr()
        {

        }

        public ma_biquad_coefficient_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_stack_ptr
    {
        public IntPtr pointer;

        public ma_stack_ptr()
        {

        }

        public ma_stack_ptr(IntPtr handle)
        {
            pointer = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_engine_config
    {
        public ma_resource_manager_ptr pResourceManager;          /* Can be null in which case a resource manager will be created for you. */
        public ma_context_ptr pContext;
        public ma_device_ptr pDevice;                             /* If set, the caller is responsible for calling ma_engine_data_callback() in the device's data callback. */
        public ma_device_id_ptr pPlaybackDeviceID;                /* The ID of the playback device to use with the default listener. */
        public ma_device_data_proc dataCallback;               /* Can be null. Can be used to provide a custom device data callback. */
        public ma_device_notification_proc notificationCallback;
        public ma_log_ptr pLog;                                   /* When set to NULL, will use the context's log. */
        public ma_uint32 listenerCount;                        /* Must be between 1 and MA_ENGINE_MAX_LISTENERS. */
        public ma_uint32 channels;                             /* The number of channels to use when mixing and spatializing. When set to 0, will use the native channel count of the device. */
        public ma_uint32 sampleRate;                           /* The sample rate. When set to 0 will use the native sample rate of the device. */
        public ma_uint32 periodSizeInFrames;                   /* If set to something other than 0, updates will always be exactly this size. The underlying device may be a different size, but from the perspective of the mixer that won't matter.*/
        public ma_uint32 periodSizeInMilliseconds;             /* Used if periodSizeInFrames is unset. */
        public ma_uint32 gainSmoothTimeInFrames;               /* The number of frames to interpolate the gain of spatialized sounds across. If set to 0, will use gainSmoothTimeInMilliseconds. */
        public ma_uint32 gainSmoothTimeInMilliseconds;         /* When set to 0, gainSmoothTimeInFrames will be used. If both are set to 0, a default value will be used. */
        public ma_uint32 defaultVolumeSmoothTimeInPCMFrames;   /* Defaults to 0. Controls the default amount of smoothing to apply to volume changes to sounds. High values means more smoothing at the expense of high latency (will take longer to reach the new volume). */
        public ma_uint32 preMixStackSizeInBytes;               /* A stack is used for internal processing in the node graph. This allows you to configure the size of this stack. Smaller values will reduce the maximum depth of your node graph. You should rarely need to modify this. */
        public ma_allocation_callbacks allocationCallbacks;
        public ma_bool32 noAutoStart;                          /* When set to true, requires an explicit call to ma_engine_start(). This is false by default, meaning the engine will be started automatically in ma_engine_init(). */
        public ma_bool32 noDevice;                             /* When set to true, don't create a default device. ma_engine_read_pcm_frames() can be called manually to read data. */
        public ma_mono_expansion_mode monoExpansionMode;       /* Controls how the mono channel should be expanded to other channels when spatialization is disabled on a sound. */
        public ma_vfs_ptr pResourceManagerVFS;                    /* A pointer to a pre-allocated VFS object to use with the resource manager. This is ignored if pResourceManager is not NULL. */
        public ma_engine_process_proc onProcess;               /* Fired at the end of each call to ma_engine_read_pcm_frames(). For engine's that manage their own internal device (the default configuration), this will be fired from the audio thread, and you do not need to call ma_engine_read_pcm_frames() manually in order to trigger this. */
        public IntPtr pProcessUserData;                         /* User data that's passed into onProcess. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_procedural_sound_config
    {
        public ma_format format;
        public ma_uint32 channels;
        public ma_uint32 sampleRate;
        public ma_procedural_sound_proc callback;
        public IntPtr pUserData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_log_callback
    {
        ma_log_callback_proc onLog;
        IntPtr pUserData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_context_config
    {
        public ma_log_ptr pLog;
        public ma_thread_priority threadPriority;
        public size_t threadStackSize;
        public IntPtr pUserData;
        public ma_allocation_callbacks allocationCallbacks;
        public dsound_info dsound;
        public alsa_info alsa;
        public pulse_info pulse;
        public coreaudio_info coreaudio;
        public jack_info jack;
        public ma_backend_callbacks custom;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct dsound_info
        {
            public ma_handle hWnd; /* HWND. Optional window handle to pass into SetCooperativeLevel(). Will default to the foreground window, and if that fails, the desktop window. */
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct alsa_info
        {
            public ma_bool32 useVerboseDeviceEnumeration;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct pulse_info
        {
            public IntPtr pApplicationName;
            public IntPtr pServerName;
            public ma_bool32 tryAutoSpawn; /* Enables autospawning of the PulseAudio daemon if necessary. */
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct coreaudio_info
        {
            public ma_ios_session_category sessionCategory;
            public ma_uint32 sessionCategoryOptions;
            public ma_bool32 noAudioSessionActivate;   /* iOS only. When set to true, does not perform an explicit [[AVAudioSession sharedInstace] setActive:true] on initialization. */
            public ma_bool32 noAudioSessionDeactivate; /* iOS only. When set to true, does not perform an explicit [[AVAudioSession sharedInstace] setActive:false] on uninitialization. */
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct jack_info
        {
            public IntPtr pClientName;
            public ma_bool32 tryStartServer;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_resource_manager_pipeline_stage_notification
    {
        public ma_async_notification_ptr pNotification;
        public ma_fence_ptr pFence;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_resource_manager_pipeline_notifications
    {
        public ma_resource_manager_pipeline_stage_notification init;    /* Initialization of the decoder. */
        public ma_resource_manager_pipeline_stage_notification done;    /* Decoding fully completed. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_backend_callbacks
    {
        public delegate* unmanaged[Cdecl]<ma_context_ptr, ref ma_context_config, ref ma_backend_callbacks, ma_result> onContextInit;
        public delegate* unmanaged[Cdecl]<ma_context_ptr, ma_result> onContextUninit;
        public delegate* unmanaged[Cdecl]<ma_context_ptr, ma_enum_devices_callback_proc, IntPtr, ma_result> onContextEnumerateDevices;
        public delegate* unmanaged[Cdecl]<ma_context_ptr, ma_device_type, ma_device_id_ptr, ma_device_info, ma_result> onContextGetDeviceInfo;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, ref ma_device_config, ref ma_device_descriptor, ref ma_device_descriptor, ma_result> onDeviceInit;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, ma_result> onDeviceUninit;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, ma_result> onDeviceStart;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, ma_result> onDeviceStop;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, IntPtr, ma_uint32, ref ma_uint32, ma_result> onDeviceRead;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, IntPtr, ma_uint32, ref ma_uint32, ma_result> onDeviceWrite;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, ma_result> onDeviceDataLoop;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, ma_result> onDeviceDataLoopWakeup;
        public delegate* unmanaged[Cdecl]<ma_device_ptr, ma_device_type, ma_device_info, ma_result> onDeviceGetInfo;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_sound_config
    {
        public IntPtr pFilePath;                      /* Set this to load from the resource manager. */
        public IntPtr pFilePathW;                  /* Set this to load from the resource manager. */
        public ma_data_source_ptr pDataSource;                /* Set this to load from an existing data source. */
        public ma_node_ptr pInitialAttachment;                /* If set, the sound will be attached to an input of this node. This can be set to a ma_sound. If set to NULL, the sound will be attached directly to the endpoint unless MA_SOUND_FLAG_NO_DEFAULT_ATTACHMENT is set in `flags`. */
        public ma_uint32 initialAttachmentInputBusIndex;   /* The index of the input bus of pInitialAttachment to attach the sound to. */
        public ma_uint32 channelsIn;                       /* Ignored if using a data source as input (the data source's channel count will be used always). Otherwise, setting to 0 will cause the engine's channel count to be used. */
        public ma_uint32 channelsOut;                      /* Set this to 0 (default) to use the engine's channel count. Set to MA_SOUND_SOURCE_CHANNEL_COUNT to use the data source's channel count (only used if using a data source as input). */
        public ma_mono_expansion_mode monoExpansionMode;   /* Controls how the mono channel should be expanded to other channels when spatialization is disabled on a sound. */
        public ma_uint32 flags;                            /* A combination of MA_SOUND_FLAG_* flags. */
        public ma_uint32 volumeSmoothTimeInPCMFrames;      /* The number of frames to smooth over volume changes. Defaults to 0 in which case no smoothing is used. */
        public ma_uint64 initialSeekPointInPCMFrames;      /* Initializes the sound such that it's seeked to this location by default. */
        public ma_uint64 rangeBegInPCMFrames;
        public ma_uint64 rangeEndInPCMFrames;
        public ma_uint64 loopPointBegInPCMFrames;
        public ma_uint64 loopPointEndInPCMFrames;
        public ma_sound_notifications notifications;
        public ma_resource_manager_pipeline_notifications initNotifications;
        public ma_fence_ptr pDoneFence;                       /* Deprecated. Use initNotifications instead. Released when the resource manager has finished decoding the entire sound. Not used with streams. */
        public ma_bool32 isLooping;                        /* Deprecated. Use the MA_SOUND_FLAG_LOOPING flag in `flags` instead. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_sound_group_config
    {
        public IntPtr pFilePath;                      /* Set this to load from the resource manager. */
        public IntPtr pFilePathW;                  /* Set this to load from the resource manager. */
        public ma_data_source_ptr pDataSource;                /* Set this to load from an existing data source. */
        public ma_node_ptr pInitialAttachment;                /* If set, the sound will be attached to an input of this node. This can be set to a ma_sound. If set to NULL, the sound will be attached directly to the endpoint unless MA_SOUND_FLAG_NO_DEFAULT_ATTACHMENT is set in `flags`. */
        public ma_uint32 initialAttachmentInputBusIndex;   /* The index of the input bus of pInitialAttachment to attach the sound to. */
        public ma_uint32 channelsIn;                       /* Ignored if using a data source as input (the data source's channel count will be used always). Otherwise, setting to 0 will cause the engine's channel count to be used. */
        public ma_uint32 channelsOut;                      /* Set this to 0 (default) to use the engine's channel count. Set to MA_SOUND_SOURCE_CHANNEL_COUNT to use the data source's channel count (only used if using a data source as input). */
        public ma_mono_expansion_mode monoExpansionMode;   /* Controls how the mono channel should be expanded to other channels when spatialization is disabled on a sound. */
        public ma_uint32 flags;                            /* A combination of MA_SOUND_FLAG_* flags. */
        public ma_uint32 volumeSmoothTimeInPCMFrames;      /* The number of frames to smooth over volume changes. Defaults to 0 in which case no smoothing is used. */
        public ma_uint64 initialSeekPointInPCMFrames;      /* Initializes the sound such that it's seeked to this location by default. */
        public ma_uint64 rangeBegInPCMFrames;
        public ma_uint64 rangeEndInPCMFrames;
        public ma_uint64 loopPointBegInPCMFrames;
        public ma_uint64 loopPointEndInPCMFrames;
        public ma_sound_notifications notifications;
        public ma_resource_manager_pipeline_notifications initNotifications;
        public ma_fence_ptr pDoneFence;                       /* Deprecated. Use initNotifications instead. Released when the resource manager has finished decoding the entire sound. Not used with streams. */
        public ma_bool32 isLooping;                        /* Deprecated. Use the MA_SOUND_FLAG_LOOPING flag in `flags` instead. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_sound_notifications
    {
        public ma_sound_end_proc onLoaded;  /* Fired by the resource manager when the sound has finished loading. */
        public ma_sound_end_proc onAtEnd;  /* Fired when the sound reaches the end of the data source. */
        public ma_sound_process_proc onProcess;
        public IntPtr pUserData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_native_data_format
    {
        public ma_uint32 format; // Assuming ma_format is a uint. Adjust as necessary.
        public ma_uint32 channels; // If set to 0, all channels are supported.
        public ma_uint32 sampleRate; // If set to 0, all sample rates are supported.
        public ma_uint32 flags; // A combination of MA_DATA_FORMAT_FLAG_* flags.
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device_info
    {
        /* Basic info. This is the only information guaranteed to be filled in during device enumeration. */
        public ma_device_id id;
        public fixed byte name[MiniAudioNative.MA_MAX_DEVICE_NAME_LENGTH + 1];
        public ma_bool32 isDefault;
        public ma_uint32 nativeDataFormatCount;
        private fixed byte nativeDataFormats[4 * 4 * 64];

        public string GetName()
        {
            unsafe
            {
                fixed (byte* pName = name)
                {
                    // Find length up to null terminator
                    int len = 0;
                    while (len < MiniAudioNative.MA_MAX_DEVICE_NAME_LENGTH && pName[len] != 0) len++;

                    if (len == 0) return string.Empty;

                    // Assume UTF-8 encoding for device names
                    return System.Text.Encoding.UTF8.GetString(pName, len);
                }
            }
        }

        public ma_native_data_format[] GetNativeDataFormats()
        {
            if (nativeDataFormatCount == 0)
                return Array.Empty<ma_native_data_format>();
            if (nativeDataFormatCount > 64)
                nativeDataFormatCount = 64;

            var result = new ma_native_data_format[nativeDataFormatCount];

            unsafe
            {
                fixed (byte* pFormats = nativeDataFormats)
                {
                    byte* ptr = pFormats;
                    for (int i = 0; i < result.Length; i++, ptr += 16)
                    {
                        result[i].format = *(uint*)(ptr + 0);
                        result[i].channels = *(uint*)(ptr + 4);
                        result[i].sampleRate = *(uint*)(ptr + 8);
                        result[i].flags = *(uint*)(ptr + 12);
                    }
                }
            }

            return result;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device_info_ex
    {
        public ma_device_info deviceInfo;
        public ma_device_id_ptr pDeviceId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_resampler_config
    {
        public ma_format format;   /* Must be either ma_format_f32 or ma_format_s16. */
        public ma_uint32 channels;
        public ma_uint32 sampleRateIn;
        public ma_uint32 sampleRateOut;
        public ma_resample_algorithm algorithm;    /* When set to ma_resample_algorithm_custom, pBackendVTable will be used. */
        public ma_resampling_backend_vtable_ptr pBackendVTable;
        public IntPtr pBackendUserData;
        public linear_info linear;
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct linear_info
        {
            public ma_uint32 lpfOrder;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device_config
    {
        public ma_device_type deviceType;
        public ma_uint32 sampleRate;
        public ma_uint32 periodSizeInFrames;
        public ma_uint32 periodSizeInMilliseconds;
        public ma_uint32 periods;
        public ma_performance_profile performanceProfile;
        public ma_bool8 noPreSilencedOutputBuffer; /* When set to true, the contents of the output buffer passed into the data callback will be left undefined rather than initialized to silence. */
        public ma_bool8 noClip;                    /* When set to true, the contents of the output buffer passed into the data callback will not be clipped after returning. Only applies when the playback sample format is f32. */
        public ma_bool8 noDisableDenormals;        /* Do not disable denormals when firing the data callback. */
        public ma_bool8 noFixedSizedCallback;      /* Disables strict fixed-sized data callbacks. Setting this to true will result in the period size being treated only as a hint to the backend. This is an optimization for those who don't need fixed sized callbacks. */
        public IntPtr dataCallback;
        public IntPtr notificationCallback;
        public IntPtr stopCallback;
        public IntPtr pUserData;
        public ma_resampler_config resampling;
        public playback_info playback;
        public capture_info capture;
        public wasapi_info wasapi;
        public alsa_info alsa;
        public pulse_info pulse;
        public coreaudio_info coreaudio;
        public opensl_info opensl;
        public aaudio_info aaudio;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct playback_info
        {
            public ma_device_id_ptr pDeviceID;
            public ma_format format;
            public ma_uint32 channels;
            public IntPtr pChannelMap;
            public ma_channel_mix_mode channelMixMode;
            public ma_bool32 calculateLFEFromSpatialChannels;  /* When an output LFE channel is present, but no input LFE, set to true to set the output LFE to the average of all spatial channels (LR, FR, etc.). Ignored when an input LFE is present. */
            public ma_share_mode shareMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct capture_info
        {
            public ma_device_id_ptr pDeviceID;
            public ma_format format;
            public ma_uint32 channels;
            public IntPtr pChannelMap;
            public ma_channel_mix_mode channelMixMode;
            public ma_bool32 calculateLFEFromSpatialChannels;  /* When an output LFE channel is present, but no input LFE, set to true to set the output LFE to the average of all spatial channels (LR, FR, etc.). Ignored when an input LFE is present. */
            public ma_share_mode shareMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct wasapi_info
        {
            public ma_wasapi_usage usage;              /* When configured, uses Avrt APIs to set the thread characteristics. */
            public ma_bool8 noAutoConvertSRC;          /* When set to true, disables the use of AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM. */
            public ma_bool8 noDefaultQualitySRC;       /* When set to true, disables the use of AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY. */
            public ma_bool8 noAutoStreamRouting;       /* Disables automatic stream routing. */
            public ma_bool8 noHardwareOffloading;      /* Disables WASAPI's hardware offloading feature. */
            public ma_uint32 loopbackProcessID;        /* The process ID to include or exclude for loopback mode. Set to 0 to capture audio from all processes. Ignored when an explicit device ID is specified. */
            public ma_bool8 loopbackProcessExclude;    /* When set to true, excludes the process specified by loopbackProcessID. By default, the process will be included. */
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct alsa_info
        {
            public ma_bool32 noMMap;           /* Disables MMap mode. */
            public ma_bool32 noAutoFormat;     /* Opens the ALSA device with SND_PCM_NO_AUTO_FORMAT. */
            public ma_bool32 noAutoChannels;   /* Opens the ALSA device with SND_PCM_NO_AUTO_CHANNELS. */
            public ma_bool32 noAutoResample;   /* Opens the ALSA device with SND_PCM_NO_AUTO_RESAMPLE. */
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct pulse_info
        {
            public IntPtr pStreamNamePlayback;
            public IntPtr pStreamNameCapture;
            public int channelMap;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct coreaudio_info
        {
            public ma_bool32 allowNominalSampleRateChange; /* Desktop only. When enabled, allows changing of the sample rate at the operating system level. */
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct opensl_info
        {
            public ma_opensl_stream_type streamType;
            public ma_opensl_recording_preset recordingPreset;
            public ma_bool32 enableCompatibilityWorkarounds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct aaudio_info
        {
            public ma_aaudio_usage usage;
            public ma_aaudio_content_type contentType;
            public ma_aaudio_input_preset inputPreset;
            public ma_aaudio_allowed_capture_policy allowedCapturePolicy;
            public ma_bool32 noAutoStartAfterReroute;
            public ma_bool32 enableCompatibilityWorkarounds;
            public ma_bool32 allowSetBufferCapacity;
        }

        public void SetDataCallback(ma_device_data_proc dataCallback)
        {
            this.dataCallback = Marshal.GetFunctionPointerForDelegate(dataCallback);
        }

        public void SetNotificationCallback(ma_device_notification_proc notificationCallback)
        {
            this.notificationCallback = Marshal.GetFunctionPointerForDelegate(notificationCallback);
        }

        public void SetStopCallback(ma_stop_proc stopCallback)
        {
            this.stopCallback = Marshal.GetFunctionPointerForDelegate(stopCallback);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 256)] // largest member size determines union size
    public unsafe struct ma_device_id
    {
        [FieldOffset(0)]
        public fixed ma_uint16 wasapi[64];
        [FieldOffset(0)]
        public fixed byte dsound[16];
        [FieldOffset(0)]
        public ma_uint32 winmm;
        [FieldOffset(0)]
        public fixed byte alsa[256];
        [FieldOffset(0)]
        public fixed byte pulse[256];
        [FieldOffset(0)]
        public int jack;
        [FieldOffset(0)]
        public fixed byte coreaudio[256];
        [FieldOffset(0)]
        public fixed byte sndio[256];
        [FieldOffset(0)]
        public fixed byte audio4[256];
        [FieldOffset(0)]
        public fixed byte oss[64];
        [FieldOffset(0)]
        public int aaudio;
        [FieldOffset(0)]
        public uint opensl;
        [FieldOffset(0)]
        public fixed byte webaudio[32];
        [FieldOffset(0)]
        public int custom_i;
        [FieldOffset(0)]
        public fixed byte custom_s[256];
        [FieldOffset(0)]
        public IntPtr custom_p;
        [FieldOffset(0)]
        public int nullbackend;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_device
    {
        public ma_context_ptr pContext;
        public ma_device_type type;
        public ma_uint32 sampleRate;
        public ma_device_state state;                      /* The state of the device is variable and can change at any time on any thread. Must be used atomically. */
        public IntPtr onData;                 /* Set once at initialization time and should not be changed after. */
        public IntPtr onNotification; /* Set once at initialization time and should not be changed after. */
        public IntPtr onStop;                        /* DEPRECATED. Use the notification callback instead. Set once at initialization time and should not be changed after. */
        public IntPtr pUserData;                            /* Application defined data. */
        //There are a lot more fields down here but they are not needed as long other ma_types only use a pointer to ma_device

        public void SetDataProc(ma_device_data_proc onData)
        {
            this.onData = Marshal.GetFunctionPointerForDelegate(onData);
        }

        public void SetNotificationProc(ma_device_notification_proc onNotification)
        {
            this.onNotification = Marshal.GetFunctionPointerForDelegate(onNotification);
        }

        public void SetStopProc(ma_stop_proc onStop)
        {
            this.onStop = Marshal.GetFunctionPointerForDelegate(onStop);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_decoder_config
    {
        public ma_format format;      /* Set to 0 or ma_format_unknown to use the stream's internal format. */
        public ma_uint32 channels;    /* Set to 0 to use the stream's internal channels. */
        public ma_uint32 sampleRate;  /* Set to 0 to use the stream's internal sample rate. */
        public IntPtr pChannelMap;
        public ma_channel_mix_mode channelMixMode;
        public ma_dither_mode ditherMode;
        public ma_resampler_config resampling;
        public ma_allocation_callbacks allocationCallbacks;
        public ma_encoding_format encodingFormat;
        public ma_uint32 seekPointCount;   /* When set to > 0, specifies the number of seek points to use for the generation of a seek table. Not all decoding backends support this. */
        public IntPtr ppCustomBackendVTables;
        public ma_uint32 customBackendCount;
        public IntPtr pCustomBackendUserData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_decoder
    {
        public ma_data_source_base ds;
        public ma_data_source_ptr pBackend;                   /* The decoding backend we'll be pulling data from. */
        public IntPtr pBackendVTable; /* The vtable for the decoding backend. This needs to be stored so we can access the onUninit() callback. */
        public IntPtr pBackendUserData;
        public ma_decoder_read_proc onRead;
        public ma_decoder_seek_proc onSeek;
        public ma_decoder_tell_proc onTell;
        public IntPtr pUserData;
        public ma_uint64 readPointerInPCMFrames;      /* In output sample rate. Used for keeping track of how many frames are available for decoding. */
        public ma_format outputFormat;
        public ma_uint32 outputChannels;
        public ma_uint32 outputSampleRate;
        public ma_data_converter converter;    /* Data conversion is achieved by running frames through this. */
        public IntPtr pInputCache;              /* In input format. Can be null if it's not needed. */
        public ma_uint64 inputCacheCap;        /* The capacity of the input cache. */
        public ma_uint64 inputCacheConsumed;   /* The number of frames that have been consumed in the cache. Used for determining the next valid frame. */
        public ma_uint64 inputCacheRemaining;  /* The number of valid frames remaining in the cache. */
        public ma_allocation_callbacks allocationCallbacks;
        public ma_decoder_data_union data;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ma_decoder_data_vfs
        {
            public ma_vfs_ptr pVFS;
            public ma_vfs_file file;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ma_decoder_data_memory
        {
            public IntPtr pData; // const ma_uint8*
            public size_t dataSize;
            public size_t currentReadPos;
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct ma_decoder_data_union
        {
            [FieldOffset(0)]
            public ma_decoder_data_vfs vfs;

            [FieldOffset(0)]
            public ma_decoder_data_memory memory;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ATOMIC_MA_BOOL32
    {
        private int _value;

        public ATOMIC_MA_BOOL32(int v) { _value = v; }
        public static implicit operator ATOMIC_MA_BOOL32(bool b) => new ATOMIC_MA_BOOL32(b ? 1 : 0);
        public static implicit operator bool(ATOMIC_MA_BOOL32 b) => b._value != 0;
        public int ToInt32() => _value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_data_source_base
    {
        public IntPtr vtable;
        public ma_uint64 rangeBegInFrames;
        public ma_uint64 rangeEndInFrames;             /* Set to -1 for unranged (default). */
        public ma_uint64 loopBegInFrames;              /* Relative to rangeBegInFrames. */
        public ma_uint64 loopEndInFrames;              /* Relative to rangeBegInFrames. Set to -1 for the end of the range. */
        public ma_data_source_ptr pCurrent;               /* When non-NULL, the data source being initialized will act as a proxy and will route all operations to pCurrent. Used in conjunction with pNext/onGetNext for seamless chaining. */
        public ma_data_source_ptr pNext;                  /* When set to NULL, onGetNext will be used. */
        public ma_data_source_get_next_proc onGetNext; /* Will be used when pNext is NULL. If both are NULL, no next will be used. */
        public ATOMIC_MA_BOOL32 isLooping;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_channel_converter
    {
        public ma_format format;
        public ma_uint32 channelsIn;
        public ma_uint32 channelsOut;
        public ma_channel_mix_mode mixingMode;
        public ma_channel_conversion_path conversionPath;
        public IntPtr pChannelMapIn;
        public IntPtr pChannelMapOut;
        public IntPtr pShuffleTable;    /* Indexed by output channel index. */
        public ma_channel_converter_weights weights;  /* [in][out] */
        /* Memory management. */
        public IntPtr _pHeap;
        public ma_bool32 _ownsHeap;
        
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct ma_channel_converter_weights
        {
            [FieldOffset(0)]
            public IntPtr f32;  // float**
            [FieldOffset(0)]
            public IntPtr s16;  // ma_int32**
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ma_biquad_coefficient
    {
        [FieldOffset(0)]
        public float f32;
        [FieldOffset(0)]
        public ma_int32 s32;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_biquad_config
    {
        public ma_format format;
        public ma_uint32 channels;
        public double b0;
        public double b1;
        public double b2;
        public double a0;
        public double a1;
        public double a2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_biquad
    {
        public ma_format format;
        public ma_uint32 channels;
        public ma_biquad_coefficient b0;
        public ma_biquad_coefficient b1;
        public ma_biquad_coefficient b2;
        public ma_biquad_coefficient a1;
        public ma_biquad_coefficient a2;
        public ma_biquad_coefficient_ptr pR1;
        public ma_biquad_coefficient_ptr pR2;
        /* Memory management. */
        public IntPtr _pHeap;
        public ma_bool32 _ownsHeap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_lpf1_config
    {
        public ma_format format;
        public ma_uint32 channels;
        public ma_uint32 sampleRate;
        public double cutoffFrequency;
        public double q;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_lpf2_config
    {
        public ma_format format;
        public ma_uint32 channels;
        public ma_uint32 sampleRate;
        public double cutoffFrequency;
        public double q;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_lpf1
    {
        public ma_format format;
        public ma_uint32 channels;
        public ma_biquad_coefficient a;
        public ma_biquad_coefficient_ptr pR1;
        /* Memory management. */
        public IntPtr _pHeap;
        public ma_bool32 _ownsHeap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_lpf2
    {
        public ma_biquad bq;   /* The second order low-pass filter is implemented as a biquad filter. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_lpf
    {
        public ma_format format;
        public ma_uint32 channels;
        public ma_uint32 sampleRate;
        public ma_uint32 lpf1Count;
        public ma_uint32 lpf2Count;
        public ma_lpf1_ptr pLPF1;
        public ma_lpf2_ptr pLPF2;
        /* Memory management. */
        public IntPtr _pHeap;
        public ma_bool32 _ownsHeap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_linear_resampler_config
    {
        public ma_format format;
        public ma_uint32 channels;
        public ma_uint32 sampleRateIn;
        public ma_uint32 sampleRateOut;
        public ma_uint32 lpfOrder;         /* The low-pass filter order. Setting this to 0 will disable low-pass filtering. */
        public double    lpfNyquistFactor; /* 0..1. Defaults to 1. 1 = Half the sampling frequency (Nyquist Frequency), 0.5 = Quarter the sampling frequency (half Nyquest Frequency), etc. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_linear_resampler
    {
        public ma_linear_resampler_config config;
        public ma_uint32 inAdvanceInt;
        public ma_uint32 inAdvanceFrac;
        public ma_uint32 inTimeInt;
        public ma_uint32 inTimeFrac;
        public ma_linear_resampler_data x0; /* The previous input frame. */
        public ma_linear_resampler_data x1; /* The next input frame. */
        public ma_lpf lpf;
        /* Memory management. */
        public IntPtr _pHeap;
        public ma_bool32 _ownsHeap;

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct ma_linear_resampler_data
        {
            [FieldOffset(0)]
            public IntPtr f32;
            [FieldOffset(0)]
            public IntPtr s16;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_resampler
    {
        public IntPtr pBackend;
        public IntPtr pBackendVTable;
        public IntPtr pBackendUserData;
        public ma_format format;
        public ma_uint32 channels;
        public ma_uint32 sampleRateIn;
        public ma_uint32 sampleRateOut;
        public ma_resampler_state state;    /* State for stock resamplers so we can avoid a malloc. For stock resamplers, pBackend will point here. */
        /* Memory management. */
        public IntPtr _pHeap;
        public ma_bool32 _ownsHeap;

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct ma_resampler_state
        {
            [FieldOffset(0)]
            public ma_linear_resampler linear;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_data_converter
    {
        public ma_format formatIn;
        public ma_format formatOut;
        public ma_uint32 channelsIn;
        public ma_uint32 channelsOut;
        public ma_uint32 sampleRateIn;
        public ma_uint32 sampleRateOut;
        public ma_dither_mode ditherMode;
        public ma_data_converter_execution_path executionPath; /* The execution path the data converter will follow when processing. */
        public ma_channel_converter channelConverter;
        public ma_resampler resampler;
        public ma_bool8 hasPreFormatConversion;
        public ma_bool8 hasPostFormatConversion;
        public ma_bool8 hasChannelConverter;
        public ma_bool8 hasResampler;
        public ma_bool8 isPassthrough;
        /* Memory management. */
        public ma_bool8 _ownsHeap;
        public IntPtr _pHeap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_resource_manager_config
    {
        public ma_allocation_callbacks allocationCallbacks;
        public ma_log_ptr pLog;
        public ma_format decodedFormat;        /* The decoded format to use. Set to ma_format_unknown (default) to use the file's native format. */
        public ma_uint32 decodedChannels;      /* The decoded channel count to use. Set to 0 (default) to use the file's native channel count. */
        public ma_uint32 decodedSampleRate;    /* the decoded sample rate to use. Set to 0 (default) to use the file's native sample rate. */
        public ma_uint32 jobThreadCount;       /* Set to 0 if you want to self-manage your job threads. Defaults to 1. */
        public size_t jobThreadStackSize;
        public ma_uint32 jobQueueCapacity;     /* The maximum number of jobs that can fit in the queue at a time. Defaults to MA_JOB_TYPE_RESOURCE_MANAGER_QUEUE_CAPACITY. Cannot be zero. */
        public ma_uint32 flags;
        public ma_vfs_ptr pVFS;                   /* Can be NULL in which case defaults will be used. */
        public IntPtr ppCustomDecodingBackendVTables;
        public ma_uint32 customDecodingBackendCount;
        public IntPtr pCustomDecodingBackendUserData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ma_decoding_backend_vtable
    {
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, ma_result> onInit;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, ma_result> onInitFile;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, ma_result> onInitFileW;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, size_t, IntPtr, IntPtr, IntPtr, ma_result> onInitMemory;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> onUninit;
    }

    public static class MiniAudioNative
    {
        public const int MA_MAX_CHANNELS = 254;
        public const int MA_MAX_DEVICE_NAME_LENGTH = 255;
        public const int MA_MAX_LOG_CALLBACKS = 4;
        public const int MA_ENGINE_MAX_LISTENERS = 4;

        private const string LIB_MINIAUDIO_EX = "miniaudioex";

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_allocate_type(ma_allocation_type type);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_deallocate_type(IntPtr pData);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint64 ma_get_size_of_type(ma_allocation_type type);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_engine_config ma_engine_config_init();

        // ma_device
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_device_config ma_device_config_init(ma_device_type deviceType);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_device_init(ma_context_ptr pContext, ref ma_device_config pConfig, ma_device_ptr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_device_uninit(ma_device_ptr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_context_ptr ma_device_get_context(ma_device_ptr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_device_start(ma_device_ptr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_device_stop(ma_device_ptr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_bool32 ma_device_is_started(ma_device_ptr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_device_state ma_device_get_state(ma_device_ptr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_device_set_master_volume(ma_device_ptr pDevice, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_device_get_master_volume(ma_device_ptr pDevice, out float pVolume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_device_set_master_volume_db(ma_device_ptr pDevice, float gainDB);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_device_get_master_volume_db(ma_device_ptr pDevice, out float pGainDB);

        // ma_context
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_context_config ma_context_config_init();

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe ma_result ma_context_init(ma_backend* backends, ma_uint32 backendCount, ma_context_config* pConfig, ma_context_ptr pContext);

        public static unsafe ma_result ma_context_init(ma_backend[] backends, ref ma_context_config config, ma_context_ptr pContext)
        {
            fixed (ma_context_config* pConfig = &config)
            {
                if (backends?.Length > 0)
                {
                    fixed (ma_backend* pBackends = &backends[0])
                    {
                        return ma_context_init(pBackends, (UInt32)backends.Length, pConfig, pContext);
                    }
                }
                else
                {
                    return ma_context_init(null, 0, pConfig, pContext);
                }
            }
        }

        public static unsafe ma_result ma_context_init(ma_backend[] backends, ma_context_ptr pContext)
        {
            if (backends?.Length > 0)
            {
                fixed (ma_backend* pBackends = &backends[0])
                {
                    return ma_context_init(pBackends, (UInt32)backends.Length, null, pContext);
                }
            }
            else
            {
                return ma_context_init(null, 0, null, pContext);
            }
        }

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_context_uninit(ma_context_ptr pContext);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern size_t ma_context_sizeof();

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_log_ptr ma_context_get_log(ma_context_ptr pContext);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_context_enumerate_devices(ma_context_ptr pContext, ma_enum_devices_callback_proc callback, IntPtr pUserData);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe ma_result ma_context_get_devices(ma_context_ptr pContext, ma_device_info** ppPlaybackDeviceInfos, ma_uint32* pPlaybackDeviceCount, ma_device_info** ppCaptureDeviceInfos, ma_uint32* pCaptureDeviceCount);

        public static unsafe ma_result ma_context_get_devices(ma_context_ptr pContext, out ma_device_info_ex[] ppPlaybackDeviceInfos, out ma_device_info_ex[] ppCaptureDeviceInfos)
        {
            ppPlaybackDeviceInfos = null;
            ppCaptureDeviceInfos = null;
            ma_uint32 captureCount = 0;
            ma_uint32 playbackCount = 0;
            ma_device_info* pPlayback = null;
            ma_device_info* pCapture = null;

            ma_result result = ma_context_get_devices(pContext, &pPlayback, &playbackCount, &pCapture, &captureCount);

            if (result != ma_result.MA_SUCCESS)
                return result;

            if (pPlayback != null && playbackCount > 0)
            {
                ppPlaybackDeviceInfos = new ma_device_info_ex[playbackCount];

                for (int i = 0; i < playbackCount; i++)
                {
                    ppPlaybackDeviceInfos[i].deviceInfo = pPlayback[i];
                    ppPlaybackDeviceInfos[i].pDeviceId = new ma_device_id_ptr(new IntPtr(&pPlayback[i]));
                }
            }

            if (pCapture != null && captureCount > 0)
            {
                ppCaptureDeviceInfos = new ma_device_info_ex[captureCount];

                for (int i = 0; i < captureCount; i++)
                {
                    ppCaptureDeviceInfos[i].deviceInfo = pCapture[i];
                    ppCaptureDeviceInfos[i].pDeviceId = new ma_device_id_ptr(new IntPtr(&pCapture[i]));
                }
            }

            return result;
        }

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_context_get_device_info(ma_context_ptr pContext, ma_device_type deviceType, ma_device_id_ptr pDeviceID, ref ma_device_info pDeviceInfo);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_bool32 ma_context_is_loopback_supported(ma_context_ptr pContext);

        // ma_engine
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_uninit(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_init(ref ma_engine_config pConfig, ma_engine_ptr pEngine);

        public static ma_result ma_engine_init(ma_engine_ptr pEngine)
        {
            ma_engine_config config = ma_engine_config_init();
            return ma_engine_init(ref config, pEngine);
        }

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe ma_result ma_engine_read_pcm_frames(ma_engine_ptr pEngine, IntPtr pFramesOut, ma_uint64 frameCount, ma_uint64* pFramesRead);

        public static unsafe ma_result ma_engine_read_pcm_frames(ma_engine_ptr pEngine, IntPtr pFramesOut, ma_uint64 frameCount, ref ma_uint64 framesRead)
        {
            fixed (ma_uint64* pFramesRead = &framesRead)
            {
                return ma_engine_read_pcm_frames(pEngine, pFramesOut, frameCount, pFramesRead);
            }
        }

        public static unsafe ma_result ma_engine_read_pcm_frames(ma_engine_ptr pEngine, IntPtr pFramesOut, ma_uint64 frameCount)
        {
            return ma_engine_read_pcm_frames(pEngine, pFramesOut, frameCount, null);
        }

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_node_graph_ptr ma_engine_get_node_graph(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_resource_manager_ptr ma_engine_get_resource_manager(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_device_ptr ma_engine_get_device(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_log_ptr ma_engine_get_log(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_node_ptr ma_engine_get_endpoint(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint64 ma_engine_get_time_in_pcm_frames(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint64 ma_engine_get_time_in_milliseconds(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_set_time_in_pcm_frames(ma_engine_ptr pEngine, ma_uint64 globalTime);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_set_time_in_milliseconds(ma_engine_ptr pEngine, ma_uint64 globalTime);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_engine_get_channels(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_engine_get_sample_rate(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_start(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_stop(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_set_volume(ma_engine_ptr pEngine, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_engine_get_volume(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_set_gain_db(ma_engine_ptr pEngine, float gainDB);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_engine_get_gain_db(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_engine_get_listener_count(ma_engine_ptr pEngine);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_engine_find_closest_listener(ma_engine_ptr pEngine, float absolutePosX, float absolutePosY, float absolutePosZ);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_listener_set_position(ma_engine_ptr pEngine, ma_uint32 listenerIndex, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_engine_listener_get_position(ma_engine_ptr pEngine, ma_uint32 listenerIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_listener_set_direction(ma_engine_ptr pEngine, ma_uint32 listenerIndex, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_engine_listener_get_direction(ma_engine_ptr pEngine, ma_uint32 listenerIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_listener_set_velocity(ma_engine_ptr pEngine, ma_uint32 listenerIndex, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_engine_listener_get_velocity(ma_engine_ptr pEngine, ma_uint32 listenerIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_listener_set_cone(ma_engine_ptr pEngine, ma_uint32 listenerIndex, float innerAngleInRadians, float outerAngleInRadians, float outerGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_listener_get_cone(ma_engine_ptr pEngine, ma_uint32 listenerIndex, out float pInnerAngleInRadians, out float pOuterAngleInRadians, out float pOuterGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_listener_set_world_up(ma_engine_ptr pEngine, ma_uint32 listenerIndex, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_engine_listener_get_world_up(ma_engine_ptr pEngine, ma_uint32 listenerIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_engine_listener_set_enabled(ma_engine_ptr pEngine, ma_uint32 listenerIndex, ma_bool32 isEnabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_bool32 ma_engine_listener_is_enabled(ma_engine_ptr pEngine, ma_uint32 listenerIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_play_sound_ex(ma_engine_ptr pEngine, string pFilePath, ma_node_ptr pNode, ma_uint32 nodeInputBusIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_play_sound(ma_engine_ptr pEngine, string pFilePath, ma_sound_group_ptr pGroup);   /* Fire and forget. */

        // ma_sound
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_init_from_file(ma_engine_ptr pEngine, string pFilePath, ma_sound_flags flags, ma_sound_group_ptr pGroup, ma_fence_ptr pDoneFence, ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_init_from_file_w(ma_engine_ptr pEngine, string pFilePath, ma_sound_flags flags, ma_sound_group_ptr pGroup, ma_fence_ptr pDoneFence, ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_init_from_memory(ma_engine_ptr pEngine, IntPtr pData, ma_uint64 dataSize, ma_sound_flags flags, ma_sound_group_ptr pGroup, ma_fence_ptr pDoneFence, ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_init_from_callback(ma_engine_ptr pEngine, ref ma_procedural_sound_config pConfig, ma_sound_flags flags, ma_sound_group_ptr pGroup, ma_fence_ptr pDoneFence, ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_init_copy(ma_engine_ptr pEngine, ma_sound_ptr pExistingSound, ma_uint32 flags, ma_sound_group_ptr pGroup, ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_init_from_data_source(ma_engine_ptr pEngine, ma_data_source_ptr pDataSource, ma_uint32 flags, ma_sound_group_ptr pGroup, ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_init_ex(ma_engine_ptr pEngine, ref ma_sound_config pConfig, ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_uninit(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_engine_ptr ma_sound_get_engine(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_data_source_ptr ma_sound_get_data_source(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_start(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_stop(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_stop_with_fade_in_pcm_frames(ma_sound_ptr pSound, ma_uint64 fadeLengthInFrames);     /* Will overwrite any scheduled stop and fade. */

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_stop_with_fade_in_milliseconds(ma_sound_ptr pSound, ma_uint64 fadeLengthInFrames);   /* Will overwrite any scheduled stop and fade. */

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_volume(ma_sound_ptr pSound, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_volume(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_pan(ma_sound_ptr pSound, float pan);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_pan(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_pan_mode(ma_sound_ptr pSound, ma_pan_mode panMode);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_pan_mode ma_sound_get_pan_mode(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_pitch(ma_sound_ptr pSound, float pitch);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_pitch(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_spatialization_enabled(ma_sound_ptr pSound, ma_bool32 enabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_bool32 ma_sound_is_spatialization_enabled(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_pinned_listener_index(ma_sound_ptr pSound, ma_uint32 listenerIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_sound_get_pinned_listener_index(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_sound_get_listener_index(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_sound_get_direction_to_listener(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_position(ma_sound_ptr pSound, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_sound_get_position(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_direction(ma_sound_ptr pSound, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_sound_get_direction(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_velocity(ma_sound_ptr pSound, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_sound_get_velocity(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_attenuation_model(ma_sound_ptr pSound, ma_attenuation_model attenuationModel);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_attenuation_model ma_sound_get_attenuation_model(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_positioning(ma_sound_ptr pSound, ma_positioning positioning);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_positioning ma_sound_get_positioning(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_rolloff(ma_sound_ptr pSound, float rolloff);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_rolloff(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_min_gain(ma_sound_ptr pSound, float minGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_min_gain(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_max_gain(ma_sound_ptr pSound, float maxGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_max_gain(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_min_distance(ma_sound_ptr pSound, float minDistance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_min_distance(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_max_distance(ma_sound_ptr pSound, float maxDistance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_max_distance(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_cone(ma_sound_ptr pSound, float innerAngleInRadians, float outerAngleInRadians, float outerGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_get_cone(ma_sound_ptr pSound, out float pInnerAngleInRadians, out float pOuterAngleInRadians, out float pOuterGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_doppler_factor(ma_sound_ptr pSound, float dopplerFactor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_doppler_factor(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_directional_attenuation_factor(ma_sound_ptr pSound, float directionalAttenuationFactor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_directional_attenuation_factor(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_fade_in_pcm_frames(ma_sound_ptr pSound, float volumeBeg, float volumeEnd, ma_uint64 fadeLengthInFrames);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_fade_in_milliseconds(ma_sound_ptr pSound, float volumeBeg, float volumeEnd, ma_uint64 fadeLengthInMilliseconds);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_fade_start_in_pcm_frames(ma_sound_ptr pSound, float volumeBeg, float volumeEnd, ma_uint64 fadeLengthInFrames, ma_uint64 absoluteGlobalTimeInFrames);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_fade_start_in_milliseconds(ma_sound_ptr pSound, float volumeBeg, float volumeEnd, ma_uint64 fadeLengthInMilliseconds, ma_uint64 absoluteGlobalTimeInMilliseconds);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_get_current_fade_volume(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_start_time_in_pcm_frames(ma_sound_ptr pSound, ma_uint64 absoluteGlobalTimeInFrames);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_start_time_in_milliseconds(ma_sound_ptr pSound, ma_uint64 absoluteGlobalTimeInMilliseconds);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_stop_time_in_pcm_frames(ma_sound_ptr pSound, ma_uint64 absoluteGlobalTimeInFrames);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_stop_time_in_milliseconds(ma_sound_ptr pSound, ma_uint64 absoluteGlobalTimeInMilliseconds);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_stop_time_with_fade_in_pcm_frames(ma_sound_ptr pSound, ma_uint64 stopAbsoluteGlobalTimeInFrames, ma_uint64 fadeLengthInFrames);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_stop_time_with_fade_in_milliseconds(ma_sound_ptr pSound, ma_uint64 stopAbsoluteGlobalTimeInMilliseconds, ma_uint64 fadeLengthInMilliseconds);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_bool32 ma_sound_is_playing(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint64 ma_sound_get_time_in_pcm_frames(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint64 ma_sound_get_time_in_milliseconds(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_set_looping(ma_sound_ptr pSound, ma_bool32 isLooping);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_bool32 ma_sound_is_looping(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_bool32 ma_sound_at_end(ma_sound_ptr pSound);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_seek_to_pcm_frame(ma_sound_ptr pSound, ma_uint64 frameIndex); /* Just a wrapper around ma_data_source_seek_to_pcm_frame(). */

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_seek_to_second(ma_sound_ptr pSound, float seekPointInSeconds); /* Abstraction to ma_sound_seek_to_pcm_frame() */

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_get_data_format(ma_sound_ptr pSound, out ma_format pFormat, out ma_uint32 pChannels, out ma_uint32 pSampleRate, Byte pChannelMap, size_t channelMapCap);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_get_cursor_in_pcm_frames(ma_sound_ptr pSound, out ma_uint64 pCursor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_get_length_in_pcm_frames(ma_sound_ptr pSound, out ma_uint64 pLength);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_get_cursor_in_seconds(ma_sound_ptr pSound, out float pCursor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_get_length_in_seconds(ma_sound_ptr pSound, out float pLength);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_sound_notifications ma_sound_notifications_init();

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_set_notifications_userdata(ma_sound_ptr pSound, IntPtr pUserData);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_set_end_notification_callback(ma_sound_ptr pSound, ma_sound_end_proc callback);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_set_load_notification_callback(ma_sound_ptr pSound, ma_sound_load_proc callback);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_set_process_notification_callback(ma_sound_ptr pSound, ma_sound_process_proc callback);

        // ma_sound_group
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_group_init(ma_engine_ptr pEngine, ma_sound_flags flags, ma_sound_group_ptr pParentGroup, ma_sound_group_ptr pGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_group_init_ex(ma_engine_ptr pEngine, ref ma_sound_group_config pConfig, ma_sound_group_ptr pGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_uninit(ma_sound_group_ptr pGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_sound_group_stop(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_sound_group_is_playing(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_volume(ma_sound_group_ptr soundGroup, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_group_get_volume(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_pitch(ma_sound_group_ptr soundGroup, float pitch);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_group_get_pitch(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_spatialization_enabled(ma_sound_group_ptr soundGroup, ma_uint32 enabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_uint32 ma_sound_group_is_spatialization_enabled(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_doppler_factor(ma_sound_group_ptr soundGroup, float dopplerFactor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_group_get_doppler_factor(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_attenuation_model(ma_sound_group_ptr soundGroup, ma_attenuation_model attenuationModel);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_attenuation_model ma_sound_group_get_attenuation_model(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_min_distance(ma_sound_group_ptr soundGroup, float minDistance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_group_get_min_distance(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_max_distance(ma_sound_group_ptr soundGroup, float minDistance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_sound_group_get_max_distance(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_position(ma_sound_group_ptr soundGroup, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_sound_group_get_position(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_direction(ma_sound_group_ptr soundGroup, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_sound_group_get_direction(ma_sound_group_ptr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_sound_group_set_velocity(ma_sound_group_ptr soundGroup, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_vec3f ma_sound_group_get_velocity(ma_sound_group_ptr soundGroup);

        // ma_procedural_sound
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_procedural_sound_config ma_procedural_sound_config_init(ma_format format, ma_uint32 channels, ma_uint32 sampleRate, ma_procedural_sound_proc pProceduralSoundProc, IntPtr pUserData);

        // ma_decoder
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_decoder_config ma_decoder_config_init(ma_format outputFormat, ma_uint32 outputChannels, ma_uint32 outputSampleRate);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_decoder_config ma_decoder_config_init_default();

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_init(ma_decoder_read_proc onRead, ma_decoder_seek_proc onSeek, IntPtr pUserData, ref ma_decoder_config pConfig, ma_decoder_ptr pDecoder);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_init_memory(IntPtr pData, size_t dataSize, ref ma_decoder_config pConfig, ma_decoder_ptr pDecoder);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_init_vfs(ma_vfs_ptr pVFS, string pFilePath, ref ma_decoder_config pConfig, ma_decoder_ptr pDecoder);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_init_vfs_w(ma_vfs_ptr pVFS, string pFilePath, ref ma_decoder_config pConfig, ma_decoder_ptr pDecoder);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_init_file(string pFilePath, ref ma_decoder_config pConfig, ma_decoder_ptr pDecoder);

        public static ma_result ma_decoder_init_file(string pFilePath, ma_decoder_ptr pDecoder)
        {
            ma_decoder_config config = ma_decoder_config_init_default();
            return ma_decoder_init_file(pFilePath, ref config, pDecoder);
        }

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_init_file_w(string pFilePath, ref ma_decoder_config pConfig, ma_decoder_ptr pDecoder);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_uninit(ma_decoder_ptr pDecoder);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe ma_result ma_decoder_read_pcm_frames(ma_decoder_ptr pDecoder, IntPtr pFramesOut, ma_uint64 frameCount, ma_uint64* pFramesRead);

        public static ma_result ma_decoder_read_pcm_frames(ma_decoder_ptr pDecoder, IntPtr pFramesOut, ma_uint64 frameCount, IntPtr pFramesRead)
        {
            unsafe
            {
                if (pFramesRead == IntPtr.Zero)
                {
                    return ma_decoder_read_pcm_frames(pDecoder, pFramesOut, frameCount, null);
                }
                else
                {
                    ma_uint64* pointer = (ma_uint64*)pFramesOut.ToPointer();
                    return ma_decoder_read_pcm_frames(pDecoder, pFramesOut, frameCount, pointer);
                }
            }
        }

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_seek_to_pcm_frame(ma_decoder_ptr pDecoder, ma_uint64 frameIndex);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_get_data_format(ma_decoder_ptr pDecoder, out ma_format pFormat, out ma_uint32 pChannels, out ma_uint32 pSampleRate, IntPtr pChannelMap, size_t channelMapCap);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_get_cursor_in_pcm_frames(ma_decoder_ptr pDecoder, out ma_uint64 pCursor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_get_length_in_pcm_frames(ma_decoder_ptr pDecoder, out ma_uint64 pLength);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decoder_get_available_frames(ma_decoder_ptr pDecoder, out ma_uint64 pAvailableFrames);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decode_from_vfs(ma_vfs_ptr pVFS, string pFilePath, ref ma_decoder_config pConfig, ref ma_uint64 pFrameCountOut, IntPtr ppPCMFramesOut);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decode_file(string pFilePath, ref ma_decoder_config pConfig, ref ma_uint64 pFrameCountOut, IntPtr ppPCMFramesOut);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_decode_memory(IntPtr pData, size_t dataSize, ref ma_decoder_config pConfig, ref ma_uint64 pFrameCountOut, IntPtr ppPCMFramesOut);

        // ma_resource_manager
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_resource_manager_config ma_resource_manager_config_init();

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_resource_manager_init(ref ma_resource_manager_config pConfig, ma_resource_manager_ptr pResourceManager);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_resource_manager_uninit(ma_resource_manager_ptr pResourceManager);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_log_ptr ma_resource_manager_get_log(ma_resource_manager_ptr pResourceManager);

        // ma_libvorbis
        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern ma_decoding_backend_vtable* ma_libvorbis_get_decoding_backend();
    }
}