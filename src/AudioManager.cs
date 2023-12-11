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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MiniAudioEx
{
    public static class AudioManager
    {
        private static IntPtr context;
        private static uint sampleRate;
        private static uint channels;
        private static ma_format format;
        private static ma_device_data_proc dataProc;
        private static List<AudioSource> sources;
        private static AudioListener listener;
        private static ConcurrentQueue<AudioSource> playbackEndedQueue;

        public static IntPtr Context
        {
            get => context;
        }

        public static uint SampleRate
        {
            get => sampleRate;
        }

        public static uint Channels
        {
            get => channels;
        }

        public static void Initialize(uint sampleRate, uint channels)
        {
            AudioManager.sampleRate = sampleRate;
            AudioManager.channels = channels;
            format = ma_format.ma_format_f32;

            dataProc = new ma_device_data_proc(OnDataProcess);
            
            ma_ex_context_config config = new ma_ex_context_config();
            config.sampleRate = sampleRate;
            config.channels = channels;
            config.format = format;
            config.dataProc = dataProc;
            
            context = MiniAudio.ma_ex_context_init(ref config);

            if(context != IntPtr.Zero)
            {
                sources = new List<AudioSource>();
                playbackEndedQueue = new ConcurrentQueue<AudioSource>();
            }
        }

        public static void Deinitialize()
        {
            if(context != IntPtr.Zero)
            {
                for(int i = 0; i < sources.Count; i++)
                {
                    sources[i].Dispose();
                }

                sources.Clear();

                if(listener != null)
                    listener.Dispose();

                MiniAudio.ma_ex_context_uninit(context);
                context = IntPtr.Zero;
            }
        }

        public static void Add(AudioSource source)
        {
            if(context == IntPtr.Zero)
                return;

            for(int i = 0; i < sources.Count; i++)
            {
                if(sources[i].Handle == source.Handle)
                    return;
            }

            sources.Add(source);
        }

        public static void Remove(AudioSource source)
        {
            if(context == IntPtr.Zero)
                return;

            int index = -1;

            for(int i = 0; i < sources.Count; i++)
            {
                if(sources[i].Handle == source.Handle)
                {
                    index = i;
                    break;
                }
            }

            if(index >= 0)
            {
                sources[index].Dispose();
                sources.RemoveAt(index);
            }
        }

        public static void Add(AudioListener listener)
        {
            if(context == IntPtr.Zero)
                return;

            AudioManager.listener = listener;
        }

        public static void Remove(AudioListener listener)
        {
            if(context == IntPtr.Zero)
                return;

            listener.Dispose();
            AudioManager.listener = null;
        }

        public static void Update()
        {
            if(context == IntPtr.Zero)
                return;

            if(playbackEndedQueue.Count > 0)
            {
                while(playbackEndedQueue.TryDequeue(out AudioSource source))
                {
                    source.TriggerPlaybackEndedCallback();
                }
            }

            if(listener == null)
                return;

            ma_vec3f position = listener.Position; 
            ma_vec3f direction = listener.Direction;
            ma_vec3f velocity = listener.Position;

            MiniAudio.ma_ex_audio_listener_set_position(listener.Handle, position.x, position.y, position.z);
            MiniAudio.ma_ex_audio_listener_set_direction(listener.Handle, direction.x, direction.y, direction.z);
            MiniAudio.ma_ex_audio_listener_set_velocity(listener.Handle, velocity.x, velocity.y, velocity.z);
            
            for(int i = 0; i < sources.Count; i++)
            {
                if(sources[i].Spatial && sources[i].IsPlaying)
                {
                    position = sources[i].Position; 
                    direction = sources[i].Direction;
                    velocity = sources[i].Velocity;
                    MiniAudio.ma_ex_audio_source_set_position(sources[i].Handle, position.x, position.y, position.z);
                    MiniAudio.ma_ex_audio_source_set_direction(sources[i].Handle, direction.x, direction.y, direction.z);
                    MiniAudio.ma_ex_audio_source_set_velocity(sources[i].Handle, velocity.x, velocity.y, velocity.z);
                }
            }
        }

        private static void OnDataProcess(ref ma_device pDevice, IntPtr pOutput, IntPtr pInput, uint frameCount)
        {
            MiniAudio.ma_engine_read_pcm_frames(pDevice.pUserData, pOutput, frameCount, out _);
        }

        internal static void SetPlaybackEndedForSource(AudioSource source)
        {
            playbackEndedQueue.Enqueue(source);
        }
    }
}