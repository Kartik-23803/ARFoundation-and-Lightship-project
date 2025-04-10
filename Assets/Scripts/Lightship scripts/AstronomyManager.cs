using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System;
using TMPro;

public class AstronomyManager : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiKey = "4954ef5f51d64ea09a7520c66978180c";

    [Header("Moon Time Information")]
    [SerializeField] private TextMeshProUGUI moonriseText;
    [SerializeField] private TextMeshProUGUI moonsetText;
    
    [Header("Moon Position Information")]
    [SerializeField] private TextMeshProUGUI moonStatusText;
    [SerializeField] private TextMeshProUGUI moonAltitudeText;
    [SerializeField] private TextMeshProUGUI moonDistanceText;
    [SerializeField] private TextMeshProUGUI moonAzimuthText;

    [Header("Moon Phase Information")]
    [SerializeField] private TextMeshProUGUI moonPhaseText;
    [SerializeField] private TextMeshProUGUI moonIlluminationText;
    [SerializeField] private TextMeshProUGUI moonParallacticAngleText;
    [SerializeField] private TextMeshProUGUI moonAngleText;

    [Header("Location Information")]
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI coordinatesText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button refreshButton;

    [Header("Moon Tracking UI")]
    [SerializeField] private TextMeshProUGUI rotationInstructionsText;
    [SerializeField] private TextMeshProUGUI alignmentStatusText;
    [SerializeField] Image turnRight;
    [SerializeField] Image turnLeft;
    [SerializeField] Image turnUp;
    [SerializeField] Image turnDown;

    [Header("Moon Tracking")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float rotationThreshold = 5f; // Degrees of acceptable error
    [SerializeField] private bool enableMoonTracking = true;

    [SerializeField] GameObject videoPlayer;    
    private Vector3 northDirection = Vector3.forward; // Z axis is North
    private Vector3 targetMoonDirection;
    private bool isFacingMoon = false;

    private const string API_URL = "https://api.ipgeolocation.io/astronomy";
    private AstronomyData currentData;
    private LocationService locationService;

    void Start()
    {
        ResetRotationImages();
        videoPlayer.SetActive(false);
        InitializeGyroscope();
        InitializeLocationService();
        if (refreshButton != null)
            refreshButton.onClick.AddListener(FetchAstronomyData);

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (enableMoonTracking && currentData != null)
        {
            UpdateDeviceOrientation();
            CheckMoonAlignment();
            
            // Debug verification
            Debug.Log($"Current Camera Rotation: {mainCamera.transform.eulerAngles}");
            Debug.Log($"Current UI Text: {rotationInstructionsText?.text}");
        }
    }

    private void UpdateDeviceOrientation()
    {
        if (!Input.gyro.enabled) return;

        // Convert gyro rotation to Unity space
        Quaternion gyroRotation = Input.gyro.attitude;
        Quaternion rotFix = new Quaternion(1, 0, 0, 0);
        Quaternion deviceRotation = Quaternion.Euler(0f, 0f, 0f) * (rotFix * gyroRotation);
        
        // Apply rotation to camera
        mainCamera.transform.rotation = deviceRotation;
    }

    // private void CheckMoonAlignment()
    // {
    //     // Calculate required rotation
    //     Vector3 requiredRotation = CalculateRequiredRotation();
        
    //     // Get current camera rotation
    //     Quaternion deviceRotation = Input.gyro.enabled ? 
    //         Input.gyro.attitude : Quaternion.identity;
        
    //     // Convert gyroscope data to camera space
    //     Quaternion cameraRotation = new Quaternion(
    //         deviceRotation.x,
    //         deviceRotation.y,
    //         -deviceRotation.z,
    //         -deviceRotation.w
    //     );
        
    //     // Apply the rotation to the camera
    //     mainCamera.transform.rotation = cameraRotation;

    //     // Get current camera angles
    //     Vector3 currentRotation = mainCamera.transform.eulerAngles;

    //     // Calculate the difference
    //     float yawDifference = Mathf.DeltaAngle(currentRotation.y, requiredRotation.y);
    //     float pitchDifference = Mathf.DeltaAngle(currentRotation.x, requiredRotation.x);

    //     // Check if camera is aligned with moon
    //     if (Mathf.Abs(yawDifference) <= rotationThreshold && 
    //         Mathf.Abs(pitchDifference) <= rotationThreshold)
    //     {
    //         if (!isFacingMoon)
    //         {
    //             Debug.Log("Camera is now facing the moon!");
    //             isFacingMoon = true;
    //             videoPlayer.SetActive(true);
    //         }
    //     }
    //     else
    //     {
    //         isFacingMoon = false;
    //         videoPlayer.SetActive(false);
    //         Debug.Log($"Rotate Camera - Yaw (Y): {yawDifference:F1}° | Pitch (X): {pitchDifference:F1}°");
    //     }

    //     // Update UI with rotation information
    //     Debug.Log("Updating rotating");
    //     UpdateRotationUI(yawDifference, pitchDifference);
    // }

    // private void CheckMoonAlignment()
    // {
    //     Vector3 requiredRotation = CalculateRequiredRotation();
    //     Vector3 currentRotation = mainCamera.transform.eulerAngles;

    //     // Normalize angles to 0-360 range
    //     currentRotation.x = (currentRotation.x + 360) % 360;
    //     currentRotation.y = (currentRotation.y + 360) % 360;
        
    //     float yawDifference = Mathf.DeltaAngle(currentRotation.y, requiredRotation.y);
    //     float pitchDifference = Mathf.DeltaAngle(currentRotation.x, requiredRotation.x);

    //     Debug.Log($"Current Rotation: {currentRotation}, Required: {requiredRotation}");
    //     Debug.Log($"Differences - Yaw: {yawDifference}, Pitch: {pitchDifference}");

    //     if (Mathf.Abs(yawDifference) <= rotationThreshold && 
    //         Mathf.Abs(pitchDifference) <= rotationThreshold)
    //     {
    //         if (!isFacingMoon)
    //         {
    //             Debug.Log("Camera is now facing the moon!");
    //             isFacingMoon = true;
    //             videoPlayer.SetActive(true);
    //         }
    //     }
    //     else
    //     {
    //         isFacingMoon = false;
    //         videoPlayer.SetActive(false);
    //     }

    //     UpdateRotationUI(yawDifference, pitchDifference);
    // }

    private void CheckMoonAlignment()
    {
        Vector3 requiredRotation = CalculateRequiredRotation();
        
        Vector3 currentRotation = mainCamera.transform.eulerAngles;

        float yawDifference = Mathf.DeltaAngle(currentRotation.y, requiredRotation.y);
        float pitchDifference = Mathf.DeltaAngle(currentRotation.x, requiredRotation.x);

        if (Mathf.Abs(yawDifference) <= rotationThreshold && 
            Mathf.Abs(pitchDifference) <= rotationThreshold)
        {
            if (!isFacingMoon)
            {
                Debug.Log("Moon Found!");
                isFacingMoon = true;
                videoPlayer.SetActive(true);
            }
        }
        else
        {
            isFacingMoon = false;
            videoPlayer.SetActive(false);
            Debug.Log($"Rotate Camera - Yaw (Y): {yawDifference:F1}° | Pitch (X): {pitchDifference:F1}°");
        }

        UpdateRotationUI(yawDifference, pitchDifference);
    }

    // private void UpdateRotationUI(float yawDifference, float pitchDifference)
    // {
    //     Debug.Log("Updating UI");
    //     if (rotationInstructionsText != null)
    //     {
    //         string instructions = "Rotation needed:\n";
            
    //         // Yaw (horizontal) instructions
    //         if (Mathf.Abs(yawDifference) > rotationThreshold)
    //         {
    //             instructions += yawDifference > 0 ? 
    //                 "Turn → " + Mathf.Abs(yawDifference).ToString("F1") + "°\n" :
    //                 "Turn ← " + Mathf.Abs(yawDifference).ToString("F1") + "°\n";
    //         }

    //         // Pitch (vertical) instructions
    //         if (Mathf.Abs(pitchDifference) > rotationThreshold)
    //         {
    //             instructions += pitchDifference > 0 ? 
    //                 "Turn ↓ " + Mathf.Abs(pitchDifference).ToString("F1") + "°" :
    //                 "Turn ↑ " + Mathf.Abs(pitchDifference).ToString("F1") + "°";
    //         }

    //         rotationInstructionsText.text = instructions;
    //     }

    //     if (alignmentStatusText != null)
    //     {
    //         alignmentStatusText.text = isFacingMoon ? 
    //             "✓ Camera aligned with moon!" : 
    //             "x Camera not aligned with moon";
    //     }
    // }

    private void UpdateRotationUI(float yawDifference, float pitchDifference)
    {
        ResetRotationImages();

        string instructions = "Rotation needed:\n";
        
        if (Mathf.Abs(yawDifference) > rotationThreshold)
        {
            if (yawDifference > 0)
            {
                instructions += "Turn " + Mathf.Abs(yawDifference).ToString("F1") + "°\n\n";
                if (turnRight != null)
                {
                    turnRight.gameObject.SetActive(true);
                }
            }
            else
            {
                instructions += "Turn " + Mathf.Abs(yawDifference).ToString("F1") + "°\n\n";
                if (turnLeft != null)
                {
                    turnLeft.gameObject.SetActive(true);
                }
            }
        }

        if (Mathf.Abs(pitchDifference) > rotationThreshold)
        {
            if (pitchDifference > 0)
            {
                instructions += "Turn " + "°\n" + Mathf.Abs(pitchDifference).ToString("F1");
                if (turnDown != null)
                {
                    turnDown.gameObject.SetActive(true);
                }
            }
            else
            {
                instructions += "Turn " + "°\n" + Mathf.Abs(pitchDifference).ToString("F1");
                if (turnUp != null)
                {
                    turnUp.gameObject.SetActive(true);
                }
            }
        }

        if (rotationInstructionsText != null)
        {
            rotationInstructionsText.text = instructions;
        }

        if (alignmentStatusText != null)
        {
            alignmentStatusText.text = isFacingMoon ? 
                "Moon Found!" : 
                "x Camera not aligned with moon";
        }
    }

    private void ResetRotationImages()
    {
        if (turnRight != null) turnRight.gameObject.SetActive(false);
        if (turnLeft != null) turnLeft.gameObject.SetActive(false);
        if (turnUp != null) turnUp.gameObject.SetActive(false);
        if (turnDown != null) turnDown.gameObject.SetActive(false);
    }

    // private Vector3 CalculateRequiredRotation()
    // {
    //     if (currentData == null) return Vector3.zero;

    //     // Convert moon's azimuth and altitude to rotation angles
    //     float azimuth = currentData.moon_azimuth;
    //     float altitude = currentData.moon_altitude;

    //     // Calculate required yaw (Y rotation)
    //     // Azimuth: 0° is North, 90° is East, 180° is South, 270° is West
    //     // Convert to Unity's coordinate system and account for rear camera
    //     float yaw = (azimuth + 180) % 360;

    //     // Calculate required pitch (X rotation)
    //     // Altitude: 0° is horizon, 90° is zenith
    //     // Invert pitch for rear camera orientation
    //     float pitch = -altitude;

    //     return new Vector3(pitch, yaw, 0);
    // }

    private Vector3 CalculateRequiredRotation()
    {
        if (currentData == null) return Vector3.zero;

        float azimuth = currentData.moon_azimuth;
        float altitude = currentData.moon_altitude;

        float yaw = azimuth;

        float pitch = altitude;

        return new Vector3(pitch, yaw, 0);
    }

    private void InitializeLocationService()
    {
        if (LocationService.Instance == null)
        {
            GameObject locationServiceObj = new GameObject("LocationService");
            locationService = locationServiceObj.AddComponent<LocationService>();
        }
        else
        {
            locationService = LocationService.Instance;
        }

        locationService.OnLocationUpdated += OnLocationUpdated;
        locationService.OnLocationError += OnLocationError;

        locationService.StartLocationService();
        UpdateStatusText("Initializing location services...");
    }

    private void OnLocationUpdated()
    {
        UpdateCoordinatesText();
        FetchAstronomyData();
    }

    private void OnLocationError(string error)
    {
        UpdateStatusText($"Location Error: {error}");
    }

    private void UpdateCoordinatesText()
    {
        if (coordinatesText != null)
        {
            coordinatesText.text = $"Lat: {locationService.Latitude:F4}, Long: {locationService.Longitude:F4}";
        }
    }

    public void FetchAstronomyData()
    {
        if (!locationService.IsLocationServiceEnabled)
        {
            UpdateStatusText("Location services are not enabled!");
            return;
        }

        StartCoroutine(GetAstronomyData());
        UpdateStatusText("Fetching astronomy data...");
    }

    private IEnumerator GetAstronomyData()
    {
        string url = $"{API_URL}?apiKey={apiKey}&lat={locationService.Latitude}&long={locationService.Longitude}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                UpdateStatusText($"Error: {webRequest.error}");
            }
            else
            {
                try
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    currentData = JsonUtility.FromJson<AstronomyData>(jsonResponse);
                    UpdateUI();
                    UpdateStatusText("Data updated successfully!");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse response: {e.Message}");
                    UpdateStatusText("Failed to parse data!");
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (currentData == null) return;

        // Update Location
        if (locationText != null) 
            locationText.text = $"Location: {currentData.location}";

        // Update Moon Times with countdown
        if (moonriseText != null)
        {
            string moonrise12Hour = TimeFormatter.Convert24To12Hour(currentData.moonrise);
            string moonriseRemaining = TimeFormatter.GetTimeRemaining(currentData.moonrise);
            moonriseText.text = $"Moonrise: {moonrise12Hour}\n{moonriseRemaining}";
        }

        if (moonsetText != null)
        {
            string moonset12Hour = TimeFormatter.Convert24To12Hour(currentData.moonset);
            string moonsetRemaining = TimeFormatter.GetTimeRemaining(currentData.moonset);
            moonsetText.text = $"Moonset: {moonset12Hour}\n{moonsetRemaining}";
        }

        // Update Moon Position
        if (moonStatusText != null)
            moonStatusText.text = $"Moon Status: {currentData.moon_status}";

        if (moonAltitudeText != null)
            moonAltitudeText.text = $"Altitude: {currentData.moon_altitude:F2}°";

        if (moonDistanceText != null)
            moonDistanceText.text = $"Distance: {FormatDistance(currentData.moon_distance)} km";

        if (moonAzimuthText != null)
            moonAzimuthText.text = $"Azimuth: {currentData.moon_azimuth:F2}°";

        // Update Moon Phase Information
        if (moonPhaseText != null)
            moonPhaseText.text = $"Phase: {FormatMoonPhase(currentData.moon_phase)}";

        if (moonIlluminationText != null)
            moonIlluminationText.text = $"Illumination: {currentData.moon_illumination_percentage}%";

        if (moonParallacticAngleText != null)
            moonParallacticAngleText.text = $"Parallactic Angle: {currentData.moon_parallactic_angle:F2}°";

        if (moonAngleText != null)
            moonAngleText.text = $"Moon Angle: {currentData.moon_angle:F2}°";

        Vector3 requiredRotation = CalculateRequiredRotation();
        Debug.Log($"Moon Position - Azimuth: {currentData.moon_azimuth:F1}° | Altitude: {currentData.moon_altitude:F1}°");
        Debug.Log($"Required Camera Rotation - Yaw: {requiredRotation.y:F1}° | Pitch: {requiredRotation.x:F1}°");
    }

    private string FormatDistance(double distanceKm)
    {
        if (distanceKm >= 1000000)
        {
            return $"{(distanceKm / 1000000):F2}M";
        }
        else if (distanceKm >= 1000)
        {
            return $"{(distanceKm / 1000):F2}K";
        }
        return $"{distanceKm:F2}";
    }

    private string FormatMoonPhase(string phase)
    {
        return phase?.Replace("_", " ").ToLower().Replace(
            phase.ToLower()[0].ToString(),
            phase[0].ToString()) ?? "Unknown";
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.Log(message);
    }

    void InitializeGyroscope()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("Gyroscope initialized");
        }
        else
        {
            Debug.Log("Gyroscope not supported on this device");
        }
    }

    private void OnDestroy()
    {
        if (locationService != null)
        {
            locationService.OnLocationUpdated -= OnLocationUpdated;
            locationService.OnLocationError -= OnLocationError;
        }
    }
}

public static class TimeFormatter
{
    public static string Convert24To12Hour(string time24)
    {
        try
        {
            DateTime dateTime = DateTime.ParseExact(time24, "HH:mm", null);
            return dateTime.ToString("hh:mm tt");
        }
        catch
        {
            return time24;
        }
    }

    public static string GetTimeRemaining(string targetTime)
    {
        try
        {
            DateTime target = DateTime.ParseExact(targetTime, "HH:mm", null);
            DateTime now = DateTime.Now;
            
            target = target.Date + target.TimeOfDay;
            
            if (target < now)
            {
                target = target.AddDays(1);
            }

            TimeSpan difference = target - now;
            return $"In {difference.Hours}h {difference.Minutes}m";
        }
        catch
        {
            return "";
        }
    }
}