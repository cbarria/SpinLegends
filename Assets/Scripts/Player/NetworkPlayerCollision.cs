using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPlayerCollision : MonoBehaviourPun, IPunObservable
{
    [Header("Collision Settings")]
    public float collisionDamageMultiplier = 1f;
    public float knockbackForce = 25f; // Aumentado de 15f a 25f para más efecto
    public float minCollisionForce = 1f; // Reducido de 2f a 1f para detectar más colisiones
    public LayerMask playerLayer = 1;
    
    [Header("Spin Impact Settings")]
    [Tooltip("Factor para convertir velocidad angular (rad/s) a contribución de fuerza de colisión.")]
    public float angularImpactScale = 0.15f;
    [Tooltip("Tiempo mínimo entre colisiones procesadas con el mismo jugador (para evitar spam)")]
    public float collisionCooldown = 0.15f;
    
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

    private float lastCollisionTime;
    
    void Awake()
    {
        // Asignaciones tempranas
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    void Start()
    {
        // Asegurar referencias por si el orden de inicialización difiere en remoto
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (rb == null) rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Collision detected with player: {collision.gameObject.name}, Force: {collision.relativeVelocity.magnitude}");
            HandlePlayerCollision(collision);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!photonView.IsMine) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time - lastCollisionTime < collisionCooldown) return;
        // Calcular fuerza efectiva incluyendo giro (golpes laterales)
        float effectiveForce = ComputeEffectiveCollisionForce(collision);
        if (effectiveForce >= minCollisionForce * 0.6f) // un poco más permisivo en Stay
        {
            HandlePlayerCollisionWithForce(collision, effectiveForce);
            lastCollisionTime = Time.time;
        }
    }
    
    float ComputeEffectiveCollisionForce(Collision collision)
    {
        float linear = collision.relativeVelocity.magnitude;
        // Contribución de giro: usar velocidad angular alrededor del eje Y como base
        float myAngular = rb != null ? Mathf.Abs(rb.angularVelocity.y) : 0f;
        Rigidbody otherRb = collision.rigidbody;
        float otherAngular = otherRb != null ? Mathf.Abs(otherRb.angularVelocity.y) : 0f;
        float angularContribution = (myAngular + otherAngular) * angularImpactScale;
        float total = linear + angularContribution;
        return Mathf.Max(total, minCollisionForce * 0.5f);
    }
    
    void HandlePlayerCollision(Collision collision)
    {
        PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
        if (otherPlayer == null) return;
        Debug.Log($"Processing collision with {otherPlayer.name}");
        float collisionForce = collision.relativeVelocity.magnitude;
        if (collisionForce < minCollisionForce) return;
        ApplyCollisionEffects(collision, otherPlayer, collisionForce);
    }

    void HandlePlayerCollisionWithForce(Collision collision, float collisionForce)
    {
        PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
        if (otherPlayer == null) return;
        Debug.Log($"Processing side/continuous collision with {otherPlayer.name} (F={collisionForce:F2})");
        ApplyCollisionEffects(collision, otherPlayer, collisionForce);
    }

    void ApplyCollisionEffects(Collision collision, PlayerController otherPlayer, float collisionForce)
    {
        // Calcular daño basado en velocidad de colisión y estado de giro
        float damage = collisionForce * collisionDamageMultiplier;
        bool isSpinning = playerController != null && playerController.IsSpinning();
        bool otherIsSpinning = otherPlayer.IsSpinning();
        if (isSpinning) damage *= 2f;
        if (isSpinning && otherIsSpinning) damage *= 1.5f;
        // Dirección de knockback basada en el punto de contacto (más estable para choques laterales)
        Vector3 contactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : otherPlayer.transform.position;
        Vector3 knockbackDirection = (transform.position - contactPoint).normalized;
        Vector3 otherKnockbackDirection = -knockbackDirection;
        float actualKnockbackForce = Mathf.Max(knockbackForce * 0.5f, knockbackForce * (collisionForce / 2f));
        actualKnockbackForce = Mathf.Clamp(actualKnockbackForce, knockbackForce * 0.5f, knockbackForce * 2.5f);
        // Aplicar daño al otro jugador
        otherPlayer.photonView.RPC("SetLastHitByRPC", RpcTarget.All, photonView.OwnerActorNr);
        otherPlayer.photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
        if (rb == null) rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * actualKnockbackForce, ForceMode.Impulse);
        }
        otherPlayer.photonView.RPC("ApplyKnockbackRPC", RpcTarget.All, otherKnockbackDirection, actualKnockbackForce);
        SyncCollisionToNetwork(contactPoint, damage, knockbackDirection);
        PlayCollisionEffects(contactPoint, collisionForce);
        Debug.Log($"Collision: Damage {damage}, Knockback {actualKnockbackForce}, Force {collisionForce}");
    }
    
    void SyncCollisionToNetwork(Vector3 collisionPoint, float damage, Vector3 knockbackDirection)
    {
        photonView.RPC("SyncCollisionRPC", RpcTarget.Others, collisionPoint, damage, knockbackDirection);
    }
    
    [PunRPC]
    void SyncCollisionRPC(Vector3 collisionPoint, float damage, Vector3 knockbackDirection)
    {
        // Lazy-assign para instancias remotas que aún no resolvieron rb
        if (rb == null) rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("⚠️ NetworkPlayerCollision: Rigidbody aún es null en SyncCollisionRPC. Reintentando en siguiente frame.");
            StartCoroutine(DelayedApplyKnockback(knockbackDirection));
        }
        PlayCollisionEffects(collisionPoint, damage);
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }
    
    System.Collections.IEnumerator DelayedApplyKnockback(Vector3 dir)
    {
        yield return null;
        if (rb == null) rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
        }
    }
    
    void PlayCollisionEffects(Vector3 collisionPoint, float collisionForce)
    {
        if (collisionEffectPrefab != null)
        {
            GameObject effect = Instantiate(collisionEffectPrefab, collisionPoint, Quaternion.identity);
            Destroy(effect, 2f);
        }
        if (audioSource != null && collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound, Mathf.Clamp01(collisionForce / 20f));
        }
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
        if (rb == null) rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
            if (photonView.IsMine && CameraShake.Instance != null)
            {
                float shakeIntensity = Mathf.Clamp01(force / 15f);
                CameraShake.Instance.ShakeCamera(shakeIntensity * 0.3f, 0.4f);
            }
            PlayCollisionEffects(transform.position, force);
        }
    }
    
    public void SyncCollision(PlayerController otherPlayer, float damage, Vector3 knockbackDirection)
    {
        if (photonView.IsMine)
        {
            otherPlayer.photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
            otherPlayer.photonView.RPC("ApplyKnockbackRPC", RpcTarget.All, knockbackDirection, knockbackForce);
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(networkCollisionOccurred);
            if (networkCollisionOccurred)
            {
                stream.SendNext(networkCollisionPoint);
                stream.SendNext(networkCollisionDamage);
                stream.SendNext(networkKnockbackDirection);
                networkCollisionOccurred = false;
            }
        }
        else
        {
            networkCollisionOccurred = (bool)stream.ReceiveNext();
            if (networkCollisionOccurred)
            {
                networkCollisionPoint = (Vector3)stream.ReceiveNext();
                networkCollisionDamage = (float)stream.ReceiveNext();
                networkKnockbackDirection = (Vector3)stream.ReceiveNext();
                ProcessReceivedCollision();
            }
        }
    }
    
    private void ProcessReceivedCollision()
    {
        if (networkCollisionOccurred)
        {
            PlayCollisionEffects(networkCollisionPoint, networkCollisionDamage);
            if (rb == null) rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(networkKnockbackDirection * knockbackForce, ForceMode.Impulse);
            }
            Debug.Log($"Network collision received at: {networkCollisionPoint} with damage: {networkCollisionDamage}");
            networkCollisionOccurred = false;
        }
    }
} 