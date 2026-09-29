using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float sensitivitySpeed;
    [SerializeField] float minLookValue;
    [SerializeField] float maxLookValue;
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
    }

    private void playerLook()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float rotX = this.transform.localEulerAngles.y + Mouse.current.delta.x.ReadValue() * sensitivitySpeed * Time.deltaTime;
        this.transform.localEulerAngles = new Vector3(0, rotX, 0);

        rotY += Mouse.current.delta.y.ReadValue() * sensitivitySpeed * Time.deltaTime;
        rotY = Mathf.Clamp(rotY, minLookValue, maxLookValue);
        playerCam.transform.localEulerAngles = new Vector3(-rotY, 0, 0);
        
    }
}
