using TMPro;
using UnityEngine;
#if UNITY_OPENHARMONY
using UnityEngine.OpenHarmony;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.UI;

public class TestMic : MonoBehaviour
{
    public AudioClip audioClip;
    public AudioSource audioSource;
    public Button recordButton;
    public TMP_Text buttonText;
    public Button playButton;
    public bool speaking;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_OPENHARMONY
        Permission.RequestUserPermission(Permission.Microphone);
#endif
#if UNITY_ANDROID
        Permission.RequestUserPermission(Permission.Microphone);
#endif
        audioSource = GetComponent<AudioSource>();
        recordButton.onClick.AddListener(Record);
        playButton.onClick.AddListener(Play);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void Record()
    {
        if (!speaking)
        {
            speaking = true;
            audioClip = Microphone.Start(null, true, 30, 16000);
            buttonText.text = "Stop";
        }
        else
        {
            speaking = false;
            Microphone.End(null);
            buttonText.text = "Record";
        }
    }

    void Play()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}