//Playing audio from a file on disk.

using MiniAudioExNET;

namespace MiniAudioExNETExamples
{
    class Example1
    {
        static AudioSource source;
        static AudioClip clip;

        static void Main(string[] args)
        {
            AudioApp application = new AudioApp(44100, 2);
            application.Loaded += OnLoaded;
            application.Run();
        }

        static void OnLoaded()
        {
            source = new AudioSource();
            clip = new AudioClip("some_audio.mp3");
            source.Play(clip);
        }
    }
}