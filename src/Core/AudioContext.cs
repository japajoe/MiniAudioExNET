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

namespace MiniAudioEx.Core
{   
    public delegate void LogEventHandler(UInt32 level, string message);

    /// <summary>
    /// This class is responsible for managing the audio context. Before creating any AudioSource/AudioListener/AudioClip, an instance of this class has to be created and made current. The AudioContext must outlive any AudioSource/AudioListener/AudioClip.
    /// </summary>
    public sealed class AudioContext : IDisposable
    {
        public event LogEventHandler Log;

		private ma_log_ptr log;
        private ma_context_ptr context;
        private ma_resource_manager_ptr resourceManager;
        private ma_device_ptr device;
        private ma_engine_ptr engine;
		private ma_log_callback_proc onLog;
        private ma_log_callback logCallback;
        private ma_device_data_proc deviceDataProc;
        private UInt32 channels;
        private UInt32 sampleRate;
        private UInt32 periodSizeInFrames;
        private AudioDevice audioDevice;
        private List<AudioSource> sources;
        private float deltaTime;
        private DateTime lastUpdateTime;
        private static AudioContext current;

        public ma_context_ptr Context => context;
        public ma_engine_ptr Engine => engine;
        public UInt32 Channels => channels;
        public UInt32 SampleRate => sampleRate;
        public float DeltaTime => deltaTime;

        public AudioContext(UInt32 sampleRate = 44100, UInt32 channels = 2, UInt32 periodSizeInFrames = 2048, AudioDevice audioDevice = null)
        {
            this.sampleRate = Math.Max(sampleRate, 1);
            this.channels = Math.Max(channels, 1);
            this.periodSizeInFrames = Math.Max(periodSizeInFrames, 1);
            this.audioDevice = audioDevice;
            sources = new List<AudioSource>();
            deltaTime = 0.0f;
            lastUpdateTime = DateTime.Now;
        }

        public void Create()
        {
            log = new ma_log_ptr(true);
            engine = new ma_engine_ptr(true);
            context = new ma_context_ptr(true);
            device = new ma_device_ptr(true);
            resourceManager = new ma_resource_manager_ptr(true);
            deviceDataProc = OnDeviceData;

            if (MiniAudio.ma_log_init(log) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize log");
            }

			logCallback = new ma_log_callback();
            onLog = OnLog;
			logCallback.SetLogCallback(onLog);
			
            if (MiniAudio.ma_log_register_callback(log, logCallback) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize log callback");
            }

            ma_context_config contextConfig = MiniAudio.ma_context_config_init();
            contextConfig.pLog = log;

            if (MiniAudio.ma_context_init(null, ref contextConfig, context) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to create context");
            }

            ma_device_config deviceConfig = MiniAudio.ma_device_config_init(ma_device_type.playback);
            deviceConfig.playback.format = ma_format.f32;
            deviceConfig.playback.channels = channels;
            deviceConfig.playback.pDeviceID = new ma_device_id_ptr(true);
            deviceConfig.sampleRate = sampleRate;
            deviceConfig.periodSizeInFrames = periodSizeInFrames;
            deviceConfig.SetDataCallback(deviceDataProc);

            if(audioDevice == null)
            {
                if (MiniAudio.ma_context_get_devices(context, out ma_device_info[] ppPlaybackDeviceInfos, out ma_device_info[] ppCaptureDeviceInfos) != ma_result.success)
                {
                    Dispose();
                    throw new Exception("Failed to get devices");
                }

                if (ppPlaybackDeviceInfos?.Length > 0)
                {
                    for (int i = 0; i < ppPlaybackDeviceInfos.Length; i++)
                    {
                        if (ppPlaybackDeviceInfos[i].isDefault > 0)
                        {
                            unsafe
                            {
                                *deviceConfig.playback.pDeviceID.Get() = ppPlaybackDeviceInfos[i].id;
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    *deviceConfig.playback.pDeviceID.Get() = audioDevice.info.id;
                }
            }

            if (MiniAudio.ma_device_init(context, ref deviceConfig, device) != ma_result.success)
            {
                deviceConfig.playback.pDeviceID.Free();
                Dispose();
                throw new Exception("Failed to initialize device");
            }

            deviceConfig.playback.pDeviceID.Free();

            ma_decoding_backend_vtable_ptr[] vtables = {
                MiniAudio.ma_libvorbis_get_decoding_backend_ptr()
            };

            ma_resource_manager_config resourceManagerConfig = MiniAudio.ma_resource_manager_config_init();
            resourceManagerConfig.SetCustomDecodingBackendVTables(vtables);

            if (MiniAudio.ma_resource_manager_init(ref resourceManagerConfig, resourceManager) != ma_result.success)
            {
                resourceManagerConfig.FreeCustomDecodingBackendVTables();
                Dispose();
                throw new Exception("Failed to initialize ma_resource_manager");
            }

            resourceManagerConfig.FreeCustomDecodingBackendVTables();

            ma_engine_config engineConfig = MiniAudio.ma_engine_config_init();
            engineConfig.listenerCount = MiniAudio.MA_ENGINE_MAX_LISTENERS;
            engineConfig.pDevice = device;
            engineConfig.pResourceManager = resourceManager;

            if (MiniAudio.ma_engine_init(ref engineConfig, engine) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize ma_engine");
            }

            unsafe
            {
                device.Get()->pUserData = engine.pointer;
            }

            if (MiniAudio.ma_device_start(device) != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to start ma_device");
            }
        }

        internal void Add(AudioSource source)
        {
            for(int i = 0; i < sources.Count; i++)
            {
                if(sources[i].GetHashCode() == source.GetHashCode())
                    return;
            }

            sources.Add(source);
        }

        internal void Remove(AudioSource source)
        {
            int index = -1;

            for(int i = 0; i < sources.Count; i++)
            {
                if(sources[i].GetHashCode() == source.GetHashCode())
                {
                    index = i;
                    break;
                }
            }

            if(index >= 0)
                sources.RemoveAt(index);
        }

        public void Dispose()
        {
			MiniAudio.ma_engine_uninit(engine);
			MiniAudio.ma_device_uninit(device);
			MiniAudio.ma_context_uninit(context);
			MiniAudio.ma_resource_manager_uninit(resourceManager);
            MiniAudio.ma_log_uninit(log);

			engine.Free();
			context.Free();
			device.Free();
			resourceManager.Free();
            log.Free();

            sources.Clear();

            if(current == this)
                current = null;
        }

        public void Update()
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan dt = currentTime - lastUpdateTime;
            deltaTime = (float)dt.TotalSeconds;
            lastUpdateTime = currentTime;

            for(int i = 0; i < sources.Count; i++)
                sources[i].Update();
        }

		private unsafe void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
		{
            ma_device* device = pDevice.Get();

            if (device == null)
                return;

            ma_engine_ptr pEngine = new ma_engine_ptr(device->pUserData);

            MiniAudio.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
		}

		private void OnLog(IntPtr pUserData, UInt32 level, IntPtr pMessage)
		{
			string message = MarshalHelper.PtrToStringUTF8(pMessage);
			Log?.Invoke(level, message);
		}

        public void MakeCurrent()
        {
            current = this;
        }

        public static AudioContext GetCurrent()
        {
            return current;
        }
    }

    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }
        }

