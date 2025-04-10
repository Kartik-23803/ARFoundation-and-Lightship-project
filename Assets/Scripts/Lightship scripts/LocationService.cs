using UnityEngine;
using System;
using System.Collections;

public class LocationService : MonoBehaviour
{
    public static LocationService Instance { get; private set; }
    
    public bool IsLocationServiceEnabled => Input.location.isEnabledByUser;
    public float Latitude { get; private set; }
    public float Longitude { get; private set; }
    
    public event Action<string> OnLocationError;
    public event Action OnLocationUpdated;

    private LocationPermissionHandler permissionHandler;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePermissionHandler();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePermissionHandler()
    {
        GameObject handlerObj = new GameObject("LocationPermissionHandler");
        handlerObj.transform.SetParent(transform);
        permissionHandler = handlerObj.AddComponent<LocationPermissionHandler>();
        
        permissionHandler.OnPermissionGranted += HandlePermissionGranted;
        permissionHandler.OnPermissionDenied += HandlePermissionDenied;
    }

    public void StartLocationService()
    {
        if (isInitialized)
        {
            RequestLocation();
        }
        else
        {
            permissionHandler.RequestLocationPermission();
        }
    }

    private void HandlePermissionGranted()
    {
        isInitialized = true;
        RequestLocation();
    }

    private void HandlePermissionDenied()
    {
        OnLocationError?.Invoke("Location permission denied");
    }

    private void RequestLocation()
    {
        if (!Input.location.isEnabledByUser)
        {
            OpenLocationSettings();
            OnLocationError?.Invoke("Please enable location services in device settings");
            return;
        }

        StartCoroutine(StartLocationUpdates());
    }

    private void OpenLocationSettings()
    {
#if PLATFORM_ANDROID
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (var intentClass = new AndroidJavaClass("android.content.Intent"))
            using (var settingsIntent = new AndroidJavaObject("android.content.Intent", 
                intentClass.GetStatic<string>("ACTION_LOCATION_SOURCE_SETTINGS")))
            {
                currentActivity.Call("startActivity", settingsIntent);
            }
        }
#elif PLATFORM_IOS
        Application.OpenURL("app-settings:");
#endif
    }

    private IEnumerator StartLocationUpdates()
    {
        Input.location.Start(1f, 1f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            OnLocationError?.Invoke("Location service initialization timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            OnLocationError?.Invoke("Location service failed to initialize");
            yield break;
        }

        Latitude = Input.location.lastData.latitude;
        Longitude = Input.location.lastData.longitude;
        OnLocationUpdated?.Invoke();

        // Start continuous updates
        StartCoroutine(ContinuousLocationUpdate());
    }

    private IEnumerator ContinuousLocationUpdate()
    {
        while (Input.location.isEnabledByUser)
        {
            Latitude = Input.location.lastData.latitude;
            Longitude = Input.location.lastData.longitude;
            OnLocationUpdated?.Invoke();
            yield return new WaitForSeconds(5f); // Update every 5 seconds
        }
    }

    private void OnDisable()
    {
        if (Input.location.isEnabledByUser)
        {
            Input.location.Stop();
        }
    }

    private void OnDestroy()
    {
        if (permissionHandler != null)
        {
            permissionHandler.OnPermissionGranted -= HandlePermissionGranted;
            permissionHandler.OnPermissionDenied -= HandlePermissionDenied;
        }
    }
}