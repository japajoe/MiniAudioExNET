# MiniAudioExNET-dev
This is a development branch for MiniAudioExNET. The goal for the future is to greatly simplify the native library, and implement as much as possible in C#. This was previously not feasible because I did not expose any MiniAudio API calls but this has changed.

What does this practically mean? No more StandardAPI/AdvancedAPI. You either use the classes in the `MiniAudioEx.Core` namespace or you are on your own and use whatever is in `MiniAudioEx.Native`. The AdvancedAPI was a novel idea but I think having too many options is counterproductive, it also takes significantly more time to maintain everything. That said, the AudioSource based functionality isn't going anywhere, I am just rewriting it and ultimately it will work almost the same as before.

# For those interested to try it out
```csharp
using System;
using MiniAudioEx.Core;

namespace Example
{
	class Program
	{
		private static AudioContext context;
		private static AudioSource source;
		private static AudioClip clip;

		static void Main(string[] args)
		{
			Console.CancelKeyPress += OnCancelKeyPress;

			context = new AudioContext(44100, 2, 2048);
			context.Log += OnLog;
			context.Create();
			context.MakeCurrent();

			source = new AudioSource();
			source.Loop = true;
			
			clip = new AudioClip("test.mp3");

			source.Play(clip);

			Console.WriteLine("Press CTRL + C to exit");

			while(true)
			{
				context.Update();
				System.Threading.Thread.Sleep(20);
			}
		}

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
			clip.Dispose();
			source.Dispose();
			context.Dispose();
        }

        private static void OnLog(UInt32 level, string message)
		{
			Console.Write("Log [{0}] {1}", level, message);
		}
	}
}
```

# Playing from an online stream
```csharp
using System;
using System.Collections.Generic;
using MiniAudioEx.Core;

namespace Example
{
	class Program
	{
		static void Main(string[] args)
		{
            using(var stream = new AudioStream())
			{
				stream.Connected += OnConnected;
				stream.Disconnected += OnDisconnected;
				stream.MetadataReceived += OnMetaDataReceived;

				stream.Play("http://ice1.somafm.com/groovesalad-128-mp3");

				Console.ReadLine();
			}
		}

        static void OnConnected(Dictionary<string, string> headers)
        {
            foreach(var h in headers)
            {
                Console.WriteLine(h.Key + ": " + h.Value);
            }
        }

        static void OnDisconnected(string reason)
        {
            Console.WriteLine("Stream disconnected: " + reason);
        }

        static void OnMetaDataReceived(string data)
        {
            Console.WriteLine("Meta data: " + data);
        }
	}
}
```