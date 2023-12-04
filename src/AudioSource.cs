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

namespace MiniAudioEx
{    
    public delegate void DSPEvent(Span<float> data, int channels);
    public delegate void AudioReadEvent(Span<float> data, int channels);
    public delegate void PlaybackEndedEvent();

    public enum AttenuationModel
    {
        None,          /* No distance attenuation and no spatialization. */
        Inverse,       /* Equivalent to OpenAL's AL_INVERSE_DISTANCE_CLAMPED. */
        Linear,        /* Linear attenuation. Equivalent to OpenAL's AL_LINEAR_DISTANCE_CLAMPED. */
        Exponential    /* Exponential attenuation. Equivalent to OpenAL's AL_EXPONENT_DISTANCE_CLAMPED. */        
    }

    public sealed class AudioSource
    {
        private IntPtr handle;
        private ma_ex_audio_source_callbacks callbacks;
        private AudioClip clip;
        private ma_vec3f position;
        private ma_vec3f direction;
        private ma_vec3f velocity;
        private float volume;
        private float pitch;
        private bool isLooping;
        private bool spatial;
        private float minDistance;
        private float maxDistance;
        private float dopplerFactor;
        private AttenuationModel attenuationModel;

        public event DSPEvent dsp;
        public event AudioReadEvent read;
        public event PlaybackEndedEvent playbackEnded;
        
        public IntPtr Handle
        {
            get => handle;
        }

        public bool IsLooping
        {
            get => isLooping;
            set 
            {
                isLooping = value;
                ApplyIsLooping();
            }
        }
        
        public float Volume
        {
            get => volume;
            set 
            {
                volume = value;
                ApplyVolume();
            }
        }
        
        public float Pitch
        {
            get => pitch;
            set 
            {
                pitch = value;
                ApplyPitch();
            }
        }
        
        public ma_vec3f Position
        {
            get => position;
            set => position = value;
        }
        
        public ma_vec3f Direction
        {
            get => direction;
            set => direction = value;
        }
        
        public ma_vec3f Velocity
        {
            get => velocity;
            set => velocity = value;
        }
        
        public bool Spatial
        {
            get => spatial;
            set 
            {
                spatial = value;
                ApplySpatial();
            }
        }

        public float MinDistance
        {
            get => minDistance;
            set 
            {
                minDistance = value;
                ApplyMinDistance();
            }
        }
        
        public float MaxDistance
        {
            get => maxDistance;
            set 
            {
                maxDistance = value;
                ApplyMaxDistance();
            }
        }

        public float DopplerFactor
        {
            get => dopplerFactor;
            set 
            {
                dopplerFactor = value;
                ApplyDopplerFactor();
            }
        }

        public AttenuationModel AttenuationMode
        {
            get => attenuationModel;
            set
            {
                attenuationModel = value;
                ApplyAttenuationModel();
            }
        }
        
        public bool IsPlaying
        {
            get => MiniAudio.ma_ex_audio_source_get_is_playing(handle) > 0;
        }

        public ulong PCMLength
        {
            get 
            {
                MiniAudio.ma_ex_audio_source_get_pcm_length(handle, out ulong length);
                return length;
            }
        }

        public AudioSource()
        {
            clip = null;
            position = new ma_vec3f(0, 0, 0);
            direction = new ma_vec3f(0, 0, -1);
            velocity = new ma_vec3f(0, 0, 0);
            volume = 1.0f;
            pitch = 1.0f;
            isLooping = false;
            spatial = false;
            minDistance = 1.0f;
            maxDistance = 100.0f;
            dopplerFactor = 1.0f;
            attenuationModel = AttenuationModel.Linear;

            callbacks = new ma_ex_audio_source_callbacks();
            callbacks.dspProc = new ma_engine_node_dsp_proc(OnDSP);
            callbacks.soundEndedProc = new ma_sound_end_proc(OnSoundEnded);
            callbacks.soundLoadedProc = new ma_ex_sound_loaded_proc(OnSoundLoaded);
            callbacks.waveformProc = new ma_waveform_custom_proc(OnCustomWaveform);

            ma_ex_audio_source_config config = MiniAudio.ma_ex_audio_source_config_init(AudioManager.Context, callbacks);

            handle = MiniAudio.ma_ex_audio_source_init(ref config);
           
            if(handle != IntPtr.Zero)
            {
                AudioManager.Add(this);
            }
        }

