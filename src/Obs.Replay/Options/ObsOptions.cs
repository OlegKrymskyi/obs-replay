namespace Obs.Replay.Options
{
    public class ObsOptions
    {
        public string DataPath { get; set; }

        public string ModulesPath { get; set; }

        public string ModulesDataPath { get; set; }

        public string ReplaysPath { get; set; }

        public int Fps { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int? NumberOfFramesScaledPerSecond { get; set; } = 1;
    }
}
