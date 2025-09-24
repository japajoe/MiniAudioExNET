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
	public delegate void MaEffectNodeProcessEvent(MaEffectNode sender, NativeArray<float> pFramesIn, UInt32 pFrameCountIn, NativeArray<float> pFramesOut, ref UInt32 pFrameCountOut, UInt32 channels);

	public sealed class MaEffectNode : MaNode, IDisposable
	{
		public event MaEffectNodeProcessEvent Process;

		private ma_effect_node_ptr handle;
		private ma_effect_node_process_proc onProcess;

		public ma_node_base_ptr Handle
		{
			get => new ma_node_base_ptr(handle.pointer);
		}

		public MaEffectNode() : base()
		{
			handle = new ma_effect_node_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			nodeHandle = new ma_node_ptr(handle.pointer);
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_effect_node_uninit(handle);
				handle.Free();
			}
			nodeHandle.pointer = IntPtr.Zero;
		}

		public ma_result Initialize(MaEngine engine, UInt32 sampleRate, UInt32 channels)
		{
			if (engine == null)
				return ma_result.invalid_args;

			return Initialize(engine.Handle, sampleRate, channels);
		}

		public ma_result Initialize(ma_engine_ptr pEngine, UInt32 sampleRate, UInt32 channels)
		{
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			if (pEngine.pointer == IntPtr.Zero)
				return ma_result.invalid_args;

			if (channels > MiniAudioNative.ma_engine_get_channels(pEngine))
				channels = MiniAudioNative.ma_engine_get_channels(pEngine);

			onProcess = OnProcess;

			ma_effect_node_config nodeConfig = MiniAudioNative.ma_effect_node_config_init(channels, sampleRate, onProcess);
			ma_node_graph_ptr pNodeGraph = MiniAudioNative.ma_engine_get_node_graph(pEngine);
			return MiniAudioNative.ma_effect_node_init(pNodeGraph, ref nodeConfig, handle);
		}

		private unsafe void OnProcess(ma_node_ptr pNode, IntPtr ppFramesIn, IntPtr pFrameCountIn, IntPtr ppFramesOut, IntPtr pFrameCountOut)
		{
            if (pNode.pointer == IntPtr.Zero)
                return;

            ma_effect_node* pEffectNode = (ma_effect_node*)pNode.pointer;
			UInt32 channels = pEffectNode->config.channels;
			UInt32 frameCountIn = *(UInt32*)pFrameCountIn;
			UInt32 frameCountOut = *(UInt32*)pFrameCountOut;

			float** framesIn = (float**)ppFramesIn;
			float** framesOut = (float**)ppFramesOut;

			// There could be more input/output streams but we only deal with 1
			NativeArray<float> bufferIn = new NativeArray<float>(framesIn[0], (int)(frameCountIn * channels));
			NativeArray<float> bufferOut = new NativeArray<float>(framesOut[0], (int)(frameCountOut * channels));

			Process?.Invoke(this, bufferIn, frameCountIn, bufferOut, ref frameCountOut, channels);
			*(UInt32*)pFrameCountOut = frameCountOut;
		}
	}
}