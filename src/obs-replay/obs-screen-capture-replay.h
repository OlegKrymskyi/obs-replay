#pragma once

#include <obs.h>
#include <stdio.h>

#pragma comment (lib, "obs.lib")

typedef struct obs_audio_info obs_audio_info_t;
typedef struct obs_video_info obs_video_info_t;

typedef void (*obs_replay_saved_callback_t)(const char* path);

void obs_replay_saved_signal_callback(void* param, calldata_t* data);

EXPORT void obs_set_replay_saved_callback(obs_output_t* replay_buffer, obs_replay_saved_callback_t callback);

EXPORT void obs_save_replay(obs_output_t* replay_buffer);

EXPORT obs_output_t* obs_init_screen_capture_replay(
	const char* data_path, 
	const char* module_path, 
	const char* module_data_path,
	obs_audio_info_t* avi,
	obs_video_info_t* ovi,
	const char* replays_path);