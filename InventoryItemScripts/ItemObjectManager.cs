using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemObjectManager : MonoBehaviour
{

    public static ItemObjectManager Instance { get; private set; }

    public void Awake()
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

    [Serializable]
    public class CollectedItemIDs
    {
        public List<string> itemIDs = new List<string>();
    }

    private CollectedItemIDs collectedItemIDs = new CollectedItemIDs();

    /// <summary>
    ///     Adds an item ID to the list of collected item IDs if it's not already present.
     ///     Called in PickupItem() of each ItemObject subclass after picking up an item, using the generated ItemID.
    /// </summary>
    /// <param name="itemID">The ID of the item to add.</param>
    public static void AddToCollectedItems(string itemID)
    {
        Debug.Log("Adding Item ID to Collected Items: " + itemID);
        if (!Instance.collectedItemIDs.itemIDs.Contains(itemID))
        {
            Instance.collectedItemIDs.itemIDs.Add(itemID);
        }
    }

    /// <summary>
    ///     Check if an item ID is in list of collected items
    ///     Used to determine whether the item should be active in scene
    /// </summary>
    /// <param name="itemID">The ID of the item to check.</param>
    /// <returns> True if the item has been collected, false otherwise. </returns>
    public static bool IsItemCollected(string itemID)
    {
        return Instance.collectedItemIDs.itemIDs.Contains(itemID);
    }

    public void SaveItemObjectIDsForCheckpoint()
    {
        CollectedItemIDs checkpointData = new CollectedItemIDs();

        foreach (string itemID in collectedItemIDs.itemIDs)
        {
            checkpointData.itemIDs.Add(itemID);
            Debug.Log("Saving Collected Item ID: " + itemID);
        }

        string json = JsonUtility.ToJson(checkpointData);
        Debug.Log("Saving Item Object IDs JSON: " + json);
        PlayerPrefs.SetString("SavedItemObjectIDs", json);
        PlayerPrefs.Save();
    }

    public void LoadItemObjectIDsFromCheckpoint()
    {
        string json = PlayerPrefs.GetString("SavedItemObjectIDs", "");
        Debug.Log("Loaded Item Object IDs JSON: " + json);

        collectedItemIDs.itemIDs.Clear();

        if (!string.IsNullOrEmpty(json))
        {
            CollectedItemIDs checkpointData = JsonUtility.FromJson<CollectedItemIDs>(json);
            if (checkpointData != null && checkpointData.itemIDs != null)
            {
                collectedItemIDs.itemIDs.AddRange(checkpointData.itemIDs);
            }

            foreach (string itemID in collectedItemIDs.itemIDs)
            {
                Debug.Log("Collected Item ID: " + itemID);
            }
        }
    }
    

}