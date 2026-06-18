using UnityEngine;

public class RitualPaperItemObject : ItemObject
{
    [Header("Ritual Paper Item Data")]
    [SerializeField] private RitualPaperItem ritualPaperItem;
    public override Item Item
    {
        get { return ritualPaperItem; }
        set { ritualPaperItem = (RitualPaperItem)value; }
    }

    public override void PickupItem() { }

    public override void GenerateItemID()
    {
        base.GenerateItemID();
        ItemID += "_RitualPaperItem";
        Debug.Log("Generated Item ID for Ritual Paper Item: " + ItemID);
    }

    /// <summary>
    ///     Directly opens the drawing screen without adding to inventory.
    /// </summary>
    public void Interact()
    {
        UserInterface.Instance.ShowInventory();
        UserInterface.Instance.SwitchToRitual();
        RitualDrawingComparison.Instance.InitializeRitual(ritualPaperItem, ritualPaperItem.AssignedSpell, gameObject);
    }

    /// <summary>
    ///     Called by RitualDrawingComparison on success.
    ///     Hides the paper and registers it with ItemObjectManager so ReloadItem in ItemObject correctly shows/hides it on checkpoint load.
    /// </summary>
    public void OnRitualSuccess()
    {
        HideObject();
        ItemObjectManager.AddToCollectedItems(ItemID);
    }
}
