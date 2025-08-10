using UnityEngine;

/// <summary>
/// Script rápido para solucionar el problema de UI que desaparece al ir al home y volver en Android.
/// El joystick y la GUI desaparecen cuando la app pierde el foco y lo recupera.
/// Simplemente agrega este script a cualquier GameObject en la escena y se configurará automáticamente.
/// </summary>
public class QuickUIRecoveryFix : MonoBehaviour
{
    [Header("Quick Fix Settings")]
    public bool enableAutoFix = true;
    public bool enableDebugLogs = true;
    public float recoveryDelay = 0.5f;
    public int maxRecoveryAttempts = 3;
    
    [Header("Auto Find Settings")]
    public bool autoFindUIComponents = true;
    public bool autoFindJoysticks = true;
    public bool autoFindButtons = true;
    public bool autoFindSliders = true;
    
    [Header("Components")]
    public UIRecoveryManager uiRecoveryManager;
    
    void Awake()
    {
        if (enableAutoFix)
        {
            // Configurar para que este objeto persista
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLogs)
                Debug.Log("🖥️ QuickUIRecoveryFix: Iniciando configuración automática...");
        }
    }
    
    void Start()
    {
        if (enableAutoFix)
        {
            // Ejecutar la configuración después de un pequeño delay
            Invoke(nameof(PerformQuickUISetup), 1f);
        }
    }
    
    void PerformQuickUISetup()
    {
        if (enableDebugLogs)
            Debug.Log("🖥️ QuickUIRecoveryFix: Configurando recuperación de UI...");
        
        // Crear UIRecoveryManager si no existe
        if (uiRecoveryManager == null)
        {
            uiRecoveryManager = FindFirstObjectByType<UIRecoveryManager>();
            if (uiRecoveryManager == null)
            {
                GameObject uiManagerObj = new GameObject("UIRecoveryManager");
                uiRecoveryManager = uiManagerObj.AddComponent<UIRecoveryManager>();
                
                // Configurar el UIRecoveryManager
                uiRecoveryManager.enableAutoRecovery = true;
                uiRecoveryManager.enableDebugLogs = enableDebugLogs;
                uiRecoveryManager.recoveryDelay = recoveryDelay;
                uiRecoveryManager.maxRecoveryAttempts = maxRecoveryAttempts;
                uiRecoveryManager.autoFindUIComponents = autoFindUIComponents;
                uiRecoveryManager.autoFindJoysticks = autoFindJoysticks;
                uiRecoveryManager.autoFindButtons = autoFindButtons;
                uiRecoveryManager.autoFindSliders = autoFindSliders;
                
                if (enableDebugLogs)
                    Debug.Log("✅ QuickUIRecoveryFix: UIRecoveryManager creado");
            }
        }
        
        if (enableDebugLogs)
            Debug.Log("✅ QuickUIRecoveryFix: Configuración completada");
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Setup UI Recovery Fix")]
    public void SetupUIRecoveryFix()
    {
        PerformQuickUISetup();
    }
    
    [ContextMenu("Force UI Recovery")]
    public void ForceUIRecovery()
    {
        if (uiRecoveryManager != null)
        {
            uiRecoveryManager.ForceUIRecovery();
        }
        else
        {
            Debug.LogWarning("⚠️ UIRecoveryManager no encontrado. Ejecuta 'Setup UI Recovery Fix' primero.");
        }
    }
    
    [ContextMenu("Check UI Status")]
    public void CheckUIStatus()
    {
        Debug.Log("🔍 QuickUIRecoveryFix: Verificando estado de UI...");
        
        if (uiRecoveryManager != null)
        {
            Debug.Log("✅ UIRecoveryManager: Activo");
            uiRecoveryManager.CheckUIStatus();
        }
        else
        {
            Debug.Log("❌ UIRecoveryManager: No encontrado");
        }
    }
    
    [ContextMenu("Refresh UI Components")]
    public void RefreshUIComponents()
    {
        if (uiRecoveryManager != null)
        {
            uiRecoveryManager.RefreshUIComponents();
        }
        else
        {
            Debug.LogWarning("⚠️ UIRecoveryManager no encontrado");
        }
    }
    
    [ContextMenu("Reset Recovery State")]
    public void ResetRecoveryState()
    {
        if (uiRecoveryManager != null)
        {
            uiRecoveryManager.ResetRecoveryState();
        }
        else
        {
            Debug.LogWarning("⚠️ UIRecoveryManager no encontrado");
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && enableAutoFix)
        {
            if (enableDebugLogs)
                Debug.Log("🖥️ QuickUIRecoveryFix: App recuperó foco - verificando UI...");
            
            // Verificar UI después de un pequeño delay
            Invoke(nameof(CheckAndRecoverUI), 0.5f);
        }
    }
    
    void CheckAndRecoverUI()
    {
        if (uiRecoveryManager != null)
        {
            // Forzar recuperación de UI si es necesario
            uiRecoveryManager.ForceUIRecovery();
        }
    }
}
