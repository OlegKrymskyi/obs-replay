#include "obs-screen-capture-replay.h"

void log_handler(int log_level, const char* format, va_list args, void* param)
{
	char out[4096];
	vsnprintf(out, sizeof(out), format, args);

	switch (log_level) {
	case LOG_DEBUG:
		fprintf(stdout, "debug: %s\n", out);
		fflush(stdout);
		break;

	case LOG_INFO:
		fprintf(stdout, "info: %s\n", out);
		fflush(stdout);
		break;

	case LOG_WARNING:
		fprintf(stdout, "warning: %s\n", out);
		fflush(stdout);
		break;

	case LOG_ERROR:
		fprintf(stderr, "error: %s\n", out);
		fflush(stderr);
	}

	UNUSED_PARAMETER(param);
}


void obs_replay_saved_signal_callback(void* param, calldata_t* data)
{
	const char* value;
	if (calldata_get_string(data, "path", &value))
	{
		printf("replay saved at %s\n", value);
	}

	obs_replay_saved_callback_t callback = (obs_replay_saved_callback_t)param;
	if (callback != NULL)
	{
		callback(value);
	}
}

void obs_set_replay_saved_callback(obs_output_t* replay_buffer, obs_replay_saved_callback_t callback)
{
	signal_handler_t* signals = obs_output_get_signal_handler(replay_buffer);
	signal_handler_connect(signals, "saved", obs_replay_saved_signal_callback, callback);
}

void obs_save_replay(obs_output_t* replay_buffer)
{
	calldata_t cd;
	proc_handler_t* ph = obs_output_get_proc_handler(replay_buffer);
	if (proc_handler_call(ph, "save", &cd))
	{
		printf("Replay save kicked off\n");
	}
}

void obs_start_screen_capture(obs_output_t* replay_buffer)
{
	bool outputStartSuccess = obs_output_start(replay_buffer);
	printf("output successful: %d\n", outputStartSuccess);
	if (outputStartSuccess != true) {
		printf("output error: %s\n", obs_output_get_last_error(replay_buffer));
	}
}

obs_output_t* obs_init_screen_capture_replay(
	const char* data_path,
	const char* module_path,
	const char* module_data_path,
	obs_audio_info_t* avi,
	obs_video_info_t* ovi,
	const char* replays_path)
{
	if (obs_initialized()) {
		return NULL;
	}

	base_set_log_handler(log_handler, NULL);

	printf("libobs version: %s\n", obs_get_version_string());
	if (!obs_startup("en-US", NULL, NULL)) {
		return NULL;
	}
	obs_add_data_path(data_path);
	obs_add_module_path(module_path, module_data_path);
	obs_load_all_modules();
	obs_log_loaded_modules();

	bool resetAudioCode = obs_reset_audio(&avi);

	int resetVideoCode = obs_reset_video(&ovi);
	if (resetVideoCode != OBS_VIDEO_SUCCESS) {
		return NULL;
	}

	obs_post_load_modules();

	// Setup screen capture
	obs_source_t* videoSource = obs_source_create("monitor_capture", "Monitor Capture Source", NULL, NULL);

	// Setup video encoder
	obs_data_t* videoEncoderSettings = obs_data_create();
	obs_data_set_bool(videoEncoderSettings, "use_bufsize", true);
	obs_data_set_string(videoEncoderSettings, "profile", "high");
	obs_data_set_string(videoEncoderSettings, "preset", "veryfast");
	obs_data_set_string(videoEncoderSettings, "rate_control", "CRF");
	obs_data_set_int(videoEncoderSettings, "crf", 20);

	obs_encoder_t* videoEncoder = obs_video_encoder_create("obs_x264", "simple_h264_recording", videoEncoderSettings, NULL);
	obs_data_release(videoEncoderSettings);

	// Setup audio capture
	obs_source_t* audioSource = obs_source_create("wasapi_output_capture", "Audio Capture Source", NULL, NULL);
	
	// Setup audio encoder
	obs_encoder_t* audioEncoder = obs_audio_encoder_create("ffmpeg_aac", "simple_aac_recording", NULL, 0, NULL);

	obs_data_t* outputSettings = obs_data_create();
	obs_data_set_string(outputSettings, "directory", replays_path);
	obs_data_set_int(outputSettings, "max_time_sec", 20);
	obs_output_t* replayBuffer = obs_output_create("replay_buffer", "ReplayBuffer", outputSettings, NULL);
	obs_data_release(outputSettings);

	obs_encoder_set_video(videoEncoder, obs_get_video());
	obs_set_output_source(0, videoSource); //0 = VIDEO CHANNEL
	obs_output_set_video_encoder(replayBuffer, videoEncoder);
	printf("video encoder active: %d", (videoEncoder != NULL));

	obs_encoder_set_audio(audioEncoder, obs_get_audio());
	obs_set_output_source(1, audioSource); //1 = AUDIO CHANNEL
	obs_output_set_audio_encoder(replayBuffer, audioEncoder, 0);
	printf("audio encoder active: %d", (audioEncoder != NULL));

	return replayBuffer;
}