// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

replayer!.Initialize();

var callback = new ReplaySavedCallback((string path) => 
{
    source.Cancel();
});

var replayTask = replayer.StartAsync(callback, source.Token);
var delayTask = Task.Delay(10*1000, source.Token).ContinueWith((param) => {
    replayer.SaveReplay();
}, source.Token);

await Task.WhenAll(replayTask, delayTask);

