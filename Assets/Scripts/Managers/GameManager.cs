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
        // Configura slider si existe
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.wholeNumbers = false;
            healthBar.value = 1f; // Empezar con salud completa
        }
        UpdateHealthUI(100, 100); // Mostrar salud completa por defecto
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
        UpdateHealthUI(localPlayer.CurrentHealth, localPlayer.MaxHealth);
    }

    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthBar == null) return;
        float v = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        healthBar.value = Mathf.Clamp01(v);
    }

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
        UpdateHealthUI(0, 1);
    }
} 