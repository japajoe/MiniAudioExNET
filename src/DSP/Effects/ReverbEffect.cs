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
using MiniAudioEx.Utilities;

namespace MiniAudioEx.DSP.Effects
{
    public struct ReverbPreset
    {
        public float roomSize;
        public float damping;
        public float wet;
        public float dry;
        public float width;
        public float inputWidth;
		
		public static ReverbPreset SmallRoom()
		{
			ReverbPreset preset;
			preset.roomSize = 0.2f;    // Small space
			preset.damping = 0.5f;     // Significant high-frequency absorption
			preset.wet = 0.3f;         // Subtle reverb level
			preset.dry = 0.9f;         // Strong original signal
			preset.width = 0.5f;       // Moderate stereo spread
			preset.inputWidth = 0.2f;  // Keep input fairly centered
			return preset;
		}

		public static ReverbPreset LargeHall()
		{
			ReverbPreset preset;
			preset.roomSize = 0.85f;   // Large expansive space
			preset.damping = 0.2f;     // Low damping for a bright tail
			preset.wet = 0.4f;         // Noticeable reverb
			preset.dry = 0.7f;         // Slightly reduced dry signal
			preset.width = 1.0f;       // Full stereo width
			preset.inputWidth = 0.8f;  // Wide input stage
			return preset;
		}

		public static ReverbPreset Cathedral()
		{
			ReverbPreset preset;
			preset.roomSize = 0.95f;   // Maximum decay time
			preset.damping = 0.15f;    // Very little absorption
			preset.wet = 0.5f;         // High wet mix
			preset.dry = 0.5f;         // Balanced with dry signal
			preset.width = 1.0f;       // Full stereo immersion
			preset.inputWidth = 1.0f;  // Maximum input widening
			return preset;
		}

		public static ReverbPreset Plate()
		{
			ReverbPreset preset;
			preset.roomSize = 0.5f;    // Mid-size simulated plate
			preset.damping = 0.1f;     // Very bright, metallic response
			preset.wet = 0.4f;         // Distinct effect
			preset.dry = 0.8f;         // High clarity for the source
			preset.width = 0.7f;       // Slightly narrower than a hall
			preset.inputWidth = 0.5f;  // Normal stereo input
			return preset;
		}

		public static ReverbPreset Tunnel()
		{
			ReverbPreset preset;
			preset.roomSize = 0.9f;    // Long narrow space
			preset.damping = 0.8f;     // High damping for a dark, muddy sound
			preset.wet = 0.6f;         // Heavy reverb presence
			preset.dry = 0.4f;         // Reverb starts to overpower dry
			preset.width = 0.3f;       // Narrow output for "tunnel" feel
			preset.inputWidth = 0.1f;  // Very narrow input
			return preset;
		}
    }

    public sealed class ReverbEffect: IAudioEffect
    {
        private Reverb reverb;
		
		public float RoomSize
		{
			get => reverb.RoomSize;
			set => reverb.RoomSize = value;
		}

		public float Damping
		{
			get => reverb.Damping;
			set => reverb.Damping = value;
		}

		public float Wet
		{
			get => reverb.Wet;
			set => reverb.Wet = value;
		}

		public float Dry
		{
			get => reverb.Dry;
			set => reverb.Dry = value;
		}

		public float Width
		{
			get => reverb.Width;
			set => reverb.Width = value;
		}

		public float InputWidth
		{
			get => reverb.InputWidth;
			set => reverb.InputWidth = value;
		}

		public float Mode
		{
			get => reverb.Mode;
			set => reverb.Mode = value;
		}

		public UInt64 DecayTimeInFrames
		{
			get => reverb.DecayTimeInFrames;
		}

        public ReverbEffect(UInt32 sampleRate, UInt32 channels)
		{
			reverb = new Reverb(sampleRate, channels);
		}
		
		public void OnProcess(NativeArray<float> framesIn, UInt32 frameCountIn, NativeArray<float> framesOut, ref UInt32 frameCountOut, UInt32 channels)
		{
			reverb.Process(framesIn, framesOut, frameCountIn);
		}

        public void OnDestroy() { }

		public void SetPreset(ReverbPreset preset)
		{
			RoomSize = preset.roomSize;
			Damping = preset.damping;
			Wet = preset.wet;
			Dry = preset.dry;
			Width = preset.width;
			InputWidth = preset.inputWidth;
		}
	}
}