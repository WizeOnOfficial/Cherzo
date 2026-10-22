using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    [SerializeField] private InventoryItemData data;
    [SerializeField] private int stackSize;

    public InventoryItemData Data => data;
    public int StackSize => stackSize;
    
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
