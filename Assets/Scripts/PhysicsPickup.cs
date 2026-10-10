using UnityEngine;

public class PhysicsPickup : MonoBehaviour, IPickupable
{
    [SerializeField] Rigidbody pickupRigidbody;
    [SerializeField] Collider pickupCollider;
    [SerializeField] Vector3 pickupPositionOffset;

    public InventoryItemData referenceItem;

    public string InteractMsg => "Press E to Pickup";

    public void Interact(InteractionController interactionController)
    {
        var pickupController = interactionController.GetComponent<PickUpController>();

        Grab(pickupController);

        SetPhysicsValues(true);
    }

    public void Drop(PickUpController pickUpController)
    {
        transform.parent = null;

        SetPhysicsValues(false);
    }

    public void Grab(PickUpController pickUpController)
    {
        if(pickUpController == null || pickUpController.HasPickup)
        {
            return;
        }

        pickUpController.GrabPickup(this);
        InventorySystem.current.Add(referenceItem);
        // Destroy(this);
    }

    public void SetPositionInParent(Transform newParent)
    {
        transform.parent = newParent;
        transform.localPosition = pickupPositionOffset;
        transform.localRotation = Quaternion.identity;
    }

    public void Use()
    {
        Debug.Log("USED");
    }

    void SetPhysicsValues(bool wasPickedup)
    {
        pickupRigidbody.isKinematic = wasPickedup;
        pickupCollider.enabled = !wasPickedup;
    }
}
