using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Spinning Top Settings")]
    public float spinSpeed = 1000f;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxHealth = 100f;
    
    [Header("Components")]
    private Rigidbody rb;
    public Transform spinningTop;
    public ParticleSystem spinEffect;
    public Joystick joystick;
    
    private float currentHealth;
    private bool isSpinning = false;
    private bool isGrounded = true;
    
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        if (spinningTop == null)
        {
            Transform found = transform.Find("SpinningTop");
            spinningTop = found != null ? found : transform;
        }
        // Asignar el joystick automáticamente si no está asignado
        if (joystick == null)
        {
            joystick = FindFirstObjectByType<FixedJoystick>();
            if (joystick == null)
                joystick = FindFirstObjectByType<FloatingJoystick>();
            if (joystick == null)
                joystick = FindFirstObjectByType<VariableJoystick>();
        }
        if (joystick == null)
            Debug.LogError("Joystick no encontrado o no asignado en PlayerController.");
        else
            Debug.Log("Joystick asignado correctamente: " + joystick.name);
        
        // Hacer que el spinner gire desde el inicio
        StartSpin();
    }
    
    void Update()
    {
        // Solo salto y giro visual si lo necesitas
        // HandleInput();
        // UpdateSpinEffect();
        // Si quieres mantener el salto en Update, puedes dejarlo aquí:
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    
    void FixedUpdate()
    {
        if (isSpinning)
            rb.AddTorque(Vector3.up * spinSpeed, ForceMode.Force);

        HandlePhysicsMovement();
    }

    void HandlePhysicsMovement()
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
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
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
            TakeDamage(damage);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        // Update UI
        GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
    }
    
    void Die()
    {
        // Handle player death
        gameObject.SetActive(false);
    }
    
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    public Rigidbody Rigidbody => rb;
} 