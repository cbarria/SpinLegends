using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarEffects : MonoBehaviour
{
    [Header("Damage Flash Effect")]
    public bool enableDamageFlash = true;
    public Color damageFlashColor = Color.white;
    public float damageFlashDuration = 0.2f;
    public float damageFlashIntensity = 0.8f;
    
    [Header("Heal Glow Effect")]
    public bool enableHealGlow = true;
    public Color healGlowColor = Color.cyan;
    public float healGlowDuration = 0.5f;
    public float healGlowIntensity = 0.6f;
    
    [Header("Critical Health Effect")]
    public bool enableCriticalHealthEffect = true;
    public Color criticalHealthColor = Color.red;
    public float criticalHealthPulseSpeed = 3f;
    public float criticalHealthPulseIntensity = 0.3f;
    
    [Header("Shake Effect")]
    public bool enableShakeEffect = true;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.3f;
    
    [Header("References")]
    public HealthBar healthBar;
    public Image healthBarFill;
    public Image healthBarBackground;
    
    private Color originalFillColor;
    private Color originalBackgroundColor;
    private Vector3 originalPosition;
    private float lastHealth;
    private bool isCriticalHealth = false;
    
    // Efectos temporales
    private float damageFlashTimer = 0f;
    private float healGlowTimer = 0f;
    private float shakeTimer = 0f;
    private Vector3 shakeOffset = Vector3.zero;
    
    void Start()
    {
        InitializeEffects();
    }
    
    void InitializeEffects()
    {
        // Buscar referencias si no están asignadas
        if (healthBar == null)
        {
            healthBar = GetComponent<HealthBar>();
        }
        
        if (healthBarFill == null)
        {
            healthBarFill = transform.Find("HealthBarBackground/HealthBarFill")?.GetComponent<Image>();
        }
        
        if (healthBarBackground == null)
        {
            healthBarBackground = transform.Find("HealthBarBackground")?.GetComponent<Image>();
        }
        
        // Guardar colores originales
        if (healthBarFill != null)
        {
            originalFillColor = healthBarFill.color;
        }
        
        if (healthBarBackground != null)
        {
            originalBackgroundColor = healthBarBackground.color;
        }
        
        // Guardar posición original
        originalPosition = transform.position;
        
        // Obtener salud inicial
        if (healthBar != null && healthBar.targetPlayer != null)
        {
            lastHealth = healthBar.targetPlayer.CurrentHealth;
        }
        
        Debug.Log("HealthBarEffects initialized");
    }
    
    void Update()
    {
        if (healthBar == null || healthBar.targetPlayer == null) return;
        
        float currentHealth = healthBar.targetPlayer.CurrentHealth;
        float maxHealth = healthBar.targetPlayer.MaxHealth;
        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        // Detectar cambios en la salud
        if (currentHealth != lastHealth)
        {
            float healthDifference = currentHealth - lastHealth;
            
            // Efecto de daño
            if (healthDifference < 0 && enableDamageFlash)
            {
                TriggerDamageFlash();
                if (enableShakeEffect)
                {
                    TriggerShakeEffect();
                }
            }
            
            // Efecto de curación
            if (healthDifference > 0 && enableHealGlow)
            {
                TriggerHealGlow();
            }
            
            lastHealth = currentHealth;
        }
        
        // Verificar si está en salud crítica
        bool wasCritical = isCriticalHealth;
        isCriticalHealth = healthPercentage <= 0.2f; // 20% o menos
        
        if (isCriticalHealth && !wasCritical && enableCriticalHealthEffect)
        {
            Debug.Log("Critical health! Activating special effects");
        }
        
        // Actualizar efectos
        UpdateDamageFlash();
        UpdateHealGlow();
        UpdateCriticalHealthEffect();
        UpdateShakeEffect();
    }
    
    void TriggerDamageFlash()
    {
        damageFlashTimer = damageFlashDuration;
        Debug.Log("Damage effect activated");
    }
    
    void TriggerHealGlow()
    {
        healGlowTimer = healGlowDuration;
        Debug.Log("Heal effect activated");
    }
    
    void TriggerShakeEffect()
    {
        shakeTimer = shakeDuration;
        Debug.Log("Shake effect activated");
    }
    
    void UpdateDamageFlash()
    {
        if (damageFlashTimer > 0)
        {
            damageFlashTimer -= Time.deltaTime;
            
            if (healthBarFill != null)
            {
                float flashProgress = damageFlashTimer / damageFlashDuration;
                Color flashColor = Color.Lerp(originalFillColor, damageFlashColor, 
                    Mathf.Sin(flashProgress * Mathf.PI) * damageFlashIntensity);
                healthBarFill.color = flashColor;
            }
        }
    }
    
    void UpdateHealGlow()
    {
        if (healGlowTimer > 0)
        {
            healGlowTimer -= Time.deltaTime;
            
            if (healthBarBackground != null)
            {
                float glowProgress = healGlowTimer / healGlowDuration;
                Color glowColor = Color.Lerp(originalBackgroundColor, healGlowColor, 
                    Mathf.Sin(glowProgress * Mathf.PI) * healGlowIntensity);
                healthBarBackground.color = glowColor;
            }
        }
    }
    
    void UpdateCriticalHealthEffect()
    {
        if (isCriticalHealth && enableCriticalHealthEffect)
        {
            if (healthBarFill != null)
            {
                // Efecto de pulso para salud crítica
                float pulse = Mathf.Sin(Time.time * criticalHealthPulseSpeed) * criticalHealthPulseIntensity + 1f;
                healthBarFill.transform.localScale = Vector3.one * pulse;
                
                // Cambiar color a rojo crítico
                Color criticalColor = Color.Lerp(healthBarFill.color, criticalHealthColor, Time.deltaTime * 2f);
                healthBarFill.color = criticalColor;
            }
        }
        else
        {
            // Restaurar escala normal
            if (healthBarFill != null)
            {
                healthBarFill.transform.localScale = Vector3.one;
            }
        }
    }
    
    void UpdateShakeEffect()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            
            // Generar offset de shake
            shakeOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0
            );
            
            // Aplicar shake
            transform.position = originalPosition + shakeOffset;
        }
        else
        {
            // Restaurar posición original
            transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * 5f);
            shakeOffset = Vector3.zero;
        }
    }
    
    // Método público para activar efectos manualmente
    public void TriggerDamageEffect()
    {
        TriggerDamageFlash();
        if (enableShakeEffect)
        {
            TriggerShakeEffect();
        }
    }
    
    public void TriggerHealEffect()
    {
        TriggerHealGlow();
    }
    
    public void SetCriticalHealthEffect(bool enabled)
    {
        enableCriticalHealthEffect = enabled;
    }
    
    // Método para configurar colores de efectos
    public void SetEffectColors(Color damageColor, Color healColor, Color criticalColor)
    {
        damageFlashColor = damageColor;
        healGlowColor = healColor;
        criticalHealthColor = criticalColor;
    }
    
    // Método para resetear efectos
    public void ResetEffects()
    {
        damageFlashTimer = 0f;
        healGlowTimer = 0f;
        shakeTimer = 0f;
        shakeOffset = Vector3.zero;
        
        if (healthBarFill != null)
        {
            healthBarFill.color = originalFillColor;
            healthBarFill.transform.localScale = Vector3.one;
        }
        
        if (healthBarBackground != null)
        {
            healthBarBackground.color = originalBackgroundColor;
        }
        
        transform.position = originalPosition;
    }
} 