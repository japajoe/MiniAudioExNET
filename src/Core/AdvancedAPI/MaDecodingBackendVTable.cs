using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public struct MaDecodingBackendVTable
	{
		public IntPtr vtable;

		public MaDecodingBackendVTable()
		{
			vtable = IntPtr.Zero;
		}

		public MaDecodingBackendVTable(IntPtr vtable)
		{
			this.vtable = vtable;
		}

		public static MaDecodingBackendVTable CreateFromLibVorbis()
		{
			unsafe
			{
				return new MaDecodingBackendVTable(new IntPtr(MiniAudioNative.ma_libvorbis_get_decoding_backend()));
			}
		}
	}
}