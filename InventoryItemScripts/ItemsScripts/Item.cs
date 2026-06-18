using UnityEngine;
using System;

[Serializable]
public abstract class Item
{
    public enum ItemType { STORY, HEALING, LORE, RITUAL_PAPER, SPELL_AMMO }

    [Header("Item Data")]
    [Tooltip("The name of the item.")]
    [SerializeField] protected string itemName;
    [Tooltip("The text description of the item (including lore).")]
    [TextArea(3, 10)]
    [SerializeField] protected string itemDescription;
    [Tooltip("The icon that will be displayed in the journal.")]
    [SerializeField] protected Sprite itemIcon;

    /// <summary>
    ///     Set in specific item children
    /// </summary>
    protected abstract ItemType itemType { get; set; }

    public string ItemName
    {
        get { return itemName; }
        set { itemName = value; }
    }
    public string ItemDescription
    {
        get { return itemDescription; }
        set { itemDescription = value; }
    }
    public Sprite ItemIcon
    {
        get { return itemIcon; }
        set { itemIcon = value; }
    }
}