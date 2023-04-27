# OBS Replay lib

## Overview
This library allows to generate replay using libobs library. Overall, OBS implements efficient way of the screen capturing with the high FPS.

MS Windows provides a several API for that, probably the most efficient is a Screen Duplication API, which is using DirectX undernise. However, only OBS provides a complete solution which allows to capture screen with the stable FPS level.

Pay attention, that 60 fps level for the 1920x1020 resolution could be achived only with the dedicated GPU card (in my case NVIDIA GeForce GTX 1660 Ti).

What is also good with OBS is that, your CPU remain almost fully unused.
![CPU usage](/docs/assets/img/cpu-gpu.png)

More over, OBS allows to keep video buffer in parallel with the custom frames processing. In my case I was recognizing video game state with AI and store the replays in case of some spectacular moment during the game.

## How to use
You can find and example in a source code but overall, everything a pretty simple
* Get compiled libOBS together with pluggings.
* Setup configuration of your replays
* Enjoy!

### C++ projects
```
    // Setup an ouput audio/video fromat
    obs_audio_info_t avi = 
    {
        .samples_per_sec = 44100,
        .speakers = SPEAKERS_STEREO
    };

    obs_video_info_t ovi = 
    {
        .adapter = 1, 
        .graphics_module = "libobs-d3d11",
        .fps_num = 60,
        .fps_den = 1,
        .base_width = 1920,
        .base_height = 1020,
        .output_width = 1920,
        .output_height = 1020,
        .output_format = VIDEO_FORMAT_NV12,
        .gpu_conversion = true,
        .colorspace = VIDEO_CS_DEFAULT,
        .range = VIDEO_RANGE_DEFAULT,
        .scale_type = OBS_SCALE_BILINEAR
    };

    // Replays configuration
    obs_screen_capture_replay_config_t config =
    {
        .data_path = dataPath, // path to libobs
        .module_path = modulePath, // path to libobs plugins
        .module_data_path = moduleDataPath, // path to libobs plugins configs / locales
        .video_encoder = "ffmpeg_nvenc",
        .audio_encoder = "ffmpeg_aac",
        .video_source = "monitor_capture",
        .audio_source = "wasapi_output_capture",
        .max_time_sec = 20
    };

    // Initialize capturing
    obs_output_t* output = obs_init_screen_capture_replay(
        &config,
        &avi,
        &ovi,
        "c:\\temp\\replays");

    // Start capturing
    obs_start_screen_capture(output);
```

Please read those articles as well:
[prerequisites](/docs/prerequisites.md)
[configuration](/docs/configuration.md)