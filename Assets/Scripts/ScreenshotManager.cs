// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;
// using System;

// public class ScreenshotManager : MonoBehaviour 
// {
//     private bool isRecording = false;
//     private string videoPath;
//     private const int RECORD_PERMISSION_CODE = 100;

//     public string GetAndroidExternalStoragePath()
//     {
//         if (Application.platform != RuntimePlatform.Android)
//             return Application.persistentDataPath;

//         var jc = new AndroidJavaClass("android.os.Environment");
//         var path = jc.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", 
//             jc.GetStatic<string>("DIRECTORY_DCIM"))
//             .Call<string>("getAbsolutePath");
//         return path;
//     }

//     public void ClickShare()
//     {
//         StartCoroutine(TakeSSAndShare());
//     }

//     private IEnumerator TakeSSAndShare()
//     {
//         string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
//         yield return new WaitForEndOfFrame();
//         Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
//         ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
//         ss.Apply();

//         string filePath = Path.Combine(GetAndroidExternalStoragePath(), "Room_Config-" + timeStamp + ".png");
//         File.WriteAllBytes(filePath, ss.EncodeToPNG());

//         Destroy(ss);
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ScreenshotManager : MonoBehaviour 
{
    [SerializeField] private Button recordButton;
    // [SerializeField] private Image recordingIndicator;
    
    [Header("Recording Settings")]
    [SerializeField] private int frameRate = 30;
    [SerializeField] private int bitRate = 10000000; // 10Mbps

    private bool isRecording = false;
    private float recordingStartTime;
    private const float MAX_RECORDING_TIME = 10f;
    private int recordingFingerID = -1;
    
    private List<Frame> recordedFrames;

    // Class to store frame data
    private class Frame
    {
        public byte[] Data;
        public int Width;
        public int Height;

        public Frame(byte[] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;
        }
    }

    void Start()
    {
        recordedFrames = new List<Frame>();
        SetupRecordButton();
    }

    private void SetupRecordButton()
    {
        EventTrigger trigger = recordButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = recordButton.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => {
            PointerEventData pointerData = (PointerEventData)data;
            recordingFingerID = pointerData.pointerId;
            StartRecording();
        });
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            PointerEventData pointerData = (PointerEventData)data;
            if (pointerData.pointerId == recordingFingerID)
            {
                StopRecording();
                recordingFingerID = -1;
            }
        });
        trigger.triggers.Add(pointerUp);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            PointerEventData pointerData = (PointerEventData)data;
            if (pointerData.pointerId == recordingFingerID)
            {
                StopRecording();
                recordingFingerID = -1;
            }
        });
        trigger.triggers.Add(pointerExit);
    }

    private void Update()
    {
        if (isRecording)
        {
            // Visual feedback
            float pulse = (Mathf.Sin(Time.time * 4) + 1) / 2;
            recordButton.image.color = new Color(1, 0, 0, 0.5f + pulse * 0.5f);
            // if (recordingIndicator != null)
            // {
            //     recordingIndicator.gameObject.SetActive(true);
            //     recordingIndicator.color = new Color(1, 0, 0, 0.5f + pulse * 0.5f);
            // }

            // Check for maximum recording time
            if (Time.time - recordingStartTime >= MAX_RECORDING_TIME)
            {
                StopRecording();
            }
        }
        else
        {
            recordButton.image.color = Color.white;
            // if (recordingIndicator != null)
            // {
            //     recordingIndicator.gameObject.SetActive(false);
            // }
        }
    }

    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            recordingStartTime = Time.time;
            recordedFrames.Clear();
            StartCoroutine(RecordFrames());
            Debug.Log("Recording Started");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            StartCoroutine(SaveVideoFromFrames());
            Debug.Log("Recording Stopped");
        }
    }

    private IEnumerator RecordFrames()
    {
        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            // Capture frame
            Texture2D frameTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            frameTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            frameTexture.Apply();

            // Store frame data
            Frame frame = new Frame(
                frameTexture.GetRawTextureData(),
                frameTexture.width,
                frameTexture.height
            );
            recordedFrames.Add(frame);

            Destroy(frameTexture);

            // Control frame rate
            yield return new WaitForSeconds(1f / frameRate);
        }
    }

    private IEnumerator SaveVideoFromFrames()
    {
        if (recordedFrames.Count == 0)
        {
            Debug.LogError("No frames recorded!");
            yield break;
        }

        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string videoFileName = $"AR_Video_{timeStamp}.mp4";

        // Combine all frames into a byte array
        List<byte> videoData = new List<byte>();
        foreach (Frame frame in recordedFrames)
        {
            videoData.AddRange(frame.Data);
            yield return null;
        }

        // Save to gallery
        NativeGallery.SaveVideoToGallery(
            videoData.ToArray(),
            "AR_Videos",
            videoFileName,
            (success, path) =>
            {
                if (success)
                {
                    Debug.Log($"Video saved successfully to: {path}");
                }
                else
                {
                    Debug.LogError("Failed to save video");
                }
            }
        );

        // Clean up
        recordedFrames.Clear();
    }

    public void ClickShare()
    {
        StartCoroutine(TakeScreenshot());
    }

    private IEnumerator TakeScreenshot()
    {
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        yield return new WaitForEndOfFrame();
        
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // Save to Photos app (Camera Roll)
            NativeGallery.SaveImageToGallery(ss, "Room_Config", "Room_Config-" + timeStamp + ".png",
                (success, path) => Debug.Log(success ? "Successfully saved to Photos" : "Failed to save to Photos"));
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            // Save to DCIM folder
            string fileName = "Room_Config-" + timeStamp + ".png";
            string filePath = Path.Combine(GetStoragePath(), fileName);
            try
            {
                File.WriteAllBytes(filePath, ss.EncodeToPNG());
                Debug.Log("Screenshot saved to: " + filePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save screenshot: " + e.Message);
            }
        }

        Destroy(ss);
    }

    public string GetStoragePath()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var jc = new AndroidJavaClass("android.os.Environment");
            var path = jc.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", 
                jc.GetStatic<string>("DIRECTORY_DCIM"))
                .Call<string>("getAbsolutePath");
            return path;
        }
        else
        {
            return Application.persistentDataPath;
        }
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;
// using System;
// using static NativeGallery;

