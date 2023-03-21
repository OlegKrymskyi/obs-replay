using Microsoft.Extensions.Options;
using static Obs.Replay.ObsReplayLib;

namespace Obs.Replay.Options
{
    public class ObsOptions
    {
        public string DataPath { get; set; }

        public string ModulesPath { get; set; }

        public string ModulesDataPath { get; set; }

        public string ReplaysPath { get; set; }

        public int Fps { get; set; }

        public int? NumberOfFramesScaledPerSecond { get; set; } = 1;

        public int Width { get; set; }

        public int Height { get; set; }

        public uint Adapter { get; set; }

        public string GraphicsModule { get; set; }

        public string VideoEncoder { get; set; }

        public string AudioEncoder { get; set; }

        public string VideoSource { get; set; }

        public string AudioSource { get; set; }

        public int MaxReplaySec { get; set; }
    }
}
