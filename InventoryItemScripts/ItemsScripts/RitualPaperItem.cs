using UnityEngine;
using System;

[Serializable]
public class RitualPaperItem : Item
{
    [SerializeField] private SpellType assignedSpell = SpellType.NONE;
    public SpellType AssignedSpell 
    {
        get { return assignedSpell; }
        set { assignedSpell = value; }
    }

    protected override ItemType itemType { get; set; } = ItemType.RITUAL_PAPER;
}
