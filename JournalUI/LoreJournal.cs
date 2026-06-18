using System.Collections.Generic;
using UnityEngine;

public static class LoreJournal
{
    private static readonly List<LoreEntry> entries = new();

    public static IReadOnlyList<LoreEntry> Entries => entries;

    // //Static constructor runs once when the class is first accessed
    // static LoreJournal()
    // {
    //     AddIfMissing(
    //         "Book",
    //         "An old journal, thick with dust and age.\n" +
    //         "At first glance it appears ordinary, but the margins are filled with symbols and diagrams you don’t recognize.\n" +
    //         "You suspect it can be used to store more than memories, perhaps even rituals best left unwritten.", null
    //     );
    // }

    /// <summary>
    /// Adds a new lore entry only if one with the same name doesn't already exist
    /// </summary>
    /// <param name="name"> Display name of the item </param>
    /// <param name="lore"> Lore description text </param>
    /// <param name="sprite"> Image to display in the journal</param>
    /// <returns> True if added, false if duplicate or invalid </returns>
    public static bool AddIfMissing(string name, string lore, Sprite sprite)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        foreach (LoreEntry entry in entries)
        {
            if (entry.name == name)
                return false;
        }

        LoreEntry newEntry = new(name, lore, sprite);
        PlayerMovement.Instance.PlayerControllerToggle(false);
        PlayerMovement.Instance.ClearCurrentInteractable();
        UserInterface.Instance.ToggleInventory();
        UserInterface.Instance.SwitchToJournal();
        JournalUIController.Instance.OpenDetailOverlay(newEntry);
        entries.Add(newEntry);

        JournalUIController.Instance.RebuildPage();
        Debug.Log($"[LoreJournal] Added: {name}");
        return true;
    }

    public static void ClearAll()
    {
        entries.Clear();
    }
}