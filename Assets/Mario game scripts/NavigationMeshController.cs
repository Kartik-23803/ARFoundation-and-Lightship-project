using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine.InputSystem;

public class NavigationMeshController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LightshipNavMeshManager _navmeshManager;
    [SerializeField] private LightshipNavMeshAgent _agentPrefab;
    [SerializeField] private Joystick _joystick;
    [SerializeField] float _movementThreshold = 0.1f;

    // New parameters for movement optimization
    [SerializeField] float _destinationUpdateThreshold = 0.3f; // Minimum distance to update destination
    [SerializeField] float _destinationDistance = 2.0f; // How far ahead to set destination
    [SerializeField] float _positionSmoothTime = 0.1f; // Smoothing time for visual position
    [SerializeField] float _rotationSmoothSpeed = 10f; // Smoothing speed for rotation
    public GameObject endCanvas;

    public LightshipNavMeshAgent _agentInstance;
    private Animator _agentAnimator;
    private Vector3 _lastMoveDirection;
    private Vector3 _lastSetDestination;
    private GameObject _agentVisual; // Reference to the visual representation
    
    // Smoothing variables
    private Vector3 _smoothedPosition;
    private Vector3 _positionVelocity;
    private Quaternion _smoothedRotation;
    
    PlayerManager player;
    SpawnManager spawnManager;
    EnemySpawnManager enemySpawnManager;
    PickupEffect pickup;
    PlayerHealth playerHealth;

    void Start()
    {
        pickup = FindObjectOfType<PickupEffect>();
        spawnManager = FindObjectOfType<SpawnManager>();
        enemySpawnManager = FindObjectOfType<EnemySpawnManager>();
        endCanvas.SetActive(false);
        // AudioManager.Instance.PlayGameMusic();
    }

    void Update()
    {
        HandleTouch();
        UpdateAgentAnimation();
        SmoothAgentVisual();
    }

    void FixedUpdate()
    {
        HandleJoystickMovement();
    }

    void UpdateAgentAnimation()
    {
        if (_agentInstance != null && _agentAnimator != null)
        {
            bool isMoving = Mathf.Abs(_joystick.Horizontal) > _movementThreshold || 
                            Mathf.Abs(_joystick.Vertical) > _movementThreshold;
            _agentAnimator.SetBool("walking", isMoving);
        }
    }

    public void HandleJumpAnimation()
    {
        if (_agentInstance != null && _agentAnimator != null)
        {
            _agentAnimator.SetTrigger("jump");
        }
    }

    void HandleJoystickMovement()
    {
        if (_agentInstance == null || _joystick == null)
            return;

        float horizontalInput = _joystick.Horizontal;
        float verticalInput = _joystick.Vertical;

        if (Mathf.Abs(horizontalInput) > _movementThreshold || Mathf.Abs(verticalInput) > _movementThreshold)
        {
            Vector3 cameraForward = _camera.transform.forward;
            Vector3 cameraRight = _camera.transform.right;
            
            cameraForward.y = 0;
            cameraRight.y = 0;
            
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

            Vector3 currentPosition = _agentInstance.transform.position;
            
            // Calculate destination further ahead (not using Time.deltaTime)
            Vector3 destination = currentPosition + moveDirection * _destinationDistance;

            // Only update destination if it's significantly different
            if (Vector3.Distance(_lastSetDestination, destination) > _destinationUpdateThreshold)
            {
                _agentInstance.SetDestination(destination);
                _lastSetDestination = destination;
                
                // Uncomment for debugging only when needed
                // DebugLog($"Updated destination to: {destination}");
            }

            // Let the nav mesh agent handle rotation, we'll smooth it visually
            _lastMoveDirection = moveDirection;
        }
    }

    void SmoothAgentVisual()
    {
        if (_agentInstance != null && _agentVisual != null)
        {
            // Smooth the position
            _smoothedPosition = Vector3.SmoothDamp(
                _smoothedPosition, 
                _agentInstance.transform.position, 
                ref _positionVelocity, 
                _positionSmoothTime);
                
            // Apply to visual only
            _agentVisual.transform.position = _smoothedPosition;
            
            // Smooth rotation if needed
            if (_lastMoveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_lastMoveDirection);
                _agentVisual.transform.rotation = Quaternion.Slerp(
                    _agentVisual.transform.rotation,
                    targetRotation,
                    Time.deltaTime * _rotationSmoothSpeed);
            }
        }
    }

    void HandleTouch()
    {
        // Null checks for critical components
        if (_camera == null)
        {
            // Debug.LogError("Camera is not assigned!");
            return;
        }

        if (_agentPrefab == null)
        {
            // Debug.LogError("Agent Prefab is not assigned!");
            return;
        }

        // Check if touch is available and pressed
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
            if (touchPosition.x > 0 && touchPosition.x < Screen.width &&
                touchPosition.y > 0 && touchPosition.y < Screen.height)
            {
                Ray ray = _camera.ScreenPointToRay(touchPosition);
                // DebugLog($"Shooting ray from touch position: {touchPosition}");

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // DebugLog($"Raycast hit point: {hit.point}");

                    if (_agentInstance == null)
                    {
                        // Create new agent
                        _agentInstance = Instantiate(_agentPrefab);
                        _agentInstance.transform.position = hit.point;
                        
                        // Get player manager component
                        player = _agentInstance.GetComponent<PlayerManager>();
                        
                        // Setup visual reference for smoothing
                        SetupAgentVisual();
                        
                        // Update pickup references
                        UpdatePickupReferences();
                        
                        if (spawnManager != null)
                        {
                            // Initialize spawning with the newly created agent
                            spawnManager.InitializeSpawningWithAgent(_agentInstance.gameObject);
                        }
                        if(enemySpawnManager != null)
                        {
                            enemySpawnManager.InitializeEnemySpawning(_agentInstance.gameObject);
                        }
                        
                        // Get animator
                        _agentAnimator = _agentInstance.GetComponentInChildren<Animator>();
                        
                        // Initialize smoothing variables
                        _smoothedPosition = _agentInstance.transform.position;
                        _lastSetDestination = _smoothedPosition;
                        
                        // DebugLog($"Agent created at: {hit.point}");
                    }
                }
                else
                {
                    DebugLog("Raycast did not hit anything");
                }
            }
            else
            {
                DebugLog($"Touch position {touchPosition} is outside screen bounds");
            }
        }
    }

    private void SetupAgentVisual()
    {
        if (_agentInstance != null)
        {
            // Find the visual child - adjust index based on your hierarchy
            // This assumes the first child is the visual representation
            if (_agentInstance.transform.childCount > 0)
            {
                _agentVisual = _agentInstance.transform.GetChild(0).gameObject;
            }
            else
            {
                // If no children, use the agent itself
                _agentVisual = _agentInstance.gameObject;
            }
        }
    }

    private void UpdatePickupReferences()
    {
        if (player != null)
        {
            // Find all pickup objects and set their playerManager reference
            PickupEffect[] pickups = FindObjectsOfType<PickupEffect>();
            foreach (var pickup in pickups)
            {
                pickup.playerManager = player;
            }
        }
    }

    public void SetVisualization(bool isVisualizationOn)
    {
        // Existing visualization logic
        if (_navmeshManager == null)
        {
            // Debug.LogWarning("NavMesh Manager is not assigned!");
            return;
        }

        var navMeshRenderer = _navmeshManager.GetComponent<LightshipNavMeshRenderer>();
        if (navMeshRenderer != null)
        {
            navMeshRenderer.enabled = isVisualizationOn;
        }

        if (_agentInstance != null)
        {
            var agentPathRenderer = _agentInstance.GetComponent<LightshipNavMeshAgentPathRenderer>();
            if (agentPathRenderer != null)
            {
                agentPathRenderer.enabled = isVisualizationOn;
            }
        }
    }

    public void ResetAgent()
    {
        if (_agentInstance != null)
        {
            if (_agentAnimator != null)
            {
                _agentAnimator.SetBool("walking", false);
            }

            Destroy(_agentInstance.gameObject);
            _agentInstance = null;
            _agentAnimator = null;
            _agentVisual = null;
            
            // DebugLog("Agent has been reset");
        }
    }
    
    // Conditional debug logging to reduce performance impact
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void DebugLog(string message)
    {
        Debug.Log(message);
    }
}

