using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AndroidSettings : MonoBehaviour
{
    [Header("Android UI Settings")]
    public float androidTextScale = 1.2f;
    public float androidButtonScale = 1.1f;
    public bool enableHighDPI = true;
    
    [Header("Console Settings")]
    public bool enableLargeConsoleText = true;
    
    [Header("Joystick Focus Settings")]
    public bool enableJoystickFocusFix = true;
    public float joystickInitializationDelay = 0.5f;
    
    [Header("Touch Input Settings")]
    public bool enableTouchInputFix = true;
    public bool forceTouchFocus = true;
    
    private JoystickFocusManager joystickFocusManager;
    private AndroidTouchInput androidTouchInput;
    
    void Awake()
    {
        #if UNITY_ANDROID
        ConfigureForAndroid();
        #endif
    }
    
    void ConfigureForAndroid()
    {
        Debug.Log("🔧 Configurando para Android...");
        
        // No forzar resolución: evita zoom/estirado en dispositivos
        // if (enableHighDPI) { Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true); }
        
        // Orientación
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        // UI scaling opcional: desactivado por defecto para no interferir con Canvas/Joystick
        // ConfigureUIScaling();
        
        // Consola Android (opcional)
        if (enableLargeConsoleText)
        {
            ConfigureConsoleForAndroid();
        }
        
        // Joystick focus manager
        if (enableJoystickFocusFix)
        {
            SetupJoystickFocusManager();
        }
        
        // Input táctil Android
        if (enableTouchInputFix)
        {
            SetupAndroidTouchInput();
        }
        
        Debug.Log("✅ Configuración de Android completada");
    }
    
    void ConfigureUIScaling()
    {
        // Deshabilitado por defecto: deja la UI como está diseñada
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null)
            {
                text.fontSize = Mathf.RoundToInt(text.fontSize * androidTextScale);
                text.fontStyle = FontStyles.Bold;
                text.enableAutoSizing = false;
                text.textWrappingMode = TextWrappingModes.Normal;
                text.richText = true;
            }
        }
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button != null)
            {
                var rt = button.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one * androidButtonScale;
                }
            }
        }
    }
    
    void ConfigureConsoleForAndroid()
    {
        // Placeholder para ajustes de consola si se usan
    }
    
    void SetupJoystickFocusManager()
    {
        var existing = FindFirstObjectByType<JoystickFocusManager>();
        if (existing == null)
        {
            GameObject go = new GameObject("JoystickFocusManager");
            joystickFocusManager = go.AddComponent<JoystickFocusManager>();
            joystickFocusManager.initializationDelay = joystickInitializationDelay;
            joystickFocusManager.enableDebugLogs = true;
            joystickFocusManager.autoFindJoysticks = true;
            joystickFocusManager.forceJoystickFocus = true;
        }
    }
    
    void SetupAndroidTouchInput()
    {
        var existing = FindFirstObjectByType<AndroidTouchInput>();
        if (existing == null)
        {
            GameObject go = new GameObject("AndroidTouchInput");
            androidTouchInput = go.AddComponent<AndroidTouchInput>();
            androidTouchInput.enableTouchDebug = false;
            androidTouchInput.forceTouchFocus = true;
            androidTouchInput.autoCreateEventSystem = true;
            androidTouchInput.ensureGraphicRaycaster = true;
        }
    }
} 