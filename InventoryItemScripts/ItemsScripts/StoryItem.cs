using System;

[Serializable]
public class StoryItem : Item
{
    protected override ItemType itemType { get; set; } = ItemType.STORY;
}
