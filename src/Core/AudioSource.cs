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
// Copyright 2026 W.M.R Jap-A-Joe

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
using System.Collections.Generic;
using MiniAudioEx.Native;
using MiniAudioEx.Utilities;
using MiniAudioEx.DSP.Effects;
using MiniAudioEx.DSP.Generators;

namespace MiniAudioEx.Core
{
    public delegate void AudioEndEvent();
    public delegate void AudioProcessEvent(NativeArray<float> framesIn, UInt32 frameCountIn, NativeArray<float> framesOut, ref UInt32 frameCountOut, UInt32 channels);
    public delegate void AudioReadEvent(NativeArray<float> framesOut, UInt64 frameCount, Int32 channels);

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

    /// <summary>
    /// This class is used to play sounds.
    /// </summary>
    public sealed class AudioSource : IDisposable
    {
        /// <summary>
        /// Callback handler for when the playback has finished. This does not trigger if the AudioSource is set to Loop.
        /// </summary>
        public event AudioEndEvent End;

        /// <summary>
        /// Callback handler for generating procedural audio when using the 'Play()' method.
        /// </summary>
        public event AudioReadEvent Read;

        /// <summary>
        /// Callback handler for implementing custom effects.
        /// </summary>
        public event AudioProcessEvent Process;

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
        private ConcurrentList<IAudioEffect> effects;
        private ConcurrentList<IAudioGenerator> generators;
        private ma_sound_group_ptr group;
        private ma_effect_node_ptr effectNode;
        private ma_effect_node_process_proc onProcess;
        private ma_sound_ptr proceduralSound;
        private ma_procedural_data_source_proc onGenerate;
        private int currentIndex;
        private bool loop;
        private Vector3f previousPosition;

