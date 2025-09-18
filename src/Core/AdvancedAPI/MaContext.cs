using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public sealed class MaContext : IDisposable
	{
		private ma_context_ptr handle;

		public ma_context_ptr Handle
		{
			get => handle;
		}

		public MaContext(MaLog log = null)
		{
			handle = new ma_context_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			ma_context_config config = MiniAudioNative.ma_context_config_init();
			config.pLog = log == null ? default : log.Handle;

			ma_result result = MiniAudioNative.ma_context_init(null, ref config, handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Dispose();
				throw new Exception("Failed to initialize MaContext");
			}
		}

		public MaDeviceInfo GetDefaultPlaybackDevice()
		{
			if (GetDevices(out MaDeviceInfo[] playbackDevices, out _))
			{
				if (playbackDevices == null)
					return default;

				for (int i = 0; i < playbackDevices.Length; i++)
				{
					if (playbackDevices[i].deviceInfo.isDefault > 0)
					{
						return playbackDevices[i];
					}
				}
			}

			return default;
		}

		public MaDeviceInfo GetDefaultCaptureDevice()
		{
			if (GetDevices(out _, out MaDeviceInfo[] captureDevices))
			{
				if(captureDevices == null)
					return default;
					
				for (int i = 0; i < captureDevices.Length; i++)
				{
					if (captureDevices[i].deviceInfo.isDefault > 0)
					{
						return captureDevices[i];
					}
				}
			}

			return default;
		}

		public bool GetDevices(out MaDeviceInfo[] playbackDevices, out MaDeviceInfo[] captureDevices)
		{
			playbackDevices = null;
			captureDevices = null;

			ma_result result = MiniAudioNative.ma_context_get_devices(handle, out ma_device_info_ex[] ppPlaybackDeviceInfos, out ma_device_info_ex[] ppCaptureDeviceInfos);

			if (result != ma_result.MA_SUCCESS)
				return false;

			if (ppPlaybackDeviceInfos?.Length > 0)
			{
				playbackDevices = new MaDeviceInfo[ppPlaybackDeviceInfos.Length];

				for (int i = 0; i < ppPlaybackDeviceInfos.Length; i++)
				{
					playbackDevices[i] = new MaDeviceInfo();
					playbackDevices[i].deviceInfo = ppPlaybackDeviceInfos[i].deviceInfo;
					playbackDevices[i].pDeviceId = ppPlaybackDeviceInfos[i].pDeviceId;
				}
			}

			if (ppCaptureDeviceInfos?.Length > 0)
			{
				captureDevices = new MaDeviceInfo[ppCaptureDeviceInfos.Length];

				for (int i = 0; i < ppCaptureDeviceInfos.Length; i++)
				{
					captureDevices[i] = new MaDeviceInfo();
					captureDevices[i].deviceInfo = ppCaptureDeviceInfos[i].deviceInfo;
					captureDevices[i].pDeviceId = ppCaptureDeviceInfos[i].pDeviceId;
				}
			}

			return true;
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_context_uninit(handle);
				handle.Free();
			}
		}
	}
}