using System;
using System.Collections.Generic;
using MiniAudioEx.Native;
using MiniAudioEx.Utilities;

namespace MiniAudioEx.Core
{
    public delegate void AudioEndEvent();

    public enum AttenuationModel
    {
        None,
        Inverse,
        Linear,
        Exponential
    }

    public enum PanMode
    {
        Balance,
        Pan
    }

    public sealed class AudioSource : IDisposable
    {
        private struct Settings
        {
            public bool loop;
        }

        private class Sound
        {
            public AudioClip clip;
            public bool atEnd;

            public Sound()
            {
                clip = new AudioClip();
                atEnd = false;
            }
        }

        private AudioContext context;
        private List<Sound> sounds;
        private ma_sound_group_ptr group;
        private ma_effect_node_ptr effectNode;
        private ma_effect_node_process_proc onProcessEffect;
        private int currentIndex;
        private Settings settings;
        public event AudioEndEvent End;
        public ma_sound_group_ptr Group => group;

        public bool IsPlaying
        {
            get
            {
                for (int i = 0; i < sounds.Count; i++)
                {
                    if (MiniAudio.ma_sound_is_playing(sounds[i].clip.Sound) > 0)
                        return true;
                }
                return false;
            }
        }

        public bool Loop
        {
            get => settings.loop;
            set => settings.loop = value;
        }

        public UInt64 Cursor
        {
            get => MiniAudio.ma_sound_get_time_in_pcm_frames(sounds[0].clip.Sound);
            set => MiniAudio.ma_sound_seek_to_pcm_frame(sounds[0].clip.Sound, value);
        }

        public float Pitch
        {
            get => MiniAudio.ma_sound_group_get_pitch(group);
            set => MiniAudio.ma_sound_group_set_pitch(group, value);
        }

        public float Volume
        {
            get => MiniAudio.ma_sound_group_get_volume(group);
            set => MiniAudio.ma_sound_group_set_volume(group, value);
        }

        public float Pan
        {
            get => MiniAudio.ma_sound_group_get_pan(group);
            set => MiniAudio.ma_sound_group_set_pan(group, value);
        }

        public AttenuationModel AttenuationModel
        {
            get => (AttenuationModel)MiniAudio.ma_sound_group_get_attenuation_model(group);
            set => MiniAudio.ma_sound_group_set_attenuation_model(group, (ma_attenuation_model)value);
        }

        public PanMode PanMode
        {
            get => (PanMode)MiniAudio.ma_sound_group_get_pan_mode(group);
            set => MiniAudio.ma_sound_group_set_pan_mode(group, (ma_pan_mode)value);
        }

        public bool Spatial
        {
            get => MiniAudio.ma_sound_group_is_spatialization_enabled(group) > 0;
            set => MiniAudio.ma_sound_group_set_spatialization_enabled(group, value ? (uint)1 : 0);
        }

        public float DopplerFactor
        {
            get => MiniAudio.ma_sound_group_get_doppler_factor(group);
            set => MiniAudio.ma_sound_group_set_doppler_factor(group, value);
        }

        public float MinDistance
        {
            get => MiniAudio.ma_sound_group_get_min_distance(group);
            set => MiniAudio.ma_sound_group_set_min_distance(group, value);
        }

        public float MaxDistance
        {
            get => MiniAudio.ma_sound_group_get_max_distance(group);
            set => MiniAudio.ma_sound_group_set_max_distance(group, value);
        }

        public float RollOff
        {
            get => MiniAudio.ma_sound_group_get_rolloff(group);
            set => MiniAudio.ma_sound_group_set_rolloff(group, value);
        }

        public float MinGain
        {
            get => MiniAudio.ma_sound_group_get_min_gain(group);
            set => MiniAudio.ma_sound_group_set_min_gain(group, value);
        }

        public float MaxGain
        {
            get => MiniAudio.ma_sound_group_get_max_gain(group);
            set => MiniAudio.ma_sound_group_set_max_gain(group, value);
        }

        public Vector3f Position
        {
            get
            {
                var result = MiniAudio.ma_sound_group_get_position(group);
                return new Vector3f(result.x, result.y, result.z);
            }
            set
            {
                var p = MiniAudio.ma_sound_group_get_position(group);
                MiniAudio.ma_sound_group_set_position(group, value.x, value.y, value.z);
            }
        }

        public Vector3f Direction
        {
            get
            {
                var result = MiniAudio.ma_sound_group_get_direction(group);
                return new Vector3f(result.x, result.y, result.z);
            }
            set => MiniAudio.ma_sound_group_set_direction(group, value.x, value.y, value.z);
        }

        public Vector3f Velocity
        {
            get
            {
                var result = MiniAudio.ma_sound_group_get_velocity(group);
                return new Vector3f(result.x, result.y, result.z);
            }
            set => MiniAudio.ma_sound_group_set_velocity(group, value.x, value.y, value.z);
        }

