using UnityEngine;
using Photon.Pun;

public class SpinEffects : MonoBehaviourPun
{
    [Header("Particle Effects")]
    public ParticleSystem spinParticles;
    public ParticleSystem impactParticles;
    public ParticleSystem trailParticles;
    
    [Header("Trail Renderer")]
    public TrailRenderer spinTrail;
    public TrailRenderer movementTrail;
    
    [Header("Light Effects")]
    public Light spinLight;
    public float lightIntensity = 2f;
    public Color spinColor = Color.cyan;
    public Color normalColor = Color.white;
    
    [Header("Audio")]
    public AudioSource spinAudio;
    public AudioSource impactAudio;
    public AudioClip spinSound;
    public AudioClip impactSound;
    
    [Header("Settings")]
    public float spinSpeedThreshold = 500f;
    public float effectIntensity = 1f;
    
    private PlayerController playerController;
    private bool isSpinning = false;
    private float currentSpinSpeed = 0f;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        SetupEffects();
    }
    
    void SetupEffects()
    {
        // Setup particle systems
        if (spinParticles != null)
        {
            var emission = spinParticles.emission;
            emission.enabled = false;
        }
        
        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            emission.enabled = false;
        }
        
        // Setup trail renderers
        if (spinTrail != null)
        {
            spinTrail.enabled = false;
        }
        
        if (movementTrail != null)
        {
            movementTrail.enabled = true;
        }
        
        // Setup light
        if (spinLight != null)
        {
            spinLight.color = normalColor;
            spinLight.intensity = 0f;
        }
        
        // Setup audio
        if (spinAudio != null && spinSound != null)
        {
            spinAudio.clip = spinSound;
            spinAudio.loop = true;
            spinAudio.volume = 0f;
        }
    }
    
    void Update()
    {
        if (playerController == null) return;
        
        UpdateSpinEffects();
        UpdateMovementEffects();
    }
    
    void UpdateSpinEffects()
    {
        // Check if player is spinning
        bool shouldSpin = playerController.IsSpinning();
        
        if (shouldSpin != isSpinning)
        {
            isSpinning = shouldSpin;
            
            if (isSpinning)
            {
                StartSpinEffects();
            }
            else
            {
                StopSpinEffects();
            }
        }
        
        if (isSpinning)
        {
            // Update spin speed based on player's spin speed
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, playerController.spinSpeed, Time.deltaTime * 5f);
            
            // Update particle emission rate based on spin speed
            if (spinParticles != null)
            {
                var emission = spinParticles.emission;
                float rate = Mathf.Lerp(0f, 50f, currentSpinSpeed / spinSpeedThreshold);
                emission.rateOverTime = rate * effectIntensity;
            }
            
            // Update trail width based on spin speed
            if (spinTrail != null)
            {
                float width = Mathf.Lerp(0.1f, 0.5f, currentSpinSpeed / spinSpeedThreshold);
                spinTrail.startWidth = width * effectIntensity;
            }
            
            // Update light intensity
            if (spinLight != null)
            {
                spinLight.intensity = Mathf.Lerp(0f, lightIntensity, currentSpinSpeed / spinSpeedThreshold);
                spinLight.color = Color.Lerp(normalColor, spinColor, currentSpinSpeed / spinSpeedThreshold);
            }
            
            // Update audio volume
            if (spinAudio != null)
            {
                spinAudio.volume = Mathf.Lerp(0f, 0.5f, currentSpinSpeed / spinSpeedThreshold);
            }
        }
    }
    
    void UpdateMovementEffects()
    {
        // Update movement trail based on velocity
        if (movementTrail != null && playerController.Rigidbody != null)
        {
            float velocity = playerController.Rigidbody.linearVelocity.magnitude;
            float width = Mathf.Lerp(0.05f, 0.2f, velocity / 10f);
            movementTrail.startWidth = width;
            
            // Change trail color based on speed
            Color trailColor = Color.Lerp(Color.white, Color.yellow, velocity / 10f);
            movementTrail.startColor = trailColor;
        }
    }
    
    void StartSpinEffects()
    {
        // Start particle effects
        if (spinParticles != null)
        {
            var emission = spinParticles.emission;
            emission.enabled = true;
            spinParticles.Play();
        }
        
        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            emission.enabled = true;
            trailParticles.Play();
        }
        
        // Enable spin trail
        if (spinTrail != null)
        {
            spinTrail.enabled = true;
        }
        
        // Start audio
        if (spinAudio != null)
        {
            spinAudio.Play();
        }
    }
    
    void StopSpinEffects()
    {
        // Stop particle effects
        if (spinParticles != null)
        {
            var emission = spinParticles.emission;
            emission.enabled = false;
        }
        
        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            emission.enabled = false;
        }
        
        // Disable spin trail
        if (spinTrail != null)
        {
            spinTrail.enabled = false;
        }
        
        // Stop audio
        if (spinAudio != null)
        {
            spinAudio.Stop();
        }
        
        // Reset light
        if (spinLight != null)
        {
            spinLight.intensity = 0f;
            spinLight.color = normalColor;
        }
    }
    
    public void PlayImpactEffect(Vector3 position, float force)
    {
        // Play impact particles
        if (impactParticles != null)
        {
            impactParticles.transform.position = position;
            var emission = impactParticles.emission;
            emission.rateOverTime = force * 10f;
            impactParticles.Play();
        }
        
        // Play impact sound
        if (impactAudio != null && impactSound != null)
        {
            impactAudio.PlayOneShot(impactSound, Mathf.Clamp01(force / 10f));
        }
        
        // Camera shake effect
        CameraShake.Instance?.ShakeCamera(force * 0.1f, 0.2f);
    }
    
    public void PlaySpecialEffect(string effectName)
    {
        switch (effectName.ToLower())
        {
            case "dodge":
                PlayDodgeEffect();
                break;
            case "attack":
                PlayAttackEffect();
                break;
            case "jump":
                PlayJumpEffect();
                break;
        }
    }
    
    void PlayDodgeEffect()
    {
        // Create a quick flash effect
        if (spinLight != null)
        {
            StartCoroutine(FlashLight(0.1f, Color.white, 3f));
        }
    }
    
    void PlayAttackEffect()
    {
        // Create attack particles
        if (spinParticles != null)
        {
            var emission = spinParticles.emission;
            emission.rateOverTime = 100f;
            StartCoroutine(ResetParticleRate(0.5f));
        }
    }
    
    void PlayJumpEffect()
    {
        // Create jump particles
        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            emission.rateOverTime = 30f;
            StartCoroutine(ResetParticleRate(0.3f));
        }
    }
    
    System.Collections.IEnumerator FlashLight(float duration, Color color, float intensity)
    {
        if (spinLight == null) yield break;
        
        Color originalColor = spinLight.color;
        float originalIntensity = spinLight.intensity;
        
        spinLight.color = color;
        spinLight.intensity = intensity;
        
        yield return new WaitForSeconds(duration);
        
        spinLight.color = originalColor;
        spinLight.intensity = originalIntensity;
    }
    
    System.Collections.IEnumerator ResetParticleRate(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (spinParticles != null && isSpinning)
        {
            var emission = spinParticles.emission;
            emission.rateOverTime = 50f;
        }
        
        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            emission.rateOverTime = 20f;
        }
    }
} 