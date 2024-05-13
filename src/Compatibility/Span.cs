using System;
using System.Runtime.CompilerServices;

namespace MiniAudioExNET.Compatibility
{
    public unsafe struct Span<T> where T : unmanaged
    {
        internal void* _pointer;
        /// <summary>The number of elements this Span contains.</summary>
        private readonly int _length;

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return 0 >= (uint)_length;
            } 
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_length)
                    new IndexOutOfRangeException();
                return ref ((T*)_pointer)[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span(void* pointer, int length)
        {
            _pointer = pointer;
            _length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span(IntPtr pointer, int length)
        {
            _pointer = pointer.ToPointer();
            _length = length;
        }
    }
}