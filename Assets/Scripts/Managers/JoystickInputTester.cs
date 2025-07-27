using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickInputTester : MonoBehaviour
{
    [Header("Test Settings")]
    public bool enableContinuousTesting = true;
    public float testInterval = 0.5f;
    public bool showTouchDebug = true;
    
    [Header("Debug Info")]
    public bool showJoystickValues = true;
    public bool showTouchPositions = true;
    public bool showEventSystemInfo = true;
    
    private float lastTestTime;
    private Joystick[] joysticks;
    private EventSystem eventSystem;
    
    void Start()
    {
        Debug.Log("🎮 JoystickInputTester iniciado");
        
        // Buscar joysticks
        joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        Debug.Log($"Encontrados {joysticks.Length} joysticks");
        
        // Buscar EventSystem
        eventSystem = FindFirstObjectByType<EventSystem>();
        
        // Ejecutar test inicial
        RunFullTest();
    }
    
    void Update()
    {
        if (enableContinuousTesting && Time.time - lastTestTime > testInterval)
        {
            RunQuickTest();
            lastTestTime = Time.time;
        }
        
        // Test de touch en tiempo real
        if (showTouchDebug && Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Debug.Log($"Touch {i}: Pos={touch.position}, Phase={touch.phase}, FingerId={touch.fingerId}");
            }
        }
    }
    
    void RunQuickTest()
    {
        if (showJoystickValues)
        {
            foreach (Joystick joystick in joysticks)
            {
                if (joystick != null)
                {
                    float h = joystick.Horizontal;
                    float v = joystick.Vertical;
                    if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
                    {
                        Debug.Log($"🎮 {joystick.name}: H={h:F2}, V={v:F2}");
                    }
                }
            }
        }
    }
    
    void RunFullTest()
    {
        Debug.Log("🔍 === TEST COMPLETO DEL JOYSTICK ===");
        
        // Test 1: Verificar EventSystem
        TestEventSystem();
        
        // Test 2: Verificar Canvas
        TestCanvas();
        
        // Test 3: Verificar Joysticks
        TestJoysticks();
        
        // Test 4: Verificar Touch Input
        TestTouchInput();
        
        // Test 5: Verificar Raycast
        TestRaycast();
        
        Debug.Log("🔍 === FIN DEL TEST ===");
    }
    
    void TestEventSystem()
    {
        Debug.Log("📋 Test 1: EventSystem");
        
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystem no encontrado!");
            return;
        }
        
        Debug.Log($"✅ EventSystem encontrado: {eventSystem.name}");
        
        // Verificar StandaloneInputModule
        StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (inputModule == null)
        {
            Debug.LogError("❌ StandaloneInputModule no encontrado en EventSystem!");
        }
        else
        {
            Debug.Log("✅ StandaloneInputModule encontrado");
        }
        
        // Verificar que el EventSystem esté activo
        if (!eventSystem.gameObject.activeInHierarchy)
        {
            Debug.LogError("❌ EventSystem no está activo!");
        }
        else
        {
            Debug.Log("✅ EventSystem está activo");
        }
    }
    
    void TestCanvas()
    {
        Debug.Log("📋 Test 2: Canvas");
        
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Encontrados {canvases.Length} canvases");
        
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Canvas: {canvas.name}");
            Debug.Log($"  - RenderMode: {canvas.renderMode}");
            Debug.Log($"  - Active: {canvas.gameObject.activeInHierarchy}");
            
            // Verificar GraphicRaycaster
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError($"❌ Canvas {canvas.name} no tiene GraphicRaycaster!");
            }
            else
            {
                Debug.Log($"✅ GraphicRaycaster encontrado en {canvas.name}");
            }
        }
    }
    
    void TestJoysticks()
    {
        Debug.Log("📋 Test 3: Joysticks");
        
        if (joysticks.Length == 0)
        {
            Debug.LogError("❌ No se encontraron joysticks!");
            return;
        }
        
        foreach (Joystick joystick in joysticks)
        {
            Debug.Log($"Joystick: {joystick.name}");
            Debug.Log($"  - Tipo: {joystick.GetType().Name}");
            Debug.Log($"  - Active: {joystick.gameObject.activeInHierarchy}");
            
            // Verificar RectTransform
            RectTransform rectTransform = joystick.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"❌ {joystick.name} no tiene RectTransform!");
            }
            else
            {
                Debug.Log($"✅ RectTransform encontrado");
                Debug.Log($"  - AnchoredPosition: {rectTransform.anchoredPosition}");
                Debug.Log($"  - SizeDelta: {rectTransform.sizeDelta}");
            }
            
            // Verificar que esté en Canvas
            Canvas parentCanvas = joystick.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError($"❌ {joystick.name} no está en un Canvas!");
            }
            else
            {
                Debug.Log($"✅ Está en Canvas: {parentCanvas.name}");
            }
            
            // Verificar Image component
            Image joystickImage = joystick.GetComponent<Image>();
            if (joystickImage == null)
            {
                Debug.LogError($"❌ {joystick.name} no tiene Image component!");
            }
            else
            {
                Debug.Log($"✅ Image component encontrado");
                Debug.Log($"  - RaycastTarget: {joystickImage.raycastTarget}");
                Debug.Log($"  - Color: {joystickImage.color}");
            }
            
            // Test de valores actuales
            float h = joystick.Horizontal;
            float v = joystick.Vertical;
            Debug.Log($"  - Valores actuales: H={h:F2}, V={v:F2}");
        }
    }
    
    void TestTouchInput()
    {
        Debug.Log("📋 Test 4: Touch Input");
        
        if (!Input.touchSupported)
        {
            Debug.LogWarning("⚠️ Touch no soportado en este dispositivo - usando input de teclado/mouse como fallback");
            Debug.Log("💡 Para testear el joystick, usa las teclas WASD o las flechas del teclado");
            return;
        }
        
        Debug.Log("✅ Touch soportado");
        Debug.Log($"Touch count: {Input.touchCount}");
        
        // Verificar si hay un EventSystem activo
        EventSystem currentEventSystem = FindFirstObjectByType<EventSystem>();
        if (currentEventSystem == null)
        {
            Debug.LogWarning("⚠️ No hay EventSystem en la escena - el touch input puede no funcionar correctamente");
        }
        else
        {
            Debug.Log($"✅ EventSystem encontrado: {currentEventSystem.name}");
        }
        
        // Simular un test de touch
        Debug.Log("💡 Para testear el joystick, toca la pantalla en el área del joystick");
    }
    
    void TestRaycast()
    {
        Debug.Log("📋 Test 5: Raycast Test");
        
        // Crear un test de raycast desde el centro de la pantalla
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        if (eventSystem != null)
        {
            PointerEventData eventData = new PointerEventData(eventSystem);
            eventData.position = screenCenter;
            
            System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
            eventSystem.RaycastAll(eventData, results);
            
            Debug.Log($"Raycast desde centro de pantalla ({screenCenter}) encontró {results.Count} objetos:");
            
            foreach (RaycastResult result in results)
            {
                Debug.Log($"  - {result.gameObject.name} (Layer: {result.gameObject.layer})");
            }
        }
        else
        {
            Debug.LogError("❌ No se puede hacer raycast sin EventSystem!");
        }
    }
    
    // Método público para ejecutar test manual
    [ContextMenu("Run Full Test")]
    public void RunFullTestManual()
    {
        RunFullTest();
    }
    
    // Método para testear joystick específico
    public void TestSpecificJoystick(string joystickName)
    {
        foreach (Joystick joystick in joysticks)
        {
            if (joystick.name == joystickName)
            {
                Debug.Log($"🎮 Test específico para {joystick.name}:");
                Debug.Log($"  H: {joystick.Horizontal:F2}");
                Debug.Log($"  V: {joystick.Vertical:F2}");
                Debug.Log($"  Direction: {joystick.Direction}");
                return;
            }
        }
        Debug.LogWarning($"Joystick '{joystickName}' no encontrado");
    }
} 