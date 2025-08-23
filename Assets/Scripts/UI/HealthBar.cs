using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public PlayerController targetPlayer;
    public Vector3 offset = new Vector3(0, 3f, 0);
    public float smoothSpeed = 5f;
    public bool followPlayer = true;
    
    [Header("UI Components")]
    public Image healthBarFill;
    public Image healthBarBackground;
    public TextMeshProUGUI healthText;
    public Canvas healthCanvas;
    
    [Header("Color Settings")]
    public Color highHealthColor = Color.green;
    public Color mediumHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    public float highHealthThreshold = 0.7f; // 70%
    public float mediumHealthThreshold = 0.3f; // 30%
    
    [Header("Animation Settings")]
    public bool enablePulseAnimation = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.1f;
    
    private Camera mainCamera;
    private float currentHealth;
    private float maxHealth;
    private bool isInitialized = false;
    private float lastDebugTime = 0f;
    
    void Start()
    {
        // Inicialización simple y directa
        InitializeHealthBar();
    }
    
    public void InitializeHealthBar()
    {
        Debug.Log($"🏥🚀 INITIALIZING HealthBar - TargetPlayer: {targetPlayer?.name}");
        
        // Buscar cámara principal
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindFirstObjectByType<Camera>();
        
        // Si no hay targetPlayer, salir (será asignado por HealthBarManager)
        if (targetPlayer == null)
        {
            Debug.LogWarning("🏥⚠️ TargetPlayer is null, skipping initialization");
            return;
        }
        
        // Obtener valores de salud
        currentHealth = targetPlayer.CurrentHealth;
        maxHealth = targetPlayer.MaxHealth;
        Debug.Log($"🏥💖 Health values: {currentHealth}/{maxHealth}");
        
        // Configurar Canvas si no existe
        if (healthCanvas == null)
        {
            healthCanvas = GetComponentInParent<Canvas>();
        }
        
        // Configurar componentes UI
        SetupUIComponents();
        
        // Actualizar barra inicial
        UpdateHealthBar();
        
        isInitialized = true;
        Debug.Log($"🏥✅ HealthBar initialized for {targetPlayer.name}");
    }
    
    void CreateHealthCanvas()
    {
        // Crear Canvas para la health bar
        GameObject canvasGO = new GameObject("HealthBarCanvas");
        healthCanvas = canvasGO.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        healthCanvas.worldCamera = mainCamera;
        
        // Agregar CanvasScaler
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Agregar GraphicRaycaster
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Hacer que la health bar sea hija del canvas
        transform.SetParent(canvasGO.transform, false);
        
        Debug.Log("HealthBar Canvas created");
    }
    
    void SetupUIComponents()
    {
        // MÉTODO DIRECTO: Buscar en toda la jerarquía por nombre exacto
        healthBarBackground = transform.GetComponentInChildren<Image>();
        
        // Buscar el fill específicamente por nombre "HealthFill"
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name == "HealthFill")
            {
                healthBarFill = child.GetComponent<Image>();
                break;
            }
        }
        
        // Verificar que encontramos los componentes
        if (healthBarBackground != null && healthBarFill != null)
        {
            Debug.Log($"🏥✅ Components found: Background={healthBarBackground.name}, Fill={healthBarFill.name}");
            Debug.Log($"🏥🔍 Fill initial state: Amount={healthBarFill.fillAmount}, Color={healthBarFill.color}, Type={healthBarFill.type}");
        }
        else
        {
            Debug.LogError($"🏥❌ Components missing! Background={healthBarBackground != null}, Fill={healthBarFill != null}");
            
            // Debug completo de la jerarquía
            Debug.LogError("🏥🔍 Full hierarchy:");
            foreach (Transform child in allChildren)
            {
                Image img = child.GetComponent<Image>();
                Debug.LogError($"  - {child.name} (Image: {img != null}, FillAmount: {(img?.fillAmount.ToString() ?? "N/A")})");
            }
        }
        
        // Buscar texto si existe
        healthText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Verificar que el fill esté configurado correctamente
        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.color = Color.green; // FORZAR COLOR VERDE
            Debug.Log($"🏥🔧 Fill configured: Type={healthBarFill.type}, Method={healthBarFill.fillMethod}, Amount={healthBarFill.fillAmount}, Color={healthBarFill.color}");
        }
        
        // Debug de la jerarquía
        if (healthBarBackground != null && healthBarFill != null)
        {
            Debug.Log($"🏥🏗️ Hierarchy: Background={healthBarBackground.transform.GetSiblingIndex()}, Fill={healthBarFill.transform.GetSiblingIndex()}");
            Debug.Log($"🏥🎨 Colors: Background={healthBarBackground.color}, Fill={healthBarFill.color}");
        }
    }
    
    void CreateHealthBarUI()
    {
        // Create background
        GameObject backgroundGO = new GameObject("HealthBarBackground");
        backgroundGO.transform.SetParent(transform, false);
        healthBarBackground = backgroundGO.AddComponent<Image>();
        healthBarBackground.color = new Color(0, 0, 0, 0.8f);

        RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create fill
        GameObject fillGO = new GameObject("HealthBarFill");
        fillGO.transform.SetParent(backgroundGO.transform, false);
        healthBarFill = fillGO.AddComponent<Image>();
        healthBarFill.color = highHealthColor;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillAmount = 1f;

        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(0.02f, 0.02f);
        fillRect.offsetMax = new Vector2(-0.02f, -0.02f);

        // Create text
        GameObject textGO = new GameObject("HealthText");
        textGO.transform.SetParent(backgroundGO.transform, false);
        healthText = textGO.AddComponent<TextMeshProUGUI>();
        healthText.text = "100/100";
        healthText.fontSize = 0.2f;
        healthText.color = Color.black;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.textWrappingMode = TextWrappingModes.NoWrap;
        healthText.fontStyle = FontStyles.Bold;
        healthText.outlineColor = Color.white;
        healthText.outlineWidth = 0.2f;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Set health bar size for World Space
        RectTransform mainRect = GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(1.5f, 0.2f);
    }
    
    void Update()
    {
        if (!isInitialized || targetPlayer == null) return;

        // Move the RectTransform of the health bar to the player's position + offset
        RectTransform rect = GetComponent<RectTransform>();
        if (followPlayer && mainCamera != null && rect != null)
        {
            rect.position = targetPlayer.transform.position + offset;
            rect.LookAt(mainCamera.transform);
            rect.Rotate(0, 180, 0);
        }

        // Update health if changed
        float newHealth = targetPlayer.CurrentHealth;
        if (newHealth != currentHealth)
        {
            currentHealth = newHealth;
            UpdateHealthBar();
        }

        // Animate pulse if enabled
        if (enablePulseAnimation && currentHealth < maxHealth * mediumHealthThreshold)
        {
            AnimatePulse();
        }
    }
    
    void UpdateHealthBar()
    {
        if (healthBarFill == null)
        {
            Debug.LogError("🏥❌ healthBarFill is NULL!");
            return;
        }
        
        // Calcular porcentaje de salud
        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 1f;
        
        // LOG CRÍTICO
        Debug.Log($"🏥📊 UPDATE: {currentHealth:F0}/{maxHealth:F0} = {healthPercentage:F2} → FillAmount");
        
        // Actualizar fill amount
        healthBarFill.fillAmount = healthPercentage;
        
        // Actualizar color
        if (healthPercentage > 0.7f) healthBarFill.color = Color.green;
        else if (healthPercentage > 0.3f) healthBarFill.color = Color.yellow;
        else healthBarFill.color = Color.red;
        
        // Actualizar texto
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
        
        Debug.Log($"🏥✅ Fill updated: Amount={healthBarFill.fillAmount:F2}, Color={healthBarFill.color}");
    }
    
    void UpdateHealthColor(float healthPercentage)
    {
        if (healthBarFill == null) return;
        
        Color targetColor;
        
        if (healthPercentage >= highHealthThreshold)
        {
            targetColor = highHealthColor;
        }
        else if (healthPercentage >= mediumHealthThreshold)
        {
            targetColor = mediumHealthColor;
        }
        else
        {
            targetColor = lowHealthColor;
        }
        
        // Debug del color
        Color oldColor = healthBarFill.color;
        
        // Aplicar color con transición suave
        healthBarFill.color = Color.Lerp(healthBarFill.color, targetColor, Time.deltaTime * 5f);
        
        // Debug si cambió el color significativamente
        if (Vector4.Distance(oldColor, healthBarFill.color) > 0.1f)
        {
            Debug.Log($"🏥🎨 Color changed: {oldColor} → {healthBarFill.color} (Target: {targetColor}, Health: {healthPercentage:P0})");
        }
    }
    
    void AnimatePulse()
    {
        if (healthBarFill == null) return;
        
        // Crear efecto de pulso
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
        healthBarFill.transform.localScale = Vector3.one * pulse;
    }
    
    // Método público para actualizar manualmente
    public void SetHealth(float current, float maximum)
    {
        currentHealth = current;
        maxHealth = maximum;
        UpdateHealthBar();
    }
    
    // Método para cambiar el target player
    public void SetTargetPlayer(PlayerController player)
    {
        targetPlayer = player;
        if (player != null)
        {
            currentHealth = player.CurrentHealth;
            maxHealth = player.MaxHealth;
            UpdateHealthBar();
        }
    }
    
    // Método para mostrar/ocultar la health bar
    public void SetVisible(bool visible)
    {
        if (healthCanvas != null)
        {
            healthCanvas.enabled = visible;
        }
    }
    
    // Método público para forzar la inicialización (llamado por HealthBarManager)
    public void ForceInitialize()
    {
        if (!isInitialized)
        {
            Debug.Log("🏥🔄 Force initializing health bar...");
            InitializeHealthBar();
        }
    }
    
    // Corrutina para reintentar la inicialización si los valores de salud están mal
    System.Collections.IEnumerator RetryHealthInitialization()
    {
        Debug.Log("🏥⏳ Waiting for health values to initialize...");
        
        // Esperar hasta 2 segundos para que el player se inicialice
        float maxWaitTime = 2f;
        float waitTime = 0f;
        
        while (waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
            
            if (targetPlayer != null)
            {
                float newCurrent = targetPlayer.CurrentHealth;
                float newMax = targetPlayer.MaxHealth;
                
                Debug.Log($"🏥🔄 Retry {waitTime:F1}s: Current={newCurrent}, Max={newMax}");
                
                if (newCurrent > 0 && newMax > 0)
                {
                    Debug.Log("🏥✅ Health values found! Continuing initialization...");
                    currentHealth = newCurrent;
                    maxHealth = newMax;
                    
                    // Continuar con la inicialización
                    ContinueInitialization();
                    yield break;
                }
            }
        }
        
        Debug.LogError("🏥❌ Failed to get health values after 2 seconds!");
    }
    
    void ContinueInitialization()
    {
        // Configurar Canvas
        if (healthCanvas == null)
        {
            healthCanvas = GetComponentInParent<Canvas>();
            if (healthCanvas == null)
            {
                CreateHealthCanvas();
            }
        }
        
        // Configurar componentes UI si no están asignados
        SetupUIComponents();
        
        // Actualizar barra inicial
        UpdateHealthBar();
        
        isInitialized = true;
        Debug.Log($"🏥🔧 HealthBar initialized for {targetPlayer.name} - Canvas: {healthCanvas != null} - Fill: {healthBarFill != null} - Position: {transform.position}");
    }
} 