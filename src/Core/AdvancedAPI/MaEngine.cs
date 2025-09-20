// This software is available as a choice of the following licenses. Choose
// whichever you prefer.

// ===============================================================================
// ALTERNATIVE 1 - Public Domain (www.unlicense.org)
// ===============================================================================
// This is free and unencumbered software released into the public domain.

// Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
// software, either in source code form or as a compiled binary, for any purpose,
// commercial or non-commercial, and by any means.

// In jurisdictions that recognize copyright laws, the author or authors of this
// software dedicate any and all copyright interest in the software to the public
// domain. We make this dedication for the benefit of the public at large and to
// the detriment of our heirs and successors. We intend this dedication to be an
// overt act of relinquishment in perpetuity of all present and future rights to
// this software under copyright law.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// For more information, please refer to <http://unlicense.org/>

// ===============================================================================
// ALTERNATIVE 2 - MIT No Attribution
// ===============================================================================
// Copyright 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

		public MaEngine()
		{
			handle = new ma_engine_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_engine_uninit(handle);
				handle.Free();
			}
		}

		public ma_engine_config GetConfig()
		{
			return MiniAudioNative.ma_engine_config_init();
		}

		public ma_result Initialize()
		{
			ma_engine_config config = MiniAudioNative.ma_engine_config_init();
			return Initialize(config);
		}

		public ma_result Initialize(ma_engine_config config)
		{
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			return MiniAudioNative.ma_engine_init(ref config, handle);
		}

		public ma_node_graph_ptr GetNodeGraph()
		{
			return MiniAudioNative.ma_engine_get_node_graph(handle);
		}

		public ma_resource_manager_ptr GetResourceManager()
		{
			return MiniAudioNative.ma_engine_get_resource_manager(handle);
		}

		public ma_device_ptr GetDevice()
		{
			return MiniAudioNative.ma_engine_get_device(handle);
		}

		public ma_log_ptr GetLog()
		{
			return MiniAudioNative.ma_engine_get_log(handle);
		}

		public ma_node_ptr GetEndPoint()
		{
			return MiniAudioNative.ma_engine_get_endpoint(handle);
		}

		public UInt64 GetTimeInMilliseconds()
		{
        	return MiniAudioNative.ma_engine_get_time_in_milliseconds(handle);
		}

		public ma_result SetTimeInMilliseconds(UInt64 globalTime)
		{
        	return MiniAudioNative.ma_engine_set_time_in_milliseconds(handle, globalTime);
		}

		public UInt64 GetTimeInPCMFrames()
		{
			return MiniAudioNative.ma_engine_get_time_in_pcm_frames(handle);
		}

		public ma_result SetTimeInPCMFrames(UInt64 globalTime)
		{
			return MiniAudioNative.ma_engine_set_time_in_pcm_frames(handle, globalTime);
		}

		public UInt32 GetChannels()
		{
			return MiniAudioNative.ma_engine_get_channels(handle);
		}

		public UInt32 GetSampleRate()
		{
			return MiniAudioNative.ma_engine_get_sample_rate(handle);
		}

		public ma_result Start()
		{
			return MiniAudioNative.ma_engine_start(handle);
		}

		public ma_result Stop()
		{
			return MiniAudioNative.ma_engine_stop(handle);
		}

		public ma_result PlaySound(string filePath, MaSoundGroup group = null)
		{
			return MiniAudioNative.ma_engine_play_sound(handle, filePath, group == null ? default : group.Handle);
		}

		public ma_result PlaySoundEx(string filePath, MaNode node, UInt32 nodeInputBusIndex)
		{
			return PlaySoundEx(filePath, node == null ? default : node.NodeHandle, nodeInputBusIndex);
		}

		public ma_result PlaySoundEx(string filePath, ma_node_ptr pNode, UInt32 nodeInputBusIndex)
		{
			return MiniAudioNative.ma_engine_play_sound_ex(handle, filePath, pNode, nodeInputBusIndex);
		}

		public float GetVolume()
		{
			return MiniAudioNative.ma_engine_get_volume(handle);
		}

		public ma_result SetVolume(float volume)
		{
			return MiniAudioNative.ma_engine_set_volume(handle, volume);
		}

		public float GetGainDB()
		{
			return MiniAudioNative.ma_engine_get_gain_db(handle);
		}

		public ma_result SetGainDB(float gainDB)
		{
			return MiniAudioNative.ma_engine_set_gain_db(handle, gainDB);
		}

		public UInt32 GetListenerCount()
		{
			return MiniAudioNative.ma_engine_get_listener_count(handle);
		}

		public UInt32 FindClosestListener(float absolutePosX, float absolutePosY, float absolutePosZ)
		{
			return MiniAudioNative.ma_engine_find_closest_listener(handle, absolutePosX, absolutePosY, absolutePosZ);
		}
	}
}