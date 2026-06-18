# Krovna Systems Showcase
 
A curated selection of C# scripts I authored for **Krovna**, a first-person Unity horror game built as a senior capstone project at Drexel University. I served as **UI Systems Lead** on the team.
 
> This is not a runnable project. Krovna depends on a full Unity project — scenes, prefabs, assets, and third-party packages — that isn't included here. This repo exists to showcase the architecture and implementation of systems I personally designed and built.
 
---
 
## Systems I Designed
 
### Player Controller: `PlayerScripts/`
 
First-person player controller built on Unity's Input System and `CharacterController`.
 
- **`PlayerMovement.cs`**: Handles movement, sprinting, camera look with clamped vertical rotation, and raycast-based interaction detection for in-world objects and doors. Implemented as a singleton that persists across scene loads.
---
 
### Journal System: `JournalUI/`
 
An in-game journal that tracks lore entries collected throughout the game and persists state across scene transitions and checkpoint reloads.
 
| File | Responsibility |
|---|---|
| `LoreEntry.cs` | Scriptable Object defining a lore entry (title, text, image) |
| `LoreJournal.cs` | Static singleton managing the collection of discovered entries |
| `JournalManager.cs` | Coordinates journal open/close state with the broader UI system |
| `JournalUIController.cs` | Drives the journal UI panel and populates entry displays |
| `JournalSlot.cs` | Individual slot component for a single journal entry in the list |
 
---
 
### Inventory & Item System: `InventoryItemScripts/`
 
The inventory items and UI system of the game. Items exist as two parallel representations: a pure data class (`Item`) for serialization and inventory logic, and a `MonoBehaviour`-based `ItemObject` for in-scene interaction. This is because Unity MonoBehaviours cannot be instantiated as plain data objects, so keeping the two seperate is a more straightforward solution.
 
#### Data / Item Types: `ItemsScripts/`
 
| File | Responsibility |
|---|---|
| `Item.cs` / `ItemObject.cs` | Base classes for the parallel data/GameObject model |
| `HealingItem.cs` / `HealingItemObject.cs` | Consumable healing items |
| `LoreItem.cs` / `LoreItemObject.cs` | Collectible lore documents |
| `StoryItem.cs` / `StoryItemObject.cs` | Key story items |
| `RitualPaperItem.cs` / `RitualPaperItemObject.cs` | Ritual puzzle items (see minigame below) |
 
#### Inventory Management
 
| File | Responsibility |
|---|---|
| `InventoryManager.cs` | Manages the data-side inventory; handles item use, removal, and serialization for checkpoint save/load |
| `ItemObjectManager.cs` | Manages the GameObject-side; tracks which item objects have been picked up so they can be correctly respawned on checkpoint reload |
| `ItemDatabase.cs` | Centralized registry of all items by stable ID |
| `InteractableItem.cs` | In-world item component the player raycasts against to pick up |
 
#### Inventory UI
 
| File | Responsibility |
|---|---|
| `InventoryUI.cs` | Main inventory panel controller |
| `InventoryItemSlot.cs` | Individual slot component in the inventory grid |
| `ItemButtonEffect.cs` | Hover/click visual feedback on inventory slots |
| `UIHoverEffect.cs` | Reusable hover animation component |
 
#### Ritual Drawing Minigame
 
| File | Responsibility |
|---|---|
| `RitualDrawingComparison.cs` | Compares a player's drawn symbol against a target ritual pattern to determine match accuracy, driving one of the game's core puzzle mechanics |
 
#### Editor Tool
 
| File | Responsibility |
|---|---|
| `Editor/ItemDatabaseBuilder.cs` | Custom Unity Editor window for assigning and managing stable item IDs across the database; built to reduce human error from manual ID entry during team development |
 
---
 
## Shared Systems (Included for Context)
 
The following files at the repo root are referenced by the systems above but reflect contributions from multiple team members. I chose to include these for additional context to the systems I implemented. I'm not claiming sole authorship of these.
 
| File | Purpose |
|---|---|
| `GameManager.cs` | Central game state manager; handles checkpoint save/load, spell state, room shuffling, and scene transitions |
| `UserInterface.cs` | Master UI controller coordinating all panels (inventory, journal, pause, dialogue, etc.) |
| `PauseMenuManager.cs` | Pause menu logic and settings |
| `Constants.cs` | Shared game constants (move speed, gravity, etc.) |
 
---
 
## Team Context
 
Krovna was built by a 15-person team of CCI and DIGM students at Drexel University as a senior capstone project. Other systems in the full game — enemy AI, audio, level design, spells, and dialogue — were built by teammates and are not included here.
 
- [Studio Website](https://silverandflamestudio.com/)
- [Team Members](https://silverandflamestudio.com/team.html)
- [Game Info & Press Kit](https://silverandflamestudio.com/presskit/index.html)
- [Download the Game](https://silverandflamestudio.com/download.html)