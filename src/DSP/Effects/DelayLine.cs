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

// Ported from The Synthesis ToolKit in C++ (STK)
// Copyright (c) 1995--2023 Perry R. Cook and Gary P. Scavone
// STK GitHub site: https://github.com/thestk/stk
// STK WWW site: http://ccrma.stanford.edu/software/stk/

using System;
using System.Runtime.CompilerServices;

namespace MiniAudioEx.DSP.Effects
{
	public sealed class DelayLine
	{
		private UInt32 inPoint;
		private UInt32 outPoint;
		private float delay;
		private float alpha;
		private float omAlpha;
		private float nextOutput;
		private bool doNextOut;
		private float[] lastFrame;
		private float[] inputs;
		private float gain;

		public float Delay
		{
			get
			{
				return delay;
			}
			set
			{
				if (value + 1 > inputs.Length)
				{ // The value is too big.
					return;
				}

				if (value < 0)
				{
					return;
				}

				delay = value;

				float outPointer = inPoint - delay;  // read chases write

				while (outPointer < 0)
					outPointer += inputs.Length; // modulo maximum length

				outPoint = (UInt32)outPointer;   // integer part

				alpha = outPointer - outPoint; // fractional part
				omAlpha = 1.0f - alpha;

				if (outPoint == inputs.Length)
					outPoint = 0;
				doNextOut = true;
			}
		}

		public UInt32 MaximumDelay
		{
			get => (UInt32)inputs.Length;
			set
			{
				if (value < inputs.Length)
					return;
				inputs = new float[value + 1];
			}
		}

		public DelayLine(float delay = 0.0f, UInt32 maxDelay = 4095)
		{
			if (delay < 0.0f)
				delay = 0.0f;

			if (delay > maxDelay)
				delay = (float)maxDelay;

			inputs = new float[maxDelay + 1];
			lastFrame = new float[2];

			gain = 1.0f;
			inPoint = 0;
			Delay = delay;
			doNextOut = true;
		}

		public void Clear()
		{
			for (int i = 0; i < inputs.Length; i++)
				inputs[i] = 0.0f;
			for (int i = 0; i < lastFrame.Length; i++)
				lastFrame[i] = 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Tick(float input)
		{
			inputs[inPoint++] = input * gain;

			// Increment input pointer modulo length.
			if (inPoint == inputs.Length)
				inPoint = 0;

			lastFrame[0] = GetNextOut();
			doNextOut = true;

			// Increment output pointer modulo length.
			if (++outPoint == inputs.Length)
				outPoint = 0;

			return lastFrame[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float GetNextOut()
		{
			if (doNextOut)
			{
				// First 1/2 of interpolation
				nextOutput = inputs[outPoint] * omAlpha;
				// Second 1/2 of interpolation
				if (outPoint + 1 < inputs.Length)
					nextOutput += inputs[outPoint + 1] * alpha;
				else
					nextOutput += inputs[0] * alpha;
				doNextOut = false;
			}

			return nextOutput;
		}
	}
}