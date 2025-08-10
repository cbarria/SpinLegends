using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Maneja la recuperación automática de UI y joystick cuando la app regresa del background en Android.
/// Soluciona el problema donde el joystick y la UI desaparecen al ir al home y volver.
/// </summary>
public class UIRecoveryManager : MonoBehaviour
{
    [Header("UI Recovery Settings")]
    public bool enableAutoRecovery = true;
    public bool enableDebugLogs = true;
    public float recoveryDelay = 0.5f;
    public int maxRecoveryAttempts = 3;
    
    [Header("UI Components")]
    public Canvas[] uiCanvases;
    public Joystick[] joysticks;
    public Button[] uiButtons;
    public Slider[] uiSliders;
    
    [Header("Auto Find Settings")]
    public bool autoFindUIComponents = true;
    public bool autoFindJoysticks = true;
    public bool autoFindButtons = true;
    public bool autoFindSliders = true;
    
    private bool isRecovering = false;
    private int recoveryAttempts = 0;
    private Dictionary<GameObject, bool> originalStates = new Dictionary<GameObject, bool>();
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        if (enableDebugLogs)
            Debug.Log("🖥️ UIRecoveryManager iniciado");
        
        // Auto-encontrar componentes si está habilitado
        if (autoFindUIComponents)
        {
            AutoFindUIComponents();
        }
        
