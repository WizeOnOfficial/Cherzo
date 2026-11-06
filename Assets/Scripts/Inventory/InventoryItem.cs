using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    [SerializeField] private InventoryItemData data;
    [SerializeField] private int stackSize;

    public InventoryItemData Data => data;
    public int StackSize => stackSize;

    // Backward-compatibility property getters for existing quest adapter code
    public InventoryItemData itemData => data;
    
    public InventoryItem(InventoryItemData source)
    {
        data = source;
        AddToStack();
    }
    
    public void AddToStack()
    {
        stackSize++;
    }

    public void RemoveFromStack()
    {
        stackSize--;
    }
}
