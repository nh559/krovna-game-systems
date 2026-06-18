using UnityEngine;

/// <summary>
/// Data container for a single journal/lore entry.
/// Stores the item name, lore description, and its display sprite.
/// </summary>
[System.Serializable]
public class LoreEntry
{
    public string name;
    public string lore;
    public Sprite sprite;

    public LoreEntry(string name, string lore, Sprite sprite = null)
    {
        this.name = name;
        this.lore = lore;
        this.sprite = sprite;
    }
}