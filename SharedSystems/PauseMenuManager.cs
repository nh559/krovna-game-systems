using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
///     Attach this to top level UI and then drag in PauseMenuUI in inspector
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("Help Menu Spell Buttons")]
    [SerializeField] private Button helpSwitchButton;
    [SerializeField] private Button helpRotateButton;
    [SerializeField] private Button helpFlashButton;
    public static PauseMenuManager Instance { get; private set; }
    public void Awake()
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
    }

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject helpMenuUI;
    [SerializeField] private GameObject backFromHelpButton;

    public void TogglePauseMenu()
    {
        if (UserInterface.Instance?.ShowingWinLoseScreen ?? false) return;
        if (Time.timeScale == 0f)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    // bool to check if the help menu is active, used to prevent pressing esc in the menu
    public bool IsHelpMenuVisible => helpMenuUI != null && helpMenuUI.activeSelf;
    public void Help()
    {
        pauseMenuUI.SetActive(false);
        backFromHelpButton.SetActive(true);
        helpMenuUI.SetActive(true);

        // Only show buttons for spells the player has unlocked
        helpSwitchButton.gameObject.SetActive(GameManager.Instance.HasSpell(SpellType.SWITCH_ROOM));
        helpRotateButton.gameObject.SetActive(GameManager.Instance.HasSpell(SpellType.ROTATE_ROOM));
        helpFlashButton.gameObject.SetActive(GameManager.Instance.HasSpell(SpellType.FLASH));
    }

    public void InfoButtons(int spellType)
    {
        UserInterface.Instance.ShowSpellInfoPanel((SpellType)spellType, fromMenu: true);
    }

    public void BackFromHelp()
    {
        helpMenuUI.SetActive(false);
        backFromHelpButton.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