        // Guardar estados originales
        SaveOriginalStates();
    }
    
    void AutoFindUIComponents()
    {
        if (enableDebugLogs)
            Debug.Log("🔍 Buscando componentes de UI automáticamente...");
        
        // Buscar Canvas
        if (uiCanvases == null || uiCanvases.Length == 0)
        {
            uiCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (enableDebugLogs)
                Debug.Log($"📱 Encontrados {uiCanvases.Length} Canvas");
        }
        
        // Buscar Joysticks
        if (autoFindJoysticks && (joysticks == null || joysticks.Length == 0))
        {
            List<Joystick> foundJoysticks = new List<Joystick>();
            
            // Buscar diferentes tipos de joysticks
            FixedJoystick[] fixedJoysticks = FindObjectsByType<FixedJoystick>(FindObjectsSortMode.None);
            FloatingJoystick[] floatingJoysticks = FindObjectsByType<FloatingJoystick>(FindObjectsSortMode.None);
            VariableJoystick[] variableJoysticks = FindObjectsByType<VariableJoystick>(FindObjectsSortMode.None);
            DynamicJoystick[] dynamicJoysticks = FindObjectsByType<DynamicJoystick>(FindObjectsSortMode.None);
            
            foundJoysticks.AddRange(fixedJoysticks);
            foundJoysticks.AddRange(floatingJoysticks);
            foundJoysticks.AddRange(variableJoysticks);
            foundJoysticks.AddRange(dynamicJoysticks);
            
            joysticks = foundJoysticks.ToArray();
            
            if (enableDebugLogs)
                Debug.Log($"🎮 Encontrados {joysticks.Length} Joysticks");
        }
        
        // Buscar Buttons
        if (autoFindButtons && (uiButtons == null || uiButtons.Length == 0))
        {
            uiButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            if (enableDebugLogs)
                Debug.Log($"🔘 Encontrados {uiButtons.Length} Buttons");
        }
        
        // Buscar Sliders
        if (autoFindSliders && (uiSliders == null || uiSliders.Length == 0))
        {
            uiSliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
            if (enableDebugLogs)
                Debug.Log($"📊 Encontrados {uiSliders.Length} Sliders");
        }
    }
    
    void SaveOriginalStates()
    {
        originalStates.Clear();
        
        // Guardar estados de Canvas
        if (uiCanvases != null)
        {
            foreach (Canvas canvas in uiCanvases)
            {
                if (canvas != null)
                {
                    originalStates[canvas.gameObject] = canvas.gameObject.activeInHierarchy;
                }
            }
        }
        
        // Guardar estados de Joysticks
        if (joysticks != null)
        {
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null)
                {
                    originalStates[joystick.gameObject] = joystick.gameObject.activeInHierarchy;
                }
            }
        }
        
        // Guardar estados de Buttons
        if (uiButtons != null)
        {
            foreach (Button button in uiButtons)
            {
                if (button != null)
                {
                    originalStates[button.gameObject] = button.gameObject.activeInHierarchy;
                }
            }
        }
        
        // Guardar estados de Sliders
        if (uiSliders != null)
        {
            foreach (Slider slider in uiSliders)
            {
                if (slider != null)
                {
                    originalStates[slider.gameObject] = slider.gameObject.activeInHierarchy;
                }
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"💾 Guardados estados de {originalStates.Count} componentes de UI");
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && enableAutoRecovery)
        {
            if (enableDebugLogs)
                Debug.Log("🖥️ App recuperó foco - iniciando recuperación de UI...");
            
            StartCoroutine(RecoverUIWithDelay());
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && enableAutoRecovery) // pauseStatus = false significa que la app se resumió
        {
            if (enableDebugLogs)
                Debug.Log("🖥️ App se resumió - iniciando recuperación de UI...");
            
            StartCoroutine(RecoverUIWithDelay());
        }
    }
    
    IEnumerator RecoverUIWithDelay()
    {
        if (isRecovering)
        {
            if (enableDebugLogs)
                Debug.Log("⚠️ Recuperación ya en progreso, ignorando...");
            yield break;
        }
        
        isRecovering = true;
        recoveryAttempts = 0;
        
        yield return new WaitForSeconds(recoveryDelay);
        
        while (recoveryAttempts < maxRecoveryAttempts)
        {
            if (enableDebugLogs)
                Debug.Log($"🔄 Intento de recuperación {recoveryAttempts + 1}/{maxRecoveryAttempts}");
            
            bool recoverySuccessful = RecoverUIComponents();
            
            if (recoverySuccessful)
            {
                if (enableDebugLogs)
                    Debug.Log("✅ Recuperación de UI completada exitosamente");
                break;
            }
            else
            {
                recoveryAttempts++;
                if (recoveryAttempts < maxRecoveryAttempts)
                {
                    if (enableDebugLogs)
                        Debug.Log($"⚠️ Recuperación falló, reintentando en {recoveryDelay}s...");
                    yield return new WaitForSeconds(recoveryDelay);
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.LogError("❌ Falló la recuperación después de múltiples intentos");
                }
            }
        }
        
        isRecovering = false;
    }
    
    bool RecoverUIComponents()
    {
        bool allRecovered = true;
        
        // Recuperar Canvas
        if (uiCanvases != null)
        {
            foreach (Canvas canvas in uiCanvases)
            {
                if (canvas != null && !canvas.gameObject.activeInHierarchy)
                {
                    canvas.gameObject.SetActive(true);
                    if (enableDebugLogs)
                        Debug.Log($"✅ Canvas recuperado: {canvas.name}");
                }
            }
        }
        
        // Recuperar Joysticks
        if (joysticks != null)
        {
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null && !joystick.gameObject.activeInHierarchy)
                {
                    joystick.gameObject.SetActive(true);
                    
                    // Forzar re-inicialización del joystick
                    StartCoroutine(ReinitializeJoystick(joystick));
                    
                    if (enableDebugLogs)
                        Debug.Log($"✅ Joystick recuperado: {joystick.name}");
                }
            }
        }
        
        // Recuperar Buttons
        if (uiButtons != null)
        {
            foreach (Button button in uiButtons)
            {
                if (button != null && !button.gameObject.activeInHierarchy)
                {
                    button.gameObject.SetActive(true);
                    if (enableDebugLogs)
                        Debug.Log($"✅ Button recuperado: {button.name}");
                }
            }
        }
        
        // Recuperar Sliders
        if (uiSliders != null)
        {
            foreach (Slider slider in uiSliders)
            {
                if (slider != null && !slider.gameObject.activeInHierarchy)
                {
                    slider.gameObject.SetActive(true);
                    if (enableDebugLogs)
                        Debug.Log($"✅ Slider recuperado: {slider.name}");
                }
            }
        }
        
        // Verificar que todos los componentes estén activos
        allRecovered = VerifyUIComponentsActive();
        
        return allRecovered;
    }
    
    IEnumerator ReinitializeJoystick(Joystick joystick)
    {
        // Pequeño delay para asegurar que el joystick esté completamente activo
        yield return new WaitForSeconds(0.1f);
        
        // Forzar re-inicialización del joystick
        if (joystick != null)
        {
            // Simular un toque para activar el joystick
            StartCoroutine(ActivateJoystickTouch(joystick));
        }
    }
    
    IEnumerator ActivateJoystickTouch(Joystick joystick)
    {
        if (joystick == null) yield break;
        
        // Obtener la posición del joystick en pantalla
        Vector2 joystickPosition = joystick.GetComponent<RectTransform>().position;
        
        // Crear evento de pointer down
        UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        pointerData.position = joystickPosition;
        pointerData.button = UnityEngine.EventSystems.PointerEventData.InputButton.Left;
        
        // Ejecutar eventos de pointer
        UnityEngine.EventSystems.ExecuteEvents.Execute(joystick.gameObject, pointerData, UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
        UnityEngine.EventSystems.ExecuteEvents.Execute(joystick.gameObject, pointerData, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
        
        // Pequeño delay
        yield return new WaitForSeconds(0.05f);
        
        // Ejecutar eventos de pointer up
        UnityEngine.EventSystems.ExecuteEvents.Execute(joystick.gameObject, pointerData, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
        UnityEngine.EventSystems.ExecuteEvents.Execute(joystick.gameObject, pointerData, UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);
        
        if (enableDebugLogs)
            Debug.Log($"🎯 Joystick re-inicializado: {joystick.name}");
    }
    
    bool VerifyUIComponentsActive()
    {
        int inactiveComponents = 0;
        
        // Verificar Canvas
        if (uiCanvases != null)
        {
            foreach (Canvas canvas in uiCanvases)
            {
                if (canvas != null && !canvas.gameObject.activeInHierarchy)
                {
                    inactiveComponents++;
                }
            }
        }
        
        // Verificar Joysticks
        if (joysticks != null)
        {
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null && !joystick.gameObject.activeInHierarchy)
                {
                    inactiveComponents++;
                }
            }
        }
        
        // Verificar Buttons
        if (uiButtons != null)
        {
            foreach (Button button in uiButtons)
            {
                if (button != null && !button.gameObject.activeInHierarchy)
                {
                    inactiveComponents++;
                }
            }
        }
        
        // Verificar Sliders
        if (uiSliders != null)
        {
            foreach (Slider slider in uiSliders)
            {
                if (slider != null && !slider.gameObject.activeInHierarchy)
                {
                    inactiveComponents++;
                }
            }
        }
        
        if (inactiveComponents > 0)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"⚠️ {inactiveComponents} componentes de UI aún inactivos");
            return false;
        }
        
        return true;
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Force UI Recovery")]
    public void ForceUIRecovery()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Forzando recuperación de UI...");
        
        StartCoroutine(RecoverUIWithDelay());
    }
    
    [ContextMenu("Refresh UI Components")]
    public void RefreshUIComponents()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Refrescando componentes de UI...");
        
        AutoFindUIComponents();
        SaveOriginalStates();
    }
    
    [ContextMenu("Check UI Status")]
    public void CheckUIStatus()
    {
        Debug.Log("🔍 Estado de componentes de UI:");
        
        int totalComponents = 0;
        int activeComponents = 0;
        
        // Canvas
        if (uiCanvases != null)
        {
            foreach (Canvas canvas in uiCanvases)
            {
                if (canvas != null)
                {
                    totalComponents++;
                    if (canvas.gameObject.activeInHierarchy)
                        activeComponents++;
                    Debug.Log($"   Canvas '{canvas.name}': {(canvas.gameObject.activeInHierarchy ? "✅" : "❌")}");
                }
            }
        }
        
        // Joysticks
        if (joysticks != null)
        {
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null)
                {
                    totalComponents++;
                    if (joystick.gameObject.activeInHierarchy)
                        activeComponents++;
                    Debug.Log($"   Joystick '{joystick.name}': {(joystick.gameObject.activeInHierarchy ? "✅" : "❌")}");
                }
            }
        }
        
        // Buttons
        if (uiButtons != null)
        {
            foreach (Button button in uiButtons)
            {
                if (button != null)
                {
                    totalComponents++;
                    if (button.gameObject.activeInHierarchy)
                        activeComponents++;
                    Debug.Log($"   Button '{button.name}': {(button.gameObject.activeInHierarchy ? "✅" : "❌")}");
                }
            }
        }
        
        // Sliders
        if (uiSliders != null)
        {
            foreach (Slider slider in uiSliders)
            {
                if (slider != null)
                {
                    totalComponents++;
                    if (slider.gameObject.activeInHierarchy)
                        activeComponents++;
                    Debug.Log($"   Slider '{slider.name}': {(slider.gameObject.activeInHierarchy ? "✅" : "❌")}");
                }
            }
        }
        
        Debug.Log($"📊 Resumen: {activeComponents}/{totalComponents} componentes activos");
    }
    
    [ContextMenu("Reset Recovery State")]
    public void ResetRecoveryState()
    {
        isRecovering = false;
        recoveryAttempts = 0;
        
        if (enableDebugLogs)
            Debug.Log("🔄 Estado de recuperación reseteado");
    }
}
