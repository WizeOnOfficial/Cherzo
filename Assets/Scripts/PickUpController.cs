using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpController : MonoBehaviour
{
    [SerializeField] Transform pickupHolder;
    [SerializeField] InventoryUI inventoryUI;

    IPickupable currentPickup;
    private bool isInventoryOpened;

    public bool HasPickup => currentPickup != null;

    private void OnEnable()
    {
        if(inventoryUI != null)
        {
            inventoryUI.OnItemSelected += HandleItemSelected;
            inventoryUI.OnInventoryClosed += HandleInventoryClosed;
            inventoryUI.OnInventoryOpened += HandleInventoryOpened;
        }
    }

    private void OnDisable()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnItemSelected -= HandleItemSelected;
            inventoryUI.OnInventoryClosed -= HandleInventoryClosed;
            inventoryUI.OnInventoryOpened -= HandleInventoryOpened;
        }
    }

    private void HandleItemSelected(InventoryItemData itemData)
    {
        if(itemData == null || itemData.prefab == null)
        {
            Debug.LogWarning("Selected item has no pickup prefab!");
            return;
        } 

        if(HasPickup)
        {
            Debug.Log("Already holding an item!");
            return;
        }

        if(pickupHolder == null)
        {
            Debug.LogError("Pickup Holder is not assigned!");
            return;
        }

        GameObject instance = Instantiate(itemData.prefab);
        IPickupable pickupable = instance.GetComponent<IPickupable>();

        if (pickupable == null)
        {
            Debug.LogError(
                $"Prefab '{instance.name}' does not implement IPickupable!"
            );

            Destroy(instance);
            return;
        }

        HoldPickup(pickupable); 
    }

    private void HandleInventoryOpened()
    {
        isInventoryOpened = true;
    }

    private void HandleInventoryClosed()
    {
        isInventoryOpened = false;
    }

    public void HoldPickup(IPickupable newPickup)
    {
        if(newPickup == null || HasPickup) return;
        
        currentPickup = newPickup;
        currentPickup.SetPositionInParent(pickupHolder);
    }

    void Update()
    {
        CheckDropInput();
        CheckUsePickupInput();
    }

    void CheckDropInput()
    {
        if(Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame && HasPickup && !isInventoryOpened)
        {
            currentPickup.Drop(this);
            currentPickup = null;
        }
    }

    void CheckUsePickupInput()
    {
        if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && HasPickup && !isInventoryOpened)
        {
            currentPickup.Use();  
        }
    }
}