        public void Dispose()
        {
            Stop();
            MiniAudio.ma_ex_audio_source_uninit(handle);
        }

        public void Play(AudioClip clip)
        {
            if(clip == null)
                return;

            Stop();

            if(this.clip != clip)
                this.clip = clip;

            MiniAudio.ma_ex_audio_source_play(handle, clip.FilePath, clip.StreamFromDisk ? (byte)1 : (byte)0);
        }

        public void Play()
        {
            Stop();
            MiniAudio.ma_ex_audio_source_play_from_waveform_proc(handle);
        }

        public void Stop()
        {
            MiniAudio.ma_ex_audio_source_stop(handle);
            MiniAudio.ma_ex_audio_source_set_pcm_position(handle, 0);
        }

        public void Seek(ulong position)
        {
            MiniAudio.ma_ex_audio_source_set_pcm_position(handle, position);
        }

        private void ApplyIsLooping()
        {
            MiniAudio.ma_ex_audio_source_set_loop(handle, isLooping ? (uint)1 : (uint)0);
        }

        private void ApplyVolume()
        {
            MiniAudio.ma_ex_audio_source_set_volume(handle, volume);
        }

        private void ApplyPitch()
        {
            MiniAudio.ma_ex_audio_source_set_pitch(handle, pitch);
        }

        private void ApplySpatial()
        {
            MiniAudio.ma_ex_audio_source_set_spatialization(handle, spatial ? (uint)1 : (uint)0);
        }

        private void ApplyMinDistance()
        {
            MiniAudio.ma_ex_audio_source_set_min_distance(handle, minDistance);
        }

        private void ApplyMaxDistance()
        {
            MiniAudio.ma_ex_audio_source_set_max_distance(handle, maxDistance);
        }

        private void ApplyDopplerFactor()
        {
            MiniAudio.ma_ex_audio_source_set_doppler_factor(handle, dopplerFactor);
        }

        private void ApplyAttenuationModel()
        {
            ma_attenuation_model model = (ma_attenuation_model)attenuationModel;
            MiniAudio.ma_ex_audio_source_set_attenuation_model(handle, model);
        }

        private void OnSoundLoaded(IntPtr userData, IntPtr sound)
        {
            if(userData == handle)
            {
                ApplyIsLooping();
                ApplyVolume();
                ApplyPitch();
                ApplySpatial();
                ApplyMinDistance();
                ApplyMaxDistance();
                ApplyDopplerFactor();
                ApplyAttenuationModel();
            }
        }

        //Note: This is called from the audio thread, message must be moved into a queue so to main thread can pick it up
        private void OnSoundEnded(IntPtr userData, IntPtr sound)
        {
            if(userData == handle)
            {
                AudioManager.SetPlaybackEndedForSource(this);
            }
        }

        //Called by audio mixer from the main thread
        internal void TriggerPlaybackEndedCallback()
        {
            playbackEnded?.Invoke();
        }

        private unsafe void OnDSP(IntPtr pEngineNode, IntPtr pFramesOut, IntPtr pFramesIn, ulong frameCount, int channels)
        {
            if(frameCount > 0 && dsp != null)
            {
                int length = (int)frameCount * channels;
                Span<float> span = new Span<float>(pFramesOut.ToPointer(), length);
                dsp(span, (int)channels);
            }
        }

        private unsafe void OnCustomWaveform(IntPtr pWaveform, IntPtr pFramesOut, ulong frameCount, int channels)
        {
            if(frameCount > 0 && read != null)
            {
                int length = (int)frameCount * channels;
                Span<float> span = new Span<float>(pFramesOut.ToPointer(), length);
                read(span, channels);
            }
        }
    }
}