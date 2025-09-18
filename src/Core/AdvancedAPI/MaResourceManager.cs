using System;
using System.Runtime.InteropServices;
using MiniAudioEx.Native;

namespace MiniAudioEx.Core.AdvancedAPI
{
	public sealed class MaResourceManager : IDisposable
	{
		private ma_resource_manager_ptr handle;

		public ma_resource_manager_ptr Handle
		{
			get => handle;
		}

		public MaResourceManager(MaDecodingBackendVTable[] decodingBackendVTable = null)
		{
			handle = new ma_resource_manager_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			ma_resource_manager_config resourceManagerConfig = MiniAudioNative.ma_resource_manager_config_init();

			IntPtr vtableMemory = AllocVTableArray(decodingBackendVTable, out int vtableCount);

			if (vtableCount > 0)
			{
				resourceManagerConfig.ppCustomDecodingBackendVTables = vtableMemory;
				resourceManagerConfig.customDecodingBackendCount = (UInt32)vtableCount;
			}

			ma_result result = MiniAudioNative.ma_resource_manager_init(ref resourceManagerConfig, handle);

			if (vtableMemory != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(vtableMemory);
			}

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
				MiniAudioNative.ma_resource_manager_uninit(handle);
				handle.Free();
			}
		}

		private unsafe IntPtr AllocVTableArray(MaDecodingBackendVTable[] decodingBackendVTable, out int count)
		{
			count = 0;

			if (decodingBackendVTable != null)
			{
				for (int i = 0; i < decodingBackendVTable.Length; i++)
				{
					if (decodingBackendVTable[i].vtable != IntPtr.Zero)
						count++;
				}
			}

			IntPtr vtableMemory = IntPtr.Zero;

			if (count > 0)
			{
				vtableMemory = Marshal.AllocHGlobal(sizeof(IntPtr) * count);

				ma_decoding_backend_vtable** pCustomBackendVTables = (ma_decoding_backend_vtable**)vtableMemory;

				int index = 0;

				for (int i = 0; i < decodingBackendVTable.Length; i++)
				{
					if (decodingBackendVTable[i].vtable != IntPtr.Zero)
						pCustomBackendVTables[index++] = (ma_decoding_backend_vtable*)decodingBackendVTable[i].vtable;
				}
			}

			return vtableMemory;
		}
	}
}