        /// <summary>
        /// Gets the squared length of the vector.
        /// </summary>
        public float LengthSquared
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }

        /// <summary>
        /// Returns a vector with components set to zero.
        /// </summary>
        public static Vector3f Zero
        {
            get
            {
                return new Vector3f(0, 0, 0);
            }
        }

        /// <summary>
        /// Returns a vector with all components set to one.
        /// </summary>
        public static Vector3f One
        {
            get
            {
                return new Vector3f(1, 1, 1);
            }
        }

        /// <summary>
        /// Gets the unit vector along the X-axis.
        /// </summary>
        public static Vector3f UnitX
        {
            get
            {
                return new Vector3f(1, 0, 0);
            }
        }

        /// <summary>
        /// Gets the unit vector along the Y-axis.
        /// </summary>
        public static Vector3f UnitY
        {
            get
            {
                return new Vector3f(0, 1, 0);
            }
        }

        /// <summary>
        /// Gets the unit vector along the Z-axis.
        /// </summary>
        public static Vector3f UnitZ
        {
            get
            {
                return new Vector3f(0, 0, 1);
            }
        }

        /// <summary>
        /// Constructs a new Vector3f with the specified components.
        /// </summary>
        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        public void Normalize()
        {
            float scale = 1.0f / Length;
            x *= scale;
            y *= scale;
            z *= scale;
        }

        /// <summary>
        /// Returns the normalized vector of the input vector.
        /// </summary>
        public static Vector3f Normalize(Vector3f v)
        {
            float scale = 1.0f / v.Length;
            v.x = v.x *= scale;
            v.y = v.y *= scale;
            v.z = v.z *= scale;
            return v;
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        public static float Distance(Vector3f a, Vector3f b)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float dz = b.z - a.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        public static float DistanceSquared(Vector3f a, Vector3f b)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        public static float Dot(Vector3f a, Vector3f b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        public static Vector3f Cross(Vector3f a, Vector3f b)
        {
            float x = a.y * b.z - a.z * b.y;
            float y = a.z * b.x - a.x * b.z;
            float z = a.x * b.y - a.y * b.x;
            return new Vector3f(x, y, z);
        }

        /// <summary>
        /// Performs linear interpolation between two vectors.
        /// </summary>
        public static Vector3f Lerp(Vector3f a, Vector3f b, float t)
        {
            float x = a.x + (b.x - a.x) * t;
            float y = a.y + (b.y - a.y) * t;
            float z = a.z + (b.z - a.z) * t;
            return new Vector3f(x, y, z);
        }

        // Helper method for clamping values between min and max.
        private static float Clamp(float n, float min, float max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        /// <summary>
        /// Calculates the angle between two vectors in radians.
        /// </summary>
        public static float Angle(Vector3f a, Vector3f b)
        {
            float temp = Dot(a, b);
            return (float)Math.Acos(Clamp(temp / (a.Length * b.Length), -1.0f, 1.0f));
        }

        public static Vector3f operator +(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3f operator -(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3f operator -(Vector3f a)
        {
            return new Vector3f(-a.x, -a.y, -a.z);
        }

        public static Vector3f operator *(Vector3f a, float scalar)
        {
            return new Vector3f(a.x * scalar, a.y * scalar, a.z * scalar);
        }

        public static Vector3f operator *(float scalar, Vector3f a)
        {
            return a * scalar;
        }

        public static Vector3f operator /(Vector3f a, float scalar)
        {
            a.x /= scalar;
            a.y /= scalar;
            a.z /= scalar;
            return a;
        }

        public static bool operator ==(Vector3f a, Vector3f b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Vector3f a, Vector3f b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3f))
                return false;

            Vector3f other = (Vector3f)obj;
            return this == other;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }

        public override int GetHashCode()
        {
            int hash = 42;
            hash = hash ^ x.GetHashCode();
            hash = hash ^ y.GetHashCode();
            hash = hash ^ z.GetHashCode();
            return hash;
        }
    }
}