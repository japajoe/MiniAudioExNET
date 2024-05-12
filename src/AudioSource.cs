using System;
using System.Collections.Concurrent;
#if NETSTANDARD2_0
using System.Runtime.InteropServices;
#endif

namespace MiniAudioExNET
{
    public delegate void AudioLoadEvent();
    public delegate void AudioEndEvent();

    //netstandard 2.0 needs some workaround since it does not have the Span<T> type
#if NETSTANDARD2_0
    public delegate void AudioProcessEvent(float[] framesOut, UInt64 frameCount, Int32 channels);
    public delegate void AudioReadEvent(float[] framesOut, UInt64 frameCount, Int32 channels);
#else
    public delegate void AudioProcessEvent(Span<float> framesOut, UInt64 frameCount, Int32 channels);
    public delegate void AudioReadEvent(Span<float> framesOut, UInt64 frameCount, Int32 channels);
#endif

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
        private ma_sound_load_proc loadCallback;
        private ma_sound_end_proc endCallback;
        private ma_sound_process_proc processCallback;
        private ma_waveform_proc waveformCallback;
        private ConcurrentQueue<int> endEventQueue;
#if NETSTANDARD2_0
        private float[] processBuffer;
        private float[] readBuffer;
#endif

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

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

        public UInt64 Length
        {
            get
            {
                return Library.ma_ex_audio_source_get_pcm_length(handle);
            }
        }

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
                Library.ma_ex_audio_source_set_position(handle, value.x, value.y, value.z);
            }
        }

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

        public AudioSource()
        {
            endEventQueue = new ConcurrentQueue<int>();

            handle = Library.ma_ex_audio_source_init(MiniAudioEx.AudioContext);

            if(handle != IntPtr.Zero)
            {
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

                //Clear the queue (netstandard2.0 does not have a Clear method for ConcurrentQueue)
                while(endEventQueue.Count > 0)
                {
                    endEventQueue.TryDequeue(out _);
                }
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

#if NETSTANDARD2_0
            if(processBuffer?.Length < length)
                processBuffer = new float[length];
            Array.Clear(processBuffer, 0, processBuffer.Length);
            Marshal.Copy(pFramesOut, readBuffer, 0, length);
            Process?.Invoke(processBuffer, frameCount, (int)channels);
            Marshal.Copy(processBuffer, 0, pFramesOut, length);
#else
            unsafe
            {
                Span<float> framesOut = new Span<float>(pFramesOut.ToPointer(), length);
                Process?.Invoke(framesOut, frameCount, (int)channels);
            }
#endif
        }

        private void OnWaveform(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);            

#if NETSTANDARD2_0
            if(readBuffer?.Length < length)
                readBuffer = new float[length];
            Array.Clear(readBuffer, 0, readBuffer.Length);
            Marshal.Copy(pFramesOut, readBuffer, 0, length);
            Read?.Invoke(readBuffer, frameCount, (int)channels);
            Marshal.Copy(readBuffer, 0, pFramesOut, length);
#else
            unsafe
            {
                Span<float> framesOut = new Span<float>(pFramesOut.ToPointer(), length);
                Read?.Invoke(framesOut, frameCount, (int)channels);
            }
#endif
        }
    }
}