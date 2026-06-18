using UnityEngine;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance {get; private set; }
    [SerializeField] private ItemDatabase itemDatabase;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    ///     Top-level save structure for the journal. Stores entry names
    /// </summary>
    [System.Serializable]
    private class JournalCheckpointData
    {
        public List<string> entryNames = new List<string>();
    }

    /// <summary>
    ///     Saves the names of all current journal entries to PlayerPrefs.
    ///     Called in SaveInventoryForCheckpoint on checkpoint.
    /// </summary>
    public void SaveJournalForCheckpoint()
    {
        JournalCheckpointData data = new JournalCheckpointData();

        foreach (LoreEntry entry in LoreJournal.Entries)
            data.entryNames.Add(entry.name);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SavedJournal", json);
        PlayerPrefs.Save();
    }


    /// <summary>
    ///     Restores journal entries from the last checkpoint.
    ///     Clears the current journal first, then re-adds each entry by looking up its info from the item database.
    /// </summary>
    public void LoadJournalFromCheckpoint()
    {
        string json = PlayerPrefs.GetString("SavedJournal", string.Empty);

        JournalCheckpointData data = string.IsNullOrEmpty(json)
            ? new JournalCheckpointData()
            : JsonUtility.FromJson<JournalCheckpointData>(json);

        if (data == null)
        {
            Debug.LogWarning("JournalManager: failed to parse saved journal data.");
            return;
        }

        // Clear existing entries so stale post-checkpoint entries are removed
        LoreJournal.ClearAll();

        foreach (string entryName in data.entryNames)
        {
            if (!itemDatabase.GetLoreItemData(entryName, out ItemDatabase.ItemData itemData))
            {
                Debug.LogWarning($"JournalManager: could not restore entry '{entryName}' — not found in database.");
                continue;
            }

            LoreJournal.AddIfMissing(itemData.itemName, itemData.itemDescription, itemData.itemIcon);
        }
    }
}