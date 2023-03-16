// See https://aka.ms/new-console-template for more information
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obs.Replay;
using Obs.Replay.Options;
using static Obs.Replay.ObsReplayLib;

var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                   .SetBasePath(AppContext.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .AddUserSecrets(typeof(Program).Assembly);

var configuration = builder.Build();

var services = new ServiceCollection();

services.AddScoped<ObsReplayer>();
services.Configure<ObsOptions>(configuration);

services.AddLogging(configure =>
{
    configure.AddConfiguration(configuration.GetSection("Logging"));
    configure.AddConsole();
});

var provider = services.BuildServiceProvider();

var source = new CancellationTokenSource();

var replayer = provider.GetService<ObsReplayer>();
var options = provider.GetService<IOptions<ObsOptions>>();

replayer!.Initialize();

replayer.ReplaySaved += Replayer_ReplaySaved;
replayer.FrameRendered += Replayer_FrameRendered;

void Replayer_FrameRendered(object? sender, video_data e)
{
    var img = new Image<Bgr, byte>(options.Value.Width, options.Value.Height, (int)e.linesize[0], e.data[0]);
    //img.Save($"c:\\temp\\replays\\{e.timestamp}.png");
    // Image recognition here
}

void Replayer_ReplaySaved(object? sender, string e)
{
    Console.WriteLine($"ReplaySaved event handler");
    source.Cancel();
}

var replayTask = replayer.StartAsync(source.Token);
var delayTask = Task.Delay(100*1000, source.Token).ContinueWith((param) => {
    replayer.SaveReplay();
}, source.Token);

await Task.WhenAll(replayTask, delayTask);

