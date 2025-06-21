using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemPickup> inventoryItems = new List<ItemPickup>();  // List of items in the inventory
    public bool HasItem(string itemName)
    {
        return inventoryItems.Exists(item => item.itemName == itemName);
    }

    public void AddItem(ItemPickup item)
    {
        inventoryItems.Add(item);
        Debug.Log(item.itemName + " added to inventory!");

        // Here, you can add logic for special items (e.g., parasol) if needed
        if (item.itemName == "Parasol")
        {
            Debug.Log("Parasol is now in your inventory.");
        }

        if (item.itemName == "Key")
        {
            Debug.Log("Parasol is now in your inventory.");
        }
    }
}
