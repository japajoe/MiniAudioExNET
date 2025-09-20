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
	public abstract class MaNode
	{
		protected ma_node_ptr nodeHandle;

		public ma_node_ptr NodeHandle
		{
			get => nodeHandle;
		}

		public MaNode()
		{
			nodeHandle = new ma_node_ptr(IntPtr.Zero);
		}

		public ma_node_graph_ptr GetNodeGraph()
		{
			return MiniAudioNative.ma_node_get_node_graph(nodeHandle);
		}
		
		public UInt32 GetInputBusCount()
		{
			return MiniAudioNative.ma_node_get_input_bus_count(nodeHandle);
		}
		
		public UInt32 GetOutputBusCount()
		{
			return MiniAudioNative.ma_node_get_output_bus_count(nodeHandle);
		}
		
		public UInt32 GetInputChannels(UInt32 inputBusIndex)
		{
			return MiniAudioNative.ma_node_get_input_channels(nodeHandle, inputBusIndex);
		}
		
		public UInt32 GetOutputChannels(UInt32 outputBusIndex)
		{
			return MiniAudioNative.ma_node_get_output_channels(nodeHandle, outputBusIndex);
		}
		
		public ma_result AttachOutputBus(UInt32 outputBusIndex, ma_node_ptr pOtherNode, UInt32 otherNodeInputBusIndex)
		{
			return MiniAudioNative.ma_node_attach_output_bus(nodeHandle, outputBusIndex, pOtherNode, otherNodeInputBusIndex);
		}
		
		public ma_result DetachOutputBus(UInt32 outputBusIndex)
		{
			return MiniAudioNative.ma_node_detach_output_bus(nodeHandle, outputBusIndex);
		}
		
		public ma_result DetachAllOutputBuses()
		{
			return MiniAudioNative.ma_node_detach_all_output_buses(nodeHandle);
		}
		
		public ma_result SetOutputBusVolume(UInt32 outputBusIndex, float volume)
		{
			return MiniAudioNative.ma_node_set_output_bus_volume(nodeHandle, outputBusIndex, volume);
		}
		
		public float GetOutputBusVolume(UInt32 outputBusIndex)
		{
			return MiniAudioNative.ma_node_get_output_bus_volume(nodeHandle, outputBusIndex);
		}
		
		public ma_result SetState(ma_node_state state)
		{
			return MiniAudioNative.ma_node_set_state(nodeHandle, state);
		}
		
		public ma_node_state GetState()
		{
			return MiniAudioNative.ma_node_get_state(nodeHandle);
		}
		
		public ma_result SetStateTime(ma_node_state state, UInt64 globalTime)
		{
			return MiniAudioNative.ma_node_set_state_time(nodeHandle, state, globalTime);
		}
		
		public UInt64 GetStateTime(ma_node_state state)
		{
			return MiniAudioNative.ma_node_get_state_time(nodeHandle, state);
		}
		
		public ma_node_state GetStateByTime(UInt64 globalTime)
		{
			return MiniAudioNative.ma_node_get_state_by_time(nodeHandle, globalTime);
		}
		
		public ma_node_state GetStateByTimeRange(UInt64 globalTimeBeg, UInt64 globalTimeEnd)
		{
			return MiniAudioNative.ma_node_get_state_by_time_range(nodeHandle, globalTimeBeg, globalTimeEnd);
		}
		
		public UInt64 GetTime()
		{
			return MiniAudioNative.ma_node_get_time(nodeHandle);
		}
		
		public ma_result SetTime(UInt64 localTime)
		{
			return MiniAudioNative.ma_node_set_time(nodeHandle, localTime);
		}
	}
}