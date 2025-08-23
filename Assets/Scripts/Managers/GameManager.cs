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
        
        // ScoreManager opcional - no crear automáticamente para evitar problemas
        // Health bar superior removida - ahora usamos las pequeñas sobre cada spinner
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false); // Ocultar la barra superior
        }
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
        // Agregar PhotonView para RPCs - NO asignar ViewID manualmente
        var photonView = scoreManagerObj.AddComponent<PhotonView>();
        // PhotonNetwork asignará automáticamente un ViewID válido
        Debug.Log("🏆 ScoreManager created automatically");
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
} 