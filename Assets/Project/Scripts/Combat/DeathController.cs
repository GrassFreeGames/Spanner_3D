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
        
        // Create desaturation material if not provided
        if (desaturationMaterial == null)
        {
            // Simple desaturation shader
            Shader desatShader = Shader.Find("Hidden/Desaturation");
            if (desatShader != null)
            {
                _desatMat = new Material(desatShader);
                if (showDebugInfo)
                    Debug.Log("Created desaturation material from shader");
            }
            else
            {
                Debug.LogError("Desaturation shader not found! Make sure 'Desaturation.shader' is in your project at Assets/Shaders/Desaturation.shader", this);
            }
        }
        else
        {
            _desatMat = desaturationMaterial;
        }
        
        // Initialize saturation to full color
        if (_desatMat != null)
        {
            _desatMat.SetFloat("_Saturation", 1f);
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
        
        // Disable player controls (but keep object active for camera to follow)
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
            }
        }
        
        // Start death animation
        StartCoroutine(DeathSequenceCoroutine());
        
        if (showDebugInfo)
            Debug.Log("Death sequence started");
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
        
        // Phase 1: Zoom and desaturate over 4 seconds
        while (elapsed < deathSequenceDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time in case we pause later
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
        
        // Pause the game NOW (after death sequence, before UI)
        Time.timeScale = 0f;
        
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
            Debug.Log("Death sequence complete - game paused, UI shown");
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_isDying && _desatMat != null)
        {
            Graphics.Blit(source, destination, _desatMat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
