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

    public bool IsRunning => isRunning;
    public bool IsFatigued => isFatigued;

    CinemachineCamera playerCam;
    Rigidbody rb;
    Vector2 moveInput;
    float rotY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isMovementLocked = false;

        if (baseSpeed <= 0.1f) baseSpeed = 3.0f;
        if (sprintSpeed <= 0.1f) sprintSpeed = 5.5f;
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
            float h = moveInput.x;
            float v = moveInput.y;
            Vector3 moveDir = (transform.forward * v + transform.right * h);
            if (moveDir.sqrMagnitude > 1f)
            {
                moveDir.Normalize();
            }

            Vector3 targetVel = moveDir * movementSpeed;
            rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);
        }
    }

    void Update()
    {
        if (isMovementLocked)
        {
            return;
        } else
        {
            HandlePlayerMovement();
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void HandlePlayerMovement()
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

        // Fallback movement only if no Rigidbody is present
        if (rb == null)
        {
            float h = moveInput.x;
            float v = moveInput.y;
            this.transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * movementSpeed);
        }
    }

    private void HandlePlayerLook()
    {
        if (Mouse.current == null || playerCam == null) return;

        float rotX = this.transform.localEulerAngles.y + Mouse.current.delta.x.ReadValue() * 0.05f * sensitivitySpeed;
        this.transform.localEulerAngles = new Vector3(0, rotX, 0);

        rotY += Mouse.current.delta.y.ReadValue() * 0.05f * sensitivitySpeed;
        rotY = Mathf.Clamp(rotY, minLookValue, maxLookValue);
        playerCam.transform.localEulerAngles = new Vector3(-rotY, 0, 0);
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
