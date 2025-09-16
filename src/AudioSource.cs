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
using System.Collections.Concurrent;
using System.Collections.Generic;
using MiniAudioEx.Core;

namespace MiniAudioEx
{
    public delegate void AudioEndEvent();
    public delegate void AudioProcessEvent(AudioBuffer<float> framesOut, UInt64 frameCount, Int32 channels);
    public delegate void AudioReadEvent(AudioBuffer<float> framesOut, UInt64 frameCount, Int32 channels);

    /// <summary>
    /// This class is used to play sounds.
    /// </summary>
    public sealed class AudioSource : IDisposable
    {
        /// <summary>
        /// Callback handler for implementing custom effects.
        /// </summary>
        public event AudioProcessEvent Process;
        /// <summary>
        /// Callback handler for when the playback has finished. This does not trigger if the AudioSource is set to Loop.
        /// </summary>
        public event AudioEndEvent End;
        /// <summary>
        /// Callback handler for generating procedural audio when using the 'Play()' method.
        /// </summary>
        public event AudioReadEvent Read;

        private List<IntPtr> sources;
        private ma_sound_group_ptr soundGroup;
        private Vector3f previousPosition;
        private ma_sound_process_proc processCallback;
        private ma_sound_end_proc endCallback;
        private ma_procedural_sound_proc proceduralProcessCallback;
        private ConcurrentQueue<IntPtr> endEventQueue;
        private ConcurrentList<IAudioEffect> effects;
        private ConcurrentList<IAudioGenerator> generators;
        private int currentIndex;
        private readonly int MAX_SOURCES;

        /// <summary>
        /// Gets or sets the current position of the playback cursor in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 Cursor
        {
            get
            {
                return MiniAudioExNative.ma_ex_audio_source_get_pcm_position(sources[0]);
            }
            set
            {
                MiniAudioExNative.ma_ex_audio_source_set_pcm_position(sources[0], value);
            }
        }

        /// <summary>
        /// Gets the length of the playing audio clip in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 Length
        {
            get
            {
                return MiniAudioExNative.ma_ex_audio_source_get_pcm_length(sources[0]);
            }
        }

        /// <summary>
        /// Gets or sets the volume of the sound.
        /// </summary>
        /// <value></value>
        public float Volume
        {
            get
            {
                return MiniAudioNative.ma_sound_group_get_volume(soundGroup);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_volume(soundGroup, value);
            }
        }

        /// <summary>
        /// Gets or sets the pitch of the sound.
        /// </summary>
        /// <value></value>
        public float Pitch
        {
            get
            {
                return MiniAudioNative.ma_sound_group_get_pitch(soundGroup);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_pitch(soundGroup, value);
            }
        }

        /// <summary>
        /// Gets or sets whether the audio should loop. If true, then the 'End' event will not be called. If the Play() overload is used, this property has no effect because the audio will loop regardless. Take note that this setting only applies to the first source, so it may cause undesirable results when using the PlayOneShot method.
        /// </summary>
        /// <value></value>
        public bool Loop
        {
            get
            {
                return MiniAudioExNative.ma_ex_audio_source_get_loop(sources[0]) > 0;
            }
            set
            {
                MiniAudioExNative.ma_ex_audio_source_set_loop(sources[0], value ? (uint)1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets whether this source has spatial audio enabled or not.
        /// </summary>
        /// <value></value>
        public bool Spatial
        {
            get
            {
                return MiniAudioNative.ma_sound_group_is_spatialization_enabled(soundGroup) > 0;
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_spatialization_enabled(soundGroup, value ? (uint)1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets the intensity or strength of the simulated Doppler effect applied to the audio if Spatial is set to true.
        /// </summary>
        /// <value></value>
        public float DopplerFactor
        {
            get
            {
                return MiniAudioNative.ma_sound_group_get_doppler_factor(soundGroup);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_doppler_factor(soundGroup, value);
            }
        }

        /// <summary>
        /// Gets or sets the distance from the audio source at which the volume of the audio starts to attenuate. Sounds closer to or within this minimum distance are heard at full volume without any attenuation applied.
        /// </summary>
        /// <value></value>
        public float MinDistance
        {
            get
            {
                return MiniAudioNative.ma_sound_group_get_min_distance(soundGroup);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_min_distance(soundGroup, value);
            }
        }

        /// <summary>
        /// Gets or sets the distance from the audio source beyond which the audio is no longer audible or significantly attenuated. Sounds beyond this maximum distance are either inaudible or heard at greatly reduced volume.
        /// </summary>
        /// <value></value>
        public float MaxDistance
        {
            get
            {
                return MiniAudioNative.ma_sound_group_get_max_distance(soundGroup);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_max_distance(soundGroup, value);
            }
        }

        /// <summary>
        /// Gets or sets the mathematical model used to simulate the attenuation of sound over distance.
        /// </summary>
        /// <value></value>
        public AttenuationModel AttenuationModel
        {
            get
            {
                return (AttenuationModel)MiniAudioNative.ma_sound_group_get_attenuation_model(soundGroup);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_attenuation_model(soundGroup, (ma_attenuation_model)value);
            }
        }

        /// <summary>
        /// Gets or sets the position of the source, used for calculating spatial sound.
        /// </summary>
        /// <value></value>
        public Vector3f Position
        {
            get
            {
                var result = MiniAudioNative.ma_sound_group_get_position(soundGroup);
                return new Vector3f(result.x, result.y, result.z);
            }
            set
            {
                var p = MiniAudioNative.ma_sound_group_get_position(soundGroup);
                previousPosition.x = p.x;
                previousPosition.y = p.y;
                previousPosition.z = p.z;
                MiniAudioNative.ma_sound_group_set_position(soundGroup, value.x, value.y, value.z);
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
                var result = MiniAudioNative.ma_sound_group_get_direction(soundGroup);
                return new Vector3f(result.x, result.y, result.z);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_direction(soundGroup, value.x, value.y, value.z);
            }
        }

        /// <summary>
        /// The velocity of the source, used for calculating spatial sound.
        /// </summary>
        /// <value></value>
        public Vector3f Velocity
        {
            get
            {
                var result = MiniAudioNative.ma_sound_group_get_velocity(soundGroup);
                return new Vector3f(result.x, result.y, result.z);
            }
            set
            {
                MiniAudioNative.ma_sound_group_set_velocity(soundGroup, value.x, value.y, value.z);
            }
        }

        /// <summary>
        /// Indicates whether the source is playing audio or not.
        /// </summary>
        /// <value></value>
        public bool IsPlaying
        {
            get
            {
                return MiniAudioNative.ma_sound_group_is_playing(soundGroup) > 0;
            }
        }

        /// <summary>
        /// Creates and AudioSourceGroup instance.
        /// </summary>
        /// <param name="maxSources">The maximum number of sounds that can be played simultaneuously.</param>
        public AudioSource(Int32 maxSources = 16)
        {
            if (maxSources < 1)
                maxSources = 1;

            MAX_SOURCES = maxSources;

            previousPosition = new Vector3f(0, 0, 0);
            sources = new List<IntPtr>();
            endEventQueue = new ConcurrentQueue<IntPtr>();
            effects = new ConcurrentList<IAudioEffect>();
            generators = new ConcurrentList<IAudioGenerator>();
            currentIndex = 0;

            processCallback = OnProcess;
            endCallback = OnEnd;
            proceduralProcessCallback = OnProceduralProcess;

            soundGroup.pointer = MiniAudioExNative.ma_ex_sound_group_init(AudioContext.NativeContext);

            if (soundGroup.pointer != IntPtr.Zero)
            {
                AudioContext.Add(this);

                ma_sound_ptr s = new ma_sound_ptr();
                s.pointer = soundGroup.pointer;

                MiniAudioNative.ma_sound_set_notifications_userdata(s, IntPtr.Zero);
                MiniAudioNative.ma_sound_set_process_notification_callback(s, processCallback);

                for (int i = 0; i < MAX_SOURCES; i++)
                {
                    IntPtr source = MiniAudioExNative.ma_ex_audio_source_init(AudioContext.NativeContext);
                    sources.Add(source);

                    ma_ex_audio_source_callbacks callbacks = new ma_ex_audio_source_callbacks();
                    callbacks.pUserData = source;
                    callbacks.endCallback = endCallback;
                    callbacks.loadCallback = null;
                    callbacks.processCallback = null;

                    MiniAudioExNative.ma_ex_audio_source_set_callbacks(source, callbacks);
                    MiniAudioExNative.ma_ex_audio_source_set_group(source, soundGroup.pointer);
                }
            }
        }

        internal void Destroy()
        {
            if (soundGroup.pointer == IntPtr.Zero)
                return;

            for (int i = 0; i < sources.Count; i++)
            {
                MiniAudioExNative.ma_ex_audio_source_stop(sources[i]);
                MiniAudioExNative.ma_ex_audio_source_uninit(sources[i]);
            }

            sources.Clear();

            MiniAudioExNative.ma_ex_sound_group_uninit(soundGroup.pointer);
            soundGroup.pointer = IntPtr.Zero;

            //Clear the queues (netstandard2.0 does not have a Clear method for ConcurrentQueue)
            while (endEventQueue.Count > 0)
                endEventQueue.TryDequeue(out _);

            for (int i = 0; i < effects.Count; i++)
                effects[i].OnDestroy();

            for (int i = 0; i < generators.Count; i++)
                generators[i].OnDestroy();

            effects.Clear();
            generators.Clear();
        }

        public void Dispose()
        {
            AudioContext.Remove(this);
        }

        /// <summary>
        /// Use this method if you registered a method to the Read callback to generate audio.
        /// </summary>
        public void Play()
        {
            if (soundGroup.pointer == IntPtr.Zero)
                return;
            MiniAudioExNative.ma_ex_audio_source_play_from_callback(sources[0], proceduralProcessCallback);
        }

        /// <summary>
        /// Plays an AudioClip by given filepath or encoded data buffer (data must be either WAV/MP3/FLAC/OGG).
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        public void Play(AudioClip clip)
        {
            if (soundGroup.pointer == IntPtr.Zero)
                return;

            if (clip.Handle != IntPtr.Zero)
                MiniAudioExNative.ma_ex_audio_source_play_from_memory(sources[0], clip.Handle, clip.DataSize);
            else
                MiniAudioExNative.ma_ex_audio_source_play_from_file(sources[0], clip.FilePath, clip.StreamFromDisk ? (uint)1 : 0);
        }

        /// <summary>
        /// Plays an AudioClip by given filepath or encoded data buffer (data must be either WAV/MP3/FLAC/OGG).
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        public void PlayOneShot(AudioClip clip)
        {
            if (soundGroup.pointer == IntPtr.Zero)
                return;

            if (MiniAudioExNative.ma_ex_audio_source_get_is_playing(sources[currentIndex]) > 0)
            {
                MiniAudioExNative.ma_ex_audio_source_stop(sources[currentIndex]);
                MiniAudioExNative.ma_ex_audio_source_set_pcm_position(sources[currentIndex], 0);
            }

            if (clip.Handle != IntPtr.Zero)
                MiniAudioExNative.ma_ex_audio_source_play_from_memory(sources[currentIndex], clip.Handle, clip.DataSize);
            else
                MiniAudioExNative.ma_ex_audio_source_play_from_file(sources[currentIndex], clip.FilePath, clip.StreamFromDisk ? (uint)1 : 0);

            if (++currentIndex >= sources.Count - 1)
            {
                currentIndex = 0;
            }
        }

        /// <summary>
        /// Stop the AudioSource playback, note that this method does not set the cursor back to 0 so it could be used as a pause method.
        /// </summary>
        public void Stop()
        {
            MiniAudioNative.ma_sound_group_stop(soundGroup);
        }

        internal void Update()
        {
            if (endEventQueue.Count > 0)
            {
                while (endEventQueue.TryDequeue(out _))
                {
                    End?.Invoke();
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
            List<IAudioEffect> targets = new List<IAudioEffect>();
            for (int i = 0; i < effects.Count; i++)
            {
                targets.Add(effects[i]);
            }
            if (targets.Count > 0)
            {
                effects.Remove(targets);
            }
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
            List<IAudioGenerator> targets = new List<IAudioGenerator>();
            for (int i = 0; i < generators.Count; i++)
            {
                targets.Add(generators[i]);
            }
            if (targets.Count > 0)
            {
                generators.Remove(targets);
            }
        }

        /// <summary>
        /// Calculates the velocity based on the current position and the previous position.
        /// </summary>
        /// <returns></returns>
        public Vector3f GetCalculatedVelocity()
        {
            float deltaTime = AudioContext.DeltaTime;
            Vector3f currentPosition = Position;
            float dx = currentPosition.x - previousPosition.x;
            float dy = currentPosition.y - previousPosition.y;
            float dz = currentPosition.z - previousPosition.z;
            return new Vector3f(dx / deltaTime, dy / deltaTime, dz / deltaTime);
        }

        /// <summary>
        /// Called whenever audio has finished playing. This does not trigger when 'Loop' is true.
        /// </summary>
        /// <param name="pUserData"></param>
        /// <param name="pSound"></param>
        private void OnEnd(IntPtr pUserData, ma_sound_ptr pSound)
        {
            //This callback is called from another thread so we move the message to a queue that the main thread can safely access
            //If the audio is set to looping, this event is never triggered
            endEventQueue.Enqueue(pUserData);
        }

        /// <summary>
        /// Called whenever the audio buffer of this source is filled with data.
        /// </summary>
        /// <param name="pUserData"></param>
        /// <param name="pSound"></param>
        /// <param name="pFramesOut"></param>
        /// <param name="frameCount"></param>
        /// <param name="channels"></param>
        private void OnProcess(IntPtr pUserData, ma_sound_ptr pSound, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);

            AudioBuffer<float> framesOut = new AudioBuffer<float>(pFramesOut, length);

            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnProcess(framesOut, frameCount, (int)channels);
            }

            Process?.Invoke(framesOut, frameCount, (int)channels);
        }

        /// <summary>
        /// Called whenever the buffer of this source needs data. This is only called whenever the 'Play' method without parameters was used.
        /// </summary>
        /// <param name="pUserData"></param>
        /// <param name="pFramesOut"></param>
        /// <param name="frameCount"></param>
        /// <param name="channels"></param>
        private void OnProceduralProcess(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);

            AudioBuffer<float> framesOut = new AudioBuffer<float>(pFramesOut, length);

            for (int i = 0; i < generators.Count; i++)
            {
                generators[i].OnGenerate(framesOut, frameCount, (int)channels);
            }

            Read?.Invoke(framesOut, frameCount, (int)channels);
        }
    }

    /// <summary>
    /// An interface for implementing audio effects. These effects can be added to an AudioSource by using the AddEffect method.
    /// </summary>
    public interface IAudioEffect
    {
        void OnProcess(AudioBuffer<float> framesOut, UInt64 frameCount, Int32 channels);
        void OnDestroy();
    }

    /// <summary>
    /// An interface for implementing audio generators. These generators can be added to an AudioSource by using the AddGenerator method.
    /// </summary>
    public interface IAudioGenerator
    {
        void OnGenerate(AudioBuffer<float> framesOut, UInt64 frameCount, Int32 channels);
        void OnDestroy();
    }
}