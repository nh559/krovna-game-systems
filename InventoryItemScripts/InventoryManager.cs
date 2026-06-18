using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{

    public static InventoryManager Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    ///     List of the inventory item slots from hierarchy
    /// </summary>
    [SerializeField] private List<InventoryItemSlot> inventoryItems;
    /// <summary>
    ///    The item database to reference for loading items by name
    /// </summary>
    [SerializeField] private ItemDatabase itemDatabase;

    /// <summary>
    ///     The item currently selected
    /// </summary>
    private Item selectedItem = null;
    public Item SelectedItem => selectedItem;
    private int selectedItemIndex = -1;

    /// <summary>
    ///     Adds an item to the inventory
    /// </summary>
    /// <param name="item"> The item to add </param>
    /// <returns> True if the item was added, false if inventory is full </returns>  
    public bool AddItemToInventory(Item item)
    {
        foreach (InventoryItemSlot inventoryItem in inventoryItems)
        {
            if (inventoryItem.ItemInSlot == null)
            {
                inventoryItem.SetInventoryItemSlotIcon(item.ItemIcon);
                inventoryItem.ItemInSlot = item;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///     Sets the selected item
    ///     When an InventoryItem detects pointer click, set selected item as its item
    /// </summary>
    /// <param name="item"> The item to set as selected </param>
    public void SetSelectedItem(Item item)
    {
        selectedItem = item;
        selectedItemIndex = inventoryItems.FindIndex(i => i.ItemInSlot == item);

        // Deselect all slots then highlight the clicked one
        foreach (InventoryItemSlot slot in inventoryItems)
            slot.SetSelected(false);
        inventoryItems[selectedItemIndex].SetSelected(true);

        UserInterface.Instance.ClearItemDetails();
        
        switch (item)
        {
            case HealingItem:
                UserInterface.Instance.ShowHealingItemAction((HealingItem)item);
                break;
            case StoryItem:
                UserInterface.Instance.ShowStoryItemAction(item.ItemName, item.ItemDescription, item.ItemIcon);
                break;
            default:
                UserInterface.Instance.HideItemAction();
                break;
        }
    }

    /// <summary>
    ///     Uses the selected item
    /// </summary>
    public void DeleteSelectedItem()
    {
        inventoryItems[selectedItemIndex].ClearInventoryItemSlotIcon();
        inventoryItems[selectedItemIndex].SetSelected(false);
        inventoryItems[selectedItemIndex].ItemInSlot = null;
        selectedItemIndex = -1;
        selectedItem = null;
    }

    /// <summary>
    ///     Called by RitualDrawingComparison on success.
    ///     Reads the spell directly from the selected paper instead of matching by name.
    /// </summary>
    public void RitualItemSuccess()
    {
        if (selectedItem is not RitualPaperItem paper)
        {
            Debug.LogError("Item is not a RitualPaperItem.");
            return;
        }

        if (paper.AssignedSpell == SpellType.NONE)
        {
            Debug.LogError($"Ritual paper has no assigned spell.");
            return;
        }

        GameManager.Instance.UnlockSpell(paper.AssignedSpell);
        Debug.Log($"InventoryManager: unlocked {paper.AssignedSpell}");
        DeleteSelectedItem();
    }

    //-------------
    //SAVE / LOAD
    //-------------

    /// <summary>
    ///     Top-level save structure written to PlayerPrefs as JSON.
    ///     Stores the full inventory slot list and a snapshot of which spells were unlocked at the moment of saving
    /// </summary>
    [Serializable]
    private class InventoryCheckpointData
    {
        public List<string> itemNames = new List<String>();
    }


    /// <summary>
    ///     Saves the current inventory and spell unlock state
    ///     Clears current spell and item pickup state
    /// </summary>
    public void SaveInventoryForCheckpoint()
    {
        InventoryCheckpointData checkpointData = new InventoryCheckpointData();
        foreach (InventoryItemSlot inventoryItem in inventoryItems)
            checkpointData.itemNames.Add(inventoryItem.ItemInSlot?.ItemName ?? "None");

        string json = JsonUtility.ToJson(checkpointData);
        Debug.Log($"{json}");
        PlayerPrefs.SetString("SavedInventory", json);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    ///     Loads inventory, revert last learned spell, items are reconstructed
    ///     
    /// </summary>
    public void LoadInventoryFromCheckpoint()
    {
        string json = PlayerPrefs.GetString("SavedInventory", string.Empty);
        Debug.Log($"{json}");

        InventoryCheckpointData checkpointData = string.IsNullOrEmpty(json)
            ? new InventoryCheckpointData()
            : JsonUtility.FromJson<InventoryCheckpointData>(json);

        if (checkpointData == null) return;

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            inventoryItems[i].ClearInventoryItemSlotIcon();
            inventoryItems[i].ItemInSlot = null;

            if (i >= checkpointData.itemNames.Count) continue;

            string itemName = checkpointData.itemNames[i];
            if (itemName == "None") continue;

            itemDatabase.GetItemObjectByName(itemName, out Item item);
            if (item == null) continue;

            inventoryItems[i].SetInventoryItemSlotIcon(item.ItemIcon);
            inventoryItems[i].ItemInSlot = item;
        }
    }
}