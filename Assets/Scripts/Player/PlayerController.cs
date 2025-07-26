using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Spinning Top Settings")]
    public float spinSpeed = 1000f;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxHealth = 100f;
    
    [Header("Network Settings")]
    public float interpolationSpeed = 30f; // Aumentado para más suavidad
    public float rotationInterpolationSpeed = 35f; // Aumentado para más suavidad
    public float velocityInterpolationSpeed = 25f; // Aumentado para más suavidad
    public float spinInterpolationSpeed = 40f; // Nueva variable para interpolación del giro
    
    [Header("Components")]
    private Rigidbody rb;
    public Transform spinningTop;
    public ParticleSystem spinEffect;
    public Joystick joystick;
    
    [Header("Network Variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private bool networkIsSpinning;
    private float networkHealth;
    private float networkSpinSpeed;
    private Vector3 networkVelocity;
    private float lastNetworkTime;
    
    private float currentHealth;
    private bool isSpinning = false;
    private bool isGrounded = true;
    private float currentSpinSpeed = 0f;
    
    void Start()
    {
        currentHealth = maxHealth;
        networkHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        if (spinningTop == null)
        {
            Transform found = transform.Find("SpinningTop");
            spinningTop = found != null ? found : transform;
        }
        
        // Solo configurar controles para el jugador local
        if (photonView.IsMine)
        {
            // Asignar el joystick automáticamente si no está asignado
            if (joystick == null)
            {
                joystick = FindFirstObjectByType<FixedJoystick>();
                if (joystick == null)
                    joystick = FindFirstObjectByType<FloatingJoystick>();
                if (joystick == null)
                    joystick = FindFirstObjectByType<VariableJoystick>();
                if (joystick == null)
                    joystick = FindFirstObjectByType<DynamicJoystick>();
            }
            if (joystick == null)
                Debug.LogError("Joystick not found or not assigned in PlayerController. Is it in the scene and active?");
            else
            {
                Debug.Log("Joystick asignado correctamente: " + joystick.name);
                Debug.Log($"Joystick layer: {joystick.gameObject.layer}, Active: {joystick.gameObject.activeInHierarchy}");
                
                // Test del joystick después de un pequeño delay
                Invoke(nameof(TestJoystick), 1f);
            }
            
            // Hacer que el spinner gire desde el inicio
            StartSpin();
        }
        else
        {
            // Para jugadores remotos, configurar el Rigidbody para interpolación
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
    
    void Update()
    {
        // Solo procesar input para el jugador local
        if (!photonView.IsMine) return;
        
        // Solo salto y giro visual si lo necesitas
        // HandleInput();
        // UpdateSpinEffect();
        // Si quieres mantener el salto en Update, puedes dejarlo aquí:
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }
    
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        
        if (isSpinning)
        {
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, spinSpeed, Time.fixedDeltaTime * 5f);
            rb.AddTorque(Vector3.up * currentSpinSpeed, ForceMode.Force);
        }
        else
        {
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, 0f, Time.fixedDeltaTime * 10f);
        }

        HandlePhysicsMovement();
    }

    void HandlePhysicsMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // Priorizar joystick sobre teclado
        if (joystick != null)
        {
            horizontal = joystick.Horizontal;
            vertical = joystick.Vertical;
            
            // Debug para Android (solo cuando hay input significativo)
            if (Application.platform == RuntimePlatform.Android && (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f))
            {
                Debug.Log($"Joystick Input - H: {horizontal:F2}, V: {vertical:F2}");
            }
            
            // Aplicar deadzone para evitar drift
            float joystickMagnitude = new Vector2(horizontal, vertical).magnitude;
            if (joystickMagnitude < 0.1f)
            {
                horizontal = 0f;
                vertical = 0f;
            }
        }
        else
        {
            // Fallback para teclado
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        
        // Solo mover si hay input significativo
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }
    
    void HandleInput()
    {
        float horizontal = 0f;
        float vertical = 0f;
        if (joystick != null)
        {
            horizontal = joystick.Horizontal;
            vertical = joystick.Vertical;
        }
        else
        {
            // Fallback para teclado
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
        // Spin control
        if (Input.GetMouseButtonDown(0))
        {
            StartSpin();
        }
        if (Input.GetMouseButtonUp(0))
        {
            StopSpin();
        }
    }
    
    public void StartSpin()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            if (spinEffect != null)
                spinEffect.Play();
        }
    }
    
    void StopSpin()
    {
        isSpinning = false;
        if (spinEffect != null)
            spinEffect.Stop();
    }
    
    void UpdateSpinEffect()
    {
        // Si usas torque físico, puedes comentar el giro visual para evitar duplicidad
        // if (isSpinning && spinningTop != null)
        // {
        //     spinningTop.Rotate(0, spinSpeed * Time.deltaTime, 0);
        // }
    }
    
    public void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        // Handle collision with other players
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
    }
    
    void HandlePlayerCollision(Collision collision)
    {
        PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
        if (otherPlayer != null)
        {
            // Calculate damage based on spin speed and collision force
            float collisionForce = collision.relativeVelocity.magnitude;
            float damage = collisionForce * (isSpinning ? 2f : 1f);
            
            // Aplicar daño a través de la red
            otherPlayer.photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
            
            // Aplicar knockback
            Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
            float knockbackForce = collisionForce * 0.5f;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            
            // Efectos de colisión
            PlayCollisionEffects(collision.contacts[0].point);
        }
    }
    
    void PlayCollisionEffects(Vector3 collisionPoint)
    {
        // Aquí puedes agregar efectos de partículas, sonidos, etc.
                    Debug.Log($"Collision at point: {collisionPoint} with damage: {currentHealth}");
        
        // Shake de cámara si es el jugador local
        if (photonView.IsMine && CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeCamera(0.2f, 0.3f);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // No permitir salud negativa
        
        Debug.Log($"Player {photonView.Owner.NickName} received {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        
        // Update UI
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
            
        // Efectos de daño
        if (photonView.IsMine)
        {
            // Shake de cámara cuando recibes daño
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera(0.3f, 0.5f);
            }
        }
    }
    
    [PunRPC]
    public void TakeDamageRPC(float damage)
    {
        TakeDamage(damage);
    }
    
    void Die()
    {
        Debug.Log($"Jugador {photonView.Owner.NickName} ha muerto!");
        
        // Efectos de muerte
        if (photonView.IsMine)
        {
            // Shake de cámara más fuerte
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera(0.5f, 1f);
            }
        }
        
        // Desactivar el jugador
        gameObject.SetActive(false);
        
        // Respawn después de un tiempo (opcional)
        if (photonView.IsMine)
        {
            StartCoroutine(RespawnAfterDelay(3f));
        }
    }
    
    System.Collections.IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Respawn en una posición aleatoria
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Buscar NetworkManager para respawn
            NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager != null)
            {
                networkManager.RespawnPlayer();
            }
        }
    }
    
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    public Rigidbody Rigidbody => rb;
    
    // Propiedad pública para acceder a la salud actual
    public float CurrentHealth => currentHealth;
    
    // Propiedad pública para acceder a la salud máxima
    public float MaxHealth => maxHealth;
    
    // Método para debug del joystick
    public void TestJoystick()
    {
        if (joystick != null)
        {
            Debug.Log($"Joystick Test - Name: {joystick.name}, Active: {joystick.gameObject.activeInHierarchy}");
            Debug.Log($"Joystick Test - H: {joystick.Horizontal:F2}, V: {joystick.Vertical:F2}");
            Debug.Log($"Joystick Test - Layer: {joystick.gameObject.layer}, Canvas: {joystick.GetComponentInParent<Canvas>()?.name}");
        }
        else
        {
            Debug.LogError("Joystick is null in TestJoystick()");
        }
    }
    
    // Photon Network Synchronization
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviar datos a otros jugadores
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isSpinning);
            stream.SendNext(currentHealth);
            stream.SendNext(currentSpinSpeed);
            stream.SendNext(rb.linearVelocity);
        }
        else
        {
            // Recibir datos de otros jugadores
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkIsSpinning = (bool)stream.ReceiveNext();
            networkHealth = (float)stream.ReceiveNext();
            networkSpinSpeed = (float)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            lastNetworkTime = Time.time;
            
            // Aplicar interpolación para movimiento suave
            if (!photonView.IsMine)
            {
                // Interpolación de posición más suave con delta time compensado
                float deltaTime = Mathf.Min(Time.deltaTime, 0.1f); // Limitar delta time para evitar saltos
                transform.position = Vector3.Lerp(transform.position, networkPosition, deltaTime * interpolationSpeed);
                
                // Interpolación de rotación más suave
                transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, deltaTime * rotationInterpolationSpeed);
                
                // Interpolación de velocidad para física más suave
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, networkVelocity, deltaTime * velocityInterpolationSpeed);
                
                // Sincronizar estado de giro
                if (networkIsSpinning != isSpinning)
                {
                    isSpinning = networkIsSpinning;
                    if (isSpinning)
                        StartSpin();
                    else
                        StopSpin();
                }
                
                // Aplicar giro visual basado en la velocidad de red (más suave)
                if (spinningTop != null && isSpinning)
                {
                    // Interpolación más suave del giro
                    float targetRotation = spinningTop.rotation.eulerAngles.y + networkSpinSpeed * deltaTime;
                    float currentRotation = spinningTop.rotation.eulerAngles.y;
                    
                    // Usar LerpAngle para interpolación suave de ángulos
                    float smoothRotation = Mathf.LerpAngle(currentRotation, targetRotation, deltaTime * spinInterpolationSpeed);
                    spinningTop.rotation = Quaternion.Euler(0, smoothRotation, 0);
                }
                else if (spinningTop == null)
                {
                    Debug.LogWarning("spinningTop is null in OnPhotonSerializeView - trying to find it");
                    // Intentar encontrar el spinningTop si es null
                    spinningTop = transform.Find("SpinningTop");
                    if (spinningTop == null)
                    {
                        spinningTop = transform.GetChild(0); // Tomar el primer hijo como fallback
                    }
                }
                
                // Sincronizar salud
                if (Mathf.Abs(networkHealth - currentHealth) > 1f)
                {
                    currentHealth = networkHealth;
                    if (GameManager.Instance != null)
                        GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
                }
            }
        }
    }
} 