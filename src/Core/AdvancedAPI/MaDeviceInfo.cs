using System;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public struct MaDeviceInfo
	{
		public ma_device_info deviceInfo;
		public ma_device_id_ptr pDeviceId;

		public string GetName()
		{
			if (pDeviceId.pointer == IntPtr.Zero)
				return string.Empty;

			return deviceInfo.GetName();
		}
	}
}