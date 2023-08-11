using UnityEngine;
using System.Collections.Generic;

// returns true if the sprite is facing in a direction
public class InGameConditionalInventory : MonoBehaviour, InGameConditional
{
    public enum Mode
    {
        HasItem, HadItemThisLevel
    }

    public Mode mode;
    public int itemId;
    public List<int> alternateItemIds = new List<int>(); // 
    private bool hadItemThisLevel = false;

    public bool Evaluate()
    {
        switch (mode)
        {
            case Mode.HasItem:
                bool ret = InventoryItemsNew.InventoryHasItem(itemId);
                for (int i = 0; i < alternateItemIds.Count; ++i)
                {
                    if (ret) { break; }
                    ret |= InventoryItemsNew.InventoryHasItem(alternateItemIds[i]);
                }
                return ret;
            case Mode.HadItemThisLevel:
                return hadItemThisLevel;
        }
        return false;
    }

    public string GetInfo()
    {
        string itemName = (string)InventoryItemsNew.allItems[itemId]?["name"] ?? "N/A";
        switch (mode)
        {
            case Mode.HasItem:
                return "Doesn't have item " + itemName;
            case Mode.HadItemThisLevel:
                return "Never had " + itemName + " this level";
        }
        return "???";
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (mode == Mode.HadItemThisLevel && !hadItemThisLevel)
        {
            hadItemThisLevel |= InventoryItemsNew.InventoryHasItem(itemId);
            for (int i = 0; i < alternateItemIds.Count; ++i)
            {
                if (hadItemThisLevel) { break; }
                hadItemThisLevel |= InventoryItemsNew.InventoryHasItem(alternateItemIds[i]);
            }
        }
    }
}
