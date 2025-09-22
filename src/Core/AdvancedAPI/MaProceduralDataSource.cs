using System;
using System.Runtime.InteropServices;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void procedural_data_source_proc(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels);

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct procedural_data_source_config
	{
		public ma_format format;
		public UInt32 channels;
		public UInt32 sampleRate;
		public IntPtr pCallback;
		public IntPtr pUserData;

		public void SetCallback(procedural_data_source_proc callback)
		{
			pCallback = MarshalHelper.GetFunctionPointerForDelegate(callback);
		}
	}

	public sealed class MaProceduralDataSource : MaDataSource, IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		private unsafe struct procedural_data_source
		{
			public ma_data_source_base ds;
			public procedural_data_source_config config;
		}

		public MaProceduralDataSource() : base()
		{
			handle.pointer = MiniAudioNative.ma_allocate(Marshal.SizeOf<procedural_data_source>());

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			vtable.pointer = MiniAudioNative.ma_allocate_type(ma_allocation_type.data_source_vtable);

			if (vtable.pointer == IntPtr.Zero)
			{
				handle.Free();
				throw new OutOfMemoryException();
			}

			onRead = OnRead;
			onSeek = OnSeek;
			onGetDataFormat = OnGetDataFormat;
			onGetCursor = OnGetCursor;

			unsafe
			{
				ma_data_source_vtable* pVtable = vtable.Get();
				pVtable->SetReadProc(onRead);
				pVtable->SetSeekProc(onSeek);
				pVtable->SetGetDataFormatProc(onGetDataFormat);
				pVtable->SetGetCursorProc(onGetCursor);
			}
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_data_source_uninit(handle);
				handle.Free();
				vtable.Free();
			}
		}

		public procedural_data_source_config GetConfig(ma_format format, UInt32 channels, UInt32 sampleRate, procedural_data_source_proc callback, IntPtr pUserData)
		{
			if (callback == null)
				throw new ArgumentException("Callback can not be null");

			procedural_data_source_config config = new procedural_data_source_config();

			config.format = format;
			config.channels = channels;
			config.sampleRate = sampleRate;
			config.pUserData = pUserData;
			config.SetCallback(callback);

			return config;
		}

		public ma_result Initialize(procedural_data_source_config config)
		{
			if(handle.pointer == IntPtr.Zero)
				return ma_result.invalid_args;

			ma_data_source_config dataSourceConfig = MiniAudioNative.ma_data_source_config_init();
			dataSourceConfig.vtable = vtable;

			unsafe
			{
				procedural_data_source* pSource = (procedural_data_source*)handle.pointer;
				ma_result result = MiniAudioNative.ma_data_source_init(ref dataSourceConfig, handle);

				if (result != ma_result.success)
					return result;

				pSource->config = config;
			}

			return ma_result.success;
		}

		private unsafe ma_result OnReadPCMFrames(procedural_data_source* pCustomDataSource, IntPtr pFramesOut, UInt64 frameCount, out UInt64 pFramesRead)
		{
			pFramesRead = 0;

			if (frameCount == 0)
				return ma_result.invalid_args;

			if (pCustomDataSource == null)
				return ma_result.invalid_args;

			if (pFramesOut != IntPtr.Zero)
			{
				if (pCustomDataSource->config.pCallback != IntPtr.Zero)
				{
					procedural_data_source_proc callback = Marshal.GetDelegateForFunctionPointer<procedural_data_source_proc>(pCustomDataSource->config.pCallback);
					callback(pCustomDataSource->config.pUserData, pFramesOut, frameCount, pCustomDataSource->config.channels);
				}
			}

			pFramesRead = frameCount;

			return ma_result.success;
		}

		private unsafe ma_result OnRead(ma_data_source_ptr pDataSource, IntPtr pFramesOut, UInt64 frameCount, out UInt64 pFramesRead)
		{
			return OnReadPCMFrames((procedural_data_source*)pDataSource.pointer, pFramesOut, frameCount, out pFramesRead);
		}

		private ma_result OnSeek(ma_data_source_ptr pDataSource, UInt64 frameIndex)
		{
			return ma_result.not_implemented;
		}

		private unsafe ma_result OnGetDataFormat(ma_data_source_ptr pDataSource, out ma_format pFormat, out UInt32 pChannels, out UInt32 pSampleRate, ma_channel_ptr pChannelMap, size_t channelMapCap)
		{
			procedural_data_source* pSource = (procedural_data_source*)pDataSource.pointer;

			pFormat = pSource->config.format;
			pChannels = pSource->config.channels;
			pSampleRate = pSource->config.sampleRate;

			MiniAudioNative.ma_channel_map_init_standard(ma_standard_channel_map.standard, pChannelMap, channelMapCap, pSource->config.channels);

			return ma_result.success;
		}

		private ma_result OnGetCursor(ma_data_source_ptr pDataSource, out UInt64 pCursor)
		{
			pCursor = 0;
			return ma_result.not_implemented;
		}
	}
}