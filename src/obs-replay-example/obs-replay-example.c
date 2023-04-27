// obs-replay-example.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <windows.h>
#include <stdlib.h>
#include <obs-screen-capture-replay.h>

#pragma comment (lib, "obs-replay.lib")

int main()
{
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

    const char dataPath[MAX_PATH];
    _fullpath(dataPath, "data\\libobs\\", MAX_PATH);
    printf("OBS lib full path (%s)\n", dataPath);
    
    const char modulePath[MAX_PATH];
    _fullpath(modulePath, "obs-plugins\\64bit\\", MAX_PATH);
    printf("OBS plugins full path (%s)\n", modulePath);

    const char moduleDataPath[MAX_PATH];
    _fullpath(moduleDataPath, "data\\obs-plugins\\", MAX_PATH);
    printf("OBS plugins data full path (%s)\n", moduleDataPath);

    obs_screen_capture_replay_config_t config =
    {
        .data_path = dataPath,
        .module_path = modulePath,
        .module_data_path = moduleDataPath,
        .video_encoder = "ffmpeg_nvenc",
        .audio_encoder = "ffmpeg_aac",
        .video_source = "monitor_capture",
        .audio_source = "wasapi_output_capture",
        .max_time_sec = 20
    };

    obs_output_t* output = obs_init_screen_capture_replay(
        &config,
        &avi,
        &ovi,
        "c:\\temp\\replays");

    obs_start_screen_capture(output);

    Sleep(10 * 1000);

    obs_save_replay(output);

    Sleep(10 * 1000);
}

