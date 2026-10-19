using System.Diagnostics;
using System.Numerics;
using Meatcorps.Engine.RayLib.Stream;
using Raylib_cs;

// Create the Raylib window first.
Raylib.InitWindow(1920, 1080, "Meatcorps Streaming");

Raylib.InitAudioDevice();
// Create a separate, application-owned streaming target.
var streamTarget = Raylib.LoadRenderTexture(1920, 1080);
var sound = Raylib.LoadSound("sound.wav");
using var streamer = new VideoStream(new VideoStreamOptions
{
    Width = 1920,
    Height = 1080,
    Fps = 60,
    BitrateKbps = 8000,

    Encoder = VideoEncoder.VideoToolbox,
    Codec = VideoCodec.H265,

    Output = VideoOutput.Rtsp,
    RtspPath = "/meatcorps",
    Port = 8554,
    

    AudioEnabled = false,
    AudioSampleRate = 48000,
    AudioBitrateKbps = 128
});

Console.WriteLine(
    $"Window: {Raylib.GetScreenWidth()}x{Raylib.GetScreenHeight()}"
);

Console.WriteLine(
    $"Framebuffer: {Raylib.GetRenderWidth()}x{Raylib.GetRenderHeight()}"
);
streamer.Start();
/*using var audio = new RaylibMixedAudioCapture(
    streamer,
    sampleRate: 48000);*/

var bench = new Stopwatch();
var time = 0.0;
var timer = 0f;
var totaltime = 0.0;
while (!Raylib.WindowShouldClose())
{
    timer += Raylib.GetFrameTime();
    totaltime += Raylib.GetFrameTime();
    bench.Start();
    Raylib.BeginTextureMode(streamTarget);
    Raylib.ClearBackground(Raylib.IsSoundPlaying(sound) ? Color.DarkGray : Color.Black);
    Raylib.DrawText(DateTime.Now.ToString(), 10, 10, 64, Color.Red);
    
    Raylib.DrawText(DateTime.Now.ToString(), 10, 74, 64, Color.Green);
    
    Raylib.DrawText(DateTime.Now.ToString(), 10, 138, 64, Color.Blue);
    
    Raylib.DrawText($"LOOP TIME {(time / 1000000):f3} ms total time: {totaltime:f3}s", 10, 202, 64, Color.White);
    //Raylib.DrawText($"Dropped audio chunks: {audio.DroppedChunks}", 10 , 266, 32, Color.White);
    
    Raylib.EndTextureMode();

    if (timer > 2f)
    {
        Raylib.PlaySound(sound);
        timer = 0f;
    }
    
    Raylib.BeginDrawing();
    Raylib.DrawTexturePro(streamTarget.Texture, new Rectangle(0, 0, streamTarget.Texture.Width, -streamTarget.Texture.Height), new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), new Vector2(0, 0), 0, Color.White);
    Raylib.EndDrawing();

    // Your regular Raylib game loop:
    // 1. Render to streamTarget
    // 2. Raylib.EndTextureMode()
    // 3. Submit the finished texture:
    streamer.Update(streamTarget.Texture.Id, true);
    
    bench.Stop();
    time = 1000000000.0 * (double)bench.ElapsedTicks / Stopwatch.Frequency;
    bench.Reset();
    
    //if (audio.WorkerError is not null)
    //    Console.WriteLine(audio.WorkerError);
}

// During shutdown, while OpenGL still exists:
streamer.Stop();
Raylib.UnloadSound(sound);
Raylib.UnloadRenderTexture(streamTarget);
Raylib.CloseAudioDevice();
Raylib.CloseWindow();