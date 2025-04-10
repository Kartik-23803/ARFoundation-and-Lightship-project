using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveVideoScript : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite recordSprite;
    [SerializeField] private Sprite stopSprite;
    bool isRecording = false;
    ScreenRecorder screenRecorder;


    void Start()
    {
        screenRecorder = FindObjectOfType<ScreenRecorder>();
        if (buttonImage != null && recordSprite != null)
        {
            buttonImage.sprite = recordSprite;
        }
    }

    public void StartRecording()
    {
        if(!isRecording)
        {
            isRecording = true;
            screenRecorder.StartRecording();
            if (buttonImage != null && stopSprite != null)
            {
                buttonImage.sprite = stopSprite;
            }
            Debug.Log("Recording Started");
        }
    }

    public void StopRecording()
    {
        if(isRecording)
        {
            isRecording = false;
            screenRecorder.StopRecording();
            if (buttonImage != null && recordSprite != null)
            {
                buttonImage.sprite = recordSprite;
            }
            Debug.Log("Recording Stopped");
            // SaveCapturedContent();
        }
    }

    public void ToggleRecording()
    {
        if (!isRecording)
            StartRecording();
        else
            StopRecording();
    }

    private void SaveCapturedContent()
    {
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        
        Action<byte[]> result = bytes =>
        {
            if (bytes != null && bytes.Length > 0)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // Save to Photos app (Camera Roll)
                    string fileName = "Capture_" + timeStamp + ".gif";
                    NativeGallery.SaveImageToGallery(bytes, "Captures", fileName,
                        (success, path) => 
                        {
                            if (success)
                            {
                                Debug.Log("Successfully saved to Photos at: " + path);
                            }
                            else
                            {
                                Debug.LogError("Failed to save to Photos");
                            }
                        });
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    // Save to DCIM folder
                    string fileName = "Capture_" + timeStamp + ".gif";
                    string filePath = Path.Combine(GetStoragePath(), fileName);
                    try
                    {
                        File.WriteAllBytes(filePath, bytes);
                        // Notify Android's media scanner to make the file visible in gallery
                        using (AndroidJavaClass javaClass = new AndroidJavaClass("android.media.MediaScannerConnection"))
                        {
                            using (AndroidJavaObject currentActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
                            {
                                javaClass.CallStatic("scanFile", currentActivity, 
                                    new string[] { filePath }, 
                                    new string[] { "image/gif" }, 
                                    null);
                            }
                        }
                        Debug.Log("Capture saved to: " + filePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to save capture: " + e.Message);
                    }
                }
            }
            else
            {
                Debug.LogError("Generated capture is empty");
            }
        };

        Debug.Log("Generating Capture...");
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

// using UnityEngine;
// using UnityEngine.UI;
// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEditor.Media;
// #endif
// using System.Collections;
// using System.IO;

// public class SaveVideoScript : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] private Image buttonImage;
//     [SerializeField] private Sprite recordSprite;
//     [SerializeField] private Sprite stopSprite;
    
//     private bool isRecording = false;
//     private string temporaryPath;

//     #if UNITY_ANDROID && !UNITY_EDITOR
//     private AndroidJavaObject mediaRecorder;
//     private AndroidJavaObject currentActivity;
//     #endif

//     void Start()
//     {
//         if (buttonImage != null && recordSprite != null)
//         {
//             buttonImage.sprite = recordSprite;
//         }

//         InitializeRecorder();
//     }

//     private void InitializeRecorder()
//     {
//         #if UNITY_ANDROID && !UNITY_EDITOR
//         try
//         {
//             AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//             currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
//             // Create new MediaRecorder instance
//             mediaRecorder = new AndroidJavaObject("android.media.MediaRecorder");
            
//             // Set temporary path
//             string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
//             temporaryPath = Path.Combine(Application.temporaryCachePath, $"temp_video_{timeStamp}.mp4");
//             Debug.Log($"Temporary path: {temporaryPath}");
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError($"Failed to initialize recorder: {e.Message}");
//         }
//         #endif
//     }

//     public void ToggleRecording()
//     {
//         if (!isRecording)
//             StartRecording();
//         else
//             StopRecording();
//     }

//     public void StartRecording()
//     {
//         if (!isRecording)
//         {
//             #if UNITY_ANDROID && !UNITY_EDITOR
//             try
//             {
//                 // Reset recorder
//                 mediaRecorder.Call("reset");

//                 // Configure recorder
//                 mediaRecorder.Call("setVideoSource", 2); // SURFACE
//                 mediaRecorder.Call("setOutputFormat", 2); // MPEG_4
//                 mediaRecorder.Call("setVideoEncoder", 2); // H264
//                 mediaRecorder.Call("setVideoSize", 1280, 720);
//                 mediaRecorder.Call("setVideoFrameRate", 30);
//                 mediaRecorder.Call("setVideoEncodingBitRate", 10000000);
//                 mediaRecorder.Call("setOutputFile", temporaryPath);

//                 // Prepare and start
//                 mediaRecorder.Call("prepare");
//                 mediaRecorder.Call("start");
                
//                 isRecording = true;
//                 if (buttonImage != null && stopSprite != null)
//                 {
//                     buttonImage.sprite = stopSprite;
//                 }
//                 Debug.Log("Recording started successfully");
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Failed to start recording: {e.Message}");
//                 if (mediaRecorder != null)
//                 {
//                     try
//                     {
//                         mediaRecorder.Call("reset");
//                     }
//                     catch { }
//                 }
//                 isRecording = false;
//             }
//             #endif
//         }
//     }

//     public void StopRecording()
//     {
//         if (isRecording)
//         {
//             #if UNITY_ANDROID && !UNITY_EDITOR
//             try
//             {
//                 Debug.Log("Attempting to stop recording...");
//                 mediaRecorder.Call("stop");
//                 mediaRecorder.Call("reset");
//                 Debug.Log("Recording stopped successfully");

//                 StartCoroutine(SaveVideoToGallery());
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Failed to stop recording: {e.Message}");
//                 // Try to reset the recorder
//                 try
//                 {
//                     mediaRecorder.Call("reset");
//                 }
//                 catch { }
//             }
//             finally
//             {
//                 isRecording = false;
//                 if (buttonImage != null && recordSprite != null)
//                 {
//                     buttonImage.sprite = recordSprite;
//                 }
//             }
//             #endif
//         }
//     }

//     private IEnumerator SaveVideoToGallery()
//     {
//         // Wait a short moment to ensure the file is properly written
//         yield return new WaitForSeconds(0.5f);

//         if (File.Exists(temporaryPath))
//         {
//             Debug.Log($"Video file exists at: {temporaryPath}");
//             string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
//             string fileName = $"Recording_{timeStamp}.mp4";

//             NativeGallery.SaveVideoToGallery(
//                 temporaryPath,
//                 "Recordings",
//                 fileName,
//                 (success, path) =>
//                 {
//                     if (success)
//                     {
//                         Debug.Log($"Video saved successfully to: {path}");
//                         // Clean up temporary file
//                         try
//                         {
//                             File.Delete(temporaryPath);
//                             Debug.Log("Temporary file deleted");
//                         }
//                         catch (System.Exception e)
//                         {
//                             Debug.LogError($"Failed to delete temporary file: {e.Message}");
//                         }
//                     }
//                     else
//                     {
//                         Debug.LogError("Failed to save video to gallery");
//                     }
//                 }
//             );
//         }
//         else
//         {
//             Debug.LogError($"Video file not found at: {temporaryPath}");
//         }
//     }

//     void Update()
//     {
//         if (isRecording && buttonImage != null)
//         {
//             float pulse = (Mathf.Sin(Time.time * 4) + 1) / 2;
//             buttonImage.color = new Color(1, 1, 1, 0.5f + pulse * 0.5f);
//         }
//         else if (buttonImage != null)
//         {
//             buttonImage.color = Color.white;
//         }
//     }

//     void OnDestroy()
//     {
//         #if UNITY_ANDROID && !UNITY_EDITOR
//         if (isRecording)
//         {
//             try
//             {
//                 mediaRecorder.Call("stop");
//             }
//             catch { }
//         }

//         if (mediaRecorder != null)
//         {
//             try
//             {
//                 mediaRecorder.Call("release");
//                 mediaRecorder.Dispose();
//             }
//             catch { }
//         }
//         #endif
//     }
// }