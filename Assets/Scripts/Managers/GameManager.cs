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
        SpawnPlayer();
    }

    void SetupUI()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(OnSpinButtonPressed);
        if (jumpButton != null)
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
    }

    void SpawnPlayer()
    {
        if (spawnPoints.Length == 0 || playerPrefab == null) return;
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        localPlayer = player.GetComponent<PlayerController>();
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