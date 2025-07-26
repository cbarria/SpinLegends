using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AndroidJoystickEnabler : MonoBehaviour
{
    [Header("Android Joystick Settings")]
    public bool forceJoystickFix = true;
    public bool enableTouchDebug = true;
    public bool createBackupJoystick = false;
    
    void Start()
    {
        #if UNITY_ANDROID
        Debug.Log("🤖 AndroidJoystickEnabler iniciado en Android");
        
        if (forceJoystickFix)
        {
            Invoke(nameof(ForceJoystickFix), 1f);
        }
        #else
        Debug.Log("AndroidJoystickEnabler iniciado en plataforma no-Android");
        #endif
    }
    
    void ForceJoystickFix()
    {
        Debug.Log("🔧 Forzando reparación del joystick para Android...");
        
        // 1. Asegurar que el EventSystem esté configurado correctamente
        EnsureEventSystem();
        
        // 2. Asegurar que el Canvas esté configurado correctamente
        EnsureCanvas();
        
        // 3. Asegurar que el joystick esté configurado correctamente
        EnsureJoystick();
        
        // 4. Crear joystick de respaldo si es necesario
        if (createBackupJoystick)
        {
            CreateBackupJoystick();
        }
        
        Debug.Log("✅ Reparación del joystick para Android completada");
    }
    
    void EnsureEventSystem()
    {
        Debug.Log("🔧 Verificando EventSystem para Android...");
        
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.Log("⚠️ Creando EventSystem para Android...");
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
            
            // Agregar StandaloneInputModule específico para Android
            StandaloneInputModule inputModule = eventSystemGO.AddComponent<StandaloneInputModule>();
            
            // Configurar para Android
            inputModule.horizontalAxis = "Horizontal";
            inputModule.verticalAxis = "Vertical";
            inputModule.submitButton = "Submit";
            inputModule.cancelButton = "Cancel";
            inputModule.inputActionsPerSecond = 10;
            inputModule.repeatDelay = 0.5f;
            
            Debug.Log("✅ EventSystem creado y configurado para Android");
        }
        else
        {
            Debug.Log("✅ EventSystem ya existe");
            
            // Verificar que tenga StandaloneInputModule
            StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (inputModule == null)
            {
                Debug.Log("⚠️ Agregando StandaloneInputModule al EventSystem existente...");
                inputModule = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
        }
    }
    
    void EnsureCanvas()
    {
        Debug.Log("🔧 Verificando Canvas para Android...");
        
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Canvas encontrado: {canvas.name}");
            
            // Asegurar que esté en modo ScreenSpaceOverlay para Android
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.Log($"⚠️ Cambiando Canvas {canvas.name} a ScreenSpaceOverlay para Android...");
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            
            // Asegurar que tenga GraphicRaycaster
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.Log($"⚠️ Agregando GraphicRaycaster al Canvas {canvas.name}...");
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // Configurar GraphicRaycaster para Android
            if (raycaster != null)
            {
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                raycaster.blockingMask = -1;
                Debug.Log($"✅ GraphicRaycaster configurado en {canvas.name}");
            }
        }
    }
    
    void EnsureJoystick()
    {
        Debug.Log("🔧 Verificando Joystick para Android...");
        
        Joystick[] joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        Debug.Log($"Encontrados {joysticks.Length} joysticks");
        
        foreach (Joystick joystick in joysticks)
        {
            Debug.Log($"Configurando joystick: {joystick.name}");
            
            // Asegurar que tenga Image component
            Image joystickImage = joystick.GetComponent<Image>();
            if (joystickImage == null)
            {
                Debug.Log($"⚠️ Agregando Image component al joystick {joystick.name}...");
                joystickImage = joystick.gameObject.AddComponent<Image>();
                joystickImage.color = new Color(1, 1, 1, 0.3f); // Semi-transparente
            }
            
            // Asegurar que el raycast target esté activado
            if (joystickImage != null)
            {
                joystickImage.raycastTarget = true;
                Debug.Log($"✅ Raycast target activado en {joystick.name}");
            }
            
            // Verificar que esté en el Canvas correcto
            Canvas parentCanvas = joystick.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError($"❌ Joystick {joystick.name} no está en un Canvas!");
            }
            else
            {
                Debug.Log($"✅ Joystick {joystick.name} está en Canvas: {parentCanvas.name}");
            }
            
            // Configurar RectTransform para Android
            RectTransform rectTransform = joystick.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Asegurar que el joystick sea lo suficientemente grande para touch
                if (rectTransform.sizeDelta.x < 100f || rectTransform.sizeDelta.y < 100f)
                {
                    Debug.Log($"⚠️ Aumentando tamaño del joystick {joystick.name} para Android...");
                    rectTransform.sizeDelta = new Vector2(150f, 150f);
                }
                
                Debug.Log($"✅ RectTransform configurado: {rectTransform.sizeDelta}");
            }
            
            // Configurar componentes específicos del joystick
            ConfigureJoystickComponents(joystick);
        }
    }
    
    void ConfigureJoystickComponents(Joystick joystick)
    {
        // Usar reflection para acceder a los campos privados del joystick
        var backgroundField = joystick.GetType().GetField("background", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var handleField = joystick.GetType().GetField("handle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (backgroundField != null)
        {
            RectTransform background = backgroundField.GetValue(joystick) as RectTransform;
            if (background != null)
            {
                // Asegurar que el background tenga Image y raycast target
                Image bgImage = background.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.raycastTarget = true;
                    Debug.Log($"✅ Raycast target activado en background de {joystick.name}");
                }
                
                // Asegurar que el background sea lo suficientemente grande
                if (background.sizeDelta.x < 100f || background.sizeDelta.y < 100f)
                {
                    background.sizeDelta = new Vector2(150f, 150f);
                    Debug.Log($"✅ Tamaño del background ajustado para {joystick.name}");
                }
            }
        }
        
        if (handleField != null)
        {
            RectTransform handle = handleField.GetValue(joystick) as RectTransform;
            if (handle != null)
            {
                // Asegurar que el handle tenga Image
                Image handleImage = handle.GetComponent<Image>();
                if (handleImage == null)
                {
                    Debug.Log($"⚠️ Agregando Image component al handle de {joystick.name}...");
                    handleImage = handle.gameObject.AddComponent<Image>();
                    handleImage.color = Color.white;
                }
                
                // Asegurar que el handle sea visible
                if (handle.sizeDelta.x < 50f || handle.sizeDelta.y < 50f)
                {
                    handle.sizeDelta = new Vector2(60f, 60f);
                    Debug.Log($"✅ Tamaño del handle ajustado para {joystick.name}");
                }
            }
        }
    }
    
    void CreateBackupJoystick()
    {
        Debug.Log("🔧 Creando joystick de respaldo para Android...");
        
        // Buscar Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No se encontró Canvas para crear joystick de respaldo!");
            return;
        }
        
        // Crear joystick de respaldo
        GameObject backupJoystickGO = new GameObject("BackupJoystick");
        backupJoystickGO.transform.SetParent(canvas.transform, false);
        
        // Agregar componentes necesarios
        RectTransform rectTransform = backupJoystickGO.AddComponent<RectTransform>();
        Image image = backupJoystickGO.AddComponent<Image>();
        FixedJoystick fixedJoystick = backupJoystickGO.AddComponent<FixedJoystick>();
        
        // Configurar posición y tamaño
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = new Vector2(150, 150);
        rectTransform.sizeDelta = new Vector2(150, 150);
        
        // Configurar imagen
        image.color = new Color(1, 1, 1, 0.5f);
        image.raycastTarget = true;
        
        Debug.Log("✅ Joystick de respaldo creado");
    }
    
    void Update()
    {
        #if UNITY_ANDROID
        if (enableTouchDebug && Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    Debug.Log($"🤖 Touch Android: Pos={touch.position}, Phase={touch.phase}");
                }
            }
        }
        #endif
    }
    
    // Método público para forzar reparación manual
    [ContextMenu("Force Android Joystick Fix")]
    public void ForceAndroidJoystickFix()
    {
        ForceJoystickFix();
    }
} 