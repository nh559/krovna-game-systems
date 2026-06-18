using UnityEngine;

public class StoryItemObject : ItemObject
{
    [Header("Story Item Data")]
    [SerializeField] private StoryItem storyItem;
    public override Item Item
    {
        get { return storyItem; }
        set { storyItem = (StoryItem)value; }
    }

    public override void PickupItem()
    {
        base.PickupItem();
        Debug.Log("Picked up story item: " + Item.ItemName);
    }

    public override void GenerateItemID()
    {
        base.GenerateItemID();
        ItemID += "_StoryItem";
        Debug.Log("Generated Item ID for Story Item: " + ItemID);
    }
}
