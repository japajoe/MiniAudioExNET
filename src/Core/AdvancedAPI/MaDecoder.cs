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
	public sealed class MaDecoder : IDisposable
	{
		private ma_decoder_ptr handle;
		private bool isLoaded;
		
		public ma_decoder_ptr Handle
		{
			get => handle;
		}

		public MaDecoder()
		{
			handle = new ma_decoder_ptr(true);

			if (handle.pointer == IntPtr.Zero)
				throw new OutOfMemoryException();

			isLoaded = false;
		}

		public void Dispose()
		{
			if (handle.pointer != IntPtr.Zero)
			{
				if (isLoaded)
				{
					MiniAudioNative.ma_decoder_uninit(handle);
				}

				handle.Free();
				isLoaded = false;
			}
		}

		public ma_decoder_config GetConfig()
		{
			return MiniAudioNative.ma_decoder_config_init_default();
		}

		public ma_decoder_config GetConfig(ma_format outputFormat, UInt32 outputChannels, UInt32 outputSampleRate)
		{
			return MiniAudioNative.ma_decoder_config_init(outputFormat, outputChannels, outputSampleRate);
		}

		public ma_result InitializeFromFile(string filePath)
		{
			ma_decoder_config config = MiniAudioNative.ma_decoder_config_init_default();
			return InitializeFromFile(filePath, config);
		}

		public ma_result InitializeFromFile(string filePath, ma_decoder_config config)
		{
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			if (isLoaded)
				Unload();

			return MiniAudioNative.ma_decoder_init_file(filePath, ref config, handle);
		}

		public ma_result IntializeFromMemory(IntPtr pData, UInt64 dataSize)
		{
			ma_decoder_config config = MiniAudioNative.ma_decoder_config_init_default();
			return IntializeFromMemory(pData, dataSize, config);
		}

		public ma_result IntializeFromMemory(IntPtr pData, UInt64 dataSize, ma_decoder_config config)
		{
			if (handle.pointer == IntPtr.Zero)
				return ma_result.error;

			if (isLoaded)
				Unload();

			return MiniAudioNative.ma_decoder_init_memory(pData, new UIntPtr(dataSize), ref config, handle);
		}

		public ma_result SeekToPCMFrame(UInt64 frameIndex)
		{
			return MiniAudioNative.ma_decoder_seek_to_pcm_frame(handle, frameIndex);
		}

		public ma_result GetDataFormat(out ma_format pFormat, out UInt32 pChannels, out UInt32 pSampleRate, ma_channel_ptr pChannelMap, UInt64 channelMapCap)
		{
			return MiniAudioNative.ma_decoder_get_data_format(handle, out pFormat, out pChannels, out pSampleRate, pChannelMap, new UIntPtr(channelMapCap));
		}

		public ma_result GetCursorInPCMFrames(out UInt64 cursor)
		{
			return MiniAudioNative.ma_decoder_get_cursor_in_pcm_frames(handle, out cursor);
		}

		public ma_result GetLengthInPCMFrames(out UInt64 length)
		{
			return MiniAudioNative.ma_decoder_get_cursor_in_pcm_frames(handle, out length);
		}

		public ma_result GetAvailableFrames(out UInt64 availableFrames)
		{
			return MiniAudioNative.ma_decoder_get_available_frames(handle, out availableFrames);
		}

		private void Unload()
		{
			if (handle.pointer == IntPtr.Zero)
				return;

			if (!isLoaded)
				return;

			MiniAudioNative.ma_decoder_uninit(handle);

			isLoaded = false;
		}
	}
}