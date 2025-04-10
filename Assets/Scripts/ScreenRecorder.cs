using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

class BitmapEncoder
{
	public static void WriteBitmap(Stream stream, int width, int height, byte[] imageData)
	{
		using (BinaryWriter bw = new BinaryWriter(stream)) {

			// define the bitmap file header
			bw.Write ((UInt16)0x4D42); 								// bfType;
			bw.Write ((UInt32)(14 + 40 + (width * height * 4))); 	// bfSize;
			bw.Write ((UInt16)0);									// bfReserved1;
			bw.Write ((UInt16)0);									// bfReserved2;
			bw.Write ((UInt32)14 + 40);								// bfOffBits;
	 
			// define the bitmap information header
			bw.Write ((UInt32)40);  								// biSize;
			bw.Write ((Int32)width); 								// biWidth;
			bw.Write ((Int32)height); 								// biHeight;
			bw.Write ((UInt16)1);									// biPlanes;
			bw.Write ((UInt16)32);									// biBitCount;
			bw.Write ((UInt32)0);  									// biCompression;
			bw.Write ((UInt32)(width * height * 4));  				// biSizeImage;
			bw.Write ((Int32)0); 									// biXPelsPerMeter;
			bw.Write ((Int32)0); 									// biYPelsPerMeter;
			bw.Write ((UInt32)0);  									// biClrUsed;
			bw.Write ((UInt32)0);  									// biClrImportant;

			// switch the image data from RGB to BGR
			for (int imageIdx = 0; imageIdx < imageData.Length; imageIdx += 3) {
				bw.Write(imageData[imageIdx + 2]);
				bw.Write(imageData[imageIdx + 1]);
				bw.Write(imageData[imageIdx + 0]);
				bw.Write((byte)255);
			}
			
		}
	}

}

[RequireComponent(typeof(Camera))]
public class ScreenRecorder : MonoBehaviour 
{
    // Public Properties
    public int maxFrames; // maximum number of frames you want to record in one video
    public int frameRate = 30; // number of frames to capture per second
    public bool isRecording = false;

    // The Encoder Thread
    private Thread encoderThread;

    // Texture Readback Objects
    private RenderTexture tempRenderTexture;
    private Texture2D tempTexture2D;

    // Timing Data
    private float captureFrameTime;
    private float lastFrameTime;
    private int frameNumber;
    private int savingFrameNumber;

    // Encoder Thread Shared Resources
    private Queue<byte[]> frameQueue;
    private string persistentDataPath;
    private int screenWidth;
    private int screenHeight;
    private bool threadIsProcessing;
    private bool terminateThreadWhenDone;
    private string videoOutputPath;

    public void StartRecording()
    {
        if (isRecording) return;

        // Set target frame rate
        Application.targetFrameRate = frameRate;

        // Prepare the data directory
        persistentDataPath = Application.persistentDataPath + "/ScreenRecorder";
        print("Capturing to: " + persistentDataPath + "/");

        if (!Directory.Exists(persistentDataPath))
        {
            Directory.CreateDirectory(persistentDataPath);
        }
        else
        {
            // Clean up any existing frame files
            foreach (string file in Directory.GetFiles(persistentDataPath, "frame*.bmp"))
            {
                File.Delete(file);
            }
        }

        // Prepare textures and initial values
        screenWidth = GetComponent<Camera>().pixelWidth;
        screenHeight = GetComponent<Camera>().pixelHeight;
        
        tempRenderTexture = new RenderTexture(screenWidth, screenHeight, 0);
        tempTexture2D = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
        frameQueue = new Queue<byte[]>();

        frameNumber = 0;
        savingFrameNumber = 0;

        captureFrameTime = 1.0f / (float)frameRate;
        lastFrameTime = Time.time;

        // Kill the encoder thread if running from a previous execution
        if (encoderThread != null && (threadIsProcessing || encoderThread.IsAlive))
        {
            threadIsProcessing = false;
            encoderThread.Join();
        }

        // Start a new encoder thread
        threadIsProcessing = true;
        encoderThread = new Thread(EncodeAndSave);
        encoderThread.Start();

        isRecording = true;
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        terminateThreadWhenDone = true;

        // Wait for encoder thread to finish
        if (encoderThread != null && encoderThread.IsAlive)
        {
            encoderThread.Join();
        }

        // Reset target frame rate
        Application.targetFrameRate = -1;

        // Convert frames to video and save to gallery
        StartCoroutine(ConvertToVideoAndSaveToGallery());
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (isRecording && frameNumber <= maxFrames)
        {
            // Check if render target size has changed, if so, terminate
            if(source.width != screenWidth || source.height != screenHeight)
            {
                StopRecording();
                throw new UnityException("ScreenRecorder render target size has changed!");
            }

            // Calculate number of video frames to produce from this game frame
            float thisFrameTime = Time.time;
            int framesToCapture = ((int)(thisFrameTime / captureFrameTime)) - ((int)(lastFrameTime / captureFrameTime));

            // Capture the frame
            if(framesToCapture > 0)
            {
                Graphics.Blit(source, tempRenderTexture);
                
                RenderTexture.active = tempRenderTexture;
                tempTexture2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                RenderTexture.active = null;
            }

            // Add the required number of copies to the queue
            for(int i = 0; i < framesToCapture && frameNumber <= maxFrames; ++i)
            {
                frameQueue.Enqueue(tempTexture2D.GetRawTextureData());
                frameNumber++;

                if(frameNumber % frameRate == 0)
                {
                    print("Frame " + frameNumber);
                }
            }
            
            lastFrameTime = thisFrameTime;

            if (frameNumber >= maxFrames)
            {
                StopRecording();
            }
        }

        // Passthrough
        Graphics.Blit(source, destination);
    }
    
