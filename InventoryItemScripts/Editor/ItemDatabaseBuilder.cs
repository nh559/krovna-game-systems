using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ItemDatabaseBuilder 
{
    [MenuItem("Tools/Build Item Database")]
    public static void BuildItemDatabase()
    {
        string[] itemDatabaseGuids = AssetDatabase.FindAssets("t:ItemDatabase");
        if (itemDatabaseGuids.Length == 0)
        {
            Debug.LogError("No ItemDatabase asset was found.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(itemDatabaseGuids[0]);
        List<ItemDatabase.ItemData> rebuiltItems = new List<ItemDatabase.ItemData>();

        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        try
        {
            // In File -> Build Profiles -> Scene List loop through all scenes
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;

                Scene openedScene = EditorSceneManager.OpenScene(scene.path);
                bool sceneWasModified = false;

                GameObject[] items = GameObject.FindGameObjectsWithTag("Pickupable");
    
                foreach (GameObject item in items)
                {
                    ItemObject itemObject = item.GetComponent<ItemObject>();
                    if (itemObject == null)
                    {
                        Debug.LogWarning($"'{item.name}' is tagged Pickupable but has no ItemObject component, skipping.");
                        continue;
                    }
                    Item itemComponent = itemObject.Item;
                    if (itemComponent == null)
                    {
                        Debug.LogWarning($"'{item.name}' has an ItemObject but Item is null, skipping.");
                        continue;
                    }

                    // Generate ID for each item in scene
                    string previousItemId = itemObject.ItemID;
                    itemObject.GenerateItemID();
                    if (itemObject.ItemID != previousItemId)
                    {
                        EditorUtility.SetDirty(itemObject);
                        sceneWasModified = true;
                    }

                    ItemDatabase.ItemData itemData = new ItemDatabase.ItemData
                    {
                        itemType = GetItemType(itemComponent),
                        itemName = itemComponent.ItemName,
                        itemDescription = itemComponent.ItemDescription,
                        itemIcon = itemComponent.ItemIcon,
                        itemHealingType = itemComponent is HealingItem healingItem ? healingItem.HealingType : default,
                        assignedSpell = itemComponent is RitualPaperItem ritualPaper ? ritualPaper.AssignedSpell : default
                    };

                    rebuiltItems.Add(itemData);
                }

                if (sceneWasModified)
                {
                    EditorSceneManager.MarkSceneDirty(openedScene);
                    EditorSceneManager.SaveScene(openedScene);
                }
            }

            ItemDatabase itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
            if (itemDatabase == null)
            {
                Debug.LogError($"Failed to load ItemDatabase at path: {path}");
                return;
            }

            itemDatabase.allItems.Clear();
            itemDatabase.allItems.AddRange(rebuiltItems);

            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
            }
        }
        
        foreach (ItemDatabase.ItemData item in rebuiltItems)
        {
            Debug.Log(item);
        }
    }

    /// <summary>
    ///     Helper to get the item type as enum instead of string
    /// </summary>
    private static Item.ItemType GetItemType(Item item)
    {
        switch (item)
        {
            case HealingItem:    return Item.ItemType.HEALING;
            case StoryItem:      return Item.ItemType.STORY;
            case LoreItem:       return Item.ItemType.LORE;
            case RitualPaperItem:return Item.ItemType.RITUAL_PAPER;
            default:
                Debug.LogError($"ItemDatabaseBuilder: unknown item type '{item.GetType().Name}'");
                return default;
        }
    }
}
