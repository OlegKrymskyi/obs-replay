# AI Integration

Lets imagine that you have trained image recognition network which can indentify some objects or a state of a video game, such as Valve Dota 2.

Then you would like to make a replay according to the most spectaculler moment of a game.

In this case, you can use libOBS to keep in-memory buffer with the latest 20 secs of a captured game, and then, whenever, AI recognized that currently you have "spectacular" moment, you can send libOBS a command that it is time to flush the buffer.

This lib allow you to do that and more over, it is already wrapped with the dotnet which makes development much easier.

```
// Register everything in IoC.
var provider = services.BuildServiceProvider();
var source = new CancellationTokenSource();

// Get the obs replayer interface.
var replayer = provider.GetService<ObsReplayer>();
var options = provider.GetService<IOptions<ObsOptions>>();

// Initialize replayer and run it.
replayer!.Initialize();

// Handle replay saved event.
replayer.ReplaySaved += Replayer_ReplaySaved;

// Each time when OBS captured a new screen, it will send it over to you, however, pay attentions that most likelly, this image will be in a VIDEO_FORMAT_NV12 format, which would requires to convert it to BGR or somethings similar. Such kind of conversions might be quite expensive in terms of CPU utilization. So, try to recognize you sceen once per second of somethings, otherwise for the 60 fps, your processor will die.
replayer.FrameRendered += Replayer_FrameRendered;

void Replayer_FrameRendered(object? sender, video_data e)
{
    // Get your image
    var img = new Image<Bgr, byte>(options.Value.Width, options.Value.Height, (int)e.linesize[0], e.data[0]);    
}

void Replayer_ReplaySaved(object? sender, string e)
{
    Console.WriteLine($"ReplaySaved event handler");
    source.Cancel();
}

var replayTask = replayer.StartAsync(source.Token);
var delayTask = Task.Delay(10*1000, source.Token).ContinueWith((param) => {
    replayer.SaveReplay();
}, source.Token);

await Task.WhenAll(replayTask, delayTask);

```

