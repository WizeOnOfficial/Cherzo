using UnityEngine;
using GameDevelopmentProject.Quest;

public class PhysicsPickup : MonoBehaviour, IPickupable
{
    [SerializeField] Rigidbody pickupRigidbody;
    [SerializeField] Collider pickupCollider;
    [SerializeField] Vector3 pickupPositionOffset;
    [SerializeField] InventoryPickupAcquisition inventoryAcquisition;

    public InventoryItemData referenceItem;

    public string InteractMsg =>
        inventoryAcquisition == null || inventoryAcquisition.AcquisitionGate == null ||
        inventoryAcquisition.AcquisitionGate.CanAcquire(out _)
            ? "Press E to Pickup"
            : inventoryAcquisition.AcquisitionGate.DeniedPrompt;

    private void Awake()
    {
        if (pickupRigidbody == null)
            pickupRigidbody = GetComponent<Rigidbody>();

        if (pickupCollider == null)
            pickupCollider = GetComponent<Collider>();
    }

    public void Interact(InteractionController interactionController)
    {
        if (inventoryAcquisition != null)
        {
            if (referenceItem == null)
            {
                Debug.LogError("Inventory pickup acquisition requires a reference item.", this);
                return;
            }

            if (inventoryAcquisition.TryAcquire(referenceItem))
            {
                gameObject.SetActive(false);
            }

            return;
        }

        var pickupController = interactionController != null ? interactionController.GetComponent<PickUpController>() : null;

        Grab(pickupController);

        SetPhysicsValues(true);
    }

    public void Drop(PickUpController pickUpController)
    {
        transform.parent = null;

        SetPhysicsValues(false);
        if (InventorySystem.current != null && referenceItem != null)
        {
            InventorySystem.current.Remove(referenceItem);
        }
    }

    public void Grab(PickUpController pickUpController)
    {
        if (pickUpController == null || pickUpController.HasPickup)
        {
            return;
        }

        if (InventorySystem.current != null && referenceItem != null)
        {
            InventorySystem.current.Add(referenceItem);
        }

        Destroy(this.gameObject);
    }

    public void SetPositionInParent(Transform newParent)
    {
        transform.parent = newParent;
        transform.localPosition = pickupPositionOffset;
        transform.localRotation = Quaternion.identity;

        SetPhysicsValues(true);
    }

    public void Use()
    {
        Debug.Log("USED");
    }

    void SetPhysicsValues(bool wasPickedup)
    {
        if (pickupRigidbody != null)
        {
            pickupRigidbody.isKinematic = wasPickedup;
        }

        if (pickupCollider != null)
        {
            pickupCollider.enabled = !wasPickedup;
        }
    }
}
