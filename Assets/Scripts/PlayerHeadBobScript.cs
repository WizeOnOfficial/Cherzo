using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHeadBobScript : MonoBehaviour
{
    [Range(0.001f, 0.05f)]
    public float Amount = 0.005f;
    
    [Range(1f, 30f)]
    public float Frequency = 10.0f;

    [Range(10f, 100f)]
    public float Smooth = 10.0f;

    Vector3 startPos;
    public float horizontalInput;
    public float verticalInput;
    public float timer = 0f;

    void Start()
    {
        startPos = this.transform.localPosition;
    }

    void Update()
    {
        CheckForHeadbobTrigger();
        Debug.Log("Horizontal Value: " + horizontalInput);
    }

    private void CheckForHeadbobTrigger()
    {
        float inputMagnitude = new Vector3(horizontalInput, 0, verticalInput).magnitude;

        if(inputMagnitude > 0)
        {
            CheckAndApplyHeadbob();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveVector = context.ReadValue<Vector2>();
        horizontalInput = moveVector.x;
        verticalInput = moveVector.y;
    }
    
    private void CheckAndApplyHeadbob()
    {
        float inputMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;

        if (inputMagnitude > 0.1f)
        {
            // Advance timer based on frequency when moving
            timer += Time.deltaTime * Frequency;

            // Calculate target offsets relative to starting local position
            Vector3 targetPos = startPos + new Vector3(
                Mathf.Cos(timer / 2f) * Amount * 1.6f,
                Mathf.Sin(timer) * Amount * 1.4f,
                0f
            );

            // Smoothly move towards the bobbing position
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * Smooth);
        }
        else
        {
            // Reset timer and smoothly return camera to default starting position
            timer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * Smooth);
        }
    }

    // private Vector3 StartHeadbob()
    // {
    //     Vector3 pos = Vector3.zero;
    //     pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * Frequency) * Amount * 1.4f, Smooth * Time.deltaTime);
    //     pos.x += Mathf.Lerp(pos.x, Mathf.Sin(Time.time * Frequency / 2f) * Amount * 1.6f, Smooth * Time.deltaTime);

    //     return pos;
    // }

    // private void StopHeadbob()
    // {
    //     if (this.transform.localPosition == startPos) return;
    //     this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, startPos, 1 * Time.deltaTime);
    // }
}
