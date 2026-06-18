using System;

[Serializable]
public class LoreItem : Item
{
    protected override ItemType itemType { get; set; } = ItemType.LORE;
}
