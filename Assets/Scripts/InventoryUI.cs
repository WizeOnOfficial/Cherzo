using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject m_slotPrefab;
    [SerializeField] private GameObject m_inventoryPanel;
    [SerializeField] private Transform m_contentContainer;

    public event System.Action OnInventoryOpened;
    public event System.Action OnInventoryClosed;
    public event System.Action<InventoryItemData> OnItemSelected;
    private bool m_isSubscribed = false;

    private void Start()
    {
        if (m_contentContainer == null)
        {
            m_contentContainer = transform;
        }
        
        if (m_inventoryPanel != null)
        {
            m_inventoryPanel.SetActive(false);
        }

        TrySubscribe();
    }

    public bool IsOpen => m_inventoryPanel != null && m_inventoryPanel.activeSelf;

    private void InventoryVisibility()
    {
        if (m_inventoryPanel == null)
        {
            Debug.LogError("[InventoryUI] m_inventoryPanel is NOT assigned in the Inspector!");
            return;
        }

        var terminal = FindFirstObjectByType<GameDevelopmentProject.Quest.VirtualTerminalUI>();
        if (terminal != null && terminal.IsOpen)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (m_inventoryPanel == null) return;

        bool newState = !m_inventoryPanel.activeSelf;
        m_inventoryPanel.SetActive(newState);

        if (newState)
        {
            OnUpdateInventory();
            OnInventoryOpened?.Invoke();
        }
        else
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            OnInventoryClosed?.Invoke();
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        TrySubscribe();
        InventoryVisibility();
    }

    private void TrySubscribe()
    {
        if (m_isSubscribed) return;

        if (InventorySystem.current != null)
        {
            InventorySystem.current.onInventoryChangedEvent += OnUpdateInventory;
            m_isSubscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (m_isSubscribed && InventorySystem.current != null)
        {
            InventorySystem.current.onInventoryChangedEvent -= OnUpdateInventory;
        }
        m_isSubscribed = false;
    }

    private void OnUpdateInventory()
    {
        if (m_inventoryPanel != null && !m_inventoryPanel.activeInHierarchy) return;

        if (InventorySystem.current == null || InventorySystem.current.inventory == null) return;

        Debug.Log("[InventoryUI] Event received! Redrawing slots. Total items: " + InventorySystem.current.inventory.Count);
        
        for (int i = m_contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(m_contentContainer.GetChild(i).gameObject);
        }
        
        DrawInventory();
    }

    public void DrawInventory()
    {
        foreach (InventoryItem item in InventorySystem.current.inventory)
        {
            AddInventorySlot(item);
        }
    }

    public void AddInventorySlot(InventoryItem item)
    {
        if (m_slotPrefab == null)
        {
            Debug.LogError("[InventoryUI] Error: m_slotPrefab is NULL on " + gameObject.name);
            return;
        }

        GameObject obj = Instantiate(m_slotPrefab, m_contentContainer, false);

        ItemSlotUIScript slot = obj.GetComponent<ItemSlotUIScript>();
        if (slot != null)
        {
            slot.Set(item);
            slot.OnItemSelected += HandleItemSelected;
        }
        else
        {
            Debug.LogError("[InventoryUI] Error: ItemSlotScript missing on prefab!");
        }
    }

    private void HandleItemSelected(InventoryItemData itemData)
    {
        OnItemSelected?.Invoke(itemData);
        ToggleInventory();
    }
}
