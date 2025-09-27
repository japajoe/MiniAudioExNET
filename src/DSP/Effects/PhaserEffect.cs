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

namespace MiniAudioEx.DSP.Effects
{
	public sealed class PhaserEffect : IAudioEffect
	{
		private Phaser phaser;
		
		public float Depth
		{
			get => phaser.Depth;
			set => phaser.Depth = value;
		}

		public float Feedback
		{
			get => phaser.Feedback;
			set => phaser.Feedback = value;
		}

		public float Minimum
		{
			get => phaser.Minimum;
			set => phaser.Minimum = value;
		}

		public float Maximum
		{
			get => phaser.Maximum;
			set => phaser.Maximum = value;
		}

		public float Rate
		{
			get => phaser.Rate;
			set => phaser.Rate = value;
		}

		public PhaserEffect(UInt32 sampleRate)
		{
			phaser = new Phaser();
			phaser.Depth = 1.0f;
			phaser.Feedback = 0.7f;
			phaser.Minimum = 440.0f;
			phaser.Maximum = 1600.0f;
			phaser.Rate = 5.0f;
			phaser.SampleRate = (float)sampleRate;
		}

		public void OnProcess(NativeArray<float> framesIn, uint frameCountIn, NativeArray<float> framesOut, ref uint frameCountOut, uint channels)
		{
            for (UInt32 i = 0; i < frameCountIn; i++)
            {
                for (UInt32 ch = 0; ch < channels; ch++)
                {
                    int index = (int)(i * channels + ch);
                    framesOut[index] = phaser.Process(framesIn[index]);
                }
            }
		}
		
		public void OnDestroy() {}
	}
}