using UnityEngine;
using Photon.Pun;

/// <summary>
/// Script rápido para solucionar el problema de sincronización de jugadores en Photon PUN.
/// Los clientes que entran a una sala no ven a los jugadores que ya estaban dentro.
/// Simplemente agrega este script a cualquier GameObject en la escena y se configurará automáticamente.
/// </summary>
public class QuickPlayerSyncFix : MonoBehaviour
{
    [Header("Quick Fix Settings")]
    public bool enableAutoFix = true;
    public bool enableDebugLogs = true;
    public float syncDelay = 1f;
    public int maxSyncAttempts = 3;
    
    [Header("Components")]
    public PlayerSyncManager playerSyncManager;
    public NetworkManager networkManager;
    
    void Awake()
    {
        if (enableAutoFix)
        {
            // Configurar para que este objeto persista
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLogs)
                Debug.Log("🔄 QuickPlayerSyncFix: Iniciando configuración automática...");
        }
    }
    
    void Start()
    {
        if (enableAutoFix)
        {
            // Ejecutar la configuración después de un pequeño delay
            Invoke(nameof(PerformQuickSyncSetup), 1f);
        }
    }
    
    void PerformQuickSyncSetup()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 QuickPlayerSyncFix: Configurando sincronización de jugadores...");
        
        // Crear PlayerSyncManager si no existe
        if (playerSyncManager == null)
        {
            playerSyncManager = FindFirstObjectByType<PlayerSyncManager>();
            if (playerSyncManager == null)
            {
                GameObject syncManagerObj = new GameObject("PlayerSyncManager");
                
                // Agregar PhotonView al PlayerSyncManager
                PhotonView photonView = syncManagerObj.AddComponent<PhotonView>();
                // El ViewID y OwnerActorNr se asignarán automáticamente por Photon
                
                playerSyncManager = syncManagerObj.AddComponent<PlayerSyncManager>();
                playerSyncManager.enableDebugLogs = enableDebugLogs;
                playerSyncManager.syncDelay = syncDelay;
                playerSyncManager.maxSyncAttempts = maxSyncAttempts;
                
                if (enableDebugLogs)
                    Debug.Log("✅ QuickPlayerSyncFix: PlayerSyncManager creado con PhotonView");
            }
        }
        
        // Verificar NetworkManager
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogWarning("⚠️ NetworkManager no encontrado. Asegúrate de que esté en la escena.");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("✅ QuickPlayerSyncFix: NetworkManager encontrado");
            }
        }
        
        if (enableDebugLogs)
            Debug.Log("✅ QuickPlayerSyncFix: Configuración completada");
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Setup Player Sync Fix")]
    public void SetupPlayerSyncFix()
    {
        PerformQuickSyncSetup();
    }
    
    [ContextMenu("Force Sync All Players")]
    public void ForceSyncAllPlayers()
    {
        if (playerSyncManager != null)
        {
            playerSyncManager.ForceSyncPlayers();
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerSyncManager no encontrado. Ejecuta 'Setup Player Sync Fix' primero.");
        }
    }
    
    [ContextMenu("Check Player Sync Status")]
    public void CheckPlayerSyncStatus()
    {
        Debug.Log("🔍 QuickPlayerSyncFix: Verificando estado de sincronización...");
        
        if (playerSyncManager != null)
        {
            Debug.Log("✅ PlayerSyncManager: Activo");
            playerSyncManager.CheckPlayerStatus();
        }
        else
        {
            Debug.Log("❌ PlayerSyncManager: No encontrado");
        }
        
        if (networkManager != null)
        {
            Debug.Log("✅ NetworkManager: Activo");
            Debug.Log($"   Conectado: {networkManager.IsConnected()}");
            Debug.Log($"   En sala: {networkManager.IsInRoom()}");
            Debug.Log($"   Jugadores: {networkManager.GetPlayerCount()}");
        }
        else
        {
            Debug.Log("❌ NetworkManager: No encontrado");
        }
    }
    
    [ContextMenu("Clear Sync State")]
    public void ClearSyncState()
    {
        if (playerSyncManager != null)
        {
            playerSyncManager.ClearSyncState();
            Debug.Log("🔄 Estado de sincronización limpiado");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerSyncManager no encontrado");
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && enableAutoFix)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 QuickPlayerSyncFix: App recuperó foco - verificando sincronización...");
            
            // Verificar sincronización después de un pequeño delay
            Invoke(nameof(CheckAndFixSync), 0.5f);
        }
    }
    
    void CheckAndFixSync()
    {
        if (playerSyncManager != null && networkManager != null && networkManager.IsInRoom())
        {
            // Forzar sincronización si es necesario
            playerSyncManager.ForceSyncPlayers();
        }
    }
}
