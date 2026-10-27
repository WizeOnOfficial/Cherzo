using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpController : MonoBehaviour
{
    public static PickUpController Instance { get; private set; }

    [SerializeField] Transform pickupHolder;
    [SerializeField] InventoryUI inventoryUI;

    IPickupable currentPickup;
    private bool isInventoryOpened;

    public bool HasPickup => currentPickup != null;
    public IPickupable CurrentPickup => currentPickup;

    public bool IsHoldingItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || currentPickup == null)
        {
            return false;
        }

        if (currentPickup is PhysicsPickup physicsPickup && physicsPickup.referenceItem != null)
        {
            return physicsPickup.referenceItem.id == itemId;
        }

        return false;
    }

    public InventoryItemData GetHeldItemData()
    {
        if (currentPickup is PhysicsPickup physicsPickup)
        {
            return physicsPickup.referenceItem;
        }
        return null;
    }

    private void Awake()
    {
        Instance = this;

        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        EnsurePickupHolderMounted();
    }

    private void OnEnable()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI != null)
        {
            inventoryUI.OnItemSelected -= HandleItemSelected;
            inventoryUI.OnItemSelected += HandleItemSelected;
            inventoryUI.OnInventoryClosed -= HandleInventoryClosed;
            inventoryUI.OnInventoryClosed += HandleInventoryClosed;
            inventoryUI.OnInventoryOpened -= HandleInventoryOpened;
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

    public void EnsurePickupHolderMounted()
    {
        var playerCam = GetComponentInChildren<Unity.Cinemachine.CinemachineCamera>();
        if (playerCam != null)
        {
            if (pickupHolder == null)
            {
                var existing = playerCam.transform.Find("Pickup Holder");
                if (existing != null)
                {
                    pickupHolder = existing;
                }
                else
                {
                    var holderGo = new GameObject("Pickup Holder");
                    holderGo.transform.SetParent(playerCam.transform, false);
                    holderGo.transform.localPosition = new Vector3(0.25f, -0.3f, 0.65f);
                    holderGo.transform.localRotation = Quaternion.identity;
                    pickupHolder = holderGo.transform;
                }
            }
            else if (pickupHolder.parent != playerCam.transform)
            {
                pickupHolder.SetParent(playerCam.transform, false);
                pickupHolder.localPosition = new Vector3(0.25f, -0.3f, 0.65f);
                pickupHolder.localRotation = Quaternion.identity;
            }
        }
    }

    public void EquipItem(InventoryItemData itemData)
    {
        HandleItemSelected(itemData);
    }

    public void Unequip()
    {
        if (currentPickup != null)
        {
            if (currentPickup is Component comp && comp != null)
            {
                Destroy(comp.gameObject);
            }
            currentPickup = null;
        }
    }

    private void HandleItemSelected(InventoryItemData itemData)
    {
        if (itemData == null) return;

        EnsurePickupHolderMounted();

        if (pickupHolder == null)
        {
            Debug.LogError("[PickUpController] Pickup Holder is not assigned!");
            return;
        }

        // If clicking the same item, unequip (toggle off)
        if (HasPickup)
        {
            if (currentPickup is PhysicsPickup currentPhysics && currentPhysics.referenceItem == itemData)
            {
                Unequip();
                return;
            }
            // If switching to a different item, clean up the previous item first
            Unequip();
        }

        GameObject instance = null;
        if (itemData.prefab != null)
        {
            instance = Instantiate(itemData.prefab);
        }
        else
        {
            Debug.LogWarning($"[PickUpController] Selected item '{itemData.displayName}' has no prefab! Creating fallback visual.");
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = $"{itemData.displayName}_Visual";
            instance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            var pickup = instance.AddComponent<PhysicsPickup>();
            pickup.referenceItem = itemData;
        }

        IPickupable pickupable = instance.GetComponent<IPickupable>();
        if (pickupable == null)
        {
            Debug.LogError($"Prefab '{instance.name}' does not implement IPickupable!");
            Destroy(instance);
            return;
        }

        if (pickupable is PhysicsPickup pp && pp.referenceItem == null)
        {
            pp.referenceItem = itemData;
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
        if (newPickup == null) return;

        EnsurePickupHolderMounted();

        if (currentPickup != null && currentPickup != newPickup)
        {
            Unequip();
        }

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
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame && HasPickup && !isInventoryOpened)
        {
            currentPickup.Drop(this);
            currentPickup = null;
        }
    }

    void CheckUsePickupInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && HasPickup && !isInventoryOpened)
        {
            currentPickup.Use();  
        }
    }
}
