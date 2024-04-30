using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.OpenHarmony;
using UnityEngine.UI;

public class TestMicSLES : MonoBehaviour
{
    public AudioClip audioClip;
    public AudioSource audioSource;
    public Button recordButton;
    public TMP_Text buttonText;
    public Button playButton;
    public bool speaking;
    public int currentIndex;
    public IntPtr engineItf;
    public IntPtr pcmCapturerObject;
    public IntPtr recordItf;
    public IntPtr bufferQueueItf;
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
            IntPtr engineObject = IntPtr.Zero;
            int result;
            result = OHAudio.Tuanjie_slCreateEngine((IntPtr)(&engineObject), 0, IntPtr.Zero, 0, IntPtr.Zero,
                IntPtr.Zero);
            Debug.Log($"Tuanjie_slCreateEngine:{result}");
            result = OHAudio.Tuanjie_SLObjectItf_Realize(engineObject, OHAudio.SL_BOOLEAN_FALSE);
            Debug.Log($"Tuanjie_SLObjectItf_Realize:{result}");
            var engine = IntPtr.Zero;
            result = OHAudio.Tuanjie_SLObjectItf_GetInterface(engineObject,
                OHAudio.Tuanjie_SLInterfaceID_SL_IID_ENGINE(), (IntPtr)(&engine));
            Debug.Log($"Tuanjie_SLObjectItf_GetInterface:{result}");
            engineItf = engine;
        }
    }

    [MonoPInvokeCallback(typeof(OHAudio.SlOHBufferQueueCallback))]
    static unsafe void BufferQueueCallback(
        IntPtr bufferQueueItf,
        IntPtr pContext,
        ulong size)
    {
        var handle = GCHandle.FromIntPtr(pContext);
        var instance = (TestMicSLES)handle.Target;
        var buffer = IntPtr.Zero;
        ulong pSize = 0;
        OHAudio.Tuanjie_SLOHBufferQueueItf_GetBuffer(bufferQueueItf, (IntPtr)(&buffer), (IntPtr)(&pSize));
        if (buffer != IntPtr.Zero)
        {
            var audioDataBytes = new byte[size];
            var audioData = new float[size / 2];
            fixed (byte* audioDataBytesPtr = audioDataBytes)
            {
                UnsafeUtility.MemCpy(audioDataBytesPtr, buffer.ToPointer(), (long)size);
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
        
            OHAudio.Tuanjie_SLOHBufferQueueItf_Enqueue(bufferQueueItf, buffer, size);
        }
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
            unsafe
            {
                OHAudio.SLDataLocator_IODevice ioDevice;
                ioDevice.locatorType = OHAudio.SL_DATALOCATOR_IODEVICE;
                ioDevice.deviceType = OHAudio.SL_IODEVICE_AUDIOINPUT;
                ioDevice.deviceID = OHAudio.SL_DEFAULTDEVICEID_AUDIOINPUT;
                ioDevice.device = IntPtr.Zero;
                OHAudio.SLDataSource audioSource;
                audioSource.pLocator = (IntPtr)(&ioDevice);
                audioSource.pFormat = IntPtr.Zero;
                OHAudio.SLDataLocator_BufferQueue bufferQueue;
                bufferQueue.locatorType = OHAudio.SL_DATALOCATOR_BUFFERQUEUE;
                bufferQueue.numBuffers = 3;
                OHAudio.SLDataFormat_PCM format_pcm;
                format_pcm.formatType = OHAudio.SL_DATAFORMAT_PCM;
                format_pcm.numChannels = (ulong)channelCount;
                format_pcm.samplesPerSec = (ulong)(frequency * 1000);
                format_pcm.bitsPerSample = OHAudio.SL_PCMSAMPLEFORMAT_FIXED_16;
                format_pcm.containerSize = 0;
                format_pcm.channelMask = 0;
                format_pcm.endianness = 0;
                OHAudio.SLDataSink audioSink;
                audioSink.pLocator = (IntPtr)(&bufferQueue);
                audioSink.pFormat = (IntPtr)(&format_pcm);

                var pcm = IntPtr.Zero;
                int result;
                result = OHAudio.Tuanjie_SLEngineItf_CreateAudioRecorder(engineItf, (IntPtr)(&pcm),
                    (IntPtr)(&audioSource), (IntPtr)(&audioSink), 0, IntPtr.Zero, IntPtr.Zero);
                Debug.Log($"Tuanjie_SLEngineItf_CreateAudioRecorder:{result}");
                pcmCapturerObject = pcm;
                var record = IntPtr.Zero;
                result = OHAudio.Tuanjie_SLObjectItf_GetInterface(pcmCapturerObject,
                    OHAudio.Tuanjie_SLInterfaceID_SL_IID_RECORD(),
                    (IntPtr)(&record));
                Debug.Log($"Tuanjie_SLObjectItf_GetInterface:{result}");
                recordItf = record;
                var bq = IntPtr.Zero;
                result = OHAudio.Tuanjie_SLObjectItf_GetInterface(pcmCapturerObject,
                    OHAudio.Tuanjie_SLInterfaceID_SL_IID_OH_BUFFERQUEUE(), (IntPtr)(&bq));
                bufferQueueItf = bq;
                Debug.Log($"Tuanjie_SLObjectItf_GetInterface:{result}");
                var handle = GCHandle.Alloc(this);
                var pContext = GCHandle.ToIntPtr(handle);
                result = OHAudio.Tuanjie_SLOHBufferQueueItf_RegisterCallback(bufferQueueItf, BufferQueueCallback,
                    pContext);
                Debug.Log($"Tuanjie_SLOHBufferQueueItf_RegisterCallback:{result}");
                result = OHAudio.Tuanjie_SLRecordItf_SetRecordState(recordItf, OHAudio.SL_RECORDSTATE_RECORDING);
                Debug.Log($"Tuanjie_SLRecordItf_SetRecordState:{result}");
            }

            buttonText.text = "Stop";
        }
        else
        {
            speaking = false;
            int result;
            result = OHAudio.Tuanjie_SLRecordItf_SetRecordState(recordItf, OHAudio.SL_RECORDSTATE_STOPPED);
            Debug.Log($"Tuanjie_SLRecordItf_SetRecordState:{result}");
            result = OHAudio.Tuanjie_SLObjectItf_Destroy(pcmCapturerObject);
            Debug.Log($"Tuanjie_SLObjectItf_Destroy:{result}");
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