using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class AndroidTouchInput : MonoBehaviour
{
    [Header("Android Touch Settings")]
    public bool enableTouchDebug = true;
    public float touchSensitivity = 1f;
    public bool forceTouchFocus = true;
    
    [Header("Event System Settings")]
    public bool autoCreateEventSystem = true;
    public bool ensureGraphicRaycaster = true;
    
    private EventSystem eventSystem;
    private bool isInitialized = false;
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        #if UNITY_ANDROID
        StartCoroutine(InitializeWithDelay());
        #endif
    }
    
    IEnumerator InitializeWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        InitializeTouchInput();
    }
    
    void InitializeTouchInput()
    {
        #if UNITY_ANDROID
        if (enableTouchDebug) Debug.Log("📱 Inicializando input táctil para Android...");
        SetupEventSystem();
        SetupGraphicRaycasters();
        if (forceTouchFocus)
        {
            StartCoroutine(ForceTouchFocus());
        }
        isInitialized = true;
        if (enableTouchDebug) Debug.Log("✅ Input táctil inicializado correctamente");
        #endif
    }
    
    void SetupEventSystem()
    {
        eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null && autoCreateEventSystem)
        {
            if (enableTouchDebug) Debug.Log("📱 Creando EventSystem automáticamente...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            var inputModule = eventSystemObj.AddComponent<StandaloneInputModule>();
            inputModule.horizontalAxis = "Horizontal";
            inputModule.verticalAxis = "Vertical";
            inputModule.submitButton = "Submit";
            inputModule.cancelButton = "Cancel";
            inputModule.inputActionsPerSecond = 10;
            inputModule.repeatDelay = 0.5f;
            if (enableTouchDebug) Debug.Log("✅ EventSystem creado y configurado");
        }
        else if (eventSystem != null && enableTouchDebug)
        {
            Debug.Log("✅ EventSystem encontrado: " + eventSystem.name);
        }
    }
    
    void SetupGraphicRaycasters()
    {
        if (!ensureGraphicRaycaster) return;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null) continue;
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                if (enableTouchDebug) Debug.Log($"📱 Agregando GraphicRaycaster a Canvas: {canvas.name}");
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            // No asignar worldCamera automáticamente para evitar efectos de zoom/estirado
            // if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null && Camera.main != null)
            // {
            //     canvas.worldCamera = Camera.main;
            // }
        }
        if (enableTouchDebug) Debug.Log($"✅ Verificados {allCanvases.Length} Canvas con GraphicRaycaster");
    }
    
    IEnumerator ForceTouchFocus()
    {
        yield return new WaitForEndOfFrame();
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        foreach (Joystick joystick in joysticks)
        {
            if (joystick != null)
            {
                StartCoroutine(ActivateJoystickTouch(joystick));
            }
        }
        if (enableTouchDebug) Debug.Log($"🎮 Activado foco táctil en {joysticks.Length} joysticks");
    }
    
    IEnumerator ActivateJoystickTouch(Joystick joystick)
    {
        if (joystick == null || eventSystem == null) yield break;
        Vector2 joystickPosition = joystick.GetComponent<RectTransform>().position;
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = joystickPosition,
            button = PointerEventData.InputButton.Left
        };
        ExecuteEvents.Execute(joystick.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(joystick.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
        yield return new WaitForSeconds(0.05f);
        ExecuteEvents.Execute(joystick.gameObject, pointerData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(joystick.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
        if (enableTouchDebug) Debug.Log($"🎯 Activado foco táctil en joystick: {joystick.name}");
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        #if UNITY_ANDROID
        if (hasFocus && isInitialized)
        {
            if (enableTouchDebug) Debug.Log("📱 App recuperó foco - re-inicializando input táctil");
            StartCoroutine(ReinitializeTouchInput());
        }
        #endif
    }
    
    IEnumerator ReinitializeTouchInput()
    {
        yield return new WaitForSeconds(0.1f);
        SetupEventSystem();
        SetupGraphicRaycasters();
        if (forceTouchFocus)
        {
            StartCoroutine(ForceTouchFocus());
        }
        if (enableTouchDebug) Debug.Log("✅ Input táctil re-inicializado después de recuperar foco");
    }
    
    void Update()
    {
        #if UNITY_ANDROID
        if (enableTouchDebug && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                Debug.Log($"👆 Touch Input - Phase: {touch.phase}, Position: {touch.position}, Delta: {touch.deltaPosition}");
            }
        }
        #endif
    }
    
    [ContextMenu("Reinitialize Touch Input")]
    public void Reinitialize()
    {
        StartCoroutine(ReinitializeTouchInput());
    }
    
    [ContextMenu("Force Touch Focus")]
    public void ForceFocus()
    {
        StartCoroutine(ForceTouchFocus());
    }
    
    [ContextMenu("Check Touch Status")]
    public void CheckTouchStatus()
    {
        Debug.Log($"📱 Touch Status Check:");
        Debug.Log($"   EventSystem: {(eventSystem != null ? "✅" : "❌")}");
        Debug.Log($"   Touch Count: {Input.touchCount}");
        Debug.Log($"   Touch Supported: {Input.touchSupported}");
        Debug.Log($"   Multi Touch Enabled: {Input.multiTouchEnabled}");
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            Debug.Log($"   Canvas '{canvas.name}': {(raycaster != null ? "✅" : "❌")} GraphicRaycaster, Mode={canvas.renderMode}, Cam={(canvas.worldCamera != null)}");
        }
    }
}
