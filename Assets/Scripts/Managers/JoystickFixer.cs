using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class JoystickFixer : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool autoFixOnStart = true;
    
    [Header("Fix Options")]
    public bool fixEventSystem = true;
    public bool fixCanvas = true;
    public bool fixJoystickComponents = true;
    public bool fixRaycastTargets = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            Invoke(nameof(FixJoystickIssues), 0.5f);
        }
    }
    
    public void FixJoystickIssues()
    {
        Debug.Log("🔧 Iniciando diagnóstico y reparación del joystick...");
        
        if (fixEventSystem)
            FixEventSystem();
            
        if (fixCanvas)
            FixCanvas();
            
        if (fixJoystickComponents)
            FixJoystickComponents();
            
        if (fixRaycastTargets)
            FixRaycastTargets();
            
        Debug.Log("✅ Reparación del joystick completada");
    }
    
    void FixEventSystem()
    {
        Debug.Log("🔧 Verificando EventSystem...");
        
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.Log("⚠️ No se encontró EventSystem, creando uno...");
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }
        else
        {
            Debug.Log("✅ EventSystem encontrado: " + eventSystem.name);
            
            // Verificar que tenga StandaloneInputModule
            StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (inputModule == null)
            {
                Debug.Log("⚠️ Agregando StandaloneInputModule al EventSystem...");
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
        }
    }
    
    void FixCanvas()
    {
        Debug.Log("🔧 Verificando Canvas...");
        
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Canvas encontrado: {canvas.name}, RenderMode: {canvas.renderMode}");
            
            // Asegurar que el Canvas esté configurado correctamente
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Debug.Log("✅ Canvas en modo ScreenSpaceOverlay (correcto)");
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Debug.Log("✅ Canvas en modo ScreenSpaceCamera (correcto)");
            }
            
            // Verificar que tenga GraphicRaycaster
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.Log("⚠️ Agregando GraphicRaycaster al Canvas...");
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                Debug.Log("✅ GraphicRaycaster encontrado en Canvas");
            }
        }
    }
    
    void FixJoystickComponents()
    {
        Debug.Log("🔧 Verificando componentes del Joystick...");
        
        // Buscar todos los tipos de joystick
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        Debug.Log($"Encontrados {joysticks.Length} joysticks");
        
        foreach (Joystick joystick in joysticks)
        {
            Debug.Log($"Joystick: {joystick.name}, Tipo: {joystick.GetType().Name}");
            
            // Verificar que tenga RectTransform
            RectTransform rectTransform = joystick.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"❌ Joystick {joystick.name} no tiene RectTransform!");
                continue;
            }
            
            // Verificar que esté en el Canvas
            Canvas parentCanvas = joystick.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError($"❌ Joystick {joystick.name} no está en un Canvas!");
                continue;
            }
            
            Debug.Log($"✅ Joystick {joystick.name} está en Canvas: {parentCanvas.name}");
            
            // Verificar que tenga Image component para raycast
            Image joystickImage = joystick.GetComponent<Image>();
            if (joystickImage == null)
            {
                Debug.Log("⚠️ Agregando Image component al joystick para raycast...");
                joystickImage = joystick.gameObject.AddComponent<Image>();
                joystickImage.color = new Color(1, 1, 1, 0.1f); // Semi-transparente
            }
            
            // Asegurar que el raycast target esté activado
            if (joystickImage != null)
            {
                joystickImage.raycastTarget = true;
                Debug.Log($"✅ Raycast target activado en {joystick.name}");
            }
            
            // Verificar componentes específicos del joystick
            CheckJoystickSpecificComponents(joystick);
        }
    }
    
    void CheckJoystickSpecificComponents(Joystick joystick)
    {
        // Verificar que tenga background y handle
        var backgroundField = joystick.GetType().GetField("background", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var handleField = joystick.GetType().GetField("handle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (backgroundField != null)
        {
            RectTransform background = backgroundField.GetValue(joystick) as RectTransform;
            if (background != null)
            {
                Debug.Log($"✅ Background encontrado: {background.name}");
                
                // Verificar que el background tenga Image y raycast target
                Image bgImage = background.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.raycastTarget = true;
                    Debug.Log($"✅ Raycast target activado en background");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Background no encontrado en {joystick.name}");
            }
        }
        
        if (handleField != null)
        {
            RectTransform handle = handleField.GetValue(joystick) as RectTransform;
            if (handle != null)
            {
                Debug.Log($"✅ Handle encontrado: {handle.name}");
                
                // Verificar que el handle tenga Image
                Image handleImage = handle.GetComponent<Image>();
                if (handleImage == null)
                {
                    Debug.Log("⚠️ Agregando Image component al handle...");
                    handleImage = handle.gameObject.AddComponent<Image>();
                    handleImage.color = Color.white;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Handle no encontrado en {joystick.name}");
            }
        }
    }
    
    void FixRaycastTargets()
    {
        Debug.Log("🔧 Verificando raycast targets...");
        
        // Buscar todos los elementos UI que deberían recibir raycasts
        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image image in images)
        {
            if (image.gameObject.name.Contains("Joystick") || 
                image.gameObject.name.Contains("Button") ||
                image.gameObject.name.Contains("Background"))
            {
                if (!image.raycastTarget)
                {
                    Debug.Log($"⚠️ Activando raycast target en {image.name}");
                    image.raycastTarget = true;
                }
            }
        }
        
        // Verificar también TextMeshProUGUI
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name.Contains("Button") || text.gameObject.name.Contains("UI"))
            {
                if (!text.raycastTarget)
                {
                    Debug.Log($"⚠️ Activando raycast target en texto {text.name}");
                    text.raycastTarget = true;
                }
            }
        }
    }
    
    // Método para testear el joystick manualmente
    public void TestJoystickInput()
    {
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        foreach (Joystick joystick in joysticks)
        {
            Debug.Log($"Joystick {joystick.name} - H: {joystick.Horizontal:F2}, V: {joystick.Vertical:F2}");
        }
    }
    
    // Método para verificar touch input
    public void CheckTouchInput()
    {
        if (Input.touchSupported)
        {
            Debug.Log($"✅ Touch soportado. Touch count: {Input.touchCount}");
            
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Debug.Log($"Touch {i}: Position: {touch.position}, Phase: {touch.phase}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Touch no soportado en este dispositivo");
        }
    }
} 