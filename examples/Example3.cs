//A minimal example of spatial audio.

using MiniAudioExNET;

namespace MiniAudioExNETExamples
{
    class Example3
    {
        static AudioSource source;
        static AudioListener listener;
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
            source.Loop = true;
            source.Spatial = true;
            source.Position = new Vector3f(10, 0, 0);

            listener = new AudioListener();

            clip = new AudioClip("some_audio.mp3");

            source.Play(clip);
        }
    }
}