// using UnityEngine;
// using Niantic.Lightship.AR.NavigationMesh;
// using UnityEngine.InputSystem;

// public class NavigationMeshController : MonoBehaviour
// {
//     [SerializeField] private Camera _camera;
//     [SerializeField] private LightshipNavMeshManager _navmeshManager;
//     [SerializeField] private LightshipNavMeshAgent _agentPrefab;
//     [SerializeField] private Joystick _joystick;
//     [SerializeField] float _movementThreshold = 0.1f;
//     [SerializeField] float _moveSpeed = 3f;
//     [SerializeField] float _accelerationMultiplier = 10f;

//     private LightshipNavMeshAgent _agentInstance;
//     private Animator _agentAnimator;
//     private Vector3 _lastMoveDirection;
//     PlayerManager player;
//     SpawnManager spawnManager;
//     PickupEffect pickup;

//     void Start()
//     {
//         pickup = FindObjectOfType<PickupEffect>();
//         spawnManager = FindObjectOfType<SpawnManager>();
//     }

//     void Update()
//     {
//         HandleTouch();
//         HandleJoystickMovement();
//         UpdateAgentAnimation();
//     }

//     void UpdateAgentAnimation()
//     {
//         if (_agentInstance != null && _agentAnimator != null)
//         {
//             bool isMoving = Mathf.Abs(_joystick.Horizontal) > _movementThreshold || 
//                             Mathf.Abs(_joystick.Vertical) > _movementThreshold;
//             _agentAnimator.SetBool("walking", isMoving);
//         }
//     }

