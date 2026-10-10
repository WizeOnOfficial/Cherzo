using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float sensitivitySpeed;
    [SerializeField] float minLookValue;
    [SerializeField] float maxLookValue;
    [SerializeField] float sprintSpeed;
    [SerializeField] public float sprintCooldown;
    CinemachineCamera playerCam;
    Vector2 moveInput;
    float rotY;

    void Start()
    {
        playerCam = GetComponentInChildren<CinemachineCamera>();

        if(playerCam != null)
        {
            Debug.Log("Player camera mounted");
        } else
        {
            Debug.LogWarning("Player camera not found!");
        }
    }

    void Update()
    {
        playerMovement();
        playerLook();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void playerMovement()
    {
        float h = moveInput.x;
        float v = moveInput.y;
        this.transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * movementSpeed);

        if(Keyboard.current.shiftKey.ReadValue() > 0f)
        {
            movementSpeed = Mathf.Lerp(movementSpeed, movementSpeed * 1.5f, 2f * Time.deltaTime);
        }

        if(Keyboard.current.shiftKey.wasReleasedThisFrame)
        {
            movementSpeed = Mathf.Lerp(movementSpeed, movementSpeed / 1.5f, 2f * Time.deltaTime);
        }
    }

    private void playerLook()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float rotX = this.transform.localEulerAngles.y + Mouse.current.delta.x.ReadValue() * 5f * sensitivitySpeed * Time.deltaTime;
        this.transform.localEulerAngles = new Vector3(0, rotX, 0);

        rotY += Mouse.current.delta.y.ReadValue() * 5f * sensitivitySpeed * Time.deltaTime;
        rotY = Mathf.Clamp(rotY, minLookValue, maxLookValue);
        playerCam.transform.localEulerAngles = new Vector3(-rotY, 0, 0);
        
    }
}
