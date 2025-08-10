using UnityEngine;

/// <summary>
/// Manager completo que combina todas las soluciones para SpinLegends:
/// 1. Joystick Focus Fix - Soluciona el problema del joystick que no funciona al iniciar
/// 2. Player Sync Fix - Soluciona el problema de sincronización de jugadores
/// 3. UI Recovery Fix - Soluciona el problema de UI que desaparece al ir al home y volver
/// 
/// Simplemente agrega este script a cualquier GameObject en la escena y se configurará automáticamente.
/// </summary>
public class SpinLegendsFixManager : MonoBehaviour
{
    [Header("Fix Settings")]
    public bool enableAllFixes = true;
    public bool enableDebugLogs = true;
    
    [Header("Joystick Fix Settings")]
    public bool enableJoystickFix = true;
    public float joystickInitializationDelay = 0.5f;
    public bool forceJoystickFocus = true;
    
    [Header("Player Sync Settings")]
    public bool enablePlayerSyncFix = true;
    public float syncDelay = 1f;
    public int maxSyncAttempts = 3;
    
    [Header("UI Recovery Settings")]
    public bool enableUIRecoveryFix = true;
    public float recoveryDelay = 0.5f;
    public int maxRecoveryAttempts = 3;
    
    [Header("Components")]
    public QuickJoystickFix joystickFix;
    public QuickPlayerSyncFix playerSyncFix;
    public QuickUIRecoveryFix uiRecoveryFix;
    public JoystickRecoveryFix joystickRecoveryFix;

    [Header("UI Cleanup")]
    public bool disablePhotonDebugGuis = true;
    
