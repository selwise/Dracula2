using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class AdventureInventoryItem
{
    public string itemId = "item";
    public string displayName = "Item";
    public Sprite icon;
    public int count = 1;
}

public sealed class AdventureInventory : MonoBehaviour
{
    public List<AdventureInventoryItem> items = new List<AdventureInventoryItem>();
    public int selectedIndex;

    public int Count
    {
        get { return items.Count; }
    }

    public AdventureInventoryItem SelectedItem
    {
        get
        {
            if (items.Count == 0)
            {
                return null;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, items.Count - 1);
            return items[selectedIndex];
        }
    }

    public bool HasItem(string itemId)
    {
        return FindItem(itemId) != null;
    }

    public bool AddItem(string itemId, string displayName, Sprite icon)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = displayName;
        }

        AdventureInventoryItem existing = FindItem(itemId);
        if (existing != null)
        {
            existing.count++;
            selectedIndex = items.IndexOf(existing);
            return false;
        }

        AdventureInventoryItem item = new AdventureInventoryItem
        {
            itemId = itemId,
            displayName = string.IsNullOrEmpty(displayName) ? itemId : displayName,
            icon = icon,
            count = 1
        };

        items.Add(item);
        selectedIndex = items.Count - 1;
        return true;
    }

    public void SelectNext()
    {
        if (items.Count == 0)
        {
            return;
        }

        selectedIndex = (selectedIndex + 1) % items.Count;
    }

    public void SelectPrevious()
    {
        if (items.Count == 0)
        {
            return;
        }

        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex = items.Count - 1;
        }
    }

    private AdventureInventoryItem FindItem(string itemId)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemId == itemId)
            {
                return items[i];
            }
        }

        return null;
    }
}
