using System;
using System.Runtime.InteropServices;

namespace MiniAudioExNET
{
    /// <summary>
    /// Represents audio data that can be played back or streamed by an AudioSource. Supported file types are WAV/MP3/FlAC.
    /// </summary>
    public sealed class AudioClip : IDisposable
    {
        private string filePath;
        private byte[] data;
        private IntPtr handle;
        private bool streamFromDisk;

        /// <summary>
        /// If the constructor with 'string filePath' overloaded is used this will contain the file path, or string.Empty otherwise.
        /// </summary>
        /// <value></value>
        public string FilePath
        {
            get => filePath;
        }

        /// <summary>
        /// If true, data will be streamed from disk. This is useful when a sound is longer than just a couple of seconds. If data is loaded from memory, this property has no effect.
        /// </summary>
        /// <value></value>
        public bool StreamFromDisk
        {
            get => streamFromDisk;
        }

        /// <summary>
        /// If the constructor with 'byte[] data' overload is used this will contain a pointer to the allocated memory of the data. Do not manually free!
        /// </summary>
        /// <value></value>
        public IntPtr Handle
        {
            get => handle;
        }

        /// <summary>
        /// If the constructor with 'byte[] data' overload is used this will contain the size of the data in number of bytes.
        /// </summary>
        /// <value></value>
        public ulong DataSize
        {
            get
            {
                if(data != null)
                {
                    return (ulong)data.Length;
                }
                return 0;
            }
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from a file on disk. The file must be in an encoded format.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="streamFromDisk"></param>
        public AudioClip(string filePath, bool streamFromDisk = true)
        {
            this.filePath = filePath;
            this.streamFromDisk = streamFromDisk;
            this.handle = IntPtr.Zero;
            MiniAudioEx.Add(this);
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from memory. The data must be in an encoded format.
        /// </summary>
        /// <param name="data"></param>
        public AudioClip(byte[] data)
        {
            this.filePath = string.Empty;
            this.data = data;
            this.streamFromDisk = false;
            this.handle = Marshal.AllocHGlobal(data.Length);

            if(handle != IntPtr.Zero)
            {            
                unsafe
                {
                    byte *ptr = (byte*)handle.ToPointer();
                    for(int i = 0; i < data.Length; i++)
                        ptr[i] = data[i];
                }
                MiniAudioEx.Add(this);
            }
            
        }

        internal void Destroy()
        {
            if(handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            MiniAudioEx.Remove(this);
        }
    }
}