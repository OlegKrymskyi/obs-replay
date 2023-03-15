#pragma once

#include <stdio.h>
#include <obs.h>
#include <media-io/video-scaler.h>
#include <media-io/video-io.h>

#pragma comment (lib, "obs.lib")

typedef struct obs_audio_info obs_audio_info_t;
typedef struct obs_video_info obs_video_info_t;
typedef struct video_data video_data_t;

typedef void (*obs_replay_saved_callback_t)(const char* path);

void obs_replay_saved_signal_callback(void* param, calldata_t* data);

EXPORT void obs_set_replay_saved_callback(obs_output_t* replay_buffer, obs_replay_saved_callback_t callback);

EXPORT void obs_save_replay(obs_output_t* replay_buffer);

EXPORT void obs_start_screen_capture(obs_output_t* replay_buffer);

EXPORT void obs_pause_screen_capture(obs_output_t* replay_buffer, bool pause);

EXPORT void obs_stop_screen_capture(obs_output_t* replay_buffer);

EXPORT obs_output_t* obs_init_screen_capture_replay(
	const char* data_path, 
	const char* module_path, 
	const char* module_data_path,
	obs_audio_info_t* avi,
	obs_video_info_t* ovi,
	const char* replays_path);

EXPORT void obs_free_screen_capture_replay(obs_output_t* replay_buffer);

EXPORT video_scaler_t* obs_init_scaler(enum video_format src_format, uint32_t src_width, uint32_t src_height,
	enum video_format dst_format, uint32_t dst_width, uint32_t dst_height);

EXPORT void obs_free_scaler(video_scaler_t* scaler);

EXPORT video_data_t* obs_scale_bgr(video_scaler_t* scaler, video_data_t* src, uint32_t dst_width, uint32_t dst_height);