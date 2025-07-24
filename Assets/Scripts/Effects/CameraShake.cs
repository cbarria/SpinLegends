using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    [Header("Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeAmount = 0.1f;
    public float decreaseFactor = 1.0f;
    
    private Vector3 originalPosition;
    private float currentShakeDuration = 0f;
    private float currentShakeAmount = 0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        originalPosition = transform.localPosition;
    }
    
    void Update()
    {
        if (currentShakeDuration > 0)
        {
            // Generate random shake offset
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeAmount;
            transform.localPosition = originalPosition + shakeOffset;
            
            // Decrease shake over time
            currentShakeDuration -= Time.deltaTime * decreaseFactor;
            
            if (currentShakeDuration <= 0)
            {
                // Reset to original position
                transform.localPosition = originalPosition;
                currentShakeDuration = 0f;
            }
        }
    }
    
    public void ShakeCamera(float intensity, float duration)
    {
        currentShakeAmount = intensity;
        currentShakeDuration = duration;
    }
    
    public void ShakeCamera()
    {
        ShakeCamera(shakeAmount, shakeDuration);
    }
    
    // Overload for different shake types
    public void ShakeCamera(ShakeType shakeType)
    {
        switch (shakeType)
        {
            case ShakeType.Light:
                ShakeCamera(0.05f, 0.3f);
                break;
            case ShakeType.Medium:
                ShakeCamera(0.1f, 0.5f);
                break;
            case ShakeType.Heavy:
                ShakeCamera(0.2f, 0.8f);
                break;
            case ShakeType.Impact:
                ShakeCamera(0.15f, 0.4f);
                break;
        }
    }
}

public enum ShakeType
{
    Light,
    Medium,
    Heavy,
    Impact
} 