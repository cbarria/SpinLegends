using UnityEngine;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    [Header("Touch Controls")]
    public Joystick moveJoystick;
    public Button spinButton;
    public Button jumpButton;
    public Button attackButton;
    
    [Header("Settings")]
    public float joystickDeadZone = 0.1f;
    public float touchSensitivity = 1f;
    
    private PlayerController playerController;
    private bool isSpinning = false;
    private bool isAttacking = false;
    
    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        // Ya no se suscriben eventos al joystick del asset
    }
    
    void SetupTouchControls()
    {
        // No se suscriben eventos al joystick del asset
        // El joystick solo se usa para leer Horizontal y Vertical
        // Setup joystick
        if (moveJoystick != null)
        {
            // moveJoystick.OnPointerDownEvent.AddListener((v) => OnJoystickDown());
            // moveJoystick.OnPointerUpEvent.AddListener((v) => OnJoystickUp());
            // moveJoystick.OnDragEvent.AddListener((v) => OnJoystickDrag(v));
        }
        
        // Setup buttons
        if (spinButton != null)
            spinButton.onClick.AddListener(OnSpinButtonPressed);
        
        if (jumpButton != null)
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
        
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonPressed);
    }
    
    void Update()
    {
        if (playerController == null) return;
        
        // Handle joystick movement
        HandleJoystickMovement();
        
        // Handle touch gestures
        HandleTouchGestures();
    }
    
    void HandleJoystickMovement()
    {
        if (moveJoystick == null || playerController == null) return;
        
        Vector2 joystickInput = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
        
        // Apply dead zone
        if (joystickInput.magnitude > joystickDeadZone)
        {
            // Convert to world movement
            Vector3 movement = new Vector3(joystickInput.x, 0, joystickInput.y) * playerController.moveSpeed * Time.deltaTime;
            playerController.transform.Translate(movement);
        }
    }
    
    void HandleTouchGestures()
    {
        // Handle swipe gestures for special moves
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Start tracking touch
                    break;
                    
                case TouchPhase.Moved:
                    // Handle swipe gestures
                    HandleSwipeGesture(touch);
                    break;
                    
                case TouchPhase.Ended:
                    // End touch tracking
                    break;
            }
        }
    }
    
    void HandleSwipeGesture(Touch touch)
    {
        // Calculate swipe velocity
        Vector2 swipeDelta = touch.deltaPosition;
        float swipeSpeed = swipeDelta.magnitude / Time.deltaTime;
        
        // Detect different swipe types
        if (swipeSpeed > 500f) // Fast swipe
        {
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                // Horizontal swipe - dodge
                PerformDodge(swipeDelta.x > 0 ? 1 : -1);
            }
            else
            {
                // Vertical swipe - special attack
                PerformSpecialAttack(swipeDelta.y > 0 ? 1 : -1);
            }
        }
    }
    
    void PerformDodge(int direction)
    {
        if (playerController == null) return;
        
        // Perform dodge movement
        Vector3 dodgeDirection = playerController.transform.right * direction;
        playerController.transform.Translate(dodgeDirection * 2f, Space.World);
        
        // Add visual effect
        // TODO: Add dodge effect
    }
    
    void PerformSpecialAttack(int direction)
    {
        if (playerController == null) return;
        
        // Perform special attack based on direction
        if (direction > 0)
        {
            // Upward special attack
            // TODO: Implement upward attack
        }
        else
        {
            // Downward special attack
            // TODO: Implement downward attack
        }
    }
    
    // Button event handlers
    void OnJoystickDown()
    {
        // Joystick pressed down
    }
    
    void OnJoystickUp()
    {
        // Joystick released
    }
    
    void OnJoystickDrag(Vector2 position)
    {
        // Joystick being dragged
    }
    
    public void OnSpinButtonPressed()
    {
        if (playerController == null) return;
        
        isSpinning = !isSpinning;
        
        if (isSpinning)
        {
            playerController.SendMessage("StartSpin", SendMessageOptions.DontRequireReceiver);
            
            // Update button appearance
            if (spinButton != null)
            {
                spinButton.GetComponent<Image>().color = Color.red;
            }
        }
        else
        {
            playerController.SendMessage("StopSpin", SendMessageOptions.DontRequireReceiver);
            
            // Update button appearance
            if (spinButton != null)
            {
                spinButton.GetComponent<Image>().color = Color.white;
            }
        }
    }
    
    public void OnJumpButtonPressed()
    {
        if (playerController == null) return;
        
        playerController.SendMessage("Jump", SendMessageOptions.DontRequireReceiver);
    }
    
    public void OnAttackButtonPressed()
    {
        if (playerController == null) return;
        
        isAttacking = true;
        playerController.SendMessage("Attack", SendMessageOptions.DontRequireReceiver);
        
        // Reset attack state after delay
        Invoke(nameof(ResetAttackState), 0.5f);
    }
    
    void ResetAttackState()
    {
        isAttacking = false;
    }
    
    // Public methods for external control
    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }
    
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
} 