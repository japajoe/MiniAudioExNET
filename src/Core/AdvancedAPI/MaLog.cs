using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public delegate void LogEventHandler(UInt32 level, string message);

	public sealed class MaLog : IDisposable
	{
		public event LogEventHandler Message;

		private ma_log_ptr handle;
		private ma_log_callback_proc onLog;

		public ma_log_ptr Handle
		{
			get => handle;
		}

		public MaLog()
		{
			handle = new ma_log_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			ma_result result = MiniAudioNative.ma_log_init(handle);

			if (result != ma_result.MA_SUCCESS)
			{
				Dispose();
				throw new Exception("Failed to initialize MaLog");
			}

			onLog = OnLog;

			ma_log_callback callback = new ma_log_callback();
			callback.SetLogCallback(onLog);
			MiniAudioNative.ma_log_register_callback(handle, callback);
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_log_uninit(handle);
				handle.Free();
			}
		}

		private void OnLog(IntPtr pUserData, UInt32 level, IntPtr pMessage)
		{
			string message = MarshalHelper.PtrToStringUTF8(pMessage);
			Message?.Invoke(level, message);
		}
	}
}