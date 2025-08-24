using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public Transform[] spawnPoints;

    [Header("UI References")]
    public Slider healthBar;
    public Button spinButton;
    public Button jumpButton;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    private PlayerController localPlayer;
    private NetworkManager networkManager;
    private AudioSource bgMusic;

    void Awake()
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

    void Start()
    {
        // Limpiar Background problemático primero
        CleanupProblematicBackground();
        
        SetupUI();
        
        // Verificar si existe HealthBarManager
        var healthBarManager = FindFirstObjectByType<HealthBarManager>();
        if (healthBarManager == null)
        {
            Debug.LogWarning("🏥 HealthBarManager not found. Creating one automatically...");
            CreateHealthBarManager();
        }
        else
        {
            Debug.Log("🏥 HealthBarManager found!");
        }
        
        networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogWarning("NetworkManager not found. Creating one automatically...");
            CreateNetworkManager();
        }
        
        // Crear ScoreManager si no existe
        var scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogWarning("🏆 ScoreManager not found. Creating one automatically...");
            CreateScoreManager();
        }
        else
        {
            Debug.Log("🏆 ScoreManager found!");
        }
        
        // Crear SimpleScoreboard en lugar del ScoreboardUI problemático
        if (FindFirstObjectByType<SimpleScoreboard>() == null)
        {
            Debug.Log("🏆 SimpleScoreboard not found. Creating one automatically...");
            CreateSimpleScoreboard();
        }
        else
        {
            Debug.Log("🏆 SimpleScoreboard found!");
        }
        
        // Health bar superior removida - ahora usamos las pequeñas sobre cada spinner
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false); // Ocultar la barra superior
        }

        GameObject bgObj = new GameObject("BackgroundMusic");
        bgMusic = bgObj.AddComponent<AudioSource>();
        bgMusic.loop = true;
        AudioClip bgClip = Resources.Load<AudioClip>("Audio/background_exciting");
        if (bgClip != null)
        {
            bgMusic.clip = bgClip;
            bgMusic.Play();
            bgMusic.volume = 0.3f; // Lower volume as requested
        }
        else
        {
            Debug.LogWarning("background_exciting clip not found in Resources/Audio");
        }
        DontDestroyOnLoad(bgObj);
    }
    
    void CreateNetworkManager()
    {
        GameObject networkManagerObj = new GameObject("NetworkManager");
        networkManager = networkManagerObj.AddComponent<NetworkManager>();
        if (playerPrefab != null)
        {
            networkManager.playerPrefab = playerPrefab;
        }
        if (spawnPoints.Length > 0)
        {
            networkManager.spawnPoints = spawnPoints;
        }
        Debug.Log("NetworkManager created automatically");
    }
    
    void CreateHealthBarManager()
    {
        GameObject healthBarManagerObj = new GameObject("HealthBarManager");
        var healthBarManager = healthBarManagerObj.AddComponent<HealthBarManager>();
        Debug.Log("🏥 HealthBarManager created automatically");
    }
    
    void CreateScoreManager()
    {
        GameObject scoreManagerObj = new GameObject("ScoreManager");
        var scoreManager = scoreManagerObj.AddComponent<ScoreManager>();
        
        // Agregar PhotonView para RPCs
        var photonView = scoreManagerObj.AddComponent<PhotonView>();
        
        // Configurar el PhotonView correctamente
        photonView.ViewID = 0; // Temporal, será asignado por PhotonNetwork
        photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
        
        // Guardar referencia para asignar ViewID más tarde
        StartCoroutine(AssignScoreManagerViewID(scoreManagerObj));
        
        Debug.Log("🏆 ScoreManager created automatically");
    }
    
    System.Collections.IEnumerator AssignScoreManagerViewID(GameObject scoreManagerObj)
    {
        // Esperar a que PhotonNetwork esté listo
        while (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Asignar ViewID válido
        var photonView = scoreManagerObj.GetComponent<PhotonView>();
        if (photonView != null)
        {
            // Solicitar ViewID al servidor
            yield return new WaitForSeconds(0.5f); // Dar tiempo para que se asigne
            Debug.Log($"🏆 ScoreManager ViewID asignado: {photonView.ViewID}");
        }
    }
    
    void CreateSimpleScoreboard()
    {
        GameObject simpleScoreboardObj = new GameObject("SimpleScoreboard");
        var simpleScoreboard = simpleScoreboardObj.AddComponent<SimpleScoreboard>();
        Debug.Log("🏆 SimpleScoreboard created automatically");
    }

    void SetupUI()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(OnSpinButtonPressed);
        if (jumpButton != null)
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
    }

    public void SetLocalPlayer(PlayerController player)
    {
        localPlayer = player;
        Debug.Log("Jugador local configurado: " + player.name);
        // Health UI ahora se maneja por las health bars individuales sobre cada spinner
    }

    // UpdateHealthUI removido - ahora usamos health bars individuales

    public void OnSpinButtonPressed()
    {
        if (localPlayer != null)
        {
            localPlayer.StartSpin();
        }
    }

    public void OnJumpButtonPressed()
    {
        if (localPlayer != null)
        {
            localPlayer.Jump();
        }
    }

    public void OnLocalPlayerDied()
    {
        // Health UI inicial removido - ahora usamos health bars individuales
    }
    
    void CleanupProblematicBackground()
    {
        Debug.Log("🧹 Iniciando limpieza de UI problemática...");
        
        // ELIMINAR TODOS los elementos UI problemáticos usando FindObjectsByType (más rápido)
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"🔍 Encontrados {allCanvases.Length} canvas en la escena");
        
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"🔍 Revisando canvas: {canvas.name}");
            
            // Buscar MainMenuUI en este canvas
            Transform mainMenuUI = canvas.transform.Find("MainMenuUI");
            if (mainMenuUI != null)
            {
                Debug.Log($"🗑️ ELIMINANDO MainMenuUI del canvas {canvas.name}");
                DestroyImmediate(mainMenuUI.gameObject);
                Debug.Log($"✅ MainMenuUI eliminado del canvas {canvas.name}");
            }
            else
            {
                Debug.Log($"✅ No se encontró MainMenuUI en canvas {canvas.name}");
            }
            
            // Buscar HealthSlider en este canvas
            Transform healthSlider = canvas.transform.Find("HealthSlider");
            if (healthSlider != null)
            {
                Debug.Log($"🗑️ ELIMINANDO HealthSlider del canvas {canvas.name}");
                DestroyImmediate(healthSlider.gameObject);
                Debug.Log($"✅ HealthSlider eliminado del canvas {canvas.name}");
            }
            else
            {
                Debug.Log($"✅ No se encontró HealthSlider en canvas {canvas.name}");
            }
            
            // Buscar cualquier Background problemático
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>();
            Debug.Log($"🔍 Canvas {canvas.name} tiene {allChildren.Length} hijos");
            
            foreach (Transform child in allChildren)
            {
                if (child.name == "Background")
                {
                    Debug.Log($"🔍 Encontrado Background: {child.name} en {child.parent?.name}");
                    RectTransform rect = child.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        Debug.Log($"🔍 Background anchors: Min({rect.anchorMin.x}, {rect.anchorMin.y}), Max({rect.anchorMax.x}, {rect.anchorMax.y})");
                        
                        // Si ocupa toda la pantalla o tiene anchors problemáticos
                        if ((Mathf.Approximately(rect.anchorMin.x, 0f) && Mathf.Approximately(rect.anchorMax.x, 1f) &&
                             Mathf.Approximately(rect.anchorMin.y, 0f) && Mathf.Approximately(rect.anchorMax.y, 1f)) ||
                            (Mathf.Approximately(rect.anchorMin.y, 0.25f) && Mathf.Approximately(rect.anchorMax.y, 0.75f)))
                        {
                            Debug.Log($"🗑️ ELIMINANDO Background problemático: {child.name} en {child.parent?.name}");
                            DestroyImmediate(child.gameObject);
                            Debug.Log($"✅ Background problemático eliminado");
                        }
                        else
                        {
                            Debug.Log($"✅ Background no es problemático, manteniendo");
                        }
                    }
                }
            }
        }
        
        // VERIFICACIÓN FINAL - buscar por nombre global
        GameObject mainMenuUIGlobal = GameObject.Find("MainMenuUI");
        if (mainMenuUIGlobal != null)
        {
            Debug.Log($"🚨 ALERTA: MainMenuUI sigue existiendo globalmente!");
            DestroyImmediate(mainMenuUIGlobal);
            Debug.Log($"✅ MainMenuUI eliminado globalmente");
        }
        
        GameObject healthSliderGlobal = GameObject.Find("HealthSlider");
        if (healthSliderGlobal != null)
        {
            Debug.Log($"🚨 ALERTA: HealthSlider sigue existiendo globalmente!");
            DestroyImmediate(healthSliderGlobal);
            Debug.Log($"✅ HealthSlider eliminado globalmente");
        }
        
        Debug.Log("✅ Limpieza de UI completada");
    }
} 