        /// <summary>
        /// Indicates whether the source is playing audio or not.
        /// </summary>
        /// <value></value>
        public bool IsPlaying
        {
            get
            {
                for (int i = 0; i < sounds.Count; i++)
                {
                    if (MiniAudio.ma_sound_is_playing(sounds[i].clip.Sound) > 0)
                        return true;
                }
                if (MiniAudio.ma_sound_is_playing(proceduralSound) > 0)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gets or sets whether the audio should loop. If true, then the 'End' event will not be called. If the Play() overload is used, this property has no effect because the audio will loop regardless. Take note that this setting only applies to the first source, so it may cause undesirable results when using the PlayOneShot method.
        /// </summary>
        /// <value></value>
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        /// <summary>
        /// Gets or sets the current position of the playback cursor in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 Cursor
        {
            get => MiniAudio.ma_sound_get_time_in_pcm_frames(sounds[0].clip.Sound);
            set => MiniAudio.ma_sound_seek_to_pcm_frame(sounds[0].clip.Sound, value);
        }

        /// <summary>
        /// Gets or sets the pitch of the sound.
        /// </summary>
        /// <value></value>
        public float Pitch
        {
            get => MiniAudio.ma_sound_group_get_pitch(group);
            set => MiniAudio.ma_sound_group_set_pitch(group, value);
        }

        /// <summary>
        /// Gets or sets the volume of the sound.
        /// </summary>
        /// <value></value>
        public float Volume
        {
            get => MiniAudio.ma_sound_group_get_volume(group);
            set => MiniAudio.ma_sound_group_set_volume(group, value);
        }

        /// <summary>
        /// Gets or sets the pan of the sound.
        /// </summary>
        /// <value></value>
        public float Pan
        {
            get => MiniAudio.ma_sound_group_get_pan(group);
            set => MiniAudio.ma_sound_group_set_pan(group, value);
        }

        /// <summary>
        /// Gets or sets the mathematical model used to simulate the attenuation of sound over distance.
        /// </summary>
        /// <value></value>
        public AttenuationModel AttenuationModel
        {
            get => (AttenuationModel)MiniAudio.ma_sound_group_get_attenuation_model(group);
            set => MiniAudio.ma_sound_group_set_attenuation_model(group, (ma_attenuation_model)value);
        }

        /// <summary>
        /// Gets or sets the pan of the sound.
        /// </summary>
        /// <value></value>
        public PanMode PanMode
        {
            get => (PanMode)MiniAudio.ma_sound_group_get_pan_mode(group);
            set => MiniAudio.ma_sound_group_set_pan_mode(group, (ma_pan_mode)value);
        }

        /// <summary>
        /// Gets or sets whether this source has spatial audio enabled or not.
        /// </summary>
        /// <value></value>
        public bool Spatial
        {
            get => MiniAudio.ma_sound_group_is_spatialization_enabled(group) > 0;
            set => MiniAudio.ma_sound_group_set_spatialization_enabled(group, value ? (uint)1 : 0);
        }

        /// <summary>
        /// Gets or sets the intensity or strength of the simulated Doppler effect applied to the audio if Spatial is set to true.
        /// </summary>
        /// <value></value>
        public float DopplerFactor
        {
            get => MiniAudio.ma_sound_group_get_doppler_factor(group);
            set => MiniAudio.ma_sound_group_set_doppler_factor(group, value);
        }

        /// <summary>
        /// Gets or sets the distance from the audio source at which the volume of the audio starts to attenuate. Sounds closer to or within this minimum distance are heard at full volume without any attenuation applied.
        /// </summary>
        /// <value></value>
        public float MinDistance
        {
            get => MiniAudio.ma_sound_group_get_min_distance(group);
            set => MiniAudio.ma_sound_group_set_min_distance(group, value);
        }

        /// <summary>
        /// Gets or sets the distance from the audio source beyond which the audio is no longer audible or significantly attenuated. Sounds beyond this maximum distance are either inaudible or heard at greatly reduced volume.
        /// </summary>
        /// <value></value>
        public float MaxDistance
        {
            get => MiniAudio.ma_sound_group_get_max_distance(group);
            set => MiniAudio.ma_sound_group_set_max_distance(group, value);
        }

        /// <summary>
        /// Gets or sets the roll off value, which controls how quickly a sound rolls off as it moves away from the listener.
        /// </summary>
        /// <value></value>
        public float RollOff
        {
            get => MiniAudio.ma_sound_group_get_rolloff(group);
            set => MiniAudio.ma_sound_group_set_rolloff(group, value);
        }

        /// <summary>
        /// Gets or sets the min gain which is applied to the spatialization.
        /// </summary>
        /// <value></value>
        public float MinGain
        {
            get => MiniAudio.ma_sound_group_get_min_gain(group);
            set => MiniAudio.ma_sound_group_set_min_gain(group, value);
        }

        /// <summary>
        /// Gets or sets the min gain which is applied to the spatialization.
        /// </summary>
        /// <value></value>
        public float MaxGain
        {
            get => MiniAudio.ma_sound_group_get_max_gain(group);
            set => MiniAudio.ma_sound_group_set_max_gain(group, value);
        }

        /// <summary>
        /// Gets or sets the position of the source, used for calculating spatial sound.
        /// </summary>
        /// <value></value>
        public Vector3f Position
        {
            get
            {
                var result = MiniAudio.ma_sound_group_get_position(group);
                return new Vector3f(result.x, result.y, result.z);
            }
            set
            {
                var previous = MiniAudio.ma_sound_group_get_position(group);
                previousPosition.x = previous.x;
                previousPosition.y = previous.y;
                previousPosition.z = previous.z;
                MiniAudio.ma_sound_group_set_position(group, value.x, value.y, value.z);
            }
        }

        /// <summary>
        /// Gets or sets the direction of the source, used for calculation spatial sound.
        /// </summary>
        /// <value></value>
        public Vector3f Direction
        {
            get
            {
                var result = MiniAudio.ma_sound_group_get_direction(group);
                return new Vector3f(result.x, result.y, result.z);
            }
            set => MiniAudio.ma_sound_group_set_direction(group, value.x, value.y, value.z);
        }

        /// <summary>
        /// The velocity of the source, used for calculating spatial sound.
        /// </summary>
        /// <value></value>
        public Vector3f Velocity
        {
            get
            {
                var result = MiniAudio.ma_sound_group_get_velocity(group);
                return new Vector3f(result.x, result.y, result.z);
            }
            set => MiniAudio.ma_sound_group_set_velocity(group, value.x, value.y, value.z);
        }

        /// <summary>
        /// Gets the the velocity based on the current position and the previous position.
        /// </summary>
        /// <returns></returns>
        public Vector3f CurrentVelocity
        {
            get
            {
                if(context == null)
                    return new Vector3f(0, 0, 0);
                float deltaTime = context.DeltaTime;
                Vector3f currentPosition = Position;
                float dx = currentPosition.x - previousPosition.x;
                float dy = currentPosition.y - previousPosition.y;
                float dz = currentPosition.z - previousPosition.z;
                return new Vector3f(dx / deltaTime, dy / deltaTime, dz / deltaTime);
            }
        }

        /// <summary>
        /// Creates and AudioSourceGroup instance.
        /// </summary>
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

            effects = new ConcurrentList<IAudioEffect>();
            generators = new ConcurrentList<IAudioGenerator>();

            effectNode = new ma_effect_node_ptr(true);
            onProcess = OnEffect;

            ma_effect_node_config effectNodeConfig = MiniAudio.ma_effect_node_config_init(context.Channels, context.SampleRate, onProcess, IntPtr.Zero);

            if (MiniAudio.ma_effect_node_init(MiniAudio.ma_engine_get_node_graph(context.Engine), ref effectNodeConfig, effectNode) == ma_result.success)
            {
                MiniAudio.ma_node_attach_output_bus(new ma_node_ptr(effectNode.pointer), 0, MiniAudio.ma_engine_get_endpoint(context.Engine), 0);
                MiniAudio.ma_node_attach_output_bus(new ma_node_ptr(group.pointer), 0, new ma_node_ptr(effectNode.pointer), 0);
            }

            proceduralSound = new ma_sound_ptr(true);
            onGenerate = OnGenerate;
            ma_procedural_data_source_config config = MiniAudio.ma_procedural_data_source_config_init(ma_format.f32, context.Channels, context.SampleRate, onGenerate, IntPtr.Zero);
            
            MiniAudio.ma_sound_init_from_callback(context.Engine, ref config, (ma_sound_flags)0, group, default, proceduralSound);

            loop = false;

            currentIndex = 1;

            previousPosition = new Vector3f(0, 0, 0);

            context.Add(this);
        }

        public void Dispose()
        {
            if(context != null)
                context.Remove(this);

            Stop();

            for(int i = 0; i < sounds.Count; i++)
                sounds[i].clip?.Dispose();

            if(effectNode.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_effect_node_uninit(effectNode);
                effectNode.Free();
            }

            if(proceduralSound.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_sound_uninit(proceduralSound);
                proceduralSound.Free();
            }

            if(group.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_sound_group_uninit(group);
                group.Free();
            }

            for (int i = 0; i < effects.Count; i++)
                effects[i].OnDestroy();

            for (int i = 0; i < generators.Count; i++)
                generators[i].OnDestroy();
            
            effects.Clear();
            generators.Clear();
        }

        /// <summary>
        /// Use this method if you registered a method to the Read callback to generate audio.
        /// </summary>
        public void Play()
        {
            Stop();
            SetAtEnd(0, false);
            ApplySettings();
            MiniAudio.ma_sound_start(proceduralSound);
        }

        /// <summary>
        /// Plays an AudioClip.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        public void Play(AudioClip clip)
        {
            Stop();
            SetAtEnd(0, false);
            Invalidate(clip, false);
            ApplySettings();
            MiniAudio.ma_sound_start(sounds[0].clip.Sound);
        }

        /// <summary>
        /// Plays an AudioClip while allowing overlapping sounds. Does not cancel clips that are already being played by PlayOneShot and Play methods.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        public void PlayOneShot(AudioClip clip)
        {
            SetAtEnd(currentIndex, false);
            Invalidate(clip, true);

            if (MiniAudio.ma_sound_is_playing(sounds[currentIndex].clip.Sound) > 0)
            {
                MiniAudio.ma_sound_stop(sounds[currentIndex].clip.Sound);
                MiniAudio.ma_sound_seek_to_pcm_frame(sounds[currentIndex].clip.Sound, 0);
            }

            ApplySettings();

            MiniAudio.ma_sound_start(sounds[currentIndex].clip.Sound);

            if (++currentIndex >= sounds.Count - 1)
                currentIndex = 1;
        }

        /// <summary>
        /// Stop the AudioSource playback, note that this method does not set the cursor back to 0 so it could be used as a pause method.
        /// </summary>
        public void Stop()
        {
            MiniAudio.ma_sound_stop(proceduralSound);

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

        /// <summary>
        /// A thread safe method to add an IAudioEffect.
        /// </summary>
        /// <param name="effect"></param>
        public void AddEffect(IAudioEffect effect)
        {
            effects.Add(effect);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioEffect.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveEffect(IAudioEffect effect)
        {
            effects.Remove(effect);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioEffect by its index.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveEffect(int index)
        {
            if (index >= 0 && index < effects.Count)
            {
                var target = effects[index];
                effects.Remove(target);
            }
        }

        /// <summary>
        /// A thread safe method to remove all IAudioEffect instances.
        /// </summary>
        public void RemoveEffects()
        {
            effects.Clear();
        }

        /// <summary>
        /// A thread safe method to add an IAudioGenerator.
        /// </summary>
        /// <param name="effect"></param>
        public void AddGenerator(IAudioGenerator generator)
        {
            generators.Add(generator);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioGenerator.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveGenerator(IAudioGenerator generator)
        {
            generators.Remove(generator);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioGenerator by its index.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveGenerator(int index)
        {
            if (index >= 0 && index < generators.Count)
            {
                var target = generators[index];
                generators.Remove(target);
            }
        }

        /// <summary>
        /// A thread safe method to remove all IAudioGenerator instances.
        /// </summary>
        public void RemoveGenerators()
        {
            generators.Clear();
        }

        private void Invalidate(AudioClip clip, bool all)
        {
            if(clip.HashCode != sounds[0].clip.HashCode)
            {
                if(all)
                {
                    for(int i = 0; i < sounds.Count; i++)
                    {
                        clip.CopyTo(sounds[i].clip, group);
                    }
                }
                else
                {
                    clip.CopyTo(sounds[0].clip, group);
                }
            }
        }

        private void ApplySettings()
        {
            MiniAudio.ma_sound_set_looping(sounds[0].clip.Sound, loop ? (UInt32)1 : 0);
        }

        private void SetAtEnd(int sourceIndex, bool atEnd)
        {
            sounds[sourceIndex].atEnd = atEnd;
        }

        private void OnGenerate(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);

            NativeArray<float> framesOut = new NativeArray<float>(pFramesOut, length);

            for (int i = 0; i < generators.Count; i++)
            {
                generators[i].OnGenerate(framesOut, frameCount, (int)channels);
            }

            Read?.Invoke(framesOut, frameCount, (int)channels);
        }

        private unsafe void OnEffect(ma_node_ptr pNode, IntPtr ppFramesIn, IntPtr pFrameCountIn, IntPtr ppFramesOut, IntPtr pFrameCountOut)
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

            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnProcess(bufferIn, countIn, bufferOut, ref countOut, channels);

                //Since effects processing is like a stack, the output needs to be copied to the input for the next effect
                bufferOut.CopyTo(bufferIn);

                countIn = countOut;
            }

            Process?.Invoke(bufferIn, countIn, bufferOut, ref countOut, pEffectNode->config.channels);

            *frameCountOut = countOut;
        }
    }
}