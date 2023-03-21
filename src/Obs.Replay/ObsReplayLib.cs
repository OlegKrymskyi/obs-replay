using System;
using System.Runtime.InteropServices;
using static Obs.Replay.ObsReplayLib;

namespace Obs.Replay
{
    public static class ObsReplayLib
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct obs_video_info
        {
            public string graphics_module; //Marshal.PtrToStringAnsi

            public uint fps_num;       //Output FPS numerator
            public uint fps_den;       //Output FPS denominator

            public uint base_width;    //Base compositing width
            public uint base_height;   //Base compositing height

            public uint output_width;  //Output width
            public uint output_height; //Output height
            public video_format output_format; // Output format

            //Video adapter index to use (NOTE: avoid for optimus laptops)
            public uint adapter;

            //Use shaders to convert to different color formats

            [MarshalAs(UnmanagedType.I1)]
            public bool gpu_conversion;

            public video_colorspace colorspace;  //YUV type (if YUV)
            public video_range_type range;       //YUV range (if YUV)

            public obs_scale_type scale_type;    //How to scale if scaling
        };

        public enum video_format : int
        {
            VIDEO_FORMAT_NONE,

            /* planar 4:2:0 formats */
            VIDEO_FORMAT_I420, /* three-plane */
            VIDEO_FORMAT_NV12, /* two-plane, luma and packed chroma */

            /* packed 4:2:2 formats */
            VIDEO_FORMAT_YVYU,
            VIDEO_FORMAT_YUY2, /* YUYV */
            VIDEO_FORMAT_UYVY,

            /* packed uncompressed formats */
            VIDEO_FORMAT_RGBA,
            VIDEO_FORMAT_BGRA,
            VIDEO_FORMAT_BGRX,
            VIDEO_FORMAT_Y800, /* grayscale */

            /* planar 4:4:4 */
            VIDEO_FORMAT_I444,

            /* more packed uncompressed formats */
            VIDEO_FORMAT_BGR3,

            /* planar 4:2:2 */
            VIDEO_FORMAT_I422,

            /* planar 4:2:0 with alpha */
            VIDEO_FORMAT_I40A,

            /* planar 4:2:2 with alpha */
            VIDEO_FORMAT_I42A,

            /* planar 4:4:4 with alpha */
            VIDEO_FORMAT_YUVA,

            /* packed 4:4:4 with alpha */
            VIDEO_FORMAT_AYUV,

            /* planar 4:2:0 format, 10 bpp */
            VIDEO_FORMAT_I010, /* three-plane */
            VIDEO_FORMAT_P010, /* two-plane, luma and packed chroma */

            /* planar 4:2:2 10 bits */
            VIDEO_FORMAT_I210, // Little Endian

            /* planar 4:4:4 12 bits */
            VIDEO_FORMAT_I412, // Little Endian

            /* planar 4:4:4 12 bits with alpha */
            VIDEO_FORMAT_YA2L, // Little Endian
        };

        public enum video_colorspace : int
        {
            VIDEO_CS_DEFAULT,
            VIDEO_CS_601,
            VIDEO_CS_709,
            VIDEO_CS_SRGB,
            VIDEO_CS_2100_PQ,
            VIDEO_CS_2100_HLG,
        };

        public enum video_range_type : int
        {
            VIDEO_RANGE_DEFAULT,
            VIDEO_RANGE_PARTIAL,
            VIDEO_RANGE_FULL
        };

        public enum obs_scale_type : int
        {
            OBS_SCALE_DISABLE,
            OBS_SCALE_POINT,
            OBS_SCALE_BICUBIC,
            OBS_SCALE_BILINEAR,
            OBS_SCALE_LANCZOS,
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct obs_audio_info
        {
            public uint samples_per_sec;
            public speaker_layout speakers;
        };

        public enum speaker_layout : int
        {
            SPEAKERS_UNKNOWN,
            SPEAKERS_MONO,
            SPEAKERS_STEREO,
            SPEAKERS_2POINT1,
            SPEAKERS_QUAD,
            SPEAKERS_4POINT1,
            SPEAKERS_5POINT1,
            SPEAKERS_5POINT1_SURROUND,
            SPEAKERS_7POINT1,
            SPEAKERS_7POINT1_SURROUND,
            SPEAKERS_SURROUND,
        };

        private const int MAX_AV_PLANES = 8;

        [StructLayout(LayoutKind.Sequential)]
        public struct video_scale_info
        {
            public video_format format;
            public uint width;
            public uint height;
            public video_range_type range;
            public video_colorspace colorspace;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct video_data
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_AV_PLANES)]
            public IntPtr[] data;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_AV_PLANES)]
            public uint[] linesize;

            public ulong timestamp;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct obs_screen_capture_replay_config
        {
            public string data_path;
            public string module_path;
            public string module_data_path;
            public string video_encoder;
            public string audio_encoder;
            public string video_source;
            public string audio_source;
            public int max_time_sec;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void RawVideoCallback(IntPtr param, video_data streaming_frame, video_data recording_frame);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void RawVideoCallbackNative(IntPtr param, IntPtr streaming_frame, IntPtr recording_frame);

        [DllImport("obs", CallingConvention = CallingConvention.Cdecl)]
        public static extern void obs_add_raw_video_callback(IntPtr conversion, RawVideoCallback callback, IntPtr param);

        [DllImport("obs", CallingConvention = CallingConvention.Cdecl)]
        public static extern void obs_add_raw_video_callback(IntPtr conversion, RawVideoCallbackNative callback, IntPtr param);

        [DllImport("obs", CallingConvention = CallingConvention.Cdecl)]
        public static extern void obs_remove_raw_video_callback(RawVideoCallback callback, IntPtr param);

        [DllImport("obs", CallingConvention = CallingConvention.Cdecl)]
        public static extern void obs_remove_raw_video_callback(RawVideoCallbackNative callback, IntPtr param);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void ReplaySavedCallback(string path);

        [DllImport("obs-replay")]
        public static extern void obs_add_replay_saved_callback(IntPtr replay_buffer, ReplaySavedCallback callback);

        [DllImport("obs-replay")]
        public static extern void obs_remove_replay_saved_callback(IntPtr replay_buffer, ReplaySavedCallback callback);

        [DllImport("obs-replay")]
        public static extern void obs_save_replay(IntPtr replay_buffer);

        [DllImport("obs-replay")]
        public static extern void obs_start_screen_capture(IntPtr replay_buffer);

        [DllImport("obs-replay")]
        public static extern void obs_pause_screen_capture(IntPtr replay_buffer, bool pause);

        [DllImport("obs-replay")]
        public static extern void obs_stop_screen_capture(IntPtr replay_buffer);

        [DllImport("obs-replay")]
        public static extern IntPtr obs_init_screen_capture_replay(
            ref obs_screen_capture_replay_config config,
            ref obs_audio_info avi, 
            ref obs_video_info ovi, 
            string replays_path);

        [DllImport("obs-replay")]
        public static extern void obs_free_screen_capture_replay(IntPtr replay_buffer);

        [DllImport("obs-replay")]
        public static extern IntPtr obs_init_scaler(video_format src_format, uint src_width, uint src_height, 
            video_format dst_format, uint dst_width, uint dst_height);

        [DllImport("obs-replay")]
        public static extern void obs_free_scaler(IntPtr scaler);

        [DllImport("obs-replay")]
        public static extern IntPtr obs_scale_bgr(IntPtr scaler, IntPtr src, uint dst_width, uint dst_height);

        [DllImport("obs-replay")]
        public static extern IntPtr obs_free_frame(IntPtr frame);
    }
}
