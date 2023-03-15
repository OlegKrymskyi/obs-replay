using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obs.Replay.Options;
using static Obs.Replay.ObsReplayLib;

namespace Obs.Replay
{
    public class ObsReplayer : IDisposable
    {
        private bool disposed;

        private readonly ILogger<ObsReplayer> logger;

        private readonly IOptions<ObsOptions> options;

        private ManualResetEvent manualResetEvent;

        private IntPtr replayBuffer;

        private IntPtr scaler;

        public ObsReplayer(IOptions<ObsOptions> options, ILogger<ObsReplayer> logger)
        {
            this.options = options;
            this.logger = logger;
            this.manualResetEvent = new ManualResetEvent(false);
        }

        ~ObsReplayer()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (replayBuffer != IntPtr.Zero)
            {
                ObsReplayLib.obs_free_screen_capture_replay(replayBuffer);
                this.replayBuffer = IntPtr.Zero;
            }

            if (scaler != IntPtr.Zero)
            {
                ObsReplayLib.obs_free_scaler(scaler);
                this.scaler = IntPtr.Zero;
            }

            this.manualResetEvent.Dispose();

            this.disposed = true;
        }

        public void Initialize()
        {
            obs_audio_info avi = new()
            {
                samples_per_sec = 44100,
                speakers = speaker_layout.SPEAKERS_STEREO
            };

            obs_video_info ovi = new()
            {
                adapter = 0,
                graphics_module = "libobs-d3d11",
                fps_num = (uint)this.options.Value.Fps,
                fps_den = 1,
                base_width = (uint)options.Value.Width,
                base_height = (uint)options.Value.Height,
                output_width = (uint)options.Value.Width,
                output_height = (uint)options.Value.Height,
                output_format = video_format.VIDEO_FORMAT_NV12,
                gpu_conversion = true,
                colorspace = video_colorspace.VIDEO_CS_DEFAULT,
                range = video_range_type.VIDEO_RANGE_DEFAULT,
                scale_type = obs_scale_type.OBS_SCALE_BILINEAR
            };

            this.replayBuffer = ObsReplayLib.obs_init_screen_capture_replay(
                Path.GetFullPath(this.options.Value.DataPath),
                Path.GetFullPath(this.options.Value.ModulesPath),
                Path.GetFullPath(this.options.Value.ModulesDataPath),
                ref avi,
                ref ovi,
                this.options.Value.ReplaysPath);

            if (this.replayBuffer == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Obs replayer were not initialized");
            }

            this.scaler = ObsReplayLib.obs_init_scaler(video_format.VIDEO_FORMAT_NV12, (uint)options.Value.Width, (uint)options.Value.Height,
                video_format.VIDEO_FORMAT_BGR3, (uint)options.Value.Width, (uint)options.Value.Height);
        }

        public void SetRawVideoCallback(RawVideoCallback callback)
        {
            ObsReplayLib.obs_add_raw_video_callback(IntPtr.Zero, callback, IntPtr.Zero);
        }

        public void SaveReplay()
        {
            this.logger.LogDebug($"Save Replay was called");
            ObsReplayLib.obs_save_replay(this.replayBuffer);
        }

        public void Pause(bool pause)
        {
            this.logger.LogDebug($"screen capture pause: {pause}");
            ObsReplayLib.obs_pause_screen_capture(this.replayBuffer, true);
        }

        public Task StartAsync(ReplaySavedCallback callback, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                this.logger.LogDebug("screen capture started");
                var width = this.options.Value.Width;
                var height = this.options.Value.Height;
                var fps = this.options.Value.Fps;

                // obs signals
                if (callback != null)
                {
                    ObsReplayLib.obs_set_replay_saved_callback(this.replayBuffer, callback);
                }

                // Start replay buffer
                ObsReplayLib.obs_start_screen_capture(this.replayBuffer);

                while (!this.manualResetEvent.WaitOne(TimeSpan.FromMilliseconds(10000 * fps / fps), true) && !cancellationToken.IsCancellationRequested)
                {
                    // Do nothing
                }

                ObsReplayLib.obs_stop_screen_capture(this.replayBuffer);
                this.logger.LogDebug("screen capture stoped");
            });
        }
    }
}