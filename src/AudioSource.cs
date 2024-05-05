using System;
using System.Collections.Concurrent;

namespace MiniAudioExNET
{
    public delegate void AudioLoadEvent(AudioSource source);
    public delegate void AudioEndEvent(AudioSource source);
    public delegate void AudioDSPEvent(Span<float> framesOut, UInt64 frameCount, Int32 channels);

    public sealed class AudioSource : IDisposable
    {
        public event AudioLoadEvent Load;
        public event AudioEndEvent End;
        public event AudioDSPEvent DSP;

        private IntPtr handle;
        private ma_sound_load_proc loadCallback;
        private ma_sound_end_proc endCallback;
        private ma_engine_node_dsp_proc dspCallback;
        private ConcurrentQueue<int> endEventQueue;

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
                return MiniAudioEx.ma_ex_audio_source_get_pcm_position(handle);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_pcm_position(handle, value);
            }
        }

        public UInt64 Length
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_pcm_length(handle);
            }
        }

        public float Volume
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_volume(handle);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_volume(handle, value);
            }
        }

        public float Pitch
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_pitch(handle);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_pitch(handle, value);
            }
        }

        public bool Loop
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_loop(handle) > 0;
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_loop(handle, value ? (uint)1 : 0);
            }
        }

        public bool Spatial
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_spatialization(handle) > 0;
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_spatialization(handle, value ? (uint)1 : 0);
            }
        }

        public float DopplerFactor
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_doppler_factor(handle);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_doppler_factor(handle, value);
            }
        }

        public float MinDistance
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_min_distance(handle);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_min_distance(handle, value);
            }
        }

        public float MaxDistance
        {
            get
            {
                return MiniAudioEx.ma_ex_audio_source_get_max_distance(handle);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_max_distance(handle, value);
            }
        }

        public AttenuationModel AttenuationModel
        {
            get
            {
                return (AttenuationModel)MiniAudioEx.ma_ex_audio_source_get_attenuation_model(handle);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_attenuation_model(handle, (ma_attenuation_model)value);
            }
        }

        public Vector3f Position
        {
            get
            {
                float x, y, z;
                MiniAudioEx.ma_ex_audio_source_get_position(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_position(handle, value.x, value.y, value.z);
            }
        }

        public Vector3f Direction
        {
            get
            {
                float x, y, z;
                MiniAudioEx.ma_ex_audio_source_get_direction(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_direction(handle, value.x, value.y, value.z);
            }
        }

        public Vector3f Velocity
        {
            get
            {
                float x, y, z;
                MiniAudioEx.ma_ex_audio_source_get_velocity(handle, out x, out y, out z);
                return new Vector3f(x, y, z);
            }
            set
            {
                MiniAudioEx.ma_ex_audio_source_set_velocity(handle, value.x, value.y, value.z);
            }
        }

        public AudioSource()
        {
            endEventQueue = new ConcurrentQueue<int>();

            handle = MiniAudioEx.ma_ex_audio_source_init(MiniAudioEx.AudioContext);

            if(handle != IntPtr.Zero)
            {
                loadCallback = OnLoad;
                endCallback = OnEnd;
                dspCallback = OnDSP;

                ma_ex_sound_callbacks callbacks = new ma_ex_sound_callbacks();
                callbacks.pUserData = handle;
                callbacks.dspCallback = dspCallback;
                callbacks.endCallback = endCallback;
                callbacks.loadCallback = loadCallback;

                MiniAudioEx.ma_ex_audio_source_set_callbacks(handle, callbacks);
                
                MiniAudioEx.Add(this);
            }
        }

        public void Dispose()
        {
            if(handle != IntPtr.Zero)
            {
                MiniAudioEx.ma_ex_audio_source_uninit(handle);
                handle = IntPtr.Zero;
                endEventQueue.Clear();
            }
        }

        public void Play()
        {
            MiniAudioEx.ma_ex_audio_source_play(handle);
        }

        public void Play(AudioClip clip)
        {
            if(clip.Handle != IntPtr.Zero)
                MiniAudioEx.ma_ex_audio_source_play_from_memory(handle, clip.Handle, clip.DataSize);
            else
                MiniAudioEx.ma_ex_audio_source_play_from_file(handle, clip.FilePath, clip.StreamFromDisk ? (uint)1 : 0);
        }

        public void Stop()
        {
            MiniAudioEx.ma_ex_audio_source_stop(handle);
        }

        internal void Update()
        {
            if(endEventQueue.Count > 0)
            {
                while(endEventQueue.TryDequeue(out int result))
                {
                    End?.Invoke(this);
                }
            }
        }

        private void OnLoad(IntPtr pUserData, IntPtr pSound)
        {
            Load?.Invoke(this);
        }

        private void OnEnd(IntPtr pUserData, IntPtr pSound)
        {
            //This callback is called from another thread so we move the message to a queue that the main thread can safely access
            //If the audio is set to looping, this event is never triggered
            endEventQueue.Enqueue(1);            
        }
        
        private void OnDSP(IntPtr pUserData, IntPtr pEngineNode, IntPtr pFramesOut, UInt64 frameCount, Int32 channels)
        {
            unsafe
            {
                int length = (int)frameCount * channels;
                Span<float> framesOut = new Span<float>(pFramesOut.ToPointer(), length);
                DSP?.Invoke(framesOut, frameCount, channels);
            }
        }
    }
}