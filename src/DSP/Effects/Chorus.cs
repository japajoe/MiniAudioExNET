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
using MiniAudioEx.DSP.Generators;

namespace MiniAudioEx.DSP.Effects
{
	public sealed class Chorus
	{
		private DelayLine[] delayLines;
		private Oscillator[] mods;
		private float[] lastFrame;
		private float baseLength;
		private float modDepth;
		private float effectMix;

		public float EffectMix
		{
			get => effectMix;
			set
			{
				if (value < 0)
					value = 0;
				if (value > 1.0f)
					value = 1.0f;
				effectMix = value;
			}
		}

		public float ModDepth
		{
			get => modDepth;
			set
			{
				if (value < 0.0f)
					value = 0.0f;
				if (value > 1.0f)
					value = 1.0f;
				modDepth = value;
			}
		}

		public float ModFrequency
		{
			get => mods[0].Frequency;
			set
			{
				mods[0].Frequency = value;
				mods[1].Frequency = value * 1.1111f;
			}
		}

		public Chorus(float baseDelay = 6000)
		{
			delayLines = new DelayLine[2];
			mods = new Oscillator[2];
			lastFrame = new float[2];
			baseLength = baseDelay;

			for (int i = 0; i < delayLines.Length; i++)
			{
				delayLines[i] = new DelayLine();
			}

			for (int i = 0; i < mods.Length; i++)
			{
				mods[i] = new Oscillator(WaveType.Sine, 0.2f, 1.0f);
			}

			delayLines[0].MaximumDelay = (UInt32)(baseDelay * 1.414f) + 2;
			delayLines[0].Delay = baseDelay;
			delayLines[1].MaximumDelay = (UInt32)(baseDelay * 1.414f) + 2;
			delayLines[1].Delay = baseDelay;

			mods[0].Frequency = 0.2f;
			mods[1].Frequency = 0.222222f;

			modDepth = 0.05f;
			effectMix = 0.5f;

			Clear();
		}

		public void Clear()
		{
			delayLines[0].Clear();
			delayLines[1].Clear();
			lastFrame[0] = 0.0f;
			lastFrame[1] = 0.0f;
		}

		public float Tick(float input, UInt32 channel = 0)
		{
			if (channel > 1)
				return 0.0f;

			delayLines[0].Delay = baseLength * 0.707f * (1.0f + modDepth * mods[0].GetValue());
			delayLines[1].Delay = baseLength * 0.5f * (1.0f - modDepth * mods[1].GetValue());
			lastFrame[0] = effectMix * (delayLines[0].Tick(input) - input) + input;
			lastFrame[1] = effectMix * (delayLines[1].Tick(input) - input) + input;
			return lastFrame[channel];
		}
	}
}