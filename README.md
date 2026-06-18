Krovna Systems Showcase

This repository contains selected source code from Krovna, a Unity horror game built as a senior capstone project by a team at Drexel University. I served as UI Systems Lead Developer on the team.

This is not a runnable project. Krovna depends on a full Unity project (scenes, prefabs, assets, and third-party packages) that isn't included here. This repo exists purely to showcase the architecture and implementation of the systems I personally designed and built. If you are interested in looking at the full game you can download it [here](https://silverandflamestudio.com/download.html)

1. Systems I designed and implemented

- PlayerScripts/
First-person player controller, handling movement, camera look, sprinting, and raycast-based interaction detection for in-world objects and doors.

- JournalUI/
The in-game journal system, including lore entry data, the UI controller for browsing collected entries, and persistence logic so collected journal entries survive scene transitions and checkpoint loads.

- InventoryItemScripts/
The full inventory and item system: item data definitions, the in-world item objects, inventory UI and slot management, and the ritual paper item type that drives one of the game's core puzzle mechanics. Also includes RitualDrawingComparison.cs, the logic behind the ritual drawing minigame, and a tool (Editor/ItemDatabaseBuilder.cs) built to assign and manage stable item IDs used by the save/load system. See the README inside this folder for a deeper explanation of the Item vs. ItemObject data/GameObject split used throughout this system.

- Shared systems
Constants.cs, GameManager.cs, PauseMenuManager.cs, and UserInterface.cs are included at the repo root because the systems above reference them directly. These files show contributions from multiple team members over the course of development, not solely my own work, so I'm not claiming authorship of them. They're here so the code above makes sense in context. For example, GameManager.cs contains the checkpoint save/load logic that the inventory and journal systems hook into.

2. Design constraints
One real constraint that shaped these systems: several teammates were prone to manual data-entry mistakes (typos in string keys, mismatched IDs, etc.). Where possible, I favored direct GameObject and asset references over string-keyed lookups, and built the ItemDatabaseBuilder editor tool specifically to reduce opportunities for that kind of error when assigning item IDs.

3. Team context
Krovna was built by a small team of 15 people consist of both CCI and DIGM team members from Drexel University. Other systems in the full game, including enemy AI, audio, level design, and additional UI, were built by teammates and aren't included in this repository.