// obs-replay-example.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <windows.h>
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

    obs_screen_capture_replay_config_t config =
    {
        .data_path = "C:\\projects\\OlegKrymskyi\\obs-replay\\src\\x64\\Debug\\data\\libobs\\",
        .module_path = "C:\\projects\\OlegKrymskyi\\obs-replay\\src\\x64\\Debug\\obs-plugins\\64bit\\",
        .module_data_path = "C:\\projects\\OlegKrymskyi\\obs-replay\\src\\x64\\Debug\\data\\obs-plugins\\",
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

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
