using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
#if NETSTANDARD2_0
using MiniAudioExNET.Compatibility;
#endif

namespace MiniAudioExNET
{
    public delegate void AudioLoadEvent();
    public delegate void AudioEndEvent();

    public delegate void AudioProcessEvent(Span<float> framesOut, UInt64 frameCount, Int32 channels);
    public delegate void AudioReadEvent(Span<float> framesOut, UInt64 frameCount, Int32 channels);

    /// <summary>
    /// This class is used to play audio.
    /// </summary>
    public sealed class AudioSource : IDisposable
    {
        /// <summary>
        /// Callback handler for when an AudioClip is loaded using the 'Play(AudioClip clip)' method.
        /// </summary>
        public event AudioLoadEvent Load;
        /// <summary>
        /// Callback handler for when the playback has finished. This does not trigger if the AudioSource is set to Loop.
        /// </summary>
        public event AudioEndEvent End;
        /// <summary>
        /// Callback handler for implementing custom effects.
        /// </summary>
        public event AudioProcessEvent Process;
        /// <summary>
        /// Callback handler for generating procedural audio when using the 'Play()' method.
        /// </summary>
        public event AudioReadEvent Read;

        private IntPtr handle;
        private Vector3f previousPosition;
        private ma_sound_load_proc loadCallback;
        private ma_sound_end_proc endCallback;
        private ma_sound_process_proc processCallback;
        private ma_waveform_proc waveformCallback;
        private ConcurrentQueue<int> endEventQueue;
        private ThreadSafeQueue<IAudioEffect> effects;
        private ThreadSafeQueue<IAudioGenerator> generators;

