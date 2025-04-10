using UnityEngine;
using System;
using System.Collections;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class LocationPermissionHandler : MonoBehaviour
{
    public static LocationPermissionHandler Instance { get; private set; }
    
    public event Action OnPermissionGranted;
    public event Action OnPermissionDenied;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RequestLocationPermission()
    {
        StartCoroutine(RequestLocationPermissionCoroutine());
    }

    private IEnumerator RequestLocationPermissionCoroutine()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(0.1f);
        }

        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            OnPermissionGranted?.Invoke();
        }
        else
        {
            OnPermissionDenied?.Invoke();
        }
#elif PLATFORM_IOS
        if (Input.location.isEnabledByUser)
        {
            OnPermissionGranted?.Invoke();
        }
        else
        {
            // On iOS, this will trigger the permission dialog
            Input.location.Start();
            yield return new WaitForSeconds(0.1f);

            if (Input.location.isEnabledByUser)
            {
                OnPermissionGranted?.Invoke();
            }
            else
            {
                OnPermissionDenied?.Invoke();
                Input.location.Stop();
            }
        }
#else
        OnPermissionGranted?.Invoke();
#endif
        yield return null;
    }
}