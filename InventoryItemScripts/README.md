Just a quick document explaining what ItemObjectManager vs. InventoryManager is

Problem : You can skip this

A MonoBehavior cannot be created as these are just in scene GameObjects. We can create new purely data based classes that do not directly exist in a scene (as they aren't GameObjects)
So, we create 2 systems that work in parallel (purely data-based and the GameObject)

(Item) is the pure data that exists only for the inventory system and databases for checkpoint
(ItemObject) is the actual gameobject which the player interacts with

InventoryManager is the manager for the purely data side of things. This is responsible for serializing the inventory for checkpoint, and removing after using the item, etc.
ItemObjectManager is the manager for the game objetc side of things. When you pick up an item and it disappears, how do we respawn the item if the player dies and reloads at a previous checkpoint?
