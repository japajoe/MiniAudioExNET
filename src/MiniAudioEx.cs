using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MiniAudioExNET
{
    internal enum ma_device_type
    {
        ma_device_type_playback = 1,
        ma_device_type_capture  = 2,
        ma_device_type_duplex   = ma_device_type_playback | ma_device_type_capture, /* 3 */
        ma_device_type_loopback = 4
    }

    internal enum ma_result
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

    internal enum ma_attenuation_model
    {
        ma_attenuation_model_none,          /* No distance attenuation and no spatialization. */
        ma_attenuation_model_inverse,       /* Equivalent to OpenAL's AL_INVERSE_DISTANCE_CLAMPED. */
        ma_attenuation_model_linear,        /* Linear attenuation. Equivalent to OpenAL's AL_LINEAR_DISTANCE_CLAMPED. */
        ma_attenuation_model_exponential    /* Exponential attenuation. Equivalent to OpenAL's AL_EXPONENT_DISTANCE_CLAMPED. */
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ma_device
    {
        public IntPtr pContext;
        public ma_device_type type;
        public UInt32 sampleRate;
        public Int32 state;                 /* The state of the device is variable and can change at any time on any thread. Must be used atomically. */
        public IntPtr onData;               /* Set once at initialization time and should not be changed after. */
        public IntPtr onNotification;       /* Set once at initialization time and should not be changed after. */
        public IntPtr onStop;               /* DEPRECATED. Use the notification callback instead. Set once at initialization time and should not be changed after. */
        public IntPtr pUserData;            /* Application defined data. */
        // More fields here that we don't necessarily need
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ma_ex_context_config
    {
        public UInt32 sampleRate;
        public byte channels;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ma_ex_audio_source_callbacks
    {
        public IntPtr pUserData;
        public ma_sound_end_proc endCallback;
        public ma_sound_load_proc loadCallback;
        public ma_engine_node_dsp_proc dspCallback;
        public ma_waveform_proc waveformCallback;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ma_vec3f
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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ma_sound_load_proc(IntPtr pUserData, IntPtr pSound);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ma_sound_end_proc(IntPtr pUserData, IntPtr pSound);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ma_engine_node_dsp_proc(IntPtr pUserData, IntPtr pEngineNode, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ma_waveform_proc(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels);

    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public enum AttenuationModel
    {
        None,
        Inverse,
        Linear,
        Exponential
    }

    public static class MiniAudioEx
    {
        private const string LIB_MINIAUDIO_EX = "miniaudioex";

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ma_ex_context_config ma_ex_context_config_init(UInt32 sampleRate, byte channels);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ma_ex_context_init(ref ma_ex_context_config config);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_context_uninit(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_context_set_master_volume(IntPtr context, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float ma_ex_context_get_master_volume(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ma_ex_audio_source_init(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_uninit(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_callbacks(IntPtr source, ma_ex_audio_source_callbacks callbacks);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ma_result ma_ex_audio_source_play(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ma_result ma_ex_audio_source_play_from_file(IntPtr source, string filePath, UInt32 streamFromDisk);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ma_result ma_ex_audio_source_play_from_memory(IntPtr source, IntPtr data, UInt64 dataSize);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_stop(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_apply_settings(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_volume(IntPtr source, float value);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float ma_ex_audio_source_get_volume(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_pitch(IntPtr source, float value);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float ma_ex_audio_source_get_pitch(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_pcm_position(IntPtr source, UInt64 position);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 ma_ex_audio_source_get_pcm_position(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 ma_ex_audio_source_get_pcm_length(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_loop(IntPtr source, UInt32 loop);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt32 ma_ex_audio_source_get_loop(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_position(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_get_position(IntPtr source, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_direction(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_get_direction(IntPtr source, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_velocity(IntPtr source, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_get_velocity(IntPtr source, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_spatialization(IntPtr source, UInt32 enabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt32 ma_ex_audio_source_get_spatialization(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_attenuation_model(IntPtr source, ma_attenuation_model model);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ma_attenuation_model ma_ex_audio_source_get_attenuation_model(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_doppler_factor(IntPtr source, float factor);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float ma_ex_audio_source_get_doppler_factor(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_min_distance(IntPtr source, float distance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float ma_ex_audio_source_get_min_distance(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_source_set_max_distance(IntPtr source, float distance);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float ma_ex_audio_source_get_max_distance(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt32 ma_ex_audio_source_get_is_playing(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ma_ex_audio_listener_init(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_uninit(IntPtr listener);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_set_spatialization(IntPtr listener, UInt32 enabled);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt32 ma_ex_audio_listener_get_spatialization(IntPtr listener);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_set_position(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_get_position(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_set_direction(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_get_direction(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_set_velocity(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_get_velocity(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_set_world_up(IntPtr listener, float x, float y, float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_get_world_up(IntPtr listener, out float x, out float y, out float z);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_set_cone(IntPtr listener, float innerAngleInRadians, float outerAngleInRadians, float outerGain);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ma_ex_audio_listener_get_cone(IntPtr listener, out float innerAngleInRadians, out float outerAngleInRadians, out float outerGain);

        private static IntPtr audioContext;
        private static List<AudioSource> audioSources = new List<AudioSource>();
        private static List<AudioClip> audioClips = new List<AudioClip>();
        private static List<AudioListener> audioListeners = new List<AudioListener>();

        internal static IntPtr AudioContext
        {
            get
            {
                return audioContext;
            }
        }

        public static float MasterVolume
        {
            get
            {
                return ma_ex_context_get_master_volume(audioContext);
            }
            set
            {
                ma_ex_context_set_master_volume(audioContext, value);
            }
        }

        public static void Initialize(UInt32 sampleRate, UInt32 channels)
        {
            if(audioContext != IntPtr.Zero)
                return;

            ma_ex_context_config contextConfig = MiniAudioEx.ma_ex_context_config_init(sampleRate, (byte)channels);
            audioContext = MiniAudioEx.ma_ex_context_init(ref contextConfig);
        }

        public static void Deinitialize()
        {
            if(audioContext == IntPtr.Zero)
                return;

            for(int i = 0; i < audioSources.Count; i++)
                audioSources[i].Dispose();

            audioSources.Clear();

            for(int i = 0; i < audioClips.Count; i++)
                audioClips[i].Dispose();
            
            audioClips.Clear();

            for(int i = 0; i < audioListeners.Count; i++)
                audioListeners[i].Dispose();

            audioListeners.Clear();

            MiniAudioEx.ma_ex_context_uninit(audioContext);
            audioContext = IntPtr.Zero;
        }

        public static void Update()
        {
            if(audioContext == IntPtr.Zero)
                return;

            for(int i = 0; i < audioSources.Count; i++)
            {
                audioSources[i].Update();
            }
        }

        internal static void Add(AudioSource source)
        {
            int hashcode = source.GetHashCode();

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].GetHashCode() == hashcode)
                    return;
            }

            audioSources.Add(source);
        }

        internal static void Add(AudioClip clip)
        {
            int hashcode = clip.GetHashCode();

            for(int i = 0; i < audioClips.Count; i++)
            {
                if(audioClips[i].GetHashCode() == hashcode)
                    return;
            }

            audioClips.Add(clip);
        }

        internal static void Add(AudioListener listener)
        {
            int hashcode = audioListeners.GetHashCode();

            for(int i = 0; i < audioListeners.Count; i++)
            {
                if(audioListeners[i].GetHashCode() == hashcode)
                    return;
            }

            audioListeners.Add(listener);
        }        

        internal static void Remove(AudioSource source)
        {
            int hashcode = source.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioSources[index].Dispose();
                audioSources.RemoveAt(index);
            }
        }

        internal static void Remove(AudioClip clip)
        {
            int hashcode = clip.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioClips.Count; i++)
            {
                if(audioClips[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioClips[index].Dispose();
                audioClips.RemoveAt(index);
            }
        }

        internal static void Remove(AudioListener listener)
        {
            int hashcode = listener.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioListeners.Count; i++)
            {
                if(audioListeners[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioListeners[index].Dispose();
                audioListeners.RemoveAt(index);
            }
        }
    }
}