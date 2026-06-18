using UnityEngine;

public class HealingItemObject : ItemObject
{
    [Header("Healing Item Data")]
    [SerializeField] private HealingItem healingItem;
    public override Item Item
    {
        get { return healingItem; }
        set { healingItem = (HealingItem)value; }
    }
    
    public override void PickupItem()
    {
        base.PickupItem();
        Debug.Log("Picked up healing item: " + healingItem.HealingType);
    }

    public override void GenerateItemID()
    {
        base.GenerateItemID();
        ItemID += "_HealingItem";
        Debug.Log("Generated Item ID for Healing Item: " + ItemID);
    }
}
