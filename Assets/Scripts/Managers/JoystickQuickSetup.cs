using UnityEngine;

/// <summary>
/// Script simple para configurar rápidamente el sistema de joystick.
/// Simplemente agrega este script a cualquier GameObject en la escena.
/// </summary>
public class JoystickQuickSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    public bool enableAutoSetup = true;
    public bool enableDebugLogs = true;
    
    void Awake()
    {
        if (enableAutoSetup)
        {
            // Crear AutoJoystickInitializer si no existe
            if (FindFirstObjectByType<AutoJoystickInitializer>() == null)
            {
                if (enableDebugLogs)
                    Debug.Log("🎮 Creando AutoJoystickInitializer...");
                
                GameObject autoInitObj = new GameObject("AutoJoystickInitializer");
                AutoJoystickInitializer autoInit = autoInitObj.AddComponent<AutoJoystickInitializer>();
                
                // Configurar para inicialización rápida
                autoInit.enableAutoInit = true;
                autoInit.initDelay = 0.1f;
                autoInit.createQuickFixIfMissing = true;
                autoInit.forceImmediateSetup = true;
                autoInit.enableDebugLogs = enableDebugLogs;
                
                if (enableDebugLogs)
                    Debug.Log("✅ AutoJoystickInitializer creado y configurado");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("✅ AutoJoystickInitializer ya existe");
            }
        }
    }
    
    // Método público para forzar configuración manual
    [ContextMenu("Force Setup")]
    public void ForceSetup()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Forzando configuración manual...");
        
        // Destruir este objeto después de la configuración
        Destroy(gameObject);
    }
}
