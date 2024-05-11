using System;
using System.Runtime.InteropServices;

namespace MiniAudioExNET
{
    public sealed class AudioClip : IDisposable
    {
        private string filePath;
        private byte[] data;
        private IntPtr handle;
        private bool streamFromDisk;

        public string FilePath
        {
            get => filePath;
        }

        public bool StreamFromDisk
        {
            get => streamFromDisk;
        }

        public IntPtr Handle
        {
            get => handle;
        }

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

        public AudioClip(string filePath, bool streamFromDisk = true)
        {
            this.filePath = filePath;
            this.streamFromDisk = streamFromDisk;
            this.handle = IntPtr.Zero;
            MiniAudioEx.Add(this);
        }

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
            Console.WriteLine("Disposing");
            MiniAudioEx.Remove(this);
        }
    }
}