        public AudioSource()
        {
            context = AudioContext.GetCurrent();
            
            if(context == null)
                throw new Exception("Failed to create AudioSource because there is no current AudioContext");

            group = new ma_sound_group_ptr(true);
            MiniAudio.ma_sound_group_init(context.Engine, (ma_sound_flags)0, default, group);

            sounds = new List<Sound>();

            for(int i = 0; i < 16; i++)
            {
                sounds.Add(new Sound());
            }

            effectNode = new ma_effect_node_ptr(true);

            onProcessEffect = OnEffectProcess;

            ma_effect_node_config effectNodeConfig = MiniAudio.ma_effect_node_config_init(context.Channels, context.SampleRate, onProcessEffect, IntPtr.Zero);

            if (MiniAudio.ma_effect_node_init(MiniAudio.ma_engine_get_node_graph(context.Engine), ref effectNodeConfig, effectNode) == ma_result.success)
            {
                MiniAudio.ma_node_attach_output_bus(new ma_node_ptr(effectNode.pointer), 0, MiniAudio.ma_engine_get_endpoint(context.Engine), 0);
                MiniAudio.ma_node_attach_output_bus(new ma_node_ptr(group.pointer), 0, new ma_node_ptr(effectNode.pointer), 0);
            }

            settings = new Settings();
            settings.loop = false;

            currentIndex = 0;
        }

        public void Dispose()
        {
            Stop();

            for(int i = 0; i < sounds.Count; i++)
            {
                sounds[i].clip?.Dispose();
            }

            if(effectNode.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_effect_node_uninit(effectNode);
                effectNode.Free();
            }

            if(group.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_sound_group_uninit(group);
                group.Free();
            }
        }

        public void Play(AudioClip clip)
        {
            SetAtEnd(0, false);
            Invalidate(clip);
            ApplySettings();
            MiniAudio.ma_sound_start(sounds[0].clip.Sound);
        }

        public void PlayOneShot(AudioClip clip)
        {
            SetAtEnd(currentIndex, false);
            Invalidate(clip);

            if (MiniAudio.ma_sound_is_playing(sounds[currentIndex].clip.Sound) > 0)
            {
                MiniAudio.ma_sound_stop(sounds[currentIndex].clip.Sound);
                MiniAudio.ma_sound_seek_to_pcm_frame(sounds[currentIndex].clip.Sound, 0);
            }

            ApplySettings();

            MiniAudio.ma_sound_start(sounds[currentIndex].clip.Sound);

            if (++currentIndex >= sounds.Count - 1)
                currentIndex = 0;
        }

        public void Stop()
        {
            MiniAudio.ma_sound_group_stop(group);

            for (int i = 0; i < sounds.Count; i++)
            {
                MiniAudio.ma_sound_stop(sounds[i].clip.Sound);
                SetAtEnd(i, false);
            }
        }

        internal void Update()
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                if (MiniAudio.ma_sound_at_end(sounds[i].clip.Sound) > 0)
                {
                    if (!sounds[i].atEnd)
                    {
                        SetAtEnd(i, true);
                        End?.Invoke();
                    }
                }
            }
        }

        private void Invalidate(AudioClip clip)
        {
            if(clip.HashCode != sounds[0].clip.HashCode)
            {
                for(int i = 0; i < sounds.Count; i++)
                {
                    clip.CopyTo(sounds[i].clip, this);
                }
            }
        }

        private void ApplySettings()
        {
            MiniAudio.ma_sound_set_looping(sounds[0].clip.Sound, settings.loop ? (UInt32)1 : 0);
        }

        private void SetAtEnd(int sourceIndex, bool atEnd)
        {
            sounds[sourceIndex].atEnd = atEnd;
        }  

        private unsafe void OnEffectProcess(ma_node_ptr pNode, IntPtr ppFramesIn, IntPtr pFrameCountIn, IntPtr ppFramesOut, IntPtr pFrameCountOut)
        {
            if (pNode.pointer == IntPtr.Zero)
                return;

            ma_effect_node* pEffectNode = (ma_effect_node*)pNode.pointer;

            UInt32* frameCountIn = (UInt32*)pFrameCountIn;
            UInt32* frameCountOut = (UInt32*)pFrameCountOut;
            UInt32 channels = pEffectNode->config.channels;

            float** framesIn = (float**)ppFramesIn;
            float** framesOut = (float**)ppFramesOut;

            NativeArray<float> bufferIn = new NativeArray<float>(framesIn[0], (int)(*frameCountIn * channels));
            NativeArray<float> bufferOut = new NativeArray<float>(framesOut[0], (int)(*frameCountOut * channels));

            // Just in case we end up with no sound at all because no effects were active (prevents silence)
            bufferIn.CopyTo(bufferOut);

            // An effect can modify the number of frames it processes so we need to keep track of this
            UInt32 countIn = *frameCountIn;
            UInt32 countOut = *frameCountOut;

            // for (int i = 0; i < effects.Count; i++)
            // {
            //     effects[i].OnProcess(bufferIn, countIn, bufferOut, ref countOut, channels);

            //     //Since effects processing is like a stack, the output needs to be copied to the input for the next effect
            //     bufferOut.CopyTo(bufferIn);

            //     countIn = countOut;
            // }

            // Process?.Invoke(bufferIn, countIn, bufferOut, ref countOut, pEffectNode->config.channels);

            *frameCountOut = countOut;
        }
    }
}