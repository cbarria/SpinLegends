using UnityEngine;
using Photon.Pun;

public class NetworkPlayerCollision : MonoBehaviourPun, IPunObservable
{
    [Header("Collision Settings")]
    public float collisionDamageMultiplier = 1f;
    public float knockbackForce = 25f; // Aumentado de 15f a 25f para más efecto
    public float minCollisionForce = 1f; // Reducido de 2f a 1f para detectar más colisiones
    public LayerMask playerLayer = 1;
    
    [Header("Effects")]
    public GameObject collisionEffectPrefab;
    public AudioClip collisionSound;
    
    private PlayerController playerController;
    private Rigidbody rb;
    private AudioSource audioSource;
    
    // Network variables for collision synchronization
    private Vector3 networkCollisionPoint;
    private bool networkCollisionOccurred;
    private float networkCollisionDamage;
    private Vector3 networkKnockbackDirection;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Solo procesar colisiones para el jugador local
        if (!photonView.IsMine) return;
        
        // Verificar si es colisión con otro jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Collision detected with player: {collision.gameObject.name}, Force: {collision.relativeVelocity.magnitude}");
            HandlePlayerCollision(collision);
        }
    }
    
    void HandlePlayerCollision(Collision collision)
    {
        PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
        if (otherPlayer == null) return;
        
        Debug.Log($"Processing collision with {otherPlayer.name}");
        
        // Calcular fuerza de colisión
        float collisionForce = collision.relativeVelocity.magnitude;
        
        // Solo procesar colisiones con fuerza mínima
        if (collisionForce < minCollisionForce) return;
        
        // Calcular daño basado en velocidad de colisión y estado de giro
        float damage = collisionForce * collisionDamageMultiplier;
        
        // Bonus de daño si está girando
        bool isSpinning = playerController.IsSpinning();
        bool otherIsSpinning = otherPlayer.IsSpinning();
        
        if (isSpinning)
        {
            damage *= 2f;
        }
        
        // Bonus adicional si ambos están girando (colisión épica)
        if (isSpinning && otherIsSpinning)
        {
            damage *= 1.5f;
        }
        
        // Calcular dirección de knockback para ambos jugadores
        Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
        Vector3 otherKnockbackDirection = -knockbackDirection; // Dirección opuesta para el otro jugador
        
        // Calcular fuerza de knockback basada en la colisión
        float actualKnockbackForce = knockbackForce * (collisionForce / 2f); // Cambiado de 3f a 2f para más fuerza
        
        // Asegurar un knockback mínimo
        actualKnockbackForce = Mathf.Max(actualKnockbackForce, knockbackForce * 0.5f);
        
        // Aplicar daño al otro jugador
        otherPlayer.photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
        
        // Aplicar knockback local inmediatamente
        rb.AddForce(knockbackDirection * actualKnockbackForce, ForceMode.Impulse);
        
        // Enviar knockback al otro jugador por RPC
        otherPlayer.photonView.RPC("ApplyKnockbackRPC", RpcTarget.All, otherKnockbackDirection, actualKnockbackForce);
        
        // Sincronizar colisión en la red
        SyncCollisionToNetwork(collision.contacts[0].point, damage, knockbackDirection);
        
        // Efectos visuales de colisión
        PlayCollisionEffects(collision.contacts[0].point, collisionForce);
        
        Debug.Log($"Collision: Damage {damage}, Knockback {actualKnockbackForce}, Force {collisionForce}");
    }
    
    void SyncCollisionToNetwork(Vector3 collisionPoint, float damage, Vector3 knockbackDirection)
    {
        // Enviar RPC para sincronizar la colisión
        photonView.RPC("SyncCollisionRPC", RpcTarget.Others, collisionPoint, damage, knockbackDirection);
    }
    
    [PunRPC]
    void SyncCollisionRPC(Vector3 collisionPoint, float damage, Vector3 knockbackDirection)
    {
        // Procesar colisión recibida de la red
        PlayCollisionEffects(collisionPoint, damage);
        
        // Aplicar knockback recibido
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
    }
    
    void PlayCollisionEffects(Vector3 collisionPoint, float collisionForce)
    {
        // Efectos de partículas
        if (collisionEffectPrefab != null)
        {
            GameObject effect = Instantiate(collisionEffectPrefab, collisionPoint, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Efectos de sonido
        if (audioSource != null && collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound, Mathf.Clamp01(collisionForce / 20f));
        }
        
        // Shake de cámara
        if (photonView.IsMine && CameraShake.Instance != null)
        {
            float shakeIntensity = Mathf.Clamp01(collisionForce / 15f);
            CameraShake.Instance.ShakeCamera(shakeIntensity * 0.3f, 0.5f);
        }
        
        Debug.Log($"Collision at point: {collisionPoint} with force: {collisionForce}");
    }
    

    
    [PunRPC]
    void ApplyKnockbackRPC(Vector3 direction, float force)
    {
        if (rb != null)
        {
            // Aplicar knockback con impulso más fuerte
            rb.AddForce(direction * force, ForceMode.Impulse);
            
            // Efectos adicionales para el knockback
            if (photonView.IsMine)
            {
                // Shake de cámara para el jugador local
                if (CameraShake.Instance != null)
                {
                    float shakeIntensity = Mathf.Clamp01(force / 15f);
                    CameraShake.Instance.ShakeCamera(shakeIntensity * 0.3f, 0.4f);
                }
                
                Debug.Log($"Knockback applied: Force {force}, Direction {direction}");
            }
            
            // Efectos visuales de knockback
            PlayCollisionEffects(transform.position, force);
        }
    }
    
    // Método para sincronizar colisiones entre jugadores
    public void SyncCollision(PlayerController otherPlayer, float damage, Vector3 knockbackDirection)
    {
        if (photonView.IsMine)
        {
            // Enviar RPC para sincronizar la colisión
            otherPlayer.photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
            otherPlayer.photonView.RPC("ApplyKnockbackRPC", RpcTarget.All, knockbackDirection, knockbackForce);
        }
    }
    
    // Implementación de IPunObservable para sincronización de red
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviar datos de colisión
            stream.SendNext(networkCollisionOccurred);
            if (networkCollisionOccurred)
            {
                stream.SendNext(networkCollisionPoint);
                stream.SendNext(networkCollisionDamage);
                stream.SendNext(networkKnockbackDirection);
                networkCollisionOccurred = false; // Reset después de enviar
            }
        }
        else
        {
            // Recibir datos de colisión
            networkCollisionOccurred = (bool)stream.ReceiveNext();
            if (networkCollisionOccurred)
            {
                networkCollisionPoint = (Vector3)stream.ReceiveNext();
                networkCollisionDamage = (float)stream.ReceiveNext();
                networkKnockbackDirection = (Vector3)stream.ReceiveNext();
                
                // Procesar colisión recibida
                ProcessReceivedCollision();
            }
        }
    }
    
    private void ProcessReceivedCollision()
    {
        // Procesar colisión recibida de la red
        if (networkCollisionOccurred)
        {
            PlayCollisionEffects(networkCollisionPoint, networkCollisionDamage);
            
            // Aplicar knockback recibido
            if (rb != null)
            {
                rb.AddForce(networkKnockbackDirection * knockbackForce, ForceMode.Impulse);
            }
            
            Debug.Log($"Network collision received at: {networkCollisionPoint} with damage: {networkCollisionDamage}");
            networkCollisionOccurred = false;
        }
    }
} 