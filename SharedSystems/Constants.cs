using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Enum for the door and door frame colors
/// </summary>
public enum DoorAndFrameColor
{
    BROWN,
    BLUE,
    GREEN,
    ORANGE
}

/// <summary>
///     Enum for spells
/// </summary>
public enum SpellType
{
    NONE,
    SWITCH_ROOM,
    ROTATE_ROOM,
    FLASH,
}

/// <summary>
///     Enum for room types for levels
/// </summary>
public enum RoomType
{
    NORMAL,
    END_OF_LEVEL
}

/// <summary>
///     Enum for level state. Either the player is going into the level, or coming out
///     Used to determine which checkpoint type to look for
///     If going in, look for PRE_EOL
///     If going out, look for POST_EOL
/// </summary>
public enum LevelState
{
    GOING_IN, 
    GOING_OUT
}

public enum HealingItemType
{
    PHYSICAL,
    MENTAL
}

[Serializable]
public class MeatRenderers
{
    /// <summary>
    ///     Renderer for shared material
    /// </summary>
    public Renderer renderer;
    /// <summary>
    ///     Renderer for copy of material to reset to
    /// </summary>
    public Renderer rendererCopy;
}

/// <summary>
///     Class to hold the game state for checkpoint system
/// </summary>
[Serializable]
public class GameState
{
    // Spell unlocks, one per paper
    public bool switchSpellUnlocked;
    public bool rotateSpellUnlocked;
    public bool flashSpellUnlocked;

    // Spell usage, counts per level
    public int currentSpellAmmo;

    // Progression in current level
    public LevelState levelState;

    public CurrentLevel currentLevel;

    // Player data
    public int physicalHealth;
    public int mentalHealth;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
}

public enum CurrentLevel
{
    LEVEL1,
    LEVEL2,
    LEVEL3,
    LEVEL4
}

public class CONSTANTS
{
    // Drawing Constants
    public static Color DRAW_COLOR = Color.black;
    public const int BRUSH_SIZE = 1;

    // Enemy Constants
    public const float ENEMY_MOVE_SPEED = 4f;
    public const float STOPPING_DISTANCE = 3f; // How close before it stops
    public const float DETECTION_RANGE = 50f; // How far it can see player
    public const int DAMAGE = 34; // kill player in 3 hits
    public const float ATTACK_COOLDOWN = 4f;
    public const float KNOCKBACK_FORCE = 5f;
    public const float KNOCKBACK_DURATION = 0.3f;
    public const float BOSS_SLOWDOWN_FACTOR = 0.5f;
    public const float BOSS_SLOWDOWN_LENGTH = 10f;

    // Player Constants
    public const int MAX_HEALTH = 100;
    public const int MAX_MENTAL_HEALTH = 3;
    public const float PLAYER_MOVE_SPEED = 1f;
    public const float GRAVITY = -9.81f;
    public static readonly Quaternion HALFTURN = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    public static readonly Dictionary<DoorAndFrameColor, Color> COLORMAP = new Dictionary<DoorAndFrameColor, Color>()
    {
        { DoorAndFrameColor.BROWN, new Color(0.7f, 0.5f, 0.5f, 1.0f) },
        { DoorAndFrameColor.BLUE, new Color(0.4f, 0.8f, 0.8f, 1.0f) },
        { DoorAndFrameColor.GREEN, new Color(0.6f, 0.7f, 0.3f, 1.0f) },
        { DoorAndFrameColor.ORANGE, new Color(0.8f, 0.6f, 0.3f, 1.0f) }
    };
}

/// <summary>
///     Enum for direction of audio source in intro scene
/// </summary>
public enum IntroSceneAudioDirection
{
    LEFT,
    RIGHT
}
