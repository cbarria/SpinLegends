using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Solución específica para el problema del joystick que desaparece al volver de la app.
/// Se ejecuta solo cuando la app recupera el foco.
/// </summary>
public class JoystickRecoveryFix : MonoBehaviour
{
    [Header("Recovery Settings")]
    public bool enableRecovery = true;
    public bool enableDebugLogs = true;
    public float recoveryDelay = 1f;
    
    private bool hasRecovered = false;
    
    void Awake()
    {
        // Configurar para que este objeto persista
        DontDestroyOnLoad(gameObject);
        
        if (enableDebugLogs)
            Debug.Log("🎮 JoystickRecoveryFix: Inicializado");
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && enableRecovery && !hasRecovered)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 JoystickRecoveryFix: App recuperó foco - iniciando recuperación...");
            
            // Ejecutar recuperación después de un delay
            Invoke(nameof(RecoverJoysticks), recoveryDelay);
        }
    }
    
    void RecoverJoysticks()
    {
        if (hasRecovered)
        {
            if (enableDebugLogs)
                Debug.Log("ℹ️ JoystickRecoveryFix: Ya se recuperó anteriormente");
            return;
        }
        
        if (enableDebugLogs)
            Debug.Log("🔧 JoystickRecoveryFix: Iniciando recuperación de joysticks...");
        
        // Buscar todos los joysticks
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        
        if (joysticks.Length > 0)
        {
            bool recoveredAny = false;
            
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null)
                {
                    // Verificar si el joystick está desactivado o no funciona
                    if (!joystick.gameObject.activeInHierarchy)
                    {
                        joystick.gameObject.SetActive(true);
                        if (enableDebugLogs)
                            Debug.Log($"🔄 JoystickRecoveryFix: Reactivado joystick '{joystick.name}'");
                        recoveredAny = true;
                    }
                    
                    // Verificar si el joystick necesita re-inicialización
                    if (joystick.Horizontal == 0 && joystick.Vertical == 0)
                    {
                        // Simular un toque para activar el joystick
                        StartCoroutine(SimulateJoystickTouch(joystick));
                        recoveredAny = true;
                    }
                }
            }
            
            if (recoveredAny)
            {
                hasRecovered = true;
                if (enableDebugLogs)
                    Debug.Log("✅ JoystickRecoveryFix: Recuperación completada");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("ℹ️ JoystickRecoveryFix: Los joysticks ya están funcionando correctamente");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ JoystickRecoveryFix: No se encontraron joysticks");
        }
    }
    
    IEnumerator SimulateJoystickTouch(Joystick joystick)
    {
        if (joystick == null) yield break;
        
        // Asegurar que tenemos un EventSystem
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
        
        // Simular un toque en el centro del joystick
        Vector2 joystickCenter = joystick.GetComponent<RectTransform>().position;
        
        // Crear evento de pointer down
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = joystickCenter;
        eventData.button = PointerEventData.InputButton.Left;
        
        // Ejecutar eventos
        ExecuteEvents.Execute(joystick.gameObject, eventData, ExecuteEvents.pointerDownHandler);
        
        // Pequeño delay
        yield return new WaitForSeconds(0.05f);
        
        // Ejecutar pointer up
        ExecuteEvents.Execute(joystick.gameObject, eventData, ExecuteEvents.pointerUpHandler);
        
        if (enableDebugLogs)
            Debug.Log($"🎯 JoystickRecoveryFix: Simulado toque en joystick '{joystick.name}'");
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Force Recovery")]
    public void ForceRecovery()
    {
        hasRecovered = false;
        RecoverJoysticks();
    }
    
    [ContextMenu("Check Joystick Status")]
    public void CheckJoystickStatus()
    {
        Debug.Log("🔍 JoystickRecoveryFix: Verificando estado de joysticks...");
        
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        Debug.Log($"   Joysticks encontrados: {joysticks.Length}");
        
        foreach (Joystick joystick in joysticks)
        {
            if (joystick != null)
            {
                Debug.Log($"   Joystick '{joystick.name}':");
                Debug.Log($"     Activo: {joystick.gameObject.activeInHierarchy}");
                Debug.Log($"     Input: H={joystick.Horizontal:F2}, V={joystick.Vertical:F2}");
                Debug.Log($"     Tiene Canvas: {joystick.GetComponentInParent<Canvas>() != null}");
                Debug.Log($"     Tiene EventSystem: {EventSystem.current != null}");
            }
        }
    }
    
    [ContextMenu("Reset Recovery State")]
    public void ResetRecoveryState()
    {
        hasRecovered = false;
        if (enableDebugLogs)
            Debug.Log("🔄 JoystickRecoveryFix: Estado de recuperación reseteado");
    }
}