    private void EncodeAndSave()
    {
        print("SCREENRECORDER IO THREAD STARTED");

        while (threadIsProcessing) 
        {
            if(frameQueue.Count > 0)
            {
                // Generate file path
                string path = persistentDataPath + "/frame" + savingFrameNumber + ".bmp";

                // Dequeue the frame, encode it as a bitmap, and write it to the file
                using(FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    BitmapEncoder.WriteBitmap(fileStream, screenWidth, screenHeight, frameQueue.Dequeue());
                    fileStream.Close();
                }

                // Done
                savingFrameNumber++;
                print("Saved " + savingFrameNumber + " frames. " + frameQueue.Count + " frames remaining.");
            }
            else
            {
                if(terminateThreadWhenDone)
                {
                    break;
                }
                Thread.Sleep(1);
            }
        }

        terminateThreadWhenDone = false;
        threadIsProcessing = false;

        print("SCREENRECORDER IO THREAD FINISHED");
    }

    private IEnumerator ConvertToVideoAndSaveToGallery()
    {
        print("Starting video conversion...");

        // Set output path based on platform
        #if UNITY_ANDROID
        videoOutputPath = Path.Combine(Application.temporaryCachePath, "output.mp4");
        #elif UNITY_IOS
        videoOutputPath = Path.Combine(Application.temporaryCachePath, "output.mov");
        #else
        videoOutputPath = Path.Combine(Application.persistentDataPath, "output.mp4");
        #endif

        try
        {
            // Get all frame files in order
            string[] frameFiles = Directory.GetFiles(persistentDataPath, "frame*.bmp")
                                         .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Replace("frame", "")))
                                         .ToArray();

            if (frameFiles.Length == 0)
            {
                Debug.LogError("No frames found to convert!");
                yield break;
            }

            // Create VideoWriter
            using (VideoWriter videoWriter = new VideoWriter())
            {
                videoWriter.Open(videoOutputPath, frameRate, new Size(screenWidth, screenHeight));

                // Process each frame
                for (int i = 0; i < frameFiles.Length; i++)
                {
                    byte[] frameData = File.ReadAllBytes(frameFiles[i]);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(frameData);
                    
                    // Convert to video frame format
                    Color32[] pixels = tex.GetPixels32();
                    byte[] bgra = new byte[pixels.Length * 4];
                    for (int j = 0; j < pixels.Length; j++)
                    {
                        bgra[j * 4] = pixels[j].b;
                        bgra[j * 4 + 1] = pixels[j].g;
                        bgra[j * 4 + 2] = pixels[j].r;
                        bgra[j * 4 + 3] = pixels[j].a;
                    }

                    videoWriter.WriteFrame(bgra);
                    Destroy(tex);

                    // Report progress every 10%
                    if (i % (frameFiles.Length / 10) == 0)
                    {
                        print($"Video conversion progress: {(i * 100f / frameFiles.Length):F1}%");
                    }
                }
                // yield return null;
            }

            // Save to gallery based on platform
            #if UNITY_ANDROID
            SaveToAndroidGallery();
            #elif UNITY_IOS
            SaveToIOSGallery();
            #endif

            // Clean up frame files
            foreach (string framePath in frameFiles)
            {
                File.Delete(framePath);
            }

            print("Video conversion completed and saved to gallery!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error during video conversion: " + e.Message);
        }
    }

    #if UNITY_ANDROID
    private void SaveToAndroidGallery()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            return;
        }

        using (AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
        using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
        using (AndroidJavaObject contentResolver = GetContentResolver())
        {
            string galleryPath = environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", 
                environment.GetStatic<string>("DIRECTORY_DCIM")).Call<string>("toString");
            string dstPath = Path.Combine(galleryPath, "Camera", $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            File.Copy(videoOutputPath, dstPath, true);
            contentResolver.Call("scanFile", dstPath, "video/mp4");
        }
    }

    private AndroidJavaObject GetContentResolver()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            return activity.Call<AndroidJavaObject>("getContentResolver");
        }
    }
    #endif

    #if UNITY_IOS
    private void SaveToIOSGallery()
    {
        NativeGallery.SaveVideoToGallery(videoOutputPath, "ScreenRecording", 
            $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mov",
            (success, path) =>
            {
                if (success)
                    Debug.Log("Successfully saved video to gallery at: " + path);
                else
                    Debug.LogError("Failed to save video to gallery");
            });
    }
    #endif

    void OnDisable()
    {
        if (isRecording)
        {
            StopRecording();
        }
    }

    private class VideoWriter : IDisposable
    {
        private FileStream fileStream;
        private BinaryWriter writer;
        private int frameCount = 0;

        public void Open(string path, int fps, Size frameSize)
        {
            fileStream = new FileStream(path, FileMode.Create);
            writer = new BinaryWriter(fileStream);
            WriteVideoHeader(fps, frameSize);
        }

        public void WriteFrame(byte[] frameData)
        {
            writer.Write(frameData.Length);
            writer.Write(frameData);
            frameCount++;
        }

        private void WriteVideoHeader(int fps, Size frameSize)
        {
            writer.Write("UNITY");
            writer.Write(fps);
            writer.Write(frameSize.Width);
            writer.Write(frameSize.Height);
        }

        public void Dispose()
        {
            writer?.Dispose();
            fileStream?.Dispose();
        }
    }

    private struct Size
    {
        public int Width;
        public int Height;

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}

// BitmapEncoder class remains the same