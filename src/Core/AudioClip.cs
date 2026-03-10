using System;
using System.Runtime.InteropServices;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core
{
    public sealed class AudioClip : IDisposable
    {
        private ma_sound_ptr sound;
        private string filePath;
        private IntPtr dataHandle;
        private UInt64 dataLength;
        private bool streamFromDisk;
        private UInt64 pcmLength;
        private UInt64 hashCode;
        public ma_sound_ptr Sound => sound;

        /// <summary>
        /// Gets the length of the audio clip in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 LengthSamples => pcmLength;
        public UInt64 HashCode => hashCode;

        public AudioClip()
        {
            sound = new ma_sound_ptr(true);
            dataHandle = IntPtr.Zero;
            dataLength = 0;
            streamFromDisk = false;
            pcmLength = 0;
            hashCode = 0;
        }

        public AudioClip(string filePath, bool streamFromDisk = true)
        {
            AudioContext context = AudioContext.GetCurrent();

            if(context == null)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because AudioContext is null");
            }

            if(!System.IO.File.Exists(filePath))
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because the file does not exist");
            }

            this.filePath = filePath;
            this.streamFromDisk = streamFromDisk;
            dataHandle = IntPtr.Zero;
            dataLength = 0;
            
            sound = new ma_sound_ptr(true);

            ma_sound_flags flags = streamFromDisk ? ma_sound_flags.stream : ma_sound_flags.decode;
            ma_result result = ma_result.success;
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                result = MiniAudio.ma_sound_init_from_file_w(context.Engine, filePath, flags, default, default, sound);
            else
                result = MiniAudio.ma_sound_init_from_file(context.Engine, filePath, flags, default, default, sound);

            if (result != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip: " + result);
            }

            MiniAudio.ma_sound_get_length_in_pcm_frames(sound, out pcmLength);

            hashCode = (UInt64)filePath.GetHashCode();
        }

        public AudioClip(byte[] data)
        {
            AudioContext context = AudioContext.GetCurrent();

            if(context == null)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because AudioContext is null");
            }

            if(data == null)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because the given data is null");
            }

            sound = new ma_sound_ptr(true);
            streamFromDisk = false;
            dataHandle = Marshal.AllocHGlobal(data.Length);
            dataLength = (UInt64)data.Length;

            if(dataHandle == IntPtr.Zero)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip because data could not be allocated");
            }

            Marshal.Copy(data, 0, dataHandle, data.Length);

            ma_sound_flags flags = ma_sound_flags.decode;
            ma_result result = MiniAudio.ma_sound_init_from_memory(context.Engine, dataHandle, dataLength, flags, default, default, sound);

            if (result != ma_result.success)
            {
                Dispose();
                throw new Exception("Failed to initialize AudioClip: " + result);
            }

            MiniAudio.ma_sound_get_length_in_pcm_frames(sound, out pcmLength);

            hashCode = GetHashCode(data, data.Length);
        }

        internal bool CopyTo(AudioClip other, ma_sound_group_ptr group = default)
        {
            AudioContext context = AudioContext.GetCurrent();

            if(context == null)
            {
                Console.WriteLine("Failed to Copy AudioClip because AudioContext is null");
                return false;
            }

            if(sound.pointer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to Copy AudioClip because sound is null");
                return false;
            }
            
            if(other.sound.pointer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to Copy AudioClip because other.sound is null");
                return false;
            }

            MiniAudio.ma_sound_stop(other.sound);
            MiniAudio.ma_sound_uninit(other.sound);

            other.streamFromDisk = streamFromDisk;
            other.dataLength = dataLength;
            other.filePath = filePath;
            other.pcmLength = pcmLength;
            other.hashCode = hashCode;

            ma_sound_flags flags = streamFromDisk ? ma_sound_flags.stream : ma_sound_flags.decode;
            ma_result result = ma_result.success;

            if(flags == ma_sound_flags.stream)
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    result = MiniAudio.ma_sound_init_from_file_w(context.Engine, filePath, flags, group, default, other.sound);
                else
                    result = MiniAudio.ma_sound_init_from_file(context.Engine, filePath, flags, group, default, other.sound);
            }
            else
            {
                if(dataHandle == IntPtr.Zero)
                    result = MiniAudio.ma_sound_init_copy(context.Engine, sound, flags, group, other.sound);
                else
                    result = MiniAudio.ma_sound_init_from_memory(context.Engine, dataHandle, dataLength, flags, group, default, other.sound);
            }
            
            return result == ma_result.success;
        }

        public void Dispose()
        {
            if(sound.pointer != IntPtr.Zero)
            {
                MiniAudio.ma_sound_uninit(sound);
                sound.Free();
            }

            if(dataHandle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(dataHandle);
                dataHandle = IntPtr.Zero;
                dataLength = 0;
            }
        }

        private UInt64 GetHashCode(byte[] data, int size)
        {
            UInt64 hash = 0;

            for(int i = 0; i < size; i++) 
            {
                hash = data[i] + (hash << 6) + (hash << 16) - hash;
            }

            return hash;            
        }
    }
}