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
// Copyright 2026 W.M.R Jap-A-Joe

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
	public sealed class MaEncoder : IDisposable
	{
		private ma_encoder_ptr handle;

		public ma_encoder_ptr Handle
		{
			get => handle;
		}

		public MaEncoder()
		{
			handle = new ma_encoder_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				MiniAudioNative.ma_encoder_uninit(handle);
				handle.Free();
			}
		}

		public ma_encoder_config GetConfig(ma_encoding_format encodingFormat, ma_format format, UInt32 channels, UInt32 sampleRate)
		{
			return MiniAudioNative.ma_encoder_config_init(encodingFormat, format, channels, sampleRate);
		}

		public ma_result Initialize(ma_encoder_write_proc onWrite, ma_encoder_seek_proc onSeek, IntPtr pUserData, ma_encoder_config pConfig)
		{
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			return MiniAudioNative.ma_encoder_init(onWrite, onSeek, pUserData, ref pConfig, handle);
		}

        public ma_result InitializeFile(string pFilePath, ma_encoder_config pConfig)
        {
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			return MiniAudioNative.ma_encoder_init_file(pFilePath, ref pConfig, handle);
        }

        public ma_result InitializeFileW(string pFilePath, ma_encoder_config pConfig)
        {
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			return MiniAudioNative.ma_encoder_init_file_w(pFilePath, ref pConfig, handle);
        }

        public ma_result WritePCMFrames(IntPtr pFramesIn, UInt64 frameCount, out UInt64 pFramesWritten)
        {
            return MiniAudioNative.ma_encoder_write_pcm_frames(handle, pFramesIn, frameCount, out pFramesWritten);
        }
	}
}