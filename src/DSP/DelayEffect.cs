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
using MiniAudioEx.Core.StandardAPI;
using MiniAudioEx.Native;

namespace MiniAudioEx.DSP
{
	public sealed class DelayEffect : IAudioEffect
	{
		private Int32 channels;
		private Int32 sampleRate;
		private bool delayStart;       /* Set to true to delay the start of the output; false otherwise. */
		private float wet;                  /* 0..1. Default = 1. */
		private float dry;                  /* 0..1. Default = 1. */
		private float decay;                /* 0..1. Default = 0 (no feedback). Feedback decay. Use this for echo. */
		private Int32 cursor;               /* Feedback is written to this cursor. Always equal or in front of the read cursor. */
		private Int32 bufferSizeInFrames;
		private Int32 actualBufferSize;
		private float[] buffer;
		private readonly object lockObject = new object();

		public float Wet
		{
			get => wet;
			set => wet = value;
		}

		public float Dry
		{
			get => dry;
			set => dry = value;
		}

		public float Decay
		{
			get => decay;
			set
			{
				decay = value;
				delayStart = (decay == 0) ? true : false;
			}
		}

		public UInt32 DelayInFrames
		{
			get => (UInt32)bufferSizeInFrames;
			set
			{
				lock (lockObject)
				{
					bufferSizeInFrames = (Int32)value;

					if (bufferSizeInFrames < 1)
					{
						bufferSizeInFrames = 1;
					}

					actualBufferSize = bufferSizeInFrames * channels;

					if (actualBufferSize > buffer.Length)
					{
						buffer = new float[actualBufferSize];
					}

					cursor = cursor % bufferSizeInFrames;
				}
			}
		}

		public float DelayInSeconds
		{
			get => (float)bufferSizeInFrames / sampleRate;
			set
			{
				lock (lockObject)
				{
					bufferSizeInFrames = (Int32)Math.Ceiling(value * sampleRate);

					if (bufferSizeInFrames < 1)
					{
						bufferSizeInFrames = 1;
					}

					actualBufferSize = bufferSizeInFrames * channels;

					if (actualBufferSize > buffer.Length)
					{
						buffer = new float[actualBufferSize];
					}

					cursor = cursor % bufferSizeInFrames;
				}
			}
		}

		public DelayEffect(UInt32 sampleRate, UInt32 channels, UInt32 delayInFrames, float decay)
		{
			this.sampleRate = (Int32)sampleRate;
			this.channels = (Int32)channels;
			delayStart = (decay == 0) ? true : false;   /* Delay the start if it looks like we're not configuring an echo. */
			wet = 1.0f;
			dry = 1.0f;
			this.decay = decay;
			bufferSizeInFrames = (Int32)delayInFrames;
			actualBufferSize = (Int32)(bufferSizeInFrames * channels);
			buffer = new float[actualBufferSize];
		}

		public DelayEffect(UInt32 sampleRate, UInt32 channels, float delayInSeconds, float decay)
		{
			this.sampleRate = (Int32)sampleRate;
			this.channels = (Int32)channels;
			delayStart = (decay == 0) ? true : false;   /* Delay the start if it looks like we're not configuring an echo. */
			wet = 1.0f;
			dry = 1.0f;
			this.decay = decay;
			bufferSizeInFrames = (Int32)Math.Ceiling(delayInSeconds * sampleRate);
			actualBufferSize = (Int32)(bufferSizeInFrames * channels);
			buffer = new float[actualBufferSize];
		}

		public unsafe void OnProcess(NativeArray<float> framesIn, UInt32 frameCountIn, NativeArray<float> framesOut, ref UInt32 frameCountOut, UInt32 channels)
		{
			Int32 iFrame;
			Int32 iChannel;

			float* pFramesOutF32 = (float*)framesOut.Pointer;
			float* pFramesInF32 = (float*)framesIn.Pointer;

			for (iFrame = 0; iFrame < frameCountIn; iFrame += 1)
			{
				for (iChannel = 0; iChannel < this.channels; iChannel += 1)
				{
					Int32 iBuffer = (cursor * this.channels) + iChannel;

					if (delayStart)
					{
						/* Delayed start. */

						/* Read */
						pFramesOutF32[iChannel] = buffer[iBuffer] * wet;

						/* Feedback */
						buffer[iBuffer] = (buffer[iBuffer] * decay) + (pFramesInF32[iChannel] * dry);
					}
					else
					{
						/* Immediate start */

						/* Feedback */
						buffer[iBuffer] = (buffer[iBuffer] * decay) + (pFramesInF32[iChannel] * dry);

						/* Read */
						pFramesOutF32[iChannel] = buffer[iBuffer] * wet;
					}
				}

				cursor = (cursor + 1) % bufferSizeInFrames;

				pFramesOutF32 += this.channels;
				pFramesInF32 += this.channels;
			}
		}
		
		public void OnDestroy() {}
	}
}