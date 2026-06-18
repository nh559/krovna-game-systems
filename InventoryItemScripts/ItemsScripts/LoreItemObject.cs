using UnityEngine;

public class LoreItemObject : ItemObject
{
    [Header("Lore Item Data")]
    [SerializeField] private LoreItem loreItem;
    public override Item Item
    {
        get { return loreItem; }
        set { loreItem = (LoreItem)value; }
    }

    public override void PickupItem()
    {
        LoreJournal.AddIfMissing(loreItem.ItemName, loreItem.ItemDescription, loreItem.ItemIcon);
        ItemObjectManager.AddToCollectedItems(ItemID);
        HideObject();
        Debug.Log("Picked up lore item: " + Item.ItemName);
    }

    public override void GenerateItemID()
    {
        base.GenerateItemID();
        ItemID += "_LoreItem";
        Debug.Log("Generated Item ID for Lore Item: " + ItemID);
    }
}
