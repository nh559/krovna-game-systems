using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    ///     List of rooms in current level which will be mixed up when reaching the "END_OF_LEVEL"
    /// </summary>
    private List<Room> mixUpRooms = new List<Room>();

    /// <summary>
    ///     The InteractableItem the player is currently interacting with
    ///     To disable after successful interaction
    /// </summary>
    private InteractableItem interactedItem;
    public InteractableItem InteractedItem
    {
        get => interactedItem;
        set => interactedItem = value;
    }

    private int spellAmmo;
    public int SpellAmmo
    {
        get => spellAmmo;
        set => spellAmmo = value;
    }

    /// <summary>
    ///     HashSet of spells the player has unlocked
    ///     Used to determine if the player can use a spell
    /// </summary>
    private HashSet<SpellType> unlockedSpells = new HashSet<SpellType>();

    [Header("Debugging. Start with these spells unlocked")]
    [SerializeField] List<SpellType> startingSpells = new List<SpellType>();
    [Header("Debugging. Start at current level to open door")]
    [SerializeField] CurrentLevel startingLevel = CurrentLevel.LEVEL1;

    /// <summary>
    ///     Current Level State
    ///     Used to determine which checkpoints are active based on whether we are going in towards the EOL, or going back out to central area from EOL
    /// </summary>
    private LevelState levelState = LevelState.GOING_IN;
    public LevelState LevelState
    {
        get => levelState;
        set => levelState = value;
    }

    private CurrentLevel currentLevel = CurrentLevel.LEVEL1;
    public CurrentLevel CurrentLevel
    {
        get => currentLevel;
        set => currentLevel = value;
    }

    /// <summary>
    ///     Current game state (for checkpoint)
    /// </summary>
    private GameState gameState;
    
    [Header("References for ending doors")]
    /// <summary>
    ///     Reference for the bad ending door
    /// </summary>
    [SerializeField] private Door badEndingDoor;
    public Door BadEndingDoor => badEndingDoor;
    /// <summary>
    ///     Reference for the happy ending door
    /// </summary>
    [SerializeField] private Door happyEndingDoor;
    public Door HappyEndingDoor => happyEndingDoor;

    [SerializeField] private GameObject endingStuff;

    public void DisableEndingStuff()
    {
        endingStuff.SetActive(false);
    }

    // ------------------
    // Spells respawn
    // ------------------

    /// <summary>
    ///     Unlock a spell upon completing ritual paper item
    ///     silent: skips info panel if true. For debugging purposes when we start with certain spells unlocked
    /// </summary>
    public void UnlockSpell(SpellType spell, bool silent = false)
    {
        unlockedSpells.Add(spell);
        UserInterface.Instance.ShowSpellIcon(spell);

        if(!silent) UserInterface.Instance.ShowSpellInfoPanel(spell, fromMenu: false);
    }
    public bool HasSpell(SpellType spell) => unlockedSpells.Contains(spell);

    // ------------------
    // Item respawn
    // ------------------

    /// <summary>
    ///     Set by the InteractableItem to get the door to unlock for level progression
    /// </summary>
    private Door nextDoor;
    public Door NextDoor
    {
        get => nextDoor;
        set => nextDoor = value;
    }

    public void OpenNextDoor()
    {
        if (nextDoor != null)
            nextDoor.UnlockDoor();
    }

    /// <summary>
    ///     On awake for rooms, if mixup is toggled on, add to this master list
    /// </summary>
    /// <param name="room"> Room data we want to add </param>
    public void AddToMixUpRooms(Room room)
    {
        mixUpRooms.Add(room);
    }

    /// <summary>
    ///    Function to call to mix up the rooms in the master list. Called after picking up final item
    /// </summary>
    public void MixUpRooms()
    {
        foreach(Room room in mixUpRooms)
        {
            for (int i = 0; i < 3; i++)
            {
                RoomTraversalManager.SwitchPortalDoors(room);
            }
        }
    }
    
    /// <summary>
    ///     Create event for checkpoint loading
    ///     Used so item listeners know to check if it should exist in scene or not
    /// </summary>
    public delegate void OnCheckpointLoad();
    public event OnCheckpointLoad CheckpointLoad;

    /// <summary>
    ///     Saves the current game state to PlayerPrefs
    ///     Called when player hits a checkpoint
    /// </summary>
    /// <param name="playerPos"> Player position to save </param>
    public void CheckpointSave(Vector3 playerPos, Quaternion playerRot)
    {
        gameState = new GameState {
            switchSpellUnlocked = HasSpell(SpellType.SWITCH_ROOM),
            rotateSpellUnlocked = HasSpell(SpellType.ROTATE_ROOM),
            flashSpellUnlocked = HasSpell(SpellType.FLASH),
            currentSpellAmmo = SpellAmmo,
            levelState = LevelState,
            currentLevel = CurrentLevel,
            physicalHealth = PlayerHealth.Instance.PhysicalHealth,
            mentalHealth = PlayerHealth.Instance.MentalHealth,
            playerPosition = playerPos,
            playerRotation = playerRot
        };

        string json = JsonUtility.ToJson(gameState);
        Debug.Log("GameState JSON: " + json);

        PlayerPrefs.SetString("CheckpointSave", json);
        PlayerPrefs.Save();

        InventoryManager.Instance.SaveInventoryForCheckpoint();
        JournalManager.Instance.SaveJournalForCheckpoint();
        ItemObjectManager.Instance.SaveItemObjectIDsForCheckpoint();
    }

    /// <summary>
    ///     Loads the game state from PlayerPrefs
    /// </summary>
    /// <param name="savedPlayerPos"> Player position loaded </param>
    /// <returns> True if checkpoint was loaded, false otherwise </returns>
    public bool LoadCheckpoint(out Vector3 savedPlayerPos, out Quaternion savedPlayerRot)
    {
        savedPlayerPos = Vector3.zero;
        savedPlayerRot = Quaternion.identity;

        if (!PlayerPrefs.HasKey("CheckpointSave")) return false;    

        string json = PlayerPrefs.GetString("CheckpointSave");
        gameState = JsonUtility.FromJson<GameState>(json);
        if (gameState == null) return false;
        unlockedSpells.Clear();

        //Hide all spell icons before restoring accordingly
        UserInterface.Instance.HideSpellIcon(SpellType.SWITCH_ROOM);
        UserInterface.Instance.HideSpellIcon(SpellType.ROTATE_ROOM);
        UserInterface.Instance.HideSpellIcon(SpellType.FLASH);
        if (gameState.switchSpellUnlocked) UnlockSpell(SpellType.SWITCH_ROOM);
        if (gameState.rotateSpellUnlocked) UnlockSpell(SpellType.ROTATE_ROOM);
        if (gameState.flashSpellUnlocked) UnlockSpell(SpellType.FLASH);
        SpellAmmo = gameState.currentSpellAmmo;
        LevelState = gameState.levelState;
        CurrentLevel = gameState.currentLevel;
        PlayerHealth.Instance.PhysicalHealth = gameState.physicalHealth;
        PlayerHealth.Instance.MentalHealth = gameState.mentalHealth;
        savedPlayerPos = gameState.playerPosition;
        savedPlayerRot = gameState.playerRotation;

        ItemObjectManager.Instance.LoadItemObjectIDsFromCheckpoint();
        InventoryManager.Instance.LoadInventoryFromCheckpoint();
        JournalManager.Instance.LoadJournalFromCheckpoint();

        CheckpointLoad?.Invoke(); 
        return true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        startingSpells.ForEach(spell => UnlockSpell(spell, silent:true));
        currentLevel = startingLevel;
    }
    
    public void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