// public class ScreenshotManager : MonoBehaviour 
// {
//     public string GetStoragePath()
//     {
//         if (Application.platform == RuntimePlatform.Android)
//         {
//             var jc = new AndroidJavaClass("android.os.Environment");
//             var path = jc.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", 
//                 jc.GetStatic<string>("DIRECTORY_DCIM"))
//                 .Call<string>("getAbsolutePath");
//             return path;
//         }
//         else
//         {
//             return Application.persistentDataPath;
//         }
//     }

//     public void ClickShare()
//     {
//         StartCoroutine(TakeScreenshot());
//     }

//     private IEnumerator TakeScreenshot()
//     {
//         string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
//         yield return new WaitForEndOfFrame();
        
//         Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
//         ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
//         ss.Apply();

//         if (Application.platform == RuntimePlatform.IPhonePlayer)
//         {
//             // Save to Photos app (Camera Roll)
//             SaveImageToGallery(ss, "Room_Config", "Room_Config-" + timeStamp + ".png",
//                 (success, path) => Debug.Log(success ? "Successfully saved to Photos" : "Failed to save to Photos"));
//         }
//         else if (Application.platform == RuntimePlatform.Android)
//         {
//             // Save to DCIM folder
//             string fileName = "Room_Config-" + timeStamp + ".png";
//             string filePath = Path.Combine(GetStoragePath(), fileName);
//             try
//             {
//                 File.WriteAllBytes(filePath, ss.EncodeToPNG());
//                 Debug.Log("Screenshot saved to: " + filePath);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError("Failed to save screenshot: " + e.Message);
//             }
//         }

//         Destroy(ss);
//     }
// }