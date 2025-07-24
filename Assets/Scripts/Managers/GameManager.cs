using UnityEngine;
using UnityEngine.UI;

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
        
        // Buscar NetworkManager
        networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogWarning("NetworkManager no encontrado. Creando uno automáticamente...");
            CreateNetworkManager();
        }
    }
    
    void CreateNetworkManager()
    {
        GameObject networkManagerObj = new GameObject("NetworkManager");
        networkManager = networkManagerObj.AddComponent<NetworkManager>();
        
        // Configurar referencias básicas
        if (playerPrefab != null)
        {
            networkManager.playerPrefab = playerPrefab;
        }
        
        if (spawnPoints.Length > 0)
        {
            networkManager.spawnPoints = spawnPoints;
        }
        
        Debug.Log("NetworkManager creado automáticamente");
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
    }

    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
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
} 