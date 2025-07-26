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
            Debug.Log($"HealthBar initialized for {targetPlayer.name}");
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
        // Buscar o crear componentes UI
        if (healthBarFill == null)
        {
            healthBarFill = transform.Find("HealthBarFill")?.GetComponent<Image>();
            if (healthBarFill == null)
            {
                CreateHealthBarUI();
            }
        }
        
        if (healthBarBackground == null)
        {
            healthBarBackground = transform.Find("HealthBarBackground")?.GetComponent<Image>();
        }
        
        if (healthText == null)
        {
            healthText = transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        }
    }
    
    void CreateHealthBarUI()
    {
        // Crear background
        GameObject backgroundGO = new GameObject("HealthBarBackground");
        backgroundGO.transform.SetParent(transform, false);
        healthBarBackground = backgroundGO.AddComponent<Image>();
        healthBarBackground.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Crear fill
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
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        
        // Crear texto
        GameObject textGO = new GameObject("HealthText");
        textGO.transform.SetParent(backgroundGO.transform, false);
        healthText = textGO.AddComponent<TextMeshProUGUI>();
        healthText.text = "100/100";
        healthText.fontSize = 12;
        healthText.color = Color.white;
        healthText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Configurar tamaño de la health bar
        RectTransform mainRect = GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(100, 15);
        
        Debug.Log("HealthBar UI created");
    }
    
    void Update()
    {
        if (!isInitialized || targetPlayer == null) return;
        
        // Actualizar posición para seguir al jugador
        if (followPlayer)
        {
            FollowPlayer();
        }
        
        // Actualizar salud
        float newHealth = targetPlayer.CurrentHealth;
        if (newHealth != currentHealth)
        {
            currentHealth = newHealth;
            UpdateHealthBar();
        }
        
        // Animar pulso si está habilitado
        if (enablePulseAnimation && currentHealth < maxHealth * mediumHealthThreshold)
        {
            AnimatePulse();
        }
    }
    
    void FollowPlayer()
    {
        if (targetPlayer == null || mainCamera == null) return;
        
        // Calcular posición objetivo
        Vector3 targetPosition = targetPlayer.transform.position + offset;
        
        // Convertir posición del mundo a posición de pantalla
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
        
        // Verificar si está detrás de la cámara
        if (screenPosition.z < 0)
        {
            // Ocultar la health bar si está detrás de la cámara
            if (healthCanvas != null)
            {
                healthCanvas.enabled = false;
            }
            return;
        }
        else
        {
            // Mostrar la health bar
            if (healthCanvas != null)
            {
                healthCanvas.enabled = true;
            }
        }
        
        // Actualizar posición del canvas
        if (healthCanvas != null)
        {
            Vector3 currentPos = healthCanvas.transform.position;
            Vector3 newPos = Vector3.Lerp(currentPos, targetPosition, smoothSpeed * Time.deltaTime);
            healthCanvas.transform.position = newPos;
            
            // Hacer que la health bar mire hacia la cámara
            healthCanvas.transform.LookAt(mainCamera.transform);
            healthCanvas.transform.Rotate(0, 180, 0); // Rotar para que mire hacia la cámara
        }
    }
    
    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;
        
        // Calcular porcentaje de salud
        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        // Actualizar fill amount
        healthBarFill.fillAmount = healthPercentage;
        
        // Actualizar color basado en la salud
        UpdateHealthColor(healthPercentage);
        
        // Actualizar texto
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
        
        // Debug
        Debug.Log($"HealthBar updated: {currentHealth:F0}/{maxHealth:F0} ({healthPercentage:P0})");
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