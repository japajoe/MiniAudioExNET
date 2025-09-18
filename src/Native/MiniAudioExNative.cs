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

namespace MiniAudioEx.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_native_data_format
    {
        public ma_format format;       /* Sample format. If set to ma_format_unknown, all sample formats are supported. */
        public UInt32 channels;     /* If set to 0, all channels are supported. */
        public UInt32 sampleRate;   /* If set to 0, all sample rates are supported. */
        public UInt32 flags;        /* A combination of MA_DATA_FORMAT_FLAG_* flags. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_device_info
    {
        public IntPtr pName;
        public Int32 index;
        public Int32 isDefault;
        public UInt32 nativeDataFormatCount;
        public IntPtr nativeDataFormats;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_context_config
    {
        public ma_ex_device_info deviceInfo;
        public UInt32 sampleRate;
        public byte channels;
        public UInt32 periodSizeInFrames;
        public ma_device_data_proc deviceDataProc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_audio_source_callbacks_
    {
        public IntPtr pUserData;
        public IntPtr endCallback;
        public IntPtr loadCallback;
        public IntPtr processCallback;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ma_ex_audio_source_callbacks
    {
        public IntPtr pUserData;
        public ma_sound_end_proc endCallback;
        public ma_sound_load_proc loadCallback;
        public ma_sound_process_proc processCallback;
    }

    public static class MiniAudioExNative
    {
        private const string LIB_MINIAUDIO_EX = "miniaudioex";

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_free(IntPtr pointer);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_playback_devices_get(out UInt32 count);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_playback_devices_free(IntPtr pDeviceInfo, UInt32 count);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_ex_context_config ma_ex_context_config_init(UInt32 sampleRate, byte channels, UInt32 periodSizeInFrames, ref ma_ex_device_info pDeviceInfo);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_context_init(ref ma_ex_context_config config);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_context_uninit(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_context_set_master_volume(IntPtr context, float volume);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ma_ex_context_get_master_volume(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_context_get_engine(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_device_get_user_data(IntPtr pDevice);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_audio_source_init(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_audio_source_uninit(IntPtr source);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ma_ex_audio_source_set_callbacks(IntPtr source, ma_ex_audio_source_callbacks_ callbacks);

        public static void ma_ex_audio_source_set_callbacks(IntPtr source, ma_ex_audio_source_callbacks callbacks)
        {
            ma_ex_audio_source_callbacks_ c = new ma_ex_audio_source_callbacks_();
            c.pUserData = callbacks.pUserData;
            c.endCallback = MarshalHelper.GetFunctionPointerForDelegate(callbacks.endCallback);
            c.loadCallback = MarshalHelper.GetFunctionPointerForDelegate(callbacks.loadCallback);
            c.processCallback = MarshalHelper.GetFunctionPointerForDelegate(callbacks.processCallback);
            ma_ex_audio_source_set_callbacks(source, c);
        }

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        private static extern ma_result ma_ex_audio_source_play_from_callback(IntPtr source, IntPtr callback);

        public static ma_result ma_ex_audio_source_play_from_callback(IntPtr source, ma_procedural_sound_proc callback)
        {
            return ma_ex_audio_source_play_from_callback(source, MarshalHelper.GetFunctionPointerForDelegate(callback));
        }

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
        public static extern ma_result ma_ex_audio_source_set_group(IntPtr source, IntPtr soundGroup);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_audio_source_get_group(IntPtr source);

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

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_decode_file(string pFilePath, out UInt64 dataLength, out UInt32 channels, out UInt32 sampleRate, UInt32 desiredChannels, UInt32 desiredSampleRate);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_decode_memory(IntPtr pData, UInt64 size, out UInt64 dataLength, out UInt32 channels, out UInt32 sampleRate, UInt32 desiredChannels, UInt32 desiredSampleRate);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern ma_result ma_engine_read_pcm_frames(IntPtr pEngine, IntPtr pFramesOut, UInt64 frameCount, out UInt64 pFramesRead);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ma_ex_sound_group_init(IntPtr context);

        [DllImport(LIB_MINIAUDIO_EX, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ma_ex_sound_group_uninit(IntPtr soundGroup);
    }
}