    void Awake()
    {
        if (enableAllFixes)
        {
            // Configurar para que este objeto persista
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLogs)
                Debug.Log("🔧 SpinLegendsFixManager: Iniciando configuración completa...");
        }
    }
    
    void Start()
    {
        if (enableAllFixes)
        {
            // Ejecutar la configuración después de un pequeño delay
            Invoke(nameof(PerformCompleteSetup), 1f);
        }
        if (disablePhotonDebugGuis)
        {
            DisablePhotonDebugOverlays();
        }
    }

    void DisablePhotonDebugOverlays()
    {
        var statesGui = FindFirstObjectByType<Photon.Pun.UtilityScripts.StatesGui>();
        if (statesGui != null) statesGui.gameObject.SetActive(false);
        var statsGui = FindFirstObjectByType<Photon.Pun.UtilityScripts.PhotonStatsGui>();
        if (statsGui != null) statsGui.gameObject.SetActive(false);
    }

    void PerformCompleteSetup()
    {
        if (enableDebugLogs)
            Debug.Log("🔧 SpinLegendsFixManager: Configurando todas las soluciones...");
        
        // Configurar Joystick Fix
        if (enableJoystickFix)
        {
            SetupJoystickFix();
        }
        
        // Configurar Player Sync Fix
        if (enablePlayerSyncFix)
        {
            SetupPlayerSyncFix();
        }
        
        // Configurar UI Recovery Fix
        if (enableUIRecoveryFix)
        {
            SetupUIRecoveryFix();
        }
        
        // Configurar Joystick Recovery Fix
        SetupJoystickRecoveryFix();
        
        if (enableDebugLogs)
            Debug.Log("✅ SpinLegendsFixManager: Configuración completa finalizada");
    }
    
    void SetupJoystickFix()
    {
        if (enableDebugLogs)
            Debug.Log("🎮 Configurando Joystick Fix...");
        
        if (joystickFix == null)
        {
            joystickFix = FindFirstObjectByType<QuickJoystickFix>();
            if (joystickFix == null)
            {
                GameObject joystickFixObj = new GameObject("QuickJoystickFix");
                joystickFix = joystickFixObj.AddComponent<QuickJoystickFix>();
                
                // Configurar el Joystick Fix
                joystickFix.enableQuickFix = true;
                joystickFix.enableDebugLogs = enableDebugLogs;
                joystickFix.setupDelay = joystickInitializationDelay;
                
                if (enableDebugLogs)
                    Debug.Log("✅ Joystick Fix creado");
            }
        }
    }
    
    void SetupPlayerSyncFix()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Configurando Player Sync Fix...");
        
        if (playerSyncFix == null)
        {
            playerSyncFix = FindFirstObjectByType<QuickPlayerSyncFix>();
            if (playerSyncFix == null)
            {
                GameObject playerSyncFixObj = new GameObject("QuickPlayerSyncFix");
                playerSyncFix = playerSyncFixObj.AddComponent<QuickPlayerSyncFix>();
                
                // Configurar el Player Sync Fix
                playerSyncFix.enableAutoFix = true;
                playerSyncFix.enableDebugLogs = enableDebugLogs;
                playerSyncFix.syncDelay = syncDelay;
                playerSyncFix.maxSyncAttempts = maxSyncAttempts;
                
                if (enableDebugLogs)
                    Debug.Log("✅ Player Sync Fix creado");
            }
        }
    }
    
    void SetupUIRecoveryFix()
    {
        if (enableDebugLogs)
            Debug.Log("🖥️ Configurando UI Recovery Fix...");
        
        if (uiRecoveryFix == null)
        {
            uiRecoveryFix = FindFirstObjectByType<QuickUIRecoveryFix>();
            if (uiRecoveryFix == null)
            {
                GameObject uiRecoveryFixObj = new GameObject("QuickUIRecoveryFix");
                uiRecoveryFix = uiRecoveryFixObj.AddComponent<QuickUIRecoveryFix>();
                
                // Configurar el UI Recovery Fix
                uiRecoveryFix.enableAutoFix = true;
                uiRecoveryFix.enableDebugLogs = enableDebugLogs;
                uiRecoveryFix.recoveryDelay = recoveryDelay;
                uiRecoveryFix.maxRecoveryAttempts = maxRecoveryAttempts;
                
                if (enableDebugLogs)
                    Debug.Log("✅ UI Recovery Fix creado");
            }
        }
    }
    
    void SetupJoystickRecoveryFix()
    {
        if (enableDebugLogs)
            Debug.Log("🎮 Configurando Joystick Recovery Fix...");
        
        if (joystickRecoveryFix == null)
        {
            joystickRecoveryFix = FindFirstObjectByType<JoystickRecoveryFix>();
            if (joystickRecoveryFix == null)
            {
                GameObject joystickRecoveryFixObj = new GameObject("JoystickRecoveryFix");
                joystickRecoveryFix = joystickRecoveryFixObj.AddComponent<JoystickRecoveryFix>();
                
                // Configurar el Joystick Recovery Fix
                joystickRecoveryFix.enableRecovery = true;
                joystickRecoveryFix.enableDebugLogs = enableDebugLogs;
                joystickRecoveryFix.recoveryDelay = 1f;
                
                if (enableDebugLogs)
                    Debug.Log("✅ Joystick Recovery Fix creado");
            }
        }
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Setup All Fixes")]
    public void SetupAllFixes()
    {
        PerformCompleteSetup();
    }
    
    [ContextMenu("Force All Fixes")]
    public void ForceAllFixes()
    {
        if (enableDebugLogs)
            Debug.Log("🔧 Forzando todas las soluciones...");
        
        // Forzar Joystick Fix
        if (joystickFix != null)
        {
            joystickFix.SetupQuickFix();
        }
        
        // Forzar Player Sync Fix
        if (playerSyncFix != null)
        {
            playerSyncFix.ForceSyncAllPlayers();
        }
        
        // Forzar UI Recovery Fix
        if (uiRecoveryFix != null)
        {
            uiRecoveryFix.ForceUIRecovery();
        }
    }
    
    [ContextMenu("Check All Status")]
    public void CheckAllStatus()
    {
        Debug.Log("🔍 SpinLegendsFixManager: Verificando estado de todas las soluciones...");
        
        // Verificar Joystick Fix
        if (joystickFix != null)
        {
            Debug.Log("🎮 Joystick Fix: Activo");
            joystickFix.CheckStatus();
        }
        else
        {
            Debug.Log("❌ Joystick Fix: No encontrado");
        }
        
        // Verificar Player Sync Fix
        if (playerSyncFix != null)
        {
            Debug.Log("🔄 Player Sync Fix: Activo");
            playerSyncFix.CheckPlayerSyncStatus();
        }
        else
        {
            Debug.Log("❌ Player Sync Fix: No encontrado");
        }
        
        // Verificar UI Recovery Fix
        if (uiRecoveryFix != null)
        {
            Debug.Log("🖥️ UI Recovery Fix: Activo");
            uiRecoveryFix.CheckUIStatus();
        }
        else
        {
            Debug.Log("❌ UI Recovery Fix: No encontrado");
        }
    }
    
    [ContextMenu("Clear All States")]
    public void ClearAllStates()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Limpiando estados de todas las soluciones...");
        
        // Limpiar Joystick Fix
        if (joystickFix != null)
        {
            // No hay método de limpieza específico para joystick fix
        }
        
        // Limpiar Player Sync Fix
        if (playerSyncFix != null)
        {
            playerSyncFix.ClearSyncState();
        }
        
        // Limpiar UI Recovery Fix
        if (uiRecoveryFix != null)
        {
            uiRecoveryFix.ResetRecoveryState();
        }
    }
    
    [ContextMenu("Refresh All Components")]
    public void RefreshAllComponents()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Refrescando todos los componentes...");
        
        // Refrescar Joystick Fix
        if (joystickFix != null)
        {
            joystickFix.SetupQuickFix();
        }
        
        // Refrescar Player Sync Fix
        if (playerSyncFix != null)
        {
            playerSyncFix.SetupPlayerSyncFix();
        }
        
        // Refrescar UI Recovery Fix
        if (uiRecoveryFix != null)
        {
            uiRecoveryFix.RefreshUIComponents();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && enableAllFixes)
        {
            if (enableDebugLogs)
                Debug.Log("🔧 SpinLegendsFixManager: App recuperó foco - verificando todas las soluciones...");
            
            // Verificar todas las soluciones después de un pequeño delay
            Invoke(nameof(CheckAndFixAll), 0.5f);
        }
    }
    
    void CheckAndFixAll()
    {
        // Forzar todas las soluciones si es necesario
        ForceAllFixes();
    }
    
    // Métodos específicos para cada fix
    [ContextMenu("Force Joystick Fix Only")]
    public void ForceJoystickFixOnly()
    {
        if (joystickFix != null)
        {
            joystickFix.SetupQuickFix();
            if (enableDebugLogs)
                Debug.Log("🎮 Joystick Fix forzado");
        }
    }
    
    [ContextMenu("Force Player Sync Fix Only")]
    public void ForcePlayerSyncFixOnly()
    {
        if (playerSyncFix != null)
        {
            playerSyncFix.ForceSyncAllPlayers();
            if (enableDebugLogs)
                Debug.Log("🔄 Player Sync Fix forzado");
        }
    }
    
    [ContextMenu("Force UI Recovery Fix Only")]
    public void ForceUIRecoveryFixOnly()
    {
        if (uiRecoveryFix != null)
        {
            uiRecoveryFix.ForceUIRecovery();
            if (enableDebugLogs)
                Debug.Log("🖥️ UI Recovery Fix forzado");
        }
    }
}
