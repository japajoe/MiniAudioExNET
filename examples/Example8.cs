//An example of using the advanced API.

using System;
using MiniAudioEx.Native;
using MiniAudioEx.Core.AdvancedAPI;

namespace MiniAudioExExamples
{
	class Example8
	{
		static void Main(string[] args)
		{
            MaLog log = new MaLog();
            log.Message += OnLog;

            MaContext context = new MaContext(log);

            MaDecodingBackendVTable[] vtables = {
                MaDecodingBackendVTable.CreateFromLibVorbis()
            };

            MaResourceManager resourceManager = new MaResourceManager(vtables);

            MaDeviceInfo deviceInfo = context.GetDefaultPlaybackDevice();
            ma_device_data_proc deviceDataCallback = OnDeviceData;
            MaDevice device = new MaDevice(context, deviceInfo, ma_format.f32, 2, 44100, 2048, deviceDataCallback);

            MaEngine engine = new MaEngine(device, resourceManager);
            pEngine = engine.Handle;

            device.Start();

            MaSound sound = new MaSound(engine);
            sound.Loop = true;
            sound.LoadFromFile("some_audio.ogg", true);
            
            sound.Play();
            Console.ReadLine();

            sound.Stop();
            device.Stop();

            // Dispose in the reverse order of initialization
            sound.Dispose();
            engine.Dispose();
            resourceManager.Dispose();
            device.Dispose();
            context.Dispose();
            log.Dispose();
		}
		
        private static void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
        {
            if (pEngine.pointer == IntPtr.Zero)
                return;

            MiniAudioNative.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
        }

        private static void OnLog(UInt32 level, string message)
        {
            Console.Write("Log [{0}] {1}", level, message);
        }
    }
}