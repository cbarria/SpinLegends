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
        InitializeHealthBar();
    }
    
    void InitializeHealthBar()
    {
        // Buscar cámara principal
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // Si no hay target player asignado, buscar automáticamente
        if (targetPlayer == null)
        {
            targetPlayer = FindFirstObjectByType<PlayerController>();
        }
        
        if (targetPlayer != null)
        {
            // Obtener valores iniciales de salud
            currentHealth = targetPlayer.CurrentHealth;
            maxHealth = targetPlayer.MaxHealth;
            
            Debug.Log($"🏥🔧 Initial health values: Current={currentHealth}, Max={maxHealth}");
            
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
        else
        {
            Debug.LogWarning("No PlayerController found for HealthBar");
        }
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
        // Buscar componentes UI creados por HealthBarManager por nombre específico
        Transform backgroundTransform = transform.Find("Background");
        if (backgroundTransform != null)
        {
            healthBarBackground = backgroundTransform.GetComponent<Image>();
            
            // Buscar el fill dentro del background
            Transform fillTransform = backgroundTransform.Find("HealthFill");
            if (fillTransform != null)
            {
                healthBarFill = fillTransform.GetComponent<Image>();
                Debug.Log($"🏥✅ UI Components found by name: Background={healthBarBackground.name}, Fill={healthBarFill.name}");
            }
            else
            {
                Debug.LogError("🏥❌ HealthFill not found inside Background!");
            }
        }
        else
        {
            Debug.LogError("🏥❌ Background not found in children!");
            
            // Fallback: buscar por componentes
            Image[] images = GetComponentsInChildren<Image>();
            if (images.Length >= 2)
            {
                healthBarBackground = images[0];
                healthBarFill = images[1];
                Debug.Log($"🏥⚠️ Using fallback method: Background={healthBarBackground.name}, Fill={healthBarFill.name}");
            }
        }
        
        // Buscar texto si existe
        healthText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Verificar que el fill esté configurado correctamente
        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            Debug.Log($"🏥🔧 Fill configured: Type={healthBarFill.type}, Method={healthBarFill.fillMethod}, Amount={healthBarFill.fillAmount}");
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
        if (healthBarFill == null) return;
        
        // Calcular porcentaje de salud
        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        // Actualizar fill amount
        float oldFillAmount = healthBarFill.fillAmount;
        healthBarFill.fillAmount = healthPercentage;
        
        // Debug si cambió el fill amount
        if (Mathf.Abs(oldFillAmount - healthPercentage) > 0.01f)
        {
            Debug.Log($"🏥🔄 Fill amount changed: {oldFillAmount:F2} → {healthPercentage:F2} (Health: {currentHealth:F0}/{maxHealth:F0})");
        }
        
        // Actualizar color basado en la salud
        UpdateHealthColor(healthPercentage);
        
        // Actualizar texto
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
        
        // Debug (reducido)
        if (Time.time - lastDebugTime > 2f) // Solo cada 2s
        {
            Debug.Log($"🏥📊 HealthBar update: {currentHealth:F0}/{maxHealth:F0} ({healthPercentage:P0}) - Fill: {(healthBarFill != null ? healthBarFill.fillAmount.ToString("F2") : "NULL")} - FillType: {(healthBarFill != null ? healthBarFill.type.ToString() : "NULL")}");
            lastDebugTime = Time.time;
        }
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
        
        // Aplicar color con transición suave
        healthBarFill.color = Color.Lerp(healthBarFill.color, targetColor, Time.deltaTime * 5f);
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
} 