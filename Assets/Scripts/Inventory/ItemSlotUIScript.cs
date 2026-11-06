using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUIScript : MonoBehaviour
{
    [SerializeField] private Image m_icon;
    [SerializeField] private TextMeshProUGUI m_label;
    [SerializeField] private TextMeshProUGUI m_stackLabel;
    [SerializeField] private Button m_button;

    private InventoryItem m_item;

    public event Action<InventoryItemData> OnItemSelected;

    private void Awake()
    {
        if (m_button == null)
        {
            m_button = GetComponent<Button>();
        }

        if (m_button != null)
        {
            m_button.onClick.AddListener(HandleClick);
        }
    }

    public void Set(InventoryItem item)
    {
        m_item = item;
        m_icon.sprite = item.Data.icon;
        m_label.text = item.Data.displayName;

        if(item.StackSize > 1)
        {
            m_stackLabel.text = item.StackSize.ToString();
            m_stackLabel.gameObject.SetActive(true);
        } else
        {
            m_stackLabel.gameObject.SetActive(false);
        }
    }

    public void HandleClick()
    {
        if (m_item == null) return;

        OnItemSelected?.Invoke(m_item.Data);
    }

}
