using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    /// <summary>
    ///     Struct to hold item data for item database for rebuilding item on load
    /// </summary>
    [System.Serializable]
    public struct ItemData
    {
        public Item.ItemType itemType;
        public string itemName;
        public string itemDescription;
        public Sprite itemIcon;
        public HealingItemType itemHealingType;
        public SpellType assignedSpell;

        public override string ToString()
        {
            return $"Item Type: {itemType}, Item Name: {itemName}, Description: {itemDescription}, Icon: {(itemIcon != null ? itemIcon.name : "None")}";
        }
    }
    /// <summary>
    ///    A list of all items in the game
    /// </summary>
    public List<ItemData> allItems = new List<ItemData>();

    /// <summary>
    ///     Returns an item from the database by name
    /// </summary>
    public void GetItemObjectByName(string name, out Item item)
    {
        ItemData itemData = allItems.Find(item => item.itemName == name);
        switch (itemData.itemType)
        {
            case Item.ItemType.LORE:
                item = new LoreItem {
                    ItemName = itemData.itemName,
                    ItemDescription = itemData.itemDescription,
                    ItemIcon = itemData.itemIcon
                };
                break;
            case Item.ItemType.RITUAL_PAPER:
                item = new RitualPaperItem {
                    ItemName = itemData.itemName,
                    ItemDescription = itemData.itemDescription,
                    ItemIcon = itemData.itemIcon,
                    AssignedSpell = itemData.assignedSpell
                };
                break;
            case Item.ItemType.STORY:
                item = new StoryItem {
                    ItemName = itemData.itemName,
                    ItemDescription = itemData.itemDescription,
                    ItemIcon = itemData.itemIcon
                };
                break;
            case Item.ItemType.HEALING:
                item = new HealingItem {
                    ItemName = itemData.itemName,
                    ItemDescription = itemData.itemDescription,
                    ItemIcon = itemData.itemIcon,
                    HealingType = itemData.itemHealingType
                };
                break;
            default:
                Debug.LogError($"Unknown item type: {itemData.itemType}");
                item = null;
                return;
        }
    }

    /// <summary>
    ///     Returns raw item data by name without constructing an Item object
    ///     Use since only raw data fields is needed in journal respawn
    /// </summary>
    public bool GetLoreItemData(string name, out ItemData data)
    {
        data = allItems.Find(i => i.itemName == name);
        if (string.IsNullOrEmpty(data.itemName))
        {
            Debug.LogWarning($"ItemDatabase: no entry found for '{name}'");
            return false;
        }
        return true;
    }
}
