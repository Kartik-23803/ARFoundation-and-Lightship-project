using System.Collections;
using System.Collections.Generic;
using TextSpeech;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class VoiceController : MonoBehaviour
{
    const string LANG_CODE = "en-US";
    string lastSpokenText = "";
    DrawRect drawRect;
    private bool isInitialized = false;

    void Start()
    {
        Debug.Log("VoiceController: Starting initialization...");
        
        drawRect = FindObjectOfType<DrawRect>();
        if (drawRect == null)
            Debug.LogError("VoiceController: DrawRect component not found!");
        else
            Debug.Log("VoiceController: DrawRect component found successfully");

        try
        {
            Setup(LANG_CODE);
            Debug.Log("VoiceController: Setup completed with language code: " + LANG_CODE);
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("VoiceController: Setup failed with error: " + e.Message);
        }

    #if UNITY_ANDROID
        Debug.Log("VoiceController: Setting up Android callbacks");
        SpeechToText.Instance.onPartialResultsCallback = OnPartialSpeechResult;
    #endif
        SpeechToText.Instance.onResultCallback = OnFinalSpeechResult;
        TextToSpeech.Instance.onStartCallBack = OnSpeakStart;
        TextToSpeech.Instance.onDoneCallback = OnSpeakStop;
    }

    void Update()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("VoiceController: System not yet initialized");
            return;
        }

        UIRectObject rectObject = FindObjectOfType<UIRectObject>();
        if(rectObject != null)
        {
            Text textComponent = rectObject.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                string currentText = textComponent.text;
                Debug.Log("VoiceController: Current text found: " + currentText);
                
                if(string.IsNullOrEmpty(currentText))
                {
                    Debug.LogWarning("VoiceController: Current text is empty");
                    return;
                }

                if(currentText != lastSpokenText)
                {
                    Debug.Log("VoiceController: New text detected. Previous: '" + lastSpokenText + "' New: '" + currentText + "'");
                    drawRect.speakText = currentText;
                    
                    try
                    {
                        StartSpeaking(currentText);
                        Debug.Log("VoiceController: StartSpeaking called with text: " + currentText);
                        lastSpokenText = currentText;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("VoiceController: Failed to start speaking: " + e.Message);
                    }
                }
            }
            else
            {
                Debug.LogWarning("VoiceController: Text component not found in UIRectObject");
            }
        }
        else
        {
            Debug.Log("VoiceController: No UIRectObject found in scene");
        }
    }

    void CheckPermission()
    {
    #if UNITY_ANDROID
        Debug.Log("VoiceController: Checking Android permissions");
        if(!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.Log("VoiceController: Requesting microphone permission");
            Permission.RequestUserPermission(Permission.Microphone);
        }
        else
        {
            Debug.Log("VoiceController: Microphone permission already granted");
        }
    #endif
    }

    #region Text to Speech

    public void StartSpeaking(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogError("VoiceController: Attempted to speak empty message");
            return;
        }

        try
        {
            Debug.Log("VoiceController: Attempting to speak message: " + message);
            TextToSpeech.Instance.StartSpeak(message);
        }
        catch (System.Exception e)
        {
            Debug.LogError("VoiceController: Error in StartSpeak: " + e.Message);
        }
    }

    public void StopSpeaking()
    {
        try
        {
            Debug.Log("VoiceController: Stopping speech");
            TextToSpeech.Instance.StopSpeak();
        }
        catch (System.Exception e)
        {
            Debug.LogError("VoiceController: Error in StopSpeak: " + e.Message);
        }
    }

    void OnSpeakStart()
    {
        Debug.Log("VoiceController: Speech started successfully");
    }

    void OnSpeakStop()
    {
        Debug.Log("VoiceController: Speech completed successfully");
    }

    #endregion

    #region Speech to Text

    public void StartListening()
    {
        Debug.Log("VoiceController: Starting speech recognition");
        SpeechToText.Instance.StartRecording();
    }

    public void StopListening()
    {
        Debug.Log("VoiceController: Stopping speech recognition");
        SpeechToText.Instance.StopRecording();
    }

    void OnFinalSpeechResult(string result)
    {
        Debug.Log("VoiceController: Final speech result received: " + result);
    }

    void OnPartialSpeechResult(string result)
    {
        Debug.Log("VoiceController: Partial speech result received: " + result);
    }

    #endregion

    void Setup(string code)
    {
        Debug.Log("VoiceController: Setting up TTS and STT with language code: " + code);
        try
        {
            TextToSpeech.Instance.Setting(code, 1, 1);
            SpeechToText.Instance.Setting(code);
            Debug.Log("VoiceController: Setup completed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("VoiceController: Error during setup: " + e.Message);
            throw;
        }
    }

    void OnEnable()
    {
        Debug.Log("VoiceController: Component enabled");
        CheckPermission();
    }

    void OnDisable()
    {
        Debug.Log("VoiceController: Component disabled");
        StopSpeaking();
    }
}
// public class VoiceController : MonoBehaviour
// {
//     const string LANG_CODE = "en-US";
//     string lastSpokenText = "";
//     // TextMeshProUGUI uiText;
//     DrawRect drawRect;

//     void Start()
//     {
//         drawRect = FindObjectOfType<DrawRect>();
//         Setup(LANG_CODE);

//     #if UNITY_ANDROID
//         SpeechToText.Instance.onPartialResultsCallback = OnPartialSpeechResult;
//     #endif
//         SpeechToText.Instance.onResultCallback = OnFinalSpeechResult;
//         TextToSpeech.Instance.onStartCallBack = OnSpeakStart;
//         TextToSpeech.Instance.onDoneCallback = OnSpeakStop;
//     }

//     void Update()
//     {
//         if(FindObjectOfType<UIRectObject>() != null)
//         {
//             string currentText = FindObjectOfType<UIRectObject>().GetComponentInChildren<Text>().text;
            
//             // Only speak if the text has changed
//             if(currentText != lastSpokenText)
//             {
//                 drawRect.speakText = currentText;
//                 StartSpeaking(currentText);
//                 Debug.Log("Started speaking text");
//                 lastSpokenText = currentText;
//             }
//         }
//     }

//     void CheckPermission()
//     {
//     #if UNITY_ANDROID
//         if(!Permission.HasUserAuthorizedPermission(Permission.Microphone))
//         {
//             Permission.RequestUserPermission(Permission.Microphone);
//         }
//     #endif
//     }

//     #region Text to Speech

//     public void StartSpeaking(string message)
//     {
//         TextToSpeech.Instance.StartSpeak(message);
//     }

//     public void StopSpeaking()
//     {
//         TextToSpeech.Instance.StopSpeak();
//     }

//     void OnSpeakStart()
//     {
//         Debug.Log("Talking started...");
//     }

//     void OnSpeakStop()
//     {
//         Debug.Log("Talking stoped...");
//     }

//     #endregion

//     #region Speech to Text

//     public void StartListening()
//     {
//         SpeechToText.Instance.StartRecording();
//     }

//     public void StopListening()
//     {
//         SpeechToText.Instance.StopRecording();
//     }

//     void OnFinalSpeechResult(string result)
//     {
//         // if(uiText != null)
//         // {
//         //     uiText.text = result;
//         // }
//     }

//     void OnPartialSpeechResult(string result)
//     {
//         // if(uiText != null)
//         // {
//         //     uiText.text = result;
//         // }
//     }

//     #endregion

//     void Setup(string code)
//     {
//         TextToSpeech.Instance.Setting(code, 1, 1);
//         SpeechToText.Instance.Setting(code);
//     }
// }