//     void HandleJoystickMovement()
//     {
//         if (_agentInstance == null || _joystick == null)
//             return;

//         float horizontalInput = _joystick.Horizontal;
//         float verticalInput = _joystick.Vertical;

//         if (Mathf.Abs(horizontalInput) > _movementThreshold || Mathf.Abs(verticalInput) > _movementThreshold)
//         {
//             Vector3 cameraForward = _camera.transform.forward;
//             Vector3 cameraRight = _camera.transform.right;
            
//             cameraForward.y = 0;
//             cameraRight.y = 0;
            
//             cameraForward.Normalize();
//             cameraRight.Normalize();

//             Vector3 moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

//             Vector3 currentPosition = _agentInstance.transform.position;
//             Vector3 destination = currentPosition + moveDirection * _moveSpeed * _accelerationMultiplier * Time.deltaTime;

//             _agentInstance.SetDestination(destination);

//             if (moveDirection != Vector3.zero)
//             {
//                 _agentInstance.transform.rotation = Quaternion.LookRotation(moveDirection);
//             }

//             _lastMoveDirection = moveDirection;
//         }
//     }

//     void HandleTouch()
//     {
//         // Null checks for critical components
//         if (_camera == null)
//         {
//             Debug.LogError("Camera is not assigned!");
//             return;
//         }

//         if (_agentPrefab == null)
//         {
//             Debug.LogError("Agent Prefab is not assigned!");
//             return;
//         }

//         // Check if touch is available and pressed
//         if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
//         {
//             Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
//             if (touchPosition.x > 0 && touchPosition.x < Screen.width &&
//                 touchPosition.y > 0 && touchPosition.y < Screen.height)
//             {
//                 Ray ray = _camera.ScreenPointToRay(touchPosition);
//                 Debug.Log($"Shooting ray from touch position: {touchPosition}");

//                 RaycastHit hit;
//                 if (Physics.Raycast(ray, out hit))
//                 {
//                     Debug.Log($"Raycast hit point: {hit.point}");

//                     if (_agentInstance == null)
//                     {
//                         // Create new agent
//                         _agentInstance = Instantiate(_agentPrefab);
//                         _agentInstance.transform.position = hit.point;
//                         player = _agentInstance.GetComponent<PlayerManager>();
//                         PickupEffect[] pickups = FindObjectsOfType<PickupEffect>();
//                         foreach (var pickup in pickups)
//                         {
//                             pickup.playerManager = player;
//                         }

//                         if (spawnManager != null)
//                         {
//                             // Initialize spawning with the newly created agent
//                             spawnManager.InitializeSpawningWithAgent(_agentInstance.gameObject);
//                         }
                        
//                         // Get animator
//                         _agentAnimator = _agentInstance.GetComponentInChildren<Animator>();
                        
//                         Debug.Log($"Agent created at: {hit.point}");
//                     }
//                 }
//                 else
//                 {
//                     Debug.Log("Raycast did not hit anything");
//                 }
//             }
//             else
//             {
//                 Debug.Log($"Touch position {touchPosition} is outside screen bounds");
//             }
//         }
//     }

//     public void SetVisualization(bool isVisualizationOn)
//     {
//         // Existing visualization logic
//         if (_navmeshManager == null)
//         {
//             Debug.LogWarning("NavMesh Manager is not assigned!");
//             return;
//         }

//         var navMeshRenderer = _navmeshManager.GetComponent<LightshipNavMeshRenderer>();
//         if (navMeshRenderer != null)
//         {
//             navMeshRenderer.enabled = isVisualizationOn;
//         }

//         if (_agentInstance != null)
//         {
//             var agentPathRenderer = _agentInstance.GetComponent<LightshipNavMeshAgentPathRenderer>();
//             if (agentPathRenderer != null)
//             {
//                 agentPathRenderer.enabled = isVisualizationOn;
//             }
//         }
//     }

//     public void ResetAgent()
//     {
//         if (_agentInstance != null)
//         {
//             if (_agentAnimator != null)
//             {
//                 _agentAnimator.SetBool("walking", false);
//             }

//             Destroy(_agentInstance.gameObject);
//             _agentInstance = null;
//             _agentAnimator = null;
            
//             Debug.Log("Agent has been reset");
//         }
//     }
// }