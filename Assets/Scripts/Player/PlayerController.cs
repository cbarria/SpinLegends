using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Spinning Top Settings")]
    public float spinSpeed = 2500f; // ¡GIRO EXTREMO!
    public float moveSpeed = 550000f; // ¡DESPLAZAMIENTO PERFECTO - 550K VELOCIDAD!
    public float jumpForce = 30f; // ¡Saltos súper épicos!
    public float maxHealth = 100f;
    
    [Header("Fall Detection")]
    public float fallDeathHeight = -2f; // Altura mínima antes de morir por caída (menos profundo)
    public float arenaRadius = 50f; // Radio de la arena - morir si sales de aquí
    
    [Header("Spawn Protection")]
    public float spawnImmunityDuration = 2f; // Inmunidad después del spawn
    private float spawnTime = 0f;
    private bool isImmune = false;
    
    [Header("Network Settings")]
    public float interpolationSpeed = 300f; // SÚPER RÁPIDO para velocidades extremas
    public float rotationInterpolationSpeed = 100f; // Aumentado para mejor responsividad
    public float velocityInterpolationSpeed = 45f; // Aumentado aún más para mejor responsividad
    public float spinInterpolationSpeed = 120f; // Aumentado para animación más fluida
    
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
        
        // Activar inmunidad de spawn
        spawnTime = Time.time;
        isImmune = true;
        StartCoroutine(RemoveSpawnImmunity());
        
        // Visual feedback de inmunidad (solo para jugador local)
        if (photonView.IsMine)
        {
            Debug.Log($"🛡️ INMUNIDAD ACTIVADA: Player {photonView.OwnerActorNr} inmune por {spawnImmunityDuration}s");
        }
        
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // SIN RESISTENCIA - movimiento súper libre
            rb.linearDamping = 0f; // CERO resistencia!
            rb.angularDamping = 0f; // CERO resistencia angular!
        }
        
        if (spinningTop == null)
        {
            Transform found = transform.Find("SpinningTop");
            spinningTop = found != null ? found : transform;
        }
        
        // Solo configurar controles para el jugador local
        if (photonView.IsMine)
        {
            // Hacer que el spinner gire desde el inicio
            StartSpin();
            // Vincular UI global al jugador local inmediatamente
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetLocalPlayer(this);
            }
            
            // ANDROID FIX: Setup y Show joystick en secuencia correcta
            Invoke(nameof(SetupJoystick), 0.1f);  // Primero setup
            Invoke(nameof(ShowJoystick), 0.3f);   // Luego show con delay
        }
        else
        {
            // Para jugadores remotos ya se configuró interpolación arriba
        }
    }
    
    void SetupJoystick()
    {
        // 🎯 CRITICAL: Solo el jugador LOCAL debe tener joystick
        if (!photonView.IsMine) return;
        
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
        {
            // Intentar encontrar el joystick después de un delay
            Invoke(nameof(RetryFindJoystick), 2f);
        }
        else
        {
            // Verificar que el JoystickFocusManager esté configurado
            EnsureJoystickFocusManager();
        }
    }
    
    void Update()
    {
        // 🎯 TODOS los players deben checkear su posición (caída/arena)
        CheckPositionDeath();
        
        // Solo procesar input para el jugador local
        if (!photonView.IsMine)
        {
            // Visual: giro perfecto para jugadores remotos
            if (spinningTop != null && isSpinning)
            {
                float visualSpin = 1500f; // Giro rápido pero no mareante
                spinningTop.Rotate(0f, visualSpin * Time.deltaTime, 0f, Space.Self);
            }

            return;
        }
        
        // Giro visual igual para jugador LOCAL
        if (spinningTop != null && isSpinning)
        {
            float visualSpin = 1500f; // ¡MISMO GIRO PERFECTO que los remotos!
            spinningTop.Rotate(0f, visualSpin * Time.deltaTime, 0f, Space.Self);
        }
        
        // Salto (solo local)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }
    
    void CheckPositionDeath()
    {
        // 🕳️ DETECCIÓN DE CAÍDA - TODOS los players checkean su posición
        if (transform.position.y < fallDeathHeight)
        {
            Debug.Log($"🕳️💀 CAÍDA DETECTADA: Player {photonView.OwnerActorNr} cayó a Y={transform.position.y:F1} (límite: {fallDeathHeight}) - IsMine: {photonView.IsMine}");
            // Muerte por caída - forzar respawn inmediato
            currentHealth = 0f;
            Die();
        }
        
        // 🏟️ DETECCIÓN DE LÍMITES DE ARENA - TODOS checkean límites
        Vector3 arenaCenter = Vector3.zero; // Asumiendo arena centrada en (0,0,0)
        float distanceFromCenter = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), arenaCenter);
        if (distanceFromCenter > arenaRadius)
        {
            Debug.Log($"🏟️💀 FUERA DE ARENA: Player {photonView.OwnerActorNr} salió del radio ({distanceFromCenter:F1} > {arenaRadius}) - IsMine: {photonView.IsMine}");
            currentHealth = 0f;
            Die();
        }
    }
    
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        
        if (isSpinning)
        {
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, spinSpeed * 0.3f, Time.fixedDeltaTime * 15f); // Torque reducido para giro visual controlado
            rb.AddTorque(Vector3.up * currentSpinSpeed, ForceMode.Force);
        }
        else
        {
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, 0f, Time.fixedDeltaTime * 20f); // SÚPER RÁPIDO
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
            
            // Sin deadzone - respuesta instantánea total
        }
        else
        {
            // Fallback para teclado
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        
        // Movimiento SÚPER DIRECTO - sin umbrales
        if (horizontal != 0f || vertical != 0f)
        {
            // Movimiento directo y súper rápido - multiplicador extra para más velocidad
            Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.fixedDeltaTime * 2f; // 2X más rápido
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
        if (collision == null || collision.gameObject == null) return;
        
        PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
        if (otherPlayer != null && otherPlayer.photonView != null)
        {
            // Calculate damage based on spin speed and collision force
            float collisionForce = collision.relativeVelocity.magnitude;
            float damage = collisionForce * (isSpinning ? 2f : 1f);
            
            // Aplicar daño a través de la red
            otherPlayer.photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
            otherPlayer.photonView.RPC("SetLastHitByRPC", RpcTarget.All, photonView.OwnerActorNr);
            
            // Aplicar knockback
            Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
            float knockbackForce = collisionForce * 0.5f;
            if (rb != null)
            {
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
            
            // Efectos de colisión
            PlayCollisionEffects(collision.contacts[0].point);
        }
    }
    
    void PlayCollisionEffects(Vector3 collisionPoint)
    {
        // Shake de cámara EXPLOSIVO si es el jugador local
        if (photonView.IsMine && CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeCamera(0.5f, 0.8f); // ¡SHAKE SÚPER INTENSO!
        }
    }
    
    public void TakeDamage(float damage)
    {
        // 🛡️ INMUNIDAD DE SPAWN - No recibir daño durante los primeros segundos
        if (isImmune)
        {
            Debug.Log($"🛡️ INMUNIDAD: Player {photonView.OwnerActorNr} es inmune al daño ({damage}) - {Time.time - spawnTime:F1}s desde spawn");
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        if (photonView.IsMine)
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera(0.6f, 1.0f); // ¡SHAKE EXPLOSIVO al recibir daño!
            }
        }
    }

    [PunRPC]
    public void TakeDamageRPC(float damage)
    {
        TakeDamage(damage);
    }

    private int lastHitByActor;

    [PunRPC]
    public void SetLastHitByRPC(int attackerActor)
    {
        lastHitByActor = attackerActor;
    }

    void Die()
    {
        Debug.Log($"💀 DIE: Player {photonView.OwnerActorNr} murió - IsMine: {photonView.IsMine}, Inmune: {isImmune}");
        
        // Liberar spawn point
        var spawnMgr = FindFirstObjectByType<PlayerSpawnManager>();
        if (spawnMgr != null)
        {
            spawnMgr.ReleaseSpawnPoint(photonView.OwnerActorNr);
        }
        
        if (photonView.IsMine)
        {
            if (GameManager.Instance != null) GameManager.Instance.OnLocalPlayerDied();
            
            // Dar puntos solo si fue por combate (legacy)
            if (lastHitByActor != 0)
            {
                photonView.RPC("RequestKOScoreRPC", RpcTarget.MasterClient, lastHitByActor, photonView.OwnerActorNr);
            }
            
            // SIEMPRE hacer respawn, sin importar la causa de muerte
            photonView.RPC("NotifyDeathRPC", RpcTarget.MasterClient, photonView.OwnerActorNr);
            // Ocultar joystick cuando muere
            HideJoystick();
            
            // 🎯 RESPAWN VIA EVENT: Cliente solicita respawn al Master
            Debug.Log($"📡 RESPAWN: Cliente {photonView.OwnerActorNr} solicitando respawn al Master via Event");
            var content = new object[] { photonView.OwnerActorNr, 1f };
            var options = new ExitGames.Client.Photon.RaiseEventOptions { Receivers = ExitGames.Client.Photon.ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(102, content, options, ExitGames.Client.Photon.SendOptions.SendReliable);
            // Destruir el objeto simple
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    System.Collections.IEnumerator DestroyNextFrame()
    {
        yield return null; // esperar un frame para que los RPC salgan
        if (this != null && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    System.Collections.IEnumerator RemoveSpawnImmunity()
    {
        yield return new WaitForSeconds(spawnImmunityDuration);
        isImmune = false;
        if (photonView.IsMine)
        {
            Debug.Log($"🛡️➡️⚔️ INMUNIDAD TERMINADA: Player {photonView.OwnerActorNr} ya puede recibir daño");
        }
    }

    [PunRPC]
    void NotifyDeathRPC(int actorNumber)
    {
        // Master recibe notificación de muerte
    }



    void HideJoystick()
    {
        if (!photonView.IsMine) return; // Solo para el jugador local
        
        var joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
        foreach (var joystick in joysticks)
        {
            if (joystick != null)
            {
                joystick.gameObject.SetActive(false);
                Debug.Log($"🎮❌ Joystick ocultado: {joystick.name}");
            }
        }
    }
    
    void ShowJoystick()
    {
        if (!photonView.IsMine) return; // Solo para el jugador local
        
        Debug.Log($"🎮🔍 Buscando joysticks para mostrar...");
        
        // Usar JoystickFocusManager para forzar la activación
        var joystickFocusManager = FindFirstObjectByType<JoystickFocusManager>();
        if (joystickFocusManager != null)
        {
            Debug.Log($"🎮🔄 Forzando activación de joysticks via JoystickFocusManager");
            joystickFocusManager.ForceShowJoysticks();
        }
        else
        {
            Debug.LogWarning($"🎮⚠️ JoystickFocusManager no encontrado, búsqueda manual...");
            // Fallback: búsqueda manual
            var joysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
            Debug.Log($"🎮📊 Encontrados {joysticks.Length} joysticks");
            
            if (joysticks.Length == 0)
            {
                Debug.LogWarning($"🎮⚠️ No se encontraron joysticks, buscando en Canvas...");
                // Búsqueda más profunda en Canvas
                Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (Canvas canvas in canvases)
                {
                    Joystick[] canvasJoysticks = canvas.GetComponentsInChildren<Joystick>(true);
                    if (canvasJoysticks.Length > 0)
                    {
                        Debug.Log($"🎮🔍 Encontrados {canvasJoysticks.Length} joysticks en canvas {canvas.name}");
                        joysticks = canvasJoysticks;
                        break;
                    }
                }
            }
            
            foreach (var joystick in joysticks)
            {
                if (joystick != null)
                {
                    joystick.gameObject.SetActive(true);
                    Debug.Log($"🎮✅ Joystick mostrado: {joystick.name}");
                }
            }
        }
    }

    [PunRPC]
    void RequestKOScoreRPC(int attackerActor, int victimActor)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var scoreMgr = FindFirstObjectByType<ScoreManager>();
        if (scoreMgr != null)
        {
            scoreMgr.AddScoreMaster(attackerActor, 100);
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
    
    // Método para debug del joystick (logs removidos)
    public void TestJoystick()
    {
        // Test silencioso
    }
    
    void RetryFindJoystick()
    {
        // Buscar joysticks nuevamente
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
        
        if (joystick != null)
        {
            EnsureJoystickFocusManager();
        }
    }
    
    void EnsureJoystickFocusManager()
    {
        // Verificar que el JoystickFocusManager esté configurado
        JoystickFocusManager focusManager = FindFirstObjectByType<JoystickFocusManager>();
        if (focusManager == null)
        {
            // Crear el JoystickFocusManager si no existe
            GameObject managerObj = new GameObject("JoystickFocusManager");
            focusManager = managerObj.AddComponent<JoystickFocusManager>();
            
            // Configurar el joystick en el manager
            focusManager.joysticks = new Joystick[] { joystick };
            focusManager.forceJoystickFocus = true;
            focusManager.initializationDelay = 0.5f;
        }
    }
    
    // Photon Network Synchronization
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream == null) return;
        
        if (stream.IsWriting)
        {
            // Enviar datos a otros jugadores
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isSpinning);
            stream.SendNext(currentHealth);
            stream.SendNext(currentSpinSpeed);
            if (rb != null)
            {
                stream.SendNext(rb.linearVelocity);
            }
            else
            {
                stream.SendNext(Vector3.zero);
            }
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
                
                // Interpolación de velocidad COMENTADA - estaba limitando movimiento local
                /*
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, networkVelocity, deltaTime * velocityInterpolationSpeed);
                }
                */
                
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
                
                // Sincronizar salud (umbral fino)
                if (Mathf.Abs(networkHealth - currentHealth) > 0.01f)
                {
                    currentHealth = networkHealth;
                    // No actualizar GameManager aquí: la barra global solo refleja al jugador local
                }
            }
        }
    }
} 