using UnityEngine;
using System;

[Serializable]
public class HealingItem : Item
{
    [SerializeField] private HealingItemType healingType = HealingItemType.PHYSICAL;
    public HealingItemType HealingType
    {
        get { return healingType; }
        set { healingType = value; }
    }

    protected override ItemType itemType { get; set; } = ItemType.HEALING;
}
