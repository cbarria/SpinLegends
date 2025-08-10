using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class JoystickFocusManager : MonoBehaviour
{
    [Header("Joystick References")]
    public Joystick[] joysticks;
    
    [Header("Settings")]
    public float initializationDelay = 0.5f;
    public bool enableDebugLogs = true;
    
    [Header("Auto-find Settings")]
    public bool autoFindJoysticks = true;
    public bool forceJoystickFocus = true;
    
    private bool isInitialized = false;
    private bool hasFocus = false;
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        if (autoFindJoysticks)
        {
            FindAllJoysticks();
        }
        StartCoroutine(InitializeJoysticksWithDelay());
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        this.hasFocus = hasFocus;
        if (enableDebugLogs)
            Debug.Log($"🔍 App Focus Changed: {hasFocus}");
        if (hasFocus)
        {
            StartCoroutine(ReinitializeAfterFocus());
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (enableDebugLogs)
            Debug.Log($"⏸️ App Pause: {pauseStatus}");
        if (!pauseStatus)
        {
            StartCoroutine(ReinitializeAfterFocus());
        }
    }
    
    void FindAllJoysticks()
    {
        Joystick[] foundJoysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        if (foundJoysticks.Length > 0)
        {
            joysticks = foundJoysticks;
            if (enableDebugLogs)
                Debug.Log($"🎮 Found {joysticks.Length} joysticks in scene");
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("⚠️ No joysticks found in scene");
        }
    }
    
    IEnumerator InitializeJoysticksWithDelay()
    {
        yield return new WaitForSeconds(initializationDelay);
        InitializeJoysticks();
    }
    
    IEnumerator ReinitializeAfterFocus()
    {
        yield return new WaitForSeconds(0.6f);
        Canvas.ForceUpdateCanvases();
        EnsureEventSystem();
        if (joysticks == null || joysticks.Length == 0)
            FindAllJoysticks();
        if (joysticks != null)
        {
            foreach (var j in joysticks)
            {
                if (j == null) continue;
                var go = j.gameObject;
                var canvas = j.GetComponentInParent<Canvas>();
                if (canvas == null) continue; // no tocar si no hay canvas
                bool wasActive = go.activeSelf;
                go.SetActive(false);
                yield return null;
                go.SetActive(true);
                if (enableDebugLogs)
                    Debug.Log($"🔁 Toggled joystick active: {j.name} (wasActive={wasActive})");
            }
        }
        if (forceJoystickFocus && joysticks != null)
        {
            foreach (var j in joysticks)
            {
                if (j == null) continue;
                yield return StartCoroutine(SimulateJoystickActivation(j));
            }
        }
        isInitialized = true;
        if (enableDebugLogs)
            Debug.Log("✅ Joysticks re-initialized after focus");
    }
    
    void InitializeJoysticks()
    {
        if (isInitialized && enableDebugLogs)
        {
            Debug.Log("🔄 Re-initializing joysticks...");
        }
        if (joysticks == null || joysticks.Length == 0)
        {
            FindAllJoysticks();
        }
        foreach (Joystick joystick in joysticks)
        {
            if (joystick != null)
            {
                InitializeSingleJoystick(joystick);
            }
        }
        if (forceJoystickFocus && joysticks != null && joysticks.Length > 0 && joysticks[0] != null)
        {
            StartCoroutine(SimulateJoystickActivation(joysticks[0]));
        }
        isInitialized = true;
        if (enableDebugLogs)
            Debug.Log("✅ Joysticks initialized successfully");
    }
    
    void InitializeSingleJoystick(Joystick joystick)
    {
        if (joystick == null) return;
        if (!joystick.gameObject.activeInHierarchy)
        {
            joystick.gameObject.SetActive(true);
        }
        Canvas canvas = joystick.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"⚠️ Joystick {joystick.name} no está en un Canvas");
            return;
        }
        EnsureEventSystem();
        var raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        if (enableDebugLogs)
            Debug.Log($"🎮 Initialized joystick: {joystick.name} in canvas: {canvas.name}");
    }
    
    void EnsureEventSystem()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ No EventSystem found, creating one...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }
    
    IEnumerator SimulateJoystickActivation(Joystick joystick)
    {
        if (joystick == null) yield break;
        yield return new WaitForEndOfFrame();
        var rect = joystick.GetComponent<RectTransform>();
        if (rect == null) yield break;
        if (EventSystem.current == null) yield break; // sin EventSystem no simular
        Vector2 joystickCenter = rect.position;
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = joystickCenter,
            button = PointerEventData.InputButton.Left
        };
        ExecuteEvents.Execute(joystick.gameObject, eventData, ExecuteEvents.pointerDownHandler);
        yield return new WaitForSeconds(0.05f);
        ExecuteEvents.Execute(joystick.gameObject, eventData, ExecuteEvents.pointerUpHandler);
        if (enableDebugLogs)
            Debug.Log($"🎯 Forced focus on joystick: {joystick.name}");
    }
    
    public void Reinitialize()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Manual re-initialization requested");
        isInitialized = false;
        StartCoroutine(ReinitializeAfterFocus());
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    public void CheckJoystickStatus()
    {
        Debug.Log($"🔍 JoystickFocusManager Status:");
        Debug.Log($"   Initialized: {isInitialized}");
        Debug.Log($"   Has Focus: {hasFocus}");
        Debug.Log($"   Auto Find: {autoFindJoysticks}");
        Debug.Log($"   Force Focus: {forceJoystickFocus}");
        if (joysticks == null || joysticks.Length == 0)
        {
            Debug.LogWarning("⚠️ No joysticks assigned");
            return;
        }
        Debug.Log($"   Total Joysticks: {joysticks.Length}");
        foreach (Joystick joystick in joysticks)
        {
            if (joystick != null)
            {
                Debug.Log($"🎮 Joystick Status - Name: {joystick.name}");
                Debug.Log($"   Active: {joystick.gameObject.activeInHierarchy}");
                Debug.Log($"   Layer: {joystick.gameObject.layer}");
                Debug.Log($"   Canvas: {joystick.GetComponentInParent<Canvas>()?.name}");
                Debug.Log($"   EventSystem: {EventSystem.current != null}");
                Debug.Log($"   Current Input - H: {joystick.Horizontal:F2}, V: {joystick.Vertical:F2}");
            }
        }
    }
    
    void Update()
    {
        if (enableDebugLogs && joysticks != null)
        {
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null && (Mathf.Abs(joystick.Horizontal) > 0.1f || Mathf.Abs(joystick.Vertical) > 0.1f))
                {
                    Debug.Log($"🎮 Joystick Input - {joystick.name}: H={joystick.Horizontal:F2}, V={joystick.Vertical:F2}");
                }
            }
        }
    }
}
