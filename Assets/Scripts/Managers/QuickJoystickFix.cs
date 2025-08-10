using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Solución rápida para el problema de foco del joystick en Android.
/// Se ejecuta automáticamente al inicio y maneja la inicialización del joystick.
/// </summary>
public class QuickJoystickFix : MonoBehaviour
{
    [Header("Quick Fix Settings")]
    public bool enableQuickFix = true;
    public float setupDelay = 1f;
    public bool enableDebugLogs = true;
    
    [Header("Joystick Settings")]
    public bool forceJoystickFocus = true;
    public float joystickInitializationDelay = 0.5f;
    public int maxRetryAttempts = 5;
    
    private JoystickFocusManager joystickManager;
    private AndroidTouchInput touchInput;
    private bool isSetupComplete = false;
    private int retryCount = 0;
    
    void Awake()
    {
        // Hacer que este objeto persista entre escenas
        DontDestroyOnLoad(gameObject);
        
        if (enableDebugLogs)
            Debug.Log("🎮 QuickJoystickFix inicializado");
    }
    
    void Start()
    {
        if (enableQuickFix)
        {
            // Ejecutar la configuración después de un delay
            Invoke(nameof(PerformQuickFixSetup), setupDelay);
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && isSetupComplete)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 App recuperó foco - verificando joystick...");
            
            // Verificar y re-inicializar si es necesario
            Invoke(nameof(CheckAndFixJoystick), 1f); // Aumentado el delay para dar tiempo al Canvas
        }
    }
    
    void PerformQuickFixSetup()
    {
        if (enableDebugLogs)
            Debug.Log("🔧 Iniciando QuickJoystickFix setup...");
        
        // Crear y configurar JoystickFocusManager
        SetupJoystickFocusManager();
        
        // Crear y configurar AndroidTouchInput
        SetupAndroidTouchInput();
        
        // Forzar inicialización inmediata
        ForceImmediateJoystickInitialization();
        
        isSetupComplete = true;
        
        if (enableDebugLogs)
            Debug.Log("✅ QuickJoystickFix setup completado");
    }
    
    void SetupJoystickFocusManager()
    {
        // Buscar si ya existe un JoystickFocusManager
        joystickManager = FindFirstObjectByType<JoystickFocusManager>();
        
        if (joystickManager == null)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 Creando JoystickFocusManager...");
            
            // Crear nuevo JoystickFocusManager
            GameObject managerObj = new GameObject("JoystickFocusManager");
            joystickManager = managerObj.AddComponent<JoystickFocusManager>();
            
            // Configurar el manager
            joystickManager.autoFindJoysticks = true;
            joystickManager.forceJoystickFocus = forceJoystickFocus;
            joystickManager.initializationDelay = joystickInitializationDelay;
            joystickManager.enableDebugLogs = enableDebugLogs;
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("✅ JoystickFocusManager ya existe");
        }
    }
    
    void SetupAndroidTouchInput()
    {
        // Buscar si ya existe un AndroidTouchInput
        touchInput = FindFirstObjectByType<AndroidTouchInput>();
        
        if (touchInput == null)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 Creando AndroidTouchInput...");
            
            // Crear nuevo AndroidTouchInput
            GameObject touchObj = new GameObject("AndroidTouchInput");
            touchInput = touchObj.AddComponent<AndroidTouchInput>();
            
            // Configurar el touch input
            touchInput.enableTouchDebug = enableDebugLogs;
            touchInput.forceTouchFocus = true;
            touchInput.autoCreateEventSystem = true;
            touchInput.ensureGraphicRaycaster = true;
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("✅ AndroidTouchInput ya existe");
        }
    }
    
    void ForceImmediateJoystickInitialization()
    {
        if (enableDebugLogs)
            Debug.Log("🚀 Forzando inicialización inmediata del joystick...");
        
        // Buscar joysticks inmediatamente
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        
        if (joysticks.Length > 0)
        {
            if (enableDebugLogs)
                Debug.Log($"🎮 Encontrados {joysticks.Length} joysticks, configurando...");
            
            // Configurar el JoystickFocusManager con los joysticks encontrados
            if (joystickManager != null)
            {
                joystickManager.joysticks = joysticks;
                joystickManager.Reinitialize();
            }
            
            // Forzar foco en el primer joystick
            if (forceJoystickFocus && joysticks[0] != null)
            {
                StartCoroutine(ForceJoystickFocusWithDelay(joysticks[0]));
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ No se encontraron joysticks, reintentando...");
            
            // Reintentar si no se encontraron joysticks
            if (retryCount < maxRetryAttempts)
            {
                retryCount++;
                Invoke(nameof(ForceImmediateJoystickInitialization), 1f);
            }
        }
    }
    
    IEnumerator ForceJoystickFocusWithDelay(Joystick joystick)
    {
        // Esperar un frame para asegurar que todo esté listo
        yield return new WaitForEndOfFrame();
        
        if (enableDebugLogs)
            Debug.Log($"🎯 Forzando foco en joystick: {joystick.name}");
        
        // Simular un toque en el joystick para activarlo
        SimulateJoystickTouch(joystick);
        
        // Verificar que el joystick esté funcionando
        yield return new WaitForSeconds(0.5f);
        VerifyJoystickFunctionality(joystick);
    }
    
    void SimulateJoystickTouch(Joystick joystick)
    {
        if (joystick == null) return;
        
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
        StartCoroutine(SimulatePointerUp(joystick, eventData));
    }
    
    IEnumerator SimulatePointerUp(Joystick joystick, PointerEventData eventData)
    {
        yield return new WaitForSeconds(0.05f);
        ExecuteEvents.Execute(joystick.gameObject, eventData, ExecuteEvents.pointerUpHandler);
    }
    
    void VerifyJoystickFunctionality(Joystick joystick)
    {
        if (joystick == null) return;
        
        // Verificar que el joystick esté activo y configurado
        bool isActive = joystick.gameObject.activeInHierarchy;
        bool hasCanvas = joystick.GetComponentInParent<Canvas>() != null;
        bool hasEventSystem = EventSystem.current != null;
        
        if (enableDebugLogs)
        {
            Debug.Log($"🔍 Verificación del joystick {joystick.name}:");
            Debug.Log($"   Activo: {isActive}");
            Debug.Log($"   Tiene Canvas: {hasCanvas}");
            Debug.Log($"   Tiene EventSystem: {hasEventSystem}");
            Debug.Log($"   Input actual - H: {joystick.Horizontal:F2}, V: {joystick.Vertical:F2}");
        }
        
        if (!isActive || !hasCanvas || !hasEventSystem)
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ Joystick no está completamente configurado, reintentando...");
            
            // Reintentar configuración
            Invoke(nameof(ForceImmediateJoystickInitialization), 1f);
        }
    }
    
    void CheckAndFixJoystick()
    {
        if (enableDebugLogs)
            Debug.Log("🔍 Verificando estado del joystick después de recuperar foco...");
        
        // Buscar joysticks activos
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        
        if (joysticks.Length > 0)
        {
            bool anyJoystickActive = false;
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null && joystick.gameObject.activeInHierarchy)
                {
                    anyJoystickActive = true;
                    if (enableDebugLogs)
                        Debug.Log($"✅ Joystick '{joystick.name}' está activo");
                }
            }
            
            if (!anyJoystickActive)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("⚠️ Ningún joystick está activo, reactivando...");
                
                // Reactivar todos los joysticks
                foreach (Joystick joystick in joysticks)
                {
                    if (joystick != null)
                    {
                        joystick.gameObject.SetActive(true);
                        if (enableDebugLogs)
                            Debug.Log($"🔄 Reactivado joystick: {joystick.name}");
                    }
                }
                
                // Forzar re-inicialización
                if (joystickManager != null)
                {
                    joystickManager.Reinitialize();
                }
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ No se encontraron joysticks, forzando re-inicialización...");
            
            // Forzar re-inicialización completa
            ForceImmediateJoystickInitialization();
        }
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Setup Quick Fix")]
    public void SetupQuickFix()
    {
        PerformQuickFixSetup();
    }
    
    [ContextMenu("Force Reinitialize All")]
    public void ReinitializeAll()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Forzando re-inicialización completa...");
        
        retryCount = 0;
        isSetupComplete = false;
        PerformQuickFixSetup();
    }
    
    [ContextMenu("Check Status")]
    public void CheckStatus()
    {
        Debug.Log($"🔍 QuickJoystickFix Status:");
        Debug.Log($"   Setup Complete: {isSetupComplete}");
        Debug.Log($"   Retry Count: {retryCount}/{maxRetryAttempts}");
        Debug.Log($"   Joystick Manager: {(joystickManager != null ? "✅" : "❌")}");
        Debug.Log($"   Touch Input: {(touchInput != null ? "✅" : "❌")}");
        
        if (joystickManager != null)
        {
            joystickManager.CheckJoystickStatus();
        }
    }
}
