using UnityEngine;
using System.Collections;

/// <summary>
/// Handles player death sequence: camera zoom out and desaturation.
/// ATTACH TO MAIN CAMERA for OnRenderImage to work.
/// </summary>
public class DeathController : MonoBehaviour
{
    [Header("Death Sequence Settings")]
    [Tooltip("Duration of death sequence (zoom + desaturation)")]
    public float deathSequenceDuration = 4f;
    
    [Tooltip("How much to zoom out (field of view increase)")]
    public float zoomOutAmount = 20f;
    
    [Tooltip("Camera orbit speed (degrees per second)")]
    public float orbitSpeed = 30f; // 30 deg/sec = 12 seconds per full rotation
    
    [Header("Audio")]
    [Tooltip("Wwise death sound event")]
    public AK.Wwise.Event deathSound;
    
    [Header("Music Stop Method")]
    [Tooltip("How to stop background music")]
    public MusicStopMethod musicStopMethod = MusicStopMethod.StopAll;
    
    [Tooltip("Wwise BGM music event to stop (only used if method = StopSpecificEvent)")]
    public AK.Wwise.Event bgmMusicEvent;
    
    [Tooltip("GameObject that posted the music (leave null to search)")]
    public GameObject musicSourceObject;
    
    [Header("References")]
    [Tooltip("Main camera (auto-finds if null)")]
    public Camera mainCamera;
    
    [Tooltip("Material for desaturation effect (optional - uses shader if null)")]
    public Material desaturationMaterial;
    
    [Tooltip("Player GameObject to disable on death (auto-finds if null)")]
    public GameObject playerObject;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private float _originalFOV;
    private Material _desatMat;
    private bool _isDying = false;
    private bool _isOrbiting = false;
    private ThirdPersonCamera _thirdPersonCamera;
    private float _orbitAngle = 0f;
    private float _orbitDistance = 0f; // Store initial distance
    private float _orbitHeight = 0f;   // Store initial height offset
    
    // Singleton pattern
    private static DeathController _instance;
    public static DeathController Instance => _instance;
    
    // Enum for different stop methods
    public enum MusicStopMethod
    {
        StopAll,              // Stop ALL audio globally (nuclear option)
        StopAllMusic,         // Stop all music buses (recommended)
        StopSpecificEvent,    // Stop specific event on specific GameObject
        StopEventGlobally     // Stop all instances of specific event
    }
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple DeathController instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    void Start()
    {
        // Auto-find camera
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (mainCamera != null)
        {
            _originalFOV = mainCamera.fieldOfView;
            
            // Get ThirdPersonCamera component for orbit control
            _thirdPersonCamera = mainCamera.GetComponent<ThirdPersonCamera>();
            if (_thirdPersonCamera == null)
            {
                Debug.LogWarning("No ThirdPersonCamera found on Main Camera. Camera orbit will not work.", this);
            }
        }
        else
        {
            Debug.LogError("DeathController cannot find Main Camera!", this);
        }
        
        // Auto-find player
        if (playerObject == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerObject = player;
            }
            else
            {
                Debug.LogWarning("DeathController cannot find Player GameObject. Tag player with 'Player' tag.", this);
            }
        }
        
