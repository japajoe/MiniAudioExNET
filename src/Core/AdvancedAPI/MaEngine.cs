using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public sealed class MaEngine : IDisposable
	{
		private ma_engine_ptr handle;

		public ma_engine_ptr Handle
		{
			get => handle;
		}

		public MaEngine(MaDevice device = null, MaResourceManager resourceManager = null)
		{
			handle = new ma_engine_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

            ma_engine_config engineConfig = MiniAudioNative.ma_engine_config_init();
            engineConfig.listenerCount = MiniAudioNative.MA_ENGINE_MAX_LISTENERS;
            engineConfig.pDevice = device == null ? default : device.Handle;

			if (resourceManager != null)
				engineConfig.pResourceManager = resourceManager.Handle;

			ma_result result = MiniAudioNative.ma_engine_init(ref engineConfig, handle);
			
			if (result != ma_result.MA_SUCCESS)
			{
				Dispose();
				throw new Exception("Failed to initialize MaEngine");
			}
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_engine_uninit(handle);
				handle.Free();
			}
		}
	}
}