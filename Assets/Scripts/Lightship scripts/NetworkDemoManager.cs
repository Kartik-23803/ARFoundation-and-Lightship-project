using Niantic.Lightship.SharedAR.Colocalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Niantic.Lightship.SharedAR.Netcode;

public class NetworkDemoManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _statusText;

    [SerializeField]
    private Button _joinAsHostButton;

    [SerializeField]
    private Button _joinAsClientButton;

    [SerializeField]
    private SharedSpaceManager _sharedSpaceManager;

    // Reference to Lightship Transport
    [SerializeField]
    private LightshipNetcodeTransport _lightshipTransport;

    private void Start()
    {
        InitializeComponents();
        SetupNetworkCallbacks();
        ConfigureSharedSpace();
    }

    private void InitializeComponents()
    {
        if (_statusText == null)
            Debug.LogError("Status Text is not assigned!");
        
        if (_sharedSpaceManager == null)
            Debug.LogError("SharedSpaceManager is not assigned!");

        if (_lightshipTransport == null)
            _lightshipTransport = FindObjectOfType<LightshipNetcodeTransport>();

        // Initially hide buttons
        if (_joinAsHostButton != null)
            _joinAsHostButton.gameObject.SetActive(false);
        if (_joinAsClientButton != null)
            _joinAsClientButton.gameObject.SetActive(false);
    }

    private void SetupNetworkCallbacks()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void ConfigureSharedSpace()
    {
        try
        {
            // Configure SharedSpaceManager
            _sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;

            // Create room options
            var roomOptions = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
                "TestRoom",  // Room name
                32,         // Max players
                "Test Room" // Description
            );

            // Create tracking options (use VPS for real-world tracking)
            // var trackingOptions = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions();
            // Or use mock tracking for testing
            var trackingOptions = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();

            // Start shared space
            _sharedSpaceManager.StartSharedSpace(trackingOptions, roomOptions);
            UpdateStatus("Configuring shared space...");
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Error configuring shared space: {e.Message}");
        }
    }

    private void OnColocalizationTrackingStateChanged(
        SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
    {
        if (args.Tracking)
        {
            ShowButtons();
            UpdateStatus("Ready to connect");
        }
        else
        {
            HideButtons();
            UpdateStatus("Tracking lost");
        }
    }

    private void OnJoinAsHostClicked()
    {
        try
        {
            if (!NetworkManager.Singleton.StartHost())
            {
                UpdateStatus("Failed to start host!");
                return;
            }
            HideButtons();
            UpdateStatus("Starting as host...");
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Host error: {e.Message}");
        }
    }

    private void OnJoinAsClientClicked()
    {
        try
        {
            if (!NetworkManager.Singleton.StartClient())
            {
                UpdateStatus("Failed to start client!");
                return;
            }
            HideButtons();
            UpdateStatus("Connecting as client...");
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Client error: {e.Message}");
        }
    }

    private void ShowButtons()
    {
        if (_joinAsHostButton != null)
            _joinAsHostButton.gameObject.SetActive(true);
        if (_joinAsClientButton != null)
            _joinAsClientButton.gameObject.SetActive(true);
    }

    private void HideButtons()
    {
        if (_joinAsHostButton != null)
            _joinAsHostButton.gameObject.SetActive(false);
        if (_joinAsClientButton != null)
            _joinAsClientButton.gameObject.SetActive(false);
    }

    private void OnServerStarted()
    {
        UpdateStatus("Server started successfully!");
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        UpdateStatus($"Client connected: {clientId}");
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        UpdateStatus($"Client disconnected: {clientId}");
    }

    private void UpdateStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
            Debug.Log($"Status: {message}");
        }
    }

    private void OnDestroy()
    {
        if (_sharedSpaceManager != null)
            _sharedSpaceManager.sharedSpaceManagerStateChanged -= OnColocalizationTrackingStateChanged;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }
}
// using Niantic.Lightship.SharedAR.Colocalization;
// using TMPro;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.UI;

// public class NetworkDemoManager : MonoBehaviour
// {
//     [SerializeField]
//     private TextMeshProUGUI _statusText;

//     [SerializeField]
//     private Button _joinAsHostButton;

//     [SerializeField]
//     private Button _joinAsClientButton;

//     [SerializeField]
//     private SharedSpaceManager _sharedSpaceManager;

//     protected void Start()
//     {
//         _statusText = _statusText.GetComponent<TextMeshProUGUI>();
//         // UI event listeners
//         _joinAsHostButton.onClick.AddListener(OnJoinAsHostClicked);
//         _joinAsClientButton.onClick.AddListener(OnJoinAsClientClicked);

//         // Netcode connection event callback
//         NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;

//         // Set SharedSpaceManager and start it
//         _sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;
//         // Set room to join
//         var mockTrackingArgs = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();
//         var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
//               "ExampleRoom", // use fixed room name
//               32, // set capacity to max
//               "shared ar demo (mock mode)" // description
//         );
//         _sharedSpaceManager.StartSharedSpace(mockTrackingArgs, roomArgs);
//     }

//     private void OnColocalizationTrackingStateChanged(
//        SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
//     {
//         // Show Join UI
//         if (args.Tracking)
//         {
//             _joinAsHostButton.gameObject.SetActive(true);
//             _joinAsClientButton.gameObject.SetActive(true);
//         }
//     }
//     private void OnJoinAsHostClicked()
//     {
//         NetworkManager.Singleton.StartHost();
//         HideButtons();
//     }

//     private void OnJoinAsClientClicked()
//     {
//         NetworkManager.Singleton.StartClient();
//         HideButtons();
//     }

//     private void HideButtons()
//     {
//         _joinAsHostButton.gameObject.SetActive(false);
//         _joinAsClientButton.gameObject.SetActive(false);
//     }

//     private void OnClientConnectedCallback(ulong clientId)
//     {
//         _statusText.text = $"Connected: {clientId}";
//     }
// }