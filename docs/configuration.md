# Configuration

First of all you would need to configure an output format of a future replay:
```
    // Everything is simple of audio.
    // Standard audio format capturing.
    obs_audio_info_t avi = 
    {
        .samples_per_sec = 44100,
        .speakers = SPEAKERS_STEREO
    };
```

Everything is much more completed for the video. First of all you would have to provide a device (Adapter in terms of OBS) which will be responsilbe for the video capturing/processing:
![Video adapters](/docs/assets/img/video-adapters.png)

In my case, I have two adapters and the Adapter 1 is my GPU:
```
    obs_video_info_t ovi = 
    {
        .adapter = 1, // Adapter
        .graphics_module = "libobs-d3d11", // Standard graphics module, for Linux it will be different
        .fps_num = 60, // FPS
        .fps_den = 1,
        .base_width = 1920, // Screen size
        .base_height = 1020,
        .output_width = 1920, // Output video size
        .output_height = 1020,
        .output_format = VIDEO_FORMAT_NV12, // Frame video format
        .gpu_conversion = true, // Yes, us GPU whenever you can
        .colorspace = VIDEO_CS_DEFAULT,
        .range = VIDEO_RANGE_DEFAULT,
        .scale_type = OBS_SCALE_BILINEAR
    };
```
Pay attention, on a output_format parameter. libOBS supports amount of different frames format, such RGBA and etc. However, the most efficient way would be using VIDEO_FORMAT_NV12 as it would relay on GPU. 

libOBS will keep in memory all the captured frames but in a compressed format. For instance, you are capturing display frames such as RGBA images, then libOBS take each of those image and convert them to the video output format VIDEO_FORMAT_NV12 or VIDEO_FORMAT_I420 which are most common mp4 video formats.

So, libOBS would have to compress each and every image before put it into in-memory buffer. However, GPU Adapter allows to capture frames in VIDEO_FORMAT_NV12, so no addition compression would require, frame will be pushed directly to buffer.

Finally, you have to make some additional configuration for the replays themselves: where to store then and the replays duration.

For the video encoder try to use ffmpeg_nvenc whever you can.
Audio encoder is just ffmpeg_aac.
```
    // Replays configuration
    obs_screen_capture_replay_config_t config =
    {
        .data_path = dataPath, // path to libobs
        .module_path = modulePath, // path to libobs plugins
        .module_data_path = moduleDataPath, // path to libobs plugins configs / locales
        .video_encoder = "ffmpeg_nvenc", // 
        .audio_encoder = "ffmpeg_aac",
        .video_source = "monitor_capture",
        .audio_source = "wasapi_output_capture",
        .max_time_sec = 20 // Replays duration.
    };
```

You can also read how to use screen capture together with AI:
[AI integration](/docs/ai-integration.md)