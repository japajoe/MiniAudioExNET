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
	public delegate void MaEffectNodeProcessEventHandler(MaEffectNode sender, NativeArray<float> pFramesIn, UInt32 pFrameCountIn, NativeArray<float> pFramesOut, ref UInt32 pFrameCountOut, UInt32 channels);

	public sealed class MaEffectNode : MaNode, IDisposable
	{
		public event MaEffectNodeProcessEventHandler Process;

		private ma_node_base_ptr handle;
		private ma_node_vtable_ptr vtable;
		private ma_uint32_ptr pChannels;
		private ma_node_vtable_process_proc onProcess;
		private UInt32 channels;

		public ma_node_base_ptr Handle
		{
			get => handle;
		}

		public MaEffectNode() : base()
		{
			handle = new ma_node_base_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			vtable = new ma_node_vtable_ptr(true);

			if (vtable.pointer == IntPtr.Zero)
			{
				handle.Free();
				throw new OutOfMemoryException();
			}

			pChannels = new ma_uint32_ptr(true);

			if (pChannels.pointer == IntPtr.Zero)
			{
				handle.Free();
				vtable.Free();
				throw new OutOfMemoryException();
			}

			nodeHandle = new ma_node_ptr(handle.pointer);
			onProcess = OnProcess;
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_node_uninit(nodeHandle);
				handle.Free();
				vtable.Free();
				pChannels.Free();
			}
			nodeHandle.pointer = IntPtr.Zero;
		}

		public ma_result Initialize(MaEngine engine, UInt32 channels)
		{
			if (engine == null)
				return ma_result.invalid_args;

			if (engine.Handle.pointer == IntPtr.Zero)
				return ma_result.invalid_args;

			if (handle.pointer == IntPtr.Zero || engine.Handle.pointer == IntPtr.Zero)
				return ma_result.error;

			if (channels > engine.GetChannels())
				channels = engine.GetChannels();

			this.channels = channels;

			unsafe
			{
				ma_node_vtable* pVtable = vtable.Get();
				pVtable->inputBusCount = 1;
				pVtable->outputBusCount = 1;
				pVtable->flags = ma_node_flags.continuous_processing | ma_node_flags.allow_null_input;
				pVtable->SetOnProcess(onProcess);

				*pChannels.Get() = this.channels;

				ma_node_config nodeConfig = MiniAudioNative.ma_node_config_init();
				nodeConfig.pInputChannels = pChannels.pointer;
				nodeConfig.pOutputChannels = pChannels.pointer;
				nodeConfig.vtable = vtable;

				return MiniAudioNative.ma_node_init(engine.GetNodeGraph(), ref nodeConfig, nodeHandle);
			}
		}

		private unsafe void OnProcess(ma_node_ptr pNode, IntPtr ppFramesIn, IntPtr pFrameCountIn, IntPtr ppFramesOut, IntPtr pFrameCountOut)
		{
			UInt32 frameCountIn = *(UInt32*)pFrameCountIn;
			UInt32 frameCountOut = *(UInt32*)pFrameCountOut;

			float** framesIn = (float**)ppFramesIn;
			float** framesOut = (float**)ppFramesOut;

			// There could be more input/output streams but we only deal with 1
			NativeArray<float> bufferIn = new NativeArray<float>(new IntPtr(framesIn[0]), (int)(frameCountIn * channels));
			NativeArray<float> bufferOut = new NativeArray<float>( new IntPtr(framesOut[0]), (int)(frameCountOut * channels));

			Process?.Invoke(this, bufferIn, frameCountIn, bufferOut, ref frameCountOut, channels);
			*(UInt32*)pFrameCountOut = frameCountOut;
		}
	}
}