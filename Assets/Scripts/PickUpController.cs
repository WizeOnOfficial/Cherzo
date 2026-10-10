using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpController : MonoBehaviour
{
    [SerializeField] Transform pickupHolder;

    IPickupable currentPickup;

    public bool HasPickup => currentPickup != null;

    public void GrabPickup(IPickupable newPickup)
    {
        if(!HasPickup)
        {
            currentPickup = newPickup;

            currentPickup.SetPositionInParent(pickupHolder);
        }
    }

    void Update()
    {
        CheckDropInput();
        CheckUsePickupInput();
    }

    void CheckDropInput()
    {
        if(Keyboard.current.qKey.wasPressedThisFrame && HasPickup)
        {
            currentPickup.Drop(this);
            currentPickup = null;
        }
    }

    void CheckUsePickupInput()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame && HasPickup)
        {
            currentPickup.Use();  
        }
    }
}
