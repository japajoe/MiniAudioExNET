using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public sealed class MaProceduralDataSource : MaDataSource, IDisposable
	{
		private ma_procedural_data_source_ptr handle;

		public MaProceduralDataSource() : base()
		{
			handle = new ma_procedural_data_source_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			dataSourceHandle.pointer = handle.pointer;
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_procedural_data_source_uninit(handle);
				handle.Free();
			}
			dataSourceHandle.pointer = IntPtr.Zero;
		}

		public ma_procedural_data_source_config GetConfig(ma_format format, UInt32 channels, UInt32 sampleRate, ma_procedural_data_source_proc callback, IntPtr pUserData)
		{
			if (callback == null)
				throw new ArgumentException("Callback can not be null");

			return MiniAudioNative.ma_procedural_data_source_config_init(format, channels, sampleRate, callback, pUserData);
		}

		public ma_result Initialize(ma_procedural_data_source_config config)
		{
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;
			
			return MiniAudioNative.ma_procedural_data_source_init(ref config, handle);
		}
	}
}