using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -15); // Aumentado altura de 5 a 8, distancia de -10 a -15
    public float smoothSpeed = 5f;
    public float rotationSpeed = 2f;
    
    [Header("Camera Controls")]
    public bool enableMouseLook = true;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -45f; // Cambiado de -30f a -45f para ver más arena
    public float maxVerticalAngle = 45f; // Cambiado de 60f a 45f para mejor ángulo
    
    [Header("Mobile Settings")]
    public bool disableMouseOnMobile = true;
    public float joystickInputThreshold = 0.1f;
    
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Vector3 desiredPosition;
    private Vector3 smoothedPosition;
    private Joystick joystick;
    
    void Start()
    {
        if (target == null)
        {
            // Buscar automáticamente el jugador local
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController player in players)
            {
                if (player.photonView.IsMine)
                {
                    target = player.transform;
                    break;
                }
            }
        }
        
        // Buscar el joystick para detectar input
        joystick = FindFirstObjectByType<FixedJoystick>();
        if (joystick == null)
            joystick = FindFirstObjectByType<FloatingJoystick>();
        if (joystick == null)
            joystick = FindFirstObjectByType<VariableJoystick>();
        
        // Configurar posición inicial
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
            
            // Calcular rotación inicial
            Vector3 direction = (target.position - transform.position).normalized;
            currentRotationY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentRotationX = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Manejar input del mouse para rotación (solo si no hay input del joystick)
        if (enableMouseLook && !IsJoystickActive())
        {
            HandleMouseInput();
        }
        
        // Calcular posición deseada
        Vector3 rotatedOffset = Quaternion.Euler(currentRotationX, currentRotationY, 0) * offset;
        desiredPosition = target.position + rotatedOffset;
        
        // Suavizar movimiento de la cámara
        smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        
        // Hacer que la cámara mire al jugador
        transform.LookAt(target);
    }
    
    bool IsJoystickActive()
    {
        // En Android, deshabilitar mouse si hay input del joystick
        if (disableMouseOnMobile && Application.platform == RuntimePlatform.Android)
        {
            if (joystick != null)
            {
                float joystickMagnitude = new Vector2(joystick.Horizontal, joystick.Vertical).magnitude;
                return joystickMagnitude > joystickInputThreshold;
            }
        }
        return false;
    }
    
    void HandleMouseInput()
    {
        // Solo procesar mouse si no hay input del joystick
        if (IsJoystickActive()) return;
        
        // Rotación horizontal (Y) - solo con click derecho para evitar conflictos
        if (Input.GetMouseButton(1)) // Click derecho para rotar
        {
            currentRotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        }
        
        // Rotación vertical (X) - solo con click derecho
        if (Input.GetMouseButton(1))
        {
            currentRotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target != null)
        {
            // Configurar posición inicial para el nuevo target
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }
    
    public void ResetCamera()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
            
            // Resetear rotaciones
            currentRotationX = 0f;
            currentRotationY = 0f;
        }
    }
    
    // Método para cambiar la distancia de la cámara
    public void SetCameraDistance(float distance)
    {
        offset = new Vector3(0, offset.y, -distance);
    }
    
    // Método para cambiar la altura de la cámara
    public void SetCameraHeight(float height)
    {
        offset = new Vector3(offset.x, height, offset.z);
    }
} 