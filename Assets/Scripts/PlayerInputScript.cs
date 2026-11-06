using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] float baseSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float sprintCooldown;
    [SerializeField] float maxStamina;

    [Header("Player Vision")]
    [SerializeField] float sensitivitySpeed;
    [SerializeField] float minLookValue;
    [SerializeField] float maxLookValue;

    [Header("Inventory UI Listener")]
    [SerializeField] private InventoryUI inventoryUI;
    
    private float movementSpeed;
    private float targetSpeed;
    public float drainRate = 10f;
    public float restoreRate = 10f;
    public float fatigueTimer = 0f;
    public float stamina;
    public float exponentialPenalty = 10f;
    bool isRunning;
    bool isFatigued;
    bool isMovementLocked;
    bool isMoving;
    Animator anim;

    public bool IsRunning => isRunning;
    public bool IsFatigued => isFatigued;

    CinemachineCamera playerCam;
    Rigidbody rb;
    Vector2 moveInput;
    float rotX;
    float rotY;

    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions.Enable();
            var playerMap = playerInput.actions.FindActionMap("Player");
            if (playerMap != null)
            {
                if (!playerMap.enabled) playerMap.Enable();
                playerInput.currentActionMap = playerMap;
                playerInput.defaultActionMap = "Player";
            }
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isMovementLocked = false;

        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (baseSpeed <= 0.1f) baseSpeed = 3.5f;
        if (sprintSpeed <= 0.1f) sprintSpeed = 6.0f;
        if (maxStamina <= 0.1f) maxStamina = 100f;

        stamina = maxStamina;
        targetSpeed = baseSpeed;
        movementSpeed = baseSpeed;

        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        if (savedSensitivity > 0.05f)
        {
            sensitivitySpeed = savedSensitivity;
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        var col = GetComponent<Collider>();
        if (col != null)
        {
            var mat = new PhysicsMaterial("PlayerNoFriction")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0f,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            col.material = mat;
        }

        playerCam = GetComponentInChildren<CinemachineCamera>();

        if(playerCam != null)
        {
            Debug.Log("Player camera mounted");
        } else
        {
            Debug.LogWarning("Player camera not found!");
        }
    }

    void LateUpdate()
    {
        if (isMovementLocked)
        {
            return;
        } else
        {
            HandlePlayerLook();
        }
    }

    private Vector2 GetCurrentMoveInput()
    {
        Vector2 input = moveInput;
        if (input.sqrMagnitude < 0.01f && Keyboard.current != null)
        {
            float h = 0f;
            float v = 0f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            input = new Vector2(h, v);
        }
        return input;
    }

    void FixedUpdate()
    {
        if (isMovementLocked)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
            return;
        }

        if (rb != null)
        {
            HandlePlayerMovement();
        }
    }

    void Update()
    {
        if (isMovementLocked)
        {
            if (anim != null) anim.SetBool("isWalking", false);
            return;
        }

        HandlePlayerSprint();

        Vector2 input = GetCurrentMoveInput();
        isMoving = input.sqrMagnitude > 0.01f;
        if (anim != null)
        {
            anim.SetBool("isWalking", isMoving);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void HandlePlayerSprint()
    {
         // Sprint Logic
        bool isShiftPressed = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        if(isFatigued)
        {
            fatigueTimer += Time.deltaTime;
            if(fatigueTimer >= sprintCooldown)
            {
                isFatigued = false;
                fatigueTimer = 0f;
            }
        }

        bool canSprint = isShiftPressed && stamina > 0f && !isFatigued;

        if(canSprint)
        {
            isRunning = true;
            targetSpeed = sprintSpeed;

            exponentialPenalty += Time.deltaTime * 1f;
            stamina -= Time.deltaTime * drainRate * exponentialPenalty;

            if(stamina <= 0f)
            {
                stamina = 0f;
                isFatigued = true;
                isRunning = false;
            }
        }
        else
        {
            isRunning = false;
            targetSpeed = baseSpeed;

            exponentialPenalty = Mathf.Max(1f, exponentialPenalty - (Time.deltaTime * 1f));

            if(!isFatigued)
            {
                stamina = Mathf.Min(maxStamina, stamina + (Time.deltaTime * restoreRate));
            }
        }

        movementSpeed = Mathf.MoveTowards(movementSpeed, targetSpeed, 10f * Time.deltaTime);
    }

    private void HandlePlayerMovement()
    {
        Vector2 input = GetCurrentMoveInput();
        float h = input.x;
        float v = input.y;
        Vector3 moveDir = (transform.forward * v + transform.right * h);
        if (moveDir.sqrMagnitude > 1f)
        {
            moveDir.Normalize();
        }

        Vector3 targetVel = moveDir * movementSpeed;
        rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

        // // // Fallback movement only if no Rigidbody is present
        // // if (rb == null)
        // // {
        // Vector2 input = GetCurrentMoveInput();
        // float h = input.x;
        // float v = input.y;
        // this.transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * movementSpeed);
        // // }
    }

    private void HandlePlayerLook()
    {
        if (Mouse.current == null || playerCam == null) return;

        // Potentially screwing up the movement if Controller connected - Only Mouse.current is expected
        rotX += Mouse.current.delta.x.ReadValue() * 0.05f * sensitivitySpeed;
        this.transform.rotation = Quaternion.Euler(0, rotX, 0);

        rotY += Mouse.current.delta.y.ReadValue() * 0.05f * sensitivitySpeed;
        rotY = Mathf.Clamp(rotY, minLookValue, maxLookValue);
        playerCam.transform.localRotation = Quaternion.Euler(-rotY, 0, 0);
    }

    private void OnEnable()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryClosed += HandleInventoryClosed;
            inventoryUI.OnInventoryOpened += HandleInventoryOpened;
        }
    }

    private void OnDisable()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryClosed -= HandleInventoryClosed;
            inventoryUI.OnInventoryOpened -= HandleInventoryOpened;
        }
    }

    private void HandleInventoryOpened()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isMovementLocked = true;
    }

    private void HandleInventoryClosed()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isMovementLocked = false;
    }
}
