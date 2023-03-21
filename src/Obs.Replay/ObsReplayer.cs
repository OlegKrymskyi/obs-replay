using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obs.Replay.Options;
using static Obs.Replay.ObsReplayLib;

namespace Obs.Replay
{
    public class ObsReplayer : IDisposable
    {
        public event EventHandler<string> ReplaySaved;

        public event EventHandler<video_data> FrameRendered;

        private bool disposed;

        private readonly ILogger<ObsReplayer> logger;

        private readonly IOptions<ObsOptions> options;

        private ManualResetEvent manualResetEvent;

        private IntPtr replayBuffer;

        private IntPtr scaler;

        private readonly ObsReplayLib.ReplaySavedCallback replaySavedCallback;

        private readonly ObsReplayLib.RawVideoCallbackNative rawVideoCallback;

        private long frameCounter = 0;

        public ObsReplayer(IOptions<ObsOptions> options, ILogger<ObsReplayer> logger)
        {
            this.options = options;
            this.logger = logger;
            this.manualResetEvent = new ManualResetEvent(false);

            this.replaySavedCallback = new ObsReplayLib.ReplaySavedCallback(this.ReplaySavedCallback);
            this.rawVideoCallback = new ObsReplayLib.RawVideoCallbackNative(this.RawVideoCallback);
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

            obs_remove_replay_saved_callback(this.replayBuffer, this.replaySavedCallback);
            obs_remove_raw_video_callback(this.rawVideoCallback, IntPtr.Zero);

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
                adapter = this.options.Value.Adapter,
                graphics_module = this.options.Value.GraphicsModule,
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

            obs_screen_capture_replay_config config = new()
            {
                data_path = this.options.Value.DataPath,
                module_path = this.options.Value.ModulesPath,
                module_data_path = this.options.Value.ModulesDataPath,
                video_encoder = this.options.Value.VideoEncoder,
                audio_encoder = this.options.Value.AudioEncoder,
                video_source = this.options.Value.VideoSource,
                audio_source = this.options.Value.AudioSource,
                max_time_sec = this.options.Value.MaxReplaySec
            };

            this.replayBuffer = ObsReplayLib.obs_init_screen_capture_replay(
                ref config,
                ref avi,
                ref ovi,
                this.options.Value.ReplaysPath);

            if (this.replayBuffer == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Obs replayer were not initialized");
            }

            ObsReplayLib.obs_add_replay_saved_callback(this.replayBuffer, this.replaySavedCallback);
            ObsReplayLib.obs_add_raw_video_callback(IntPtr.Zero, this.rawVideoCallback, IntPtr.Zero);

            this.scaler = ObsReplayLib.obs_init_scaler(video_format.VIDEO_FORMAT_NV12, (uint)options.Value.Width, (uint)options.Value.Height,
                video_format.VIDEO_FORMAT_BGR3, (uint)options.Value.Width, (uint)options.Value.Height);
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                frameCounter = 0;
                this.logger.LogDebug("screen capture started");
                var width = this.options.Value.Width;
                var height = this.options.Value.Height;
                var fps = this.options.Value.Fps;                

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

        private void ReplaySavedCallback(string file)
        {
            this.logger.LogDebug($"Replay saved to: {file}");
            if (this.ReplaySaved != null)
            {
                this.ReplaySaved(this, file);
            }
        }

        private void RawVideoCallback(IntPtr param, IntPtr streaming_frame, IntPtr recording_frame)
        {            
            if (this.FrameRendered != null)
            {
                // In this way we can control how much frames would we like to be processed.
                // In some cases we might be interested to apply image recognition only once per second and twice per second.
                // Then we could significaly reduce CPU utilization, as we won't make image scaling all the time.
                var handleFrameRate = this.options.Value.Fps / (this.options.Value.NumberOfFramesScaledPerSecond.GetValueOrDefault(1));
                if (frameCounter % handleFrameRate == 0)
                {
                    var framePtr = obs_scale_bgr(this.scaler, streaming_frame, (uint)this.options.Value.Width, (uint)this.options.Value.Height);
                    var frame = Marshal.PtrToStructure<video_data>(framePtr);

                    this.FrameRendered(this, frame);

                    obs_free_frame(framePtr);
                }
            }

            frameCounter++;
        }
    }
}