        /// <summary>
        /// A handle to the native ma_audio_source instance.
        /// </summary>
        /// <value></value>
        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        /// <summary>
        /// The current position of the playback cursor in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 Cursor
        {
            get
            {
                return Library.ma_ex_audio_source_get_pcm_position(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_pcm_position(handle, value);
            }
        }

        /// <summary>
        /// The length of the playing audio clip in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 Length
        {
            get
            {
                return Library.ma_ex_audio_source_get_pcm_length(handle);
            }
        }

        /// <summary>
        /// Controls the volume of the sound.
        /// </summary>
        /// <value></value>
        public float Volume
        {
            get
            {
                return Library.ma_ex_audio_source_get_volume(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_volume(handle, value);
            }
        }

        /// <summary>
        /// Controls the pitch of the sound.
        /// </summary>
        /// <value></value>
        public float Pitch
        {
            get
            {
                return Library.ma_ex_audio_source_get_pitch(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_pitch(handle, value);
            }
        }

        /// <summary>
        /// Controls whether the audio should loop. If true, then the End event will not be called. If the Play() overload is used, this property has no effect because the audio will loop regardless.
        /// </summary>
        /// <value></value>
        public bool Loop
        {
            get
            {
                return Library.ma_ex_audio_source_get_loop(handle) > 0;
            }
            set
            {
                Library.ma_ex_audio_source_set_loop(handle, value ? (uint)1 : 0);
            }
        }

        /// <summary>
        /// Toggle to true to enable spatial audio
        /// </summary>
        /// <value></value>
        public bool Spatial
        {
            get
            {
                return Library.ma_ex_audio_source_get_spatialization(handle) > 0;
            }
            set
            {
                Library.ma_ex_audio_source_set_spatialization(handle, value ? (uint)1 : 0);
            }
        }

        /// <summary>
        /// Represents the intensity or strength of the simulated Doppler effect applied to the audio if Spatial is set to true.
        /// </summary>
        /// <value></value>
        public float DopplerFactor
        {
            get
            {
                return Library.ma_ex_audio_source_get_doppler_factor(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_doppler_factor(handle, value);
            }
        }

        /// <summary>
        /// Represents the distance from the audio source at which the volume of the audio starts to attenuate. Sounds closer to or within this minimum distance are heard at full volume without any attenuation applied.
        /// </summary>
        /// <value></value>
        public float MinDistance
        {
            get
            {
                return Library.ma_ex_audio_source_get_min_distance(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_min_distance(handle, value);
            }
        }

        /// <summary>
        /// Represents the distance from the audio source beyond which the audio is no longer audible or significantly attenuated. Sounds beyond this maximum distance are either inaudible or heard at greatly reduced volume.
        /// </summary>
        /// <value></value>
        public float MaxDistance
        {
            get
            {
                return Library.ma_ex_audio_source_get_max_distance(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_max_distance(handle, value);
            }
        }

        /// <summary>
        /// Defines the mathematical model used to simulate the attenuation of sound over distance.
        /// </summary>
        /// <value></value>
        public AttenuationModel AttenuationModel
        {
            get
            {
                return (AttenuationModel)Library.ma_ex_audio_source_get_attenuation_model(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_attenuation_model(handle, (ma_attenuation_model)value);
            }
        }

        /// <summary>
        /// The position of the source, used for calculating spatial sound.
        /// </summary>
        /// <value></value>
        public Vector3f Position
        {
            get
            {
                float x, y, z;
                Library.ma_ex_audio_source_get_position(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                Library.ma_ex_audio_source_get_position(handle, out previousPosition.x, out previousPosition.y, out previousPosition.z);
                Library.ma_ex_audio_source_set_position(handle, value.x, value.y, value.z);
            }
        }

        /// <summary>
        /// The direction of the source, used for calculation spatial sound.
        /// </summary>
        /// <value></value>
        public Vector3f Direction
        {
            get
            {
                float x, y, z;
                Library.ma_ex_audio_source_get_direction(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                Library.ma_ex_audio_source_set_direction(handle, value.x, value.y, value.z);
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
                float x, y, z;
                Library.ma_ex_audio_source_get_velocity(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                Library.ma_ex_audio_source_set_velocity(handle, value.x, value.y, value.z);
            }
        }

        public bool IsPlaying
        {
            get
            {
                return Library.ma_ex_audio_source_get_is_playing(handle) > 0;
            }
        }

        public AudioSource()
        {
            handle = Library.ma_ex_audio_source_init(MiniAudioEx.AudioContext);

            if(handle != IntPtr.Zero)
            {
                previousPosition = new Vector3f(0, 0, 0);
                endEventQueue = new ConcurrentQueue<int>();
                effects = new ThreadSafeQueue<IAudioEffect>();
                generators = new ThreadSafeQueue<IAudioGenerator>();

                loadCallback = OnLoad;
                endCallback = OnEnd;
                processCallback = OnProcess;
                waveformCallback = OnWaveform;

                ma_ex_audio_source_callbacks callbacks = new ma_ex_audio_source_callbacks();
                callbacks.pUserData = handle;
                callbacks.processCallback = processCallback;
                callbacks.endCallback = endCallback;
                callbacks.loadCallback = loadCallback;
                callbacks.waveformCallback = waveformCallback;

                Library.ma_ex_audio_source_set_callbacks(handle, callbacks);
                
                MiniAudioEx.Add(this);
            }
        }

        internal void Destroy()
        {
            if(handle != IntPtr.Zero)
            {
                Library.ma_ex_audio_source_uninit(handle);
                handle = IntPtr.Zero;

                //Clear the queues (netstandard2.0 does not have a Clear method for ConcurrentQueue)
                while(endEventQueue.Count > 0)
                    endEventQueue.TryDequeue(out _);
                
                effects.Clear();
                generators.Clear();
            }
        }

        public void Dispose()
        {
            MiniAudioEx.Remove(this);
        }

        /// <summary>
        /// Use this method if you registered a method to the Read callback to generate audio.
        /// </summary>
        public void Play()
        {
            Library.ma_ex_audio_source_play(handle);
        }

        /// <summary>
        /// Plays an AudioClip by given filepath or encoded data buffer (data must be either WAV/MP3/FLAC).
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        public void Play(AudioClip clip)
        {
            if(clip.Handle != IntPtr.Zero)
                Library.ma_ex_audio_source_play_from_memory(handle, clip.Handle, clip.DataSize);
            else
                Library.ma_ex_audio_source_play_from_file(handle, clip.FilePath, clip.StreamFromDisk ? (uint)1 : 0);
        }

        /// <summary>
        /// Stop the AudioSource playback, note that this method does not set the cursor back to 0.
        /// </summary>
        public void Stop()
        {
            Library.ma_ex_audio_source_stop(handle);
        }

        internal void Update()
        {
            if(endEventQueue.Count > 0)
            {
                while(endEventQueue.TryDequeue(out int result))
                {
                    End?.Invoke();
                }
            }
        }

        public void AddEffect(IAudioEffect effect)
        {
            effects.Add(effect);
        }

        public void RemoveEffect(IAudioEffect effect)
        {
            effects.Remove(effect);
        }

        public void AddGenerator(IAudioGenerator generator)
        {
            generators.Add(generator);
        }

        public void RemoveGenerator(IAudioGenerator generator)
        {
            generators.Remove(generator);
        }

        /// <summary>
        /// Calculates the velocity based on the current position and the previous position.
        /// </summary>
        /// <returns></returns>
        public Vector3f GetCalculatedVelocity()
        {
            float deltaTime = MiniAudioEx.DeltaTime;
            Vector3f currentPosition = Position;
            float dx = currentPosition.x - previousPosition.x;
            float dy = currentPosition.y - previousPosition.y;
            float dz = currentPosition.z - previousPosition.z;
            return new Vector3f(dx / deltaTime, dy / deltaTime, dz / deltaTime);
        }

        private void OnLoad(IntPtr pUserData, IntPtr pSound)
        {
            Load?.Invoke();
        }

        private void OnEnd(IntPtr pUserData, IntPtr pSound)
        {
            //This callback is called from another thread so we move the message to a queue that the main thread can safely access
            //If the audio is set to looping, this event is never triggered
            endEventQueue.Enqueue(1);            
        }
        
        private void OnProcess(IntPtr pUserData, IntPtr pSound, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);

            unsafe
            {
                Span<float> framesOut = new Span<float>(pFramesOut.ToPointer(), length);

                for(int i = 0; i < effects.Count; i++)
                {
                    effects[i].OnProcess(framesOut, frameCount, (int)channels);
                }

                Process?.Invoke(framesOut, frameCount, (int)channels);
            }

            effects.Flush();
        }

        private void OnWaveform(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);            

            unsafe
            {
                Span<float> framesOut = new Span<float>(pFramesOut.ToPointer(), length);

                for(int i = 0; i < generators.Count; i++)
                {
                    generators[i].OnGenerate(framesOut, frameCount, (int)channels);
                }

                Read?.Invoke(framesOut, frameCount, (int)channels);
            }

            generators.Flush();
        }
    }

    public interface IAudioEffect
    {
        void OnProcess(Span<float> framesOut, UInt64 frameCount, Int32 channels);
    }

    public interface IAudioGenerator
    {
        void OnGenerate(Span<float> framesOut, UInt64 frameCount, Int32 channels);
    }

    public sealed class ThreadSafeQueue<T>
    {
        private ConcurrentQueue<T> addQueue;
        private ConcurrentQueue<T> removeQueue;
        private List<T> items;

        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= items.Count)
                    throw new IndexOutOfRangeException();
                return items[index];
            }
            set
            {
                if (index < 0 || index >= items.Count)
                    throw new IndexOutOfRangeException();
                items[index] = value;
            }
        }

        public ThreadSafeQueue()
        {
            this.addQueue = new ConcurrentQueue<T>();
            this.removeQueue = new ConcurrentQueue<T>();
            this.items = new List<T>();
        }

        public void Clear()
        {
            while(addQueue.Count > 0)
                addQueue.TryDequeue(out _);
            while(removeQueue.Count > 0)
                removeQueue.TryDequeue(out _);
            items.Clear();
        }

        public void Add(T item)
        {
            addQueue.Enqueue(item);
        }

        public void Remove(T item)
        {
            removeQueue.Enqueue(item);
        }

        public void Flush()
        {
            if(addQueue.Count > 0)
            {
                while(addQueue.TryDequeue(out T item))
                {
                    items.Add(item);
                }
            }

            if(removeQueue.Count > 0)
            {
                while(removeQueue.TryDequeue(out T item))
                {
                    items.Remove(item);
                }
            }
        }
    }
}