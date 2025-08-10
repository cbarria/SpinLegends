using UnityEngine;
using System.Collections;

/// <summary>
/// Inicializador automático del joystick que se ejecuta al inicio de la escena.
/// Asegura que el QuickJoystickFix esté configurado y funcionando.
/// </summary>
public class AutoJoystickInitializer : MonoBehaviour
{
    [Header("Auto Initialization Settings")]
    public bool enableAutoInit = true;
    public float initDelay = 0.5f;
    public bool enableDebugLogs = true;
    
    [Header("Quick Fix Settings")]
    public bool createQuickFixIfMissing = true;
    public bool forceImmediateSetup = true;
    
    private QuickJoystickFix quickFix;
    private bool isInitialized = false;
    
    void Awake()
    {
        // Hacer que este objeto persista entre escenas
        DontDestroyOnLoad(gameObject);
        
        if (enableDebugLogs)
            Debug.Log("🎮 AutoJoystickInitializer iniciado");
    }
    
    void Start()
    {
        if (enableAutoInit)
        {
            // Ejecutar inicialización después de un delay
            Invoke(nameof(InitializeJoystickSystem), initDelay);
        }
    }
    
    void InitializeJoystickSystem()
    {
        if (isInitialized)
        {
            if (enableDebugLogs)
                Debug.Log("✅ Joystick system already initialized");
            return;
        }
        
        if (enableDebugLogs)
            Debug.Log("🔧 Iniciando sistema de joystick automático...");
        
        // Buscar o crear QuickJoystickFix
        SetupQuickJoystickFix();
        
        // Forzar configuración inmediata si está habilitado
        if (forceImmediateSetup && quickFix != null)
        {
            if (enableDebugLogs)
                Debug.Log("🚀 Forzando configuración inmediata...");
            
            quickFix.SetupQuickFix();
        }
        
        isInitialized = true;
        
        if (enableDebugLogs)
            Debug.Log("✅ Sistema de joystick automático inicializado");
    }
    
    void SetupQuickJoystickFix()
    {
        // Buscar si ya existe un QuickJoystickFix
        quickFix = FindFirstObjectByType<QuickJoystickFix>();
        
        if (quickFix == null && createQuickFixIfMissing)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 Creando QuickJoystickFix automáticamente...");
            
            // Crear nuevo QuickJoystickFix
            GameObject quickFixObj = new GameObject("QuickJoystickFix");
            quickFix = quickFixObj.AddComponent<QuickJoystickFix>();
            
            // Configurar el QuickJoystickFix
            quickFix.enableQuickFix = true;
            quickFix.setupDelay = 0.1f; // Configuración rápida
            quickFix.forceJoystickFocus = true;
            quickFix.enableDebugLogs = enableDebugLogs;
            quickFix.maxRetryAttempts = 10; // Más intentos
            
            if (enableDebugLogs)
                Debug.Log("✅ QuickJoystickFix creado y configurado");
        }
        else if (quickFix != null)
        {
            if (enableDebugLogs)
                Debug.Log("✅ QuickJoystickFix ya existe");
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && isInitialized)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 App recuperó foco - verificando joystick...");
            
            // Verificar estado del joystick cuando la app recupera el foco
            Invoke(nameof(CheckJoystickStatus), 0.5f);
        }
    }
    
    void CheckJoystickStatus()
    {
        if (quickFix != null)
        {
            quickFix.CheckStatus();
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ QuickJoystickFix no encontrado, reinicializando...");
            
            isInitialized = false;
            InitializeJoystickSystem();
        }
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Force Initialize")]
    public void ForceInitialize()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Forzando inicialización manual...");
        
        isInitialized = false;
        InitializeJoystickSystem();
    }
    
    [ContextMenu("Check Status")]
    public void CheckStatus()
    {
        Debug.Log($"🔍 AutoJoystickInitializer Status:");
        Debug.Log($"   Initialized: {isInitialized}");
        Debug.Log($"   Auto Init: {enableAutoInit}");
        Debug.Log($"   Create Quick Fix: {createQuickFixIfMissing}");
        Debug.Log($"   Force Setup: {forceImmediateSetup}");
        Debug.Log($"   Quick Fix: {(quickFix != null ? "✅" : "❌")}");
        
        if (quickFix != null)
        {
            quickFix.CheckStatus();
        }
    }
    
    [ContextMenu("Reinitialize All")]
    public void ReinitializeAll()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Re-inicializando todo el sistema...");
        
        if (quickFix != null)
        {
            quickFix.ReinitializeAll();
        }
        
        isInitialized = false;
        Invoke(nameof(InitializeJoystickSystem), 0.5f);
    }
}