        // Setup desaturation material
        if (desaturationMaterial == null)
        {
            Debug.LogError("❌ NO DESATURATION MATERIAL ASSIGNED!\n\n" +
                "To fix:\n" +
                "1. Create a Material in Unity (Right-click → Create → Material)\n" +
                "2. Name it 'DesaturationMaterial'\n" +
                "3. Set its Shader to 'Custom/Desaturation'\n" +
                "4. Drag the material to the 'Desaturation Material' field on this component\n\n" +
                "The shader file should be at: Assets/Project/Art/Shaders/Desaturation.shader\n" +
                "First line should be: Shader \"Custom/Desaturation\"", this);
        }
        else
        {
            _desatMat = desaturationMaterial;
            
            // Initialize saturation to full color
            _desatMat.SetFloat("_Saturation", 1f);
            
            if (showDebugInfo)
                Debug.Log("✓ Desaturation material assigned and initialized (Saturation = 1.0)");
        }
    }
    
    void Update()
    {
        // Handle camera orbit during death
        if (_isOrbiting && playerObject != null && mainCamera != null)
        {
            // Increment orbit angle (uses unscaled time since game is paused)
            _orbitAngle += orbitSpeed * Time.unscaledDeltaTime;
            
            // Wrap angle to 0-360
            if (_orbitAngle >= 360f)
                _orbitAngle -= 360f;
            
            // Calculate new camera position using STORED distance and height
            float radians = _orbitAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * _orbitDistance,
                _orbitHeight,
                Mathf.Sin(radians) * _orbitDistance
            );
            
            // Set camera position
            mainCamera.transform.position = playerObject.transform.position + offset;
            
            // Look at player
            Vector3 lookAtPoint = playerObject.transform.position;
            if (_thirdPersonCamera != null)
            {
                lookAtPoint += _thirdPersonCamera.lookAtOffset;
            }
            mainCamera.transform.LookAt(lookAtPoint);
        }
    }
    
    /// <summary>
    /// Trigger death sequence
    /// </summary>
    public void TriggerDeathSequence()
    {
        if (_isDying) return;
        
        _isDying = true;
        
        // Stop background music using selected method
        StopBackgroundMusic();
        
        // Play death sound
        if (deathSound != null)
        {
            deathSound.Post(gameObject);
        }
        
        // PAUSE GAME IMMEDIATELY (before animation starts)
        Time.timeScale = 0f;
        
        if (showDebugInfo)
            Debug.Log("Game paused immediately on death");
        
        // Disable ThirdPersonCamera control and start orbit
        if (_thirdPersonCamera != null)
        {
            _thirdPersonCamera.enabled = false;
            
            // Store initial camera distance and height (BEFORE zoom changes anything)
            _orbitDistance = Vector3.Distance(
                new Vector3(mainCamera.transform.position.x, 0, mainCamera.transform.position.z),
                new Vector3(playerObject.transform.position.x, 0, playerObject.transform.position.z)
            );
            _orbitHeight = mainCamera.transform.position.y - playerObject.transform.position.y;
            
            // Calculate starting orbit angle based on current camera position
            Vector3 directionToCamera = mainCamera.transform.position - playerObject.transform.position;
            _orbitAngle = Mathf.Atan2(directionToCamera.z, directionToCamera.x) * Mathf.Rad2Deg;
            
            // Start orbiting
            _isOrbiting = true;
            
            if (showDebugInfo)
                Debug.Log($"Camera orbit started at angle {_orbitAngle}° (distance: {_orbitDistance:F2}, height: {_orbitHeight:F2})");
        }
        
        // Disable player controls
        if (playerObject != null)
        {
            // Disable movement script
            PlayerMovement3D movement = playerObject.GetComponent<PlayerMovement3D>();
            if (movement != null)
            {
                movement.enabled = false;
                if (showDebugInfo)
                    Debug.Log("Disabled player movement");
            }
            
            // Disable attack controller
            AttackController attackController = playerObject.GetComponent<AttackController>();
            if (attackController != null)
            {
                attackController.enabled = false;
                if (showDebugInfo)
                    Debug.Log("Disabled player attacks");
            }
            
            // Stop player rigidbody
            Rigidbody rb = playerObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // Make kinematic to prevent physics
            }
        }
        
        // Freeze all enemies
        FreezeAllEnemies();
        
        // Disable spawn manager
        SpawnManager spawnManager = SpawnManager.Instance;
        if (spawnManager != null)
        {
            spawnManager.enabled = false;
            if (showDebugInfo)
                Debug.Log("Disabled spawn manager");
        }
        
        // Start death animation (uses unscaled time)
        StartCoroutine(DeathSequenceCoroutine());
        
        if (showDebugInfo)
            Debug.Log("Death sequence started - game frozen");
    }
    
    /// <summary>
    /// Freeze all enemies in place
    /// </summary>
    void FreezeAllEnemies()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            // Disable chase behavior
            EnemyChase3D chase = enemy.GetComponent<EnemyChase3D>();
            if (chase != null)
            {
                chase.enabled = false;
            }
            
            // Stop rigidbody
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // Make kinematic to freeze
            }
        }
        
        if (showDebugInfo)
            Debug.Log($"Froze {allEnemies.Length} enemies");
    }
    
    /// <summary>
    /// Stop camera orbit (called when player clicks "Okay" button)
    /// </summary>
    public void StopCameraOrbit()
    {
        _isOrbiting = false;
        
        if (showDebugInfo)
            Debug.Log("Camera orbit stopped");
    }
    
    /// <summary>
    /// Stop background music using the selected method
    /// </summary>
    void StopBackgroundMusic()
    {
        switch (musicStopMethod)
        {
            case MusicStopMethod.StopAll:
                // Nuclear option: stops ALL sounds in the game
                AkSoundEngine.StopAll();
                if (showDebugInfo)
                    Debug.Log("Stopped ALL audio globally");
                break;
                
            case MusicStopMethod.StopAllMusic:
                // Stop all music buses (recommended - stops only music, keeps SFX)
                // This stops the "Master Audio Bus" > "Music" bus
                AkSoundEngine.SetRTPCValue("Volume_Music", 0f);
                // Or stop by bus name if you have it set up:
                // AkSoundEngine.StopAll(gameObject, (int)AkGameObjPosOffsetMode.AK_SetBankLoadIOSettings);
                
                if (showDebugInfo)
                    Debug.Log("Stopped all music via music bus");
                break;
                
            case MusicStopMethod.StopSpecificEvent:
                // Stop specific event on specific GameObject
                if (bgmMusicEvent != null)
                {
                    // Try to find the GameObject that posted the music
                    GameObject musicObject = musicSourceObject;
                    
                    if (musicObject == null)
                    {
                        // Try to find AudioManager or similar
                        musicObject = GameObject.Find("AudioManager");
                        if (musicObject == null)
                            musicObject = GameObject.Find("GameManager");
                        if (musicObject == null)
                            musicObject = gameObject; // Fallback to this object
                    }
                    
                    bgmMusicEvent.Stop(musicObject);
                    
                    if (showDebugInfo)
                        Debug.Log($"Stopped music event on {musicObject.name}");
                }
                else
                {
                    Debug.LogWarning("No BGM music event assigned!", this);
                }
                break;
                
            case MusicStopMethod.StopEventGlobally:
                // Stop all instances of a specific event globally
                if (bgmMusicEvent != null)
                {
                    // Get the event ID and stop all playing instances
                    uint eventId = bgmMusicEvent.Id;
                    AkSoundEngine.StopPlayingID(eventId);
                    
                    if (showDebugInfo)
                        Debug.Log($"Stopped all instances of event ID {eventId} globally");
                }
                else
                {
                    Debug.LogWarning("No BGM music event assigned!", this);
                }
                break;
        }
    }
    
    IEnumerator DeathSequenceCoroutine()
    {
        float elapsed = 0f;
        
        // Phase 1: Zoom and desaturate over 4 seconds (using unscaled time since game is paused)
        while (elapsed < deathSequenceDuration)
        {
            elapsed += Time.unscaledDeltaTime; // MUST use unscaled since Time.timeScale = 0
            float t = elapsed / deathSequenceDuration;
            
            // Zoom out camera
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = Mathf.Lerp(_originalFOV, _originalFOV + zoomOutAmount, t);
            }
            
            // Desaturation handled in OnRenderImage
            if (_desatMat != null)
            {
                _desatMat.SetFloat("_Saturation", Mathf.Lerp(1f, 0f, t));
            }
            
            yield return null;
        }
        
        // Ensure final state
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = _originalFOV + zoomOutAmount;
        }
        if (_desatMat != null)
        {
            _desatMat.SetFloat("_Saturation", 0f);
        }
        
        if (showDebugInfo)
            Debug.Log("Death animation complete (4 seconds elapsed)");
        
        // Phase 2: Wait additional 0.5 seconds before showing "You Died"
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Game is already paused (Time.timeScale = 0 from TriggerDeathSequence)
        
        // Show "You Died" screen
        DeathUI deathUI = DeathUI.Instance;
        if (deathUI != null)
        {
            deathUI.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("DeathController cannot find DeathUI! Make sure DeathUI exists in scene.", this);
        }
        
        if (showDebugInfo)
            Debug.Log("Death sequence complete - UI shown");
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_isDying && _desatMat != null)
        {
            if (showDebugInfo && Time.frameCount % 60 == 0) // Log once per second
                Debug.Log($"Desaturation active - Saturation: {_desatMat.GetFloat("_Saturation")}");
            
            Graphics.Blit(source, destination, _desatMat);
        }
        else
        {
            if (_isDying && _desatMat == null && showDebugInfo)
            {
                Debug.LogError("Desaturation material is NULL! OnRenderImage cannot apply effect.");
            }
            Graphics.Blit(source, destination);
        }
    }
}
