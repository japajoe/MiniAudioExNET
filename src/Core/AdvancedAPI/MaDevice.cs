using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public sealed class MaDevice : IDisposable
	{
		private ma_device_ptr handle;
		private ma_device_data_proc deviceDataCallback;

		public ma_device_ptr Handle
		{
			get => handle;
		}

		public MaDevice(MaContext context, MaDeviceInfo deviceInfo, ma_format playbackFormat, UInt32 channels, UInt32 sampleRate, UInt32 periodSizeInFrames, ma_device_data_proc deviceCallback)
		{
			if (context.Handle.pointer == IntPtr.Zero)
				throw new ArgumentException("context isn't initialized");

			handle = new ma_device_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			ma_device_config deviceConfig = MiniAudioNative.ma_device_config_init(ma_device_type.playback);
			deviceConfig.playback.format = playbackFormat;
			deviceConfig.playback.channels = channels;
			deviceConfig.sampleRate = sampleRate;
			deviceConfig.periodSizeInFrames = periodSizeInFrames;

			if (deviceInfo.pDeviceId.pointer != IntPtr.Zero)
				deviceConfig.playback.pDeviceID = deviceInfo.pDeviceId;

			if (deviceCallback != null)
			{
				deviceConfig.SetDataCallback(deviceCallback);
			}
			else
			{
				deviceDataCallback = OnDeviceData;
				deviceConfig.SetDataCallback(deviceDataCallback);
			}

			ma_result result = MiniAudioNative.ma_device_init(context.Handle, ref deviceConfig, handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Dispose();
				throw new Exception("Failed to initialize MaDevice");
			}
		}

		public void Start()
		{
			MiniAudioNative.ma_device_start(handle);
		}

		public void Stop()
		{
			MiniAudioNative.ma_device_stop(handle);
		}

		public void SetEngine(MaEngine engine)
		{
			if (handle.pointer == IntPtr.Zero)
				return;

			unsafe
			{
				ma_device* device = (ma_device*)handle.pointer;
				device->pUserData = engine.Handle.pointer;
			}
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_device_uninit(handle);
				handle.Free();
			}
		}
		
        private static unsafe void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
        {
            ma_device* device = (ma_device*)pDevice.pointer;

            if (device == null)
                return;

            if (device->pUserData == IntPtr.Zero)
                return;

            ma_engine_ptr pEngine = new ma_engine_ptr(device->pUserData);

            MiniAudioNative.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
        }
	}
}