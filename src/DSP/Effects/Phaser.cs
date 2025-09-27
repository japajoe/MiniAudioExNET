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

// based on https://www.musicdsp.org/en/latest/Effects/78-phaser-code.html
//     purpose: Phaser is a six stage phase shifter, intended to reproduce the
//              sound of a traditional analogue phaser effect.
//      Author: Thaddy de Koning, based on a musicdsp.pdf C++ Phaser by
//              Ross Bencina.http://www.musicdsp.org/musicdsp.pdf
//   Copyright: This version (c) 2003, Thaddy de Koning
//              Copyrighted Freeware

//     Remarks: his implementation uses six first order all-pass filters in
//              series, with delay time modulated by a sinusoidal.
//              This implementation was created to be clear, not efficient.
//              Obvious modifications include using a table lookup for the lfo,
//              not updating the filter delay times every sample, and not
//              tuning all of the filters to the same delay time.

//              It sounds sensationally good!

using System;

namespace MiniAudioEx.DSP.Effects
{
	public sealed class Phaser
	{
		private float zm1;
		private float depth;
		private float lfoInc;
		private float lfoPhase;
		private float feedback;
		private float rate;
		private float minimum;
		private float maximum;
		private float min;
		private float max;
		private float sampleRate;
		private readonly AllPass[] _allpassDelay = new AllPass[6];

		private const float KDenorm = 1E-25f;

		public Phaser()
		{
			sampleRate = 44100f;
			feedback = 0.7f;
			lfoPhase = 0;
			depth = 1;
			zm1 = 0;
			Minimum = 440f;
			Maximum = 1600f;
			Rate = 5f;
			for (int i = 0; i < _allpassDelay.Length; i++)
				_allpassDelay[i] = new AllPass();
		}

		public float Process(float x)
		{
			// Calculate and update phaser sweep LFO
			float d = min + (max - min) * ((float)(Math.Sin(lfoPhase) + 1) / 2);
			lfoPhase += lfoInc;
			if (lfoPhase >= Math.PI * 2)
				lfoPhase -= (float)(Math.PI * 2);

			// Update filter coeffs
			for (int i = 0; i < _allpassDelay.Length; i++)
				_allpassDelay[i].Delay = d;

			// Calculate output
			float result = _allpassDelay[0].Process(
						   _allpassDelay[1].Process(
						   _allpassDelay[2].Process(
						   _allpassDelay[3].Process(
						   _allpassDelay[4].Process(
						   _allpassDelay[5].Process(KDenorm + x + zm1 * feedback))))));

			zm1 = (float)Math.Tanh(result);

			return (float)Math.Tanh(1.4f * (x + result * depth));
		}

		private void Calculate()
		{
			min = minimum / (sampleRate / 2);
			max = maximum / (sampleRate / 2);
		}

		public float SampleRate
		{
			get => sampleRate;
			set
			{
				sampleRate = value;
				Rate = rate;
				Calculate();
			}
		}

		public float Depth
		{
			get => depth;
			set => depth = value;
		}

		public float Feedback
		{
			get => feedback;
			set => feedback = value;
		}

		public float Minimum
		{
			get => minimum;
			set
			{
				minimum = value;
				Calculate();
			}
		}

		public float Maximum
		{
			get => maximum;
			set
			{
				maximum = value;
				Calculate();
			}
		}

		public float Rate
		{
			get => rate;
			set
			{
				rate = value;
				lfoInc = 2 * (float)Math.PI * (rate / sampleRate);
			}
		}

		private class AllPass
		{
			private float _delay;
			private float _a1, _zm1;
			private float _sampleRate;

			public AllPass()
			{
				_a1 = 0;
				_zm1 = 0;
				_delay = 0;
				_sampleRate = 44100f;
			}

			public float Process(float x)
			{
				float result = x * -_a1 + _zm1;
				_zm1 = result * _a1 + x;
				return result;
			}

			public float SampleRate
			{
				get => _sampleRate;
				set => _sampleRate = value;
			}

			public float Delay
			{
				get => _delay;
				set
				{
					_delay = value;
					_a1 = (1 - value) / (1 + value);
				}
			}
		}
	}
}