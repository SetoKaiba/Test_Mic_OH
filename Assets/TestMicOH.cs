using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.OpenHarmony;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class TestMicOH : MonoBehaviour
{
    public AudioClip audioClip;
    public AudioSource audioSource;
    public Button recordButton;
    public TMP_Text buttonText;
    public Button playButton;
    public bool speaking;
    public int currentIndex;
    public IntPtr capturer;
    public int frequency = 16000;
    public int audioLength = 30;
    public int channelCount = 1;
    public Queue<(float[], int)> queue = new();

    // Start is called before the first frame update
    void Start()
    {
        var permissionCallbacks = new PermissionCallbacks();
        permissionCallbacks.PermissionGranted += _ =>
        {
            Init();
        };
        Permission.RequestUserPermission(Permission.Microphone, permissionCallbacks);
    }

    void Init()
    {
        audioSource = GetComponent<AudioSource>();
        recordButton.onClick.AddListener(Record);
        playButton.onClick.AddListener(Play);

        unsafe
        {
            IntPtr builder;
            int result;
            result = OHAudio.Tuanjie_OH_AudioStreamBuilder_Create((IntPtr)(&builder),
                OHAudio.AUDIOSTREAM_TYPE_CAPTURER);
            if (result != 0)
            {
                Debug.LogError("Tuanjie_OH_AudioStreamBuilder_Create failed: " + result);
            }

            OHAudio.Tuanjie_OH_AudioStreamBuilder_SetSamplingRate(builder, frequency);
            OHAudio.Tuanjie_OH_AudioStreamBuilder_SetChannelCount(builder, channelCount);
            OHAudio.Tuanjie_OH_AudioStreamBuilder_SetLatencyMode(builder, OHAudio.AUDIOSTREAM_LATENCY_MODE_NORMAL);
            OHAudio.Tuanjie_OH_AudioStreamBuilder_SetSampleFormat(builder, OHAudio.AUDIOSTREAM_SAMPLE_S16LE);
            OHAudio.Tuanjie_OH_AudioStreamBuilder_SetEncodingType(builder, OHAudio.AUDIOSTREAM_ENCODING_TYPE_RAW);
            OHAudio.Tuanjie_OH_AudioStreamBuilder_SetCapturerInfo(builder, OHAudio.AUDIOSTREAM_SOURCE_TYPE_MIC);

            OHAudio.OH_AudioCapturer_Callbacks callbacks = new OHAudio.OH_AudioCapturer_Callbacks
            {
                onReadData = OnReadData,
                // onStreamEvent = OnStreamEvent,
                // onInterruptEvent = OnInterruptEvent,
                // onError = OnError
            };
            var handle = GCHandle.Alloc(this);
            OHAudio.Tuanjie_OH_AudioStreamBuilder_SetCapturerCallback(builder, callbacks, GCHandle.ToIntPtr(handle));

            IntPtr cap;
            result = OHAudio.Tuanjie_OH_AudioStreamBuilder_GenerateCapturer(builder, (IntPtr)(&cap));
            capturer = cap;
            if (result != 0)
            {
                Debug.LogError("Tuanjie_OH_AudioStreamBuilder_GenerateCapturer failed: " + result);
            }
        }
    }

    [MonoPInvokeCallback(typeof(OHAudio.OH_AudioCapturer_OnReadData))]
    static unsafe int OnReadData(IntPtr capturer, IntPtr userData, IntPtr buffer, int length)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var instance = (TestMicOH)handle.Target;
        var audioDataBytes = new byte[length];
        var audioData = new float[length / 2];
        fixed(byte* audioDataBytesPtr = audioDataBytes)
        {
            UnsafeUtility.MemCpy(audioDataBytesPtr, buffer.ToPointer(), length);
        }
        for (var i = 0; i < audioData.Length; i++)
        {
            var sample = (short)((audioDataBytes[i * 2 + 1] << 8) | audioDataBytes[i * 2]);
            audioData[i] = sample / 32768.0f;
        }

        var maxSamples = instance.audioLength * instance.frequency;
        instance.queue.Enqueue((audioData, instance.currentIndex));
        instance.currentIndex += audioData.Length;
        if (instance.currentIndex >= maxSamples)
        {
            instance.currentIndex -= maxSamples;
        }

        return OHAudio.AUDIOSTREAM_SUCCESS;
    }

    [MonoPInvokeCallback(typeof(OHAudio.OH_AudioCapturer_OnStreamEvent))]
    static unsafe int OnStreamEvent(IntPtr capturer, IntPtr userData, int @event)
    {
        Debug.Log("OnStreamEvent");
        return OHAudio.AUDIOSTREAM_SUCCESS;
    }

    [MonoPInvokeCallback(typeof(OHAudio.OH_AudioCapturer_OnInterruptEvent))]
    static unsafe int OnInterruptEvent(IntPtr capturer, IntPtr userData, int type, int hint)
    {
        Debug.Log("OnInterruptEvent");
        return OHAudio.AUDIOSTREAM_SUCCESS;
    }

    [MonoPInvokeCallback(typeof(OHAudio.OH_AudioCapturer_OnError))]
    static unsafe int OnError(IntPtr capturer, IntPtr userData, int error)
    {
        Debug.Log("OnError");
        return OHAudio.AUDIOSTREAM_SUCCESS;
    }

    // Update is called once per frame
    void Update()
    {
        while (queue.Count > 0)
        {
            var (data, index) = queue.Dequeue();
            audioClip.SetData(data, index);
        }
    }

    void Record()
    {
        if (!speaking)
        {
            speaking = true;
            currentIndex = 0;
            audioClip = AudioClip.Create("Microphone", frequency * audioLength, channelCount, frequency, false);
            OHAudio.Tuanjie_OH_AudioCapturer_Start(capturer);
            buttonText.text = "Stop";
        }
        else
        {
            speaking = false;
            OHAudio.Tuanjie_OH_AudioCapturer_Stop(capturer);
            // while (queue.Count > 0)
            // {
            //     var (data, index) = queue.Dequeue();
            //     Debug.Log($"SetData: {index}, {data.Length}");
            //     audioClip.SetData(data, index);
            // }
            buttonText.text = "Record";
        }
    }

    void Play()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}