using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class HealthBarManager : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset = new Vector3(0, 3f, 0);
    public bool showHealthBarsForAllPlayers = true;
    public bool showHealthBarForLocalPlayer = true;
    
    [Header("Auto Setup")]
    public bool autoCreateHealthBars = true;
    public bool destroyHealthBarsOnPlayerDeath = true;
    
    private Dictionary<int, HealthBar> playerHealthBars = new Dictionary<int, HealthBar>();
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (autoCreateHealthBars)
        {
            Invoke(nameof(SetupHealthBars), 1f);
        }
    }
    
    void Update()
    {
        // Verificar si hay nuevos jugadores y crear health bars para ellos
        CheckForNewPlayers();
    }
    
    void SetupHealthBars()
    {
        Debug.Log("🏥 Setting up Health Bars...");
        
        // Crear health bar para el jugador local
        if (showHealthBarForLocalPlayer)
        {
            CreateHealthBarForLocalPlayer();
        }
        
        // Crear health bars para otros jugadores
        if (showHealthBarsForAllPlayers)
        {
            CreateHealthBarsForOtherPlayers();
        }
    }
    
    void CreateHealthBarForLocalPlayer()
    {
        PlayerController localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            CreateHealthBar(localPlayer, true);
            Debug.Log($"Health bar created for local player: {localPlayer.name}");
        }
    }
    
    void CreateHealthBarsForOtherPlayers()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        foreach (PlayerController player in allPlayers)
        {
            if (player != null && !player.photonView.IsMine)
            {
                CreateHealthBar(player, false);
                Debug.Log($"Health bar created for remote player: {player.name}");
            }
        }
    }
    
    void CheckForNewPlayers()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        foreach (PlayerController player in allPlayers)
        {
            if (player != null)
            {
                int playerId = player.photonView.ViewID;
                
                // Verificar si ya existe una health bar para este jugador
                if (!playerHealthBars.ContainsKey(playerId))
                {
                    bool isLocalPlayer = player.photonView.IsMine;
                    
                    // Crear health bar solo si cumple con las condiciones
                    if ((isLocalPlayer && showHealthBarForLocalPlayer) || 
                        (!isLocalPlayer && showHealthBarsForAllPlayers))
                    {
                        CreateHealthBar(player, isLocalPlayer);
                        Debug.Log($"Health bar created for new player: {player.name} (Local: {isLocalPlayer})");
                    }
                }
            }
        }
        
        // Limpiar health bars de jugadores que ya no existen
        CleanupDeadPlayers();
    }
    
    void CreateHealthBar(PlayerController player, bool isLocalPlayer)
    {
        if (player == null) return;
        
        int playerId = player.photonView.ViewID;
        
        // Verificar si ya existe una health bar para este jugador
        if (playerHealthBars.ContainsKey(playerId))
        {
            Debug.Log($"Health bar already exists for player {player.name}");
            return;
        }
        
        // Crear GameObject para la health bar
        GameObject healthBarGO = new GameObject($"HealthBar_{player.name}");
        
        // Agregar componente HealthBar
        HealthBar healthBar = healthBarGO.AddComponent<HealthBar>();
        
        // Agregar efectos a la health bar
        HealthBarEffects healthBarEffects = healthBarGO.AddComponent<HealthBarEffects>();
        
        // Configurar la health bar
        healthBar.targetPlayer = player;
        healthBar.offset = healthBarOffset;
        healthBar.followPlayer = true;
        healthBar.smoothSpeed = 5f;
        
        // Configurar colores específicos para jugador local vs remoto
        if (isLocalPlayer)
        {
            healthBar.highHealthColor = Color.green;
            healthBar.mediumHealthColor = Color.yellow;
            healthBar.lowHealthColor = Color.red;
        }
        else
        {
            healthBar.highHealthColor = new Color(0.2f, 0.8f, 0.2f); // Verde más oscuro
            healthBar.mediumHealthColor = new Color(0.8f, 0.8f, 0.2f); // Amarillo más oscuro
            healthBar.lowHealthColor = new Color(0.8f, 0.2f, 0.2f); // Rojo más oscuro
        }
        
        // Configurar umbrales
        healthBar.highHealthThreshold = 0.7f;
        healthBar.mediumHealthThreshold = 0.3f;
        
        // Configurar animaciones
        healthBar.enablePulseAnimation = true;
        healthBar.pulseSpeed = 2f;
        healthBar.pulseIntensity = 0.1f;
        
        // Agregar a la lista de health bars
        playerHealthBars[playerId] = healthBar;
        
        Debug.Log($"Health bar created successfully for {player.name} (Local: {isLocalPlayer})");
    }
    
    void CleanupDeadPlayers()
    {
        List<int> playersToRemove = new List<int>();
        
        foreach (var kvp in playerHealthBars)
        {
            int playerId = kvp.Key;
            HealthBar healthBar = kvp.Value;
            
            // Verificar si el jugador aún existe
            if (healthBar.targetPlayer == null)
            {
                playersToRemove.Add(playerId);
                Debug.Log($"Player with ID {playerId} no longer exists, removing health bar");
            }
        }
        
        // Remover health bars de jugadores muertos
        foreach (int playerId in playersToRemove)
        {
            if (playerHealthBars.ContainsKey(playerId))
            {
                HealthBar healthBar = playerHealthBars[playerId];
                if (healthBar != null)
                {
                    if (destroyHealthBarsOnPlayerDeath)
                    {
                        Destroy(healthBar.gameObject);
                    }
                    else
                    {
                        healthBar.SetVisible(false);
                    }
                }
                playerHealthBars.Remove(playerId);
            }
        }
    }
    
    PlayerController FindLocalPlayer()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        foreach (PlayerController player in allPlayers)
        {
            if (player != null && player.photonView.IsMine)
            {
                return player;
            }
        }
        
        return null;
    }
    
    // Método público para crear health bar manualmente
    public void CreateHealthBarForPlayer(PlayerController player)
    {
        if (player != null)
        {
            CreateHealthBar(player, player.photonView.IsMine);
        }
    }
    
    // Método para remover health bar de un jugador específico
    public void RemoveHealthBarForPlayer(PlayerController player)
    {
        if (player != null)
        {
            int playerId = player.photonView.ViewID;
            
            if (playerHealthBars.ContainsKey(playerId))
            {
                HealthBar healthBar = playerHealthBars[playerId];
                if (healthBar != null)
                {
                    Destroy(healthBar.gameObject);
                }
                playerHealthBars.Remove(playerId);
                Debug.Log($"Health bar removed for {player.name}");
            }
        }
    }
    
    // Método para mostrar/ocultar todas las health bars
    public void SetAllHealthBarsVisible(bool visible)
    {
        foreach (var kvp in playerHealthBars)
        {
            HealthBar healthBar = kvp.Value;
            if (healthBar != null)
            {
                healthBar.SetVisible(visible);
            }
        }
    }
    
    // Método para actualizar configuración de todas las health bars
    public void UpdateAllHealthBarSettings(Vector3 newOffset, float newSmoothSpeed)
    {
        foreach (var kvp in playerHealthBars)
        {
            HealthBar healthBar = kvp.Value;
            if (healthBar != null)
            {
                healthBar.offset = newOffset;
                healthBar.smoothSpeed = newSmoothSpeed;
            }
        }
    }
    
    // Método para obtener health bar de un jugador específico
    public HealthBar GetHealthBarForPlayer(PlayerController player)
    {
        if (player != null)
        {
            int playerId = player.photonView.ViewID;
            
            if (playerHealthBars.ContainsKey(playerId))
            {
                return playerHealthBars[playerId];
            }
        }
        
        return null;
    }
    
    // Método para limpiar todas las health bars
    public void ClearAllHealthBars()
    {
        foreach (var kvp in playerHealthBars)
        {
            HealthBar healthBar = kvp.Value;
            if (healthBar != null)
            {
                Destroy(healthBar.gameObject);
            }
        }
        
        playerHealthBars.Clear();
        Debug.Log("All health bars have been cleared");
    }
} 