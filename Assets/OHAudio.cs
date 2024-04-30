using System;
using System.Runtime.InteropServices;

public class OHAudio
{
    public delegate int OH_AudioCapturer_OnReadData(IntPtr capturer, IntPtr userData, IntPtr buffer, int length);

    public delegate int OH_AudioCapturer_OnStreamEvent(IntPtr capturer, IntPtr userData, int @event);

    public delegate int OH_AudioCapturer_OnInterruptEvent(IntPtr capturer, IntPtr userData, int type, int hint);

    public delegate int OH_AudioCapturer_OnError(IntPtr capturer, IntPtr userData, int error);

    public struct OH_AudioCapturer_Callbacks
    {
        public OH_AudioCapturer_OnReadData onReadData;
        public OH_AudioCapturer_OnStreamEvent onStreamEvent;
        public OH_AudioCapturer_OnInterruptEvent onInterruptEvent;
        public OH_AudioCapturer_OnError onError;
    }

    public static int AUDIOSTREAM_EVENT_ROUTING_CHANGED = 0;

    public static int AUDIOSTREAM_INTERRUPT_FORCE = 0;
    public static int AUDIOSTREAM_INTERRUPT_SHARE = 1;

    public static int AUDIOSTREAM_INTERRUPT_HINT_NONE = 0;
    public static int AUDIOSTREAM_INTERRUPT_HINT_RESUME = 1;
    public static int AUDIOSTREAM_INTERRUPT_HINT_PAUSE = 2;
    public static int AUDIOSTREAM_INTERRUPT_HINT_STOP = 3;
    public static int AUDIOSTREAM_INTERRUPT_HINT_DUCK = 4;
    public static int AUDIOSTREAM_INTERRUPT_HINT_UNDUCK = 5;

    public static int AUDIOSTREAM_SUCCESS = 0;
    public static int AUDIOSTREAM_ERROR_INVALID_PARAM = 1;
    public static int AUDIOSTREAM_ERROR_ILLEGAL_STATE = 2;
    public static int AUDIOSTREAM_ERROR_SYSTEM = 3;


    public static int AUDIOSTREAM_TYPE_RENDERER = 1;
    public static int AUDIOSTREAM_TYPE_CAPTURER = 2;

    public static int AUDIOSTREAM_SAMPLE_U8 = 0;
    public static int AUDIOSTREAM_SAMPLE_S16LE = 1;
    public static int AUDIOSTREAM_SAMPLE_S24LE = 2;
    public static int AUDIOSTREAM_SAMPLE_S32LE = 3;

    public static int AUDIOSTREAM_ENCODING_TYPE_RAW = 0;

    public static int AUDIOSTREAM_LATENCY_MODE_NORMAL = 0;
    public static int AUDIOSTREAM_LATENCY_MODE_FAST = 1;

    public static int AUDIOSTREAM_SOURCE_TYPE_INVALID = -1;
    public static int AUDIOSTREAM_SOURCE_TYPE_MIC = 0;
    public static int AUDIOSTREAM_SOURCE_TYPE_VOICE_RECOGNITION = 1;
    public static int AUDIOSTREAM_SOURCE_TYPE_PLAYBACK_CAPTURE = 2;
    public static int AUDIOSTREAM_SOURCE_TYPE_VOICE_COMMUNICATION = 7;

    // SteamBuilder
    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_Create(IntPtr builder, int type);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_Destroy(IntPtr builder);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_SetSamplingRate(IntPtr builder, int rate);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_SetChannelCount(IntPtr builder, int channelCount);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_SetSampleFormat(IntPtr builder, int format);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_SetEncodingType(IntPtr builder, int encodingType);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_SetLatencyMode(IntPtr builder, int latencyMode);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_SetCapturerInfo(IntPtr builder, int sourceType);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_SetCapturerCallback(IntPtr builder,
        OH_AudioCapturer_Callbacks sourceType, IntPtr userData);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioStreamBuilder_GenerateCapturer(IntPtr builder, IntPtr audioCapturer);

    // AudioCapturer
    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioCapturer_Release(IntPtr capturer);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioCapturer_Start(IntPtr capturer);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioCapturer_Pause(IntPtr capturer);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioCapturer_Stop(IntPtr capturer);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_OH_AudioCapturer_Flush(IntPtr capturer);

    public delegate void SlOHBufferQueueCallback(
        IntPtr caller,
        IntPtr pContext,
        ulong size);

    public struct SLDataLocator_IODevice
    {
        public ulong locatorType;
        public ulong deviceType;
        public ulong deviceID;
        public IntPtr device;
    }

    public struct SLDataSource
    {
        public IntPtr pLocator;
        public IntPtr pFormat;
    }

    public struct SLDataLocator_BufferQueue
    {
        public ulong locatorType;
        public ulong numBuffers;
    }

    public struct SLDataFormat_PCM
    {
        public ulong formatType;
        public ulong numChannels;
        public ulong samplesPerSec;
        public ulong bitsPerSample;
        public ulong containerSize;
        public ulong channelMask;
        public ulong endianness;
    }
    
    public struct SLDataSink {
        public IntPtr pLocator;
        public IntPtr pFormat;
    }

    public static ulong SL_BOOLEAN_FALSE = 0x00000000;
    public static ulong SL_BOOLEAN_TRUE = 0x00000001;

    public static ulong SL_DATALOCATOR_IODEVICE = 0x00000003;
    public static ulong SL_IODEVICE_AUDIOINPUT = 0x00000001;
    public static ulong SL_DEFAULTDEVICEID_AUDIOINPUT = 0xFFFFFFFF;

    public static ulong SL_DATALOCATOR_BUFFERQUEUE = 0x00000006;

    public static ulong SL_DATAFORMAT_PCM = 0x00000002;

    public static ushort SL_PCMSAMPLEFORMAT_FIXED_16 = 0x0010;

    public static ulong SL_RECORDSTATE_STOPPED = 0x00000001;
    public static ulong SL_RECORDSTATE_PAUSED = 0x00000002;
    public static ulong SL_RECORDSTATE_RECORDING = 0x00000003;

    // slCreatEngine
    [DllImport("OHAudio")]
    public static extern int Tuanjie_slCreateEngine(
        IntPtr pEngine,
        ulong numOptions,
        IntPtr pEngineOptions,
        ulong numInterfaces,
        IntPtr pInterfaceIds,
        IntPtr pInterfaceRequired);

    // SLInterfaceID
    [DllImport("OHAudio")]
    public static extern IntPtr Tuanjie_SLInterfaceID_SL_IID_ENGINE();
    [DllImport("OHAudio")]
    public static extern IntPtr Tuanjie_SLInterfaceID_SL_IID_RECORD();
    [DllImport("OHAudio")]
    public static extern IntPtr Tuanjie_SLInterfaceID_SL_IID_OH_BUFFERQUEUE();

    // SLObjectItf
    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLObjectItf_Realize(
        IntPtr self,
        ulong async);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLObjectItf_GetInterface(
        IntPtr self,
        IntPtr iid,
        IntPtr pInterface);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLObjectItf_Destroy(
        IntPtr self);

    // SLEngineItf
    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLEngineItf_CreateAudioRecorder(
        IntPtr self,
        IntPtr pRecorder,
        IntPtr pAudioSrc,
        IntPtr pAudioSnk,
        ulong numInterfaces,
        IntPtr pInterfaceIds,
        IntPtr pInterfaceRequired);

    // SLRecordItf
    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLRecordItf_SetRecordState(
        IntPtr self,
        ulong state);

    // SLOHBufferQueueItf
    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLOHBufferQueueItf_RegisterCallback(
        IntPtr self,
        [MarshalAs(UnmanagedType.FunctionPtr)]SlOHBufferQueueCallback callback,
        IntPtr pContext);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLOHBufferQueueItf_GetBuffer(
        IntPtr self,
        IntPtr buffer,
        IntPtr size);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLOHBufferQueueItf_Enqueue(
        IntPtr self,
        IntPtr buffer,
        ulong size);

    [DllImport("OHAudio")]
    public static extern int Tuanjie_SLOHBufferQueueItf_Clear(
        IntPtr self);
}