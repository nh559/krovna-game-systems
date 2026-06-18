using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class UserInterface : MonoBehaviour
{
    public static UserInterface Instance { get; private set; }

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

        if (volume == null)
        {
            Debug.LogError("Volume is not assigned!");
            return;
        }

        // Create a runtime copy so we don't edit the asset
        volume.profile = Instantiate(volume.sharedProfile);
        profile = volume.profile;
        SetStandardVolume();
        InventorySetup();
    }
    [Header("UI Panels")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject lowHealthVignette;
    [Header("Hotkey Prompt UI")]
    [SerializeField] private GameObject hotkeyPrompt;
    [SerializeField] private TextMeshProUGUI hotkeyPromptText;
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryUI;
    
    /// <summary>
    ///     Item selection UI for InteractableItem
    /// </summary>
    [SerializeField] private GameObject itemSelectionUI;
    [SerializeField] private TextMeshProUGUI itemSelectText;

    [Header("Dialogue Box UI")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Image dialoguePortrait;
    [SerializeField] private TextMeshProUGUI dialogueNameText;
    [SerializeField] private TextMeshProUGUI dialogueBodyText;
    [SerializeField] private float dialogueTimeout = 3f;
    [SerializeField] private float dialogueSpeed = 0.01f;
    private Coroutine dialogueCoroutine;
    private bool advanceRequested = false;
    private bool dialogueRunning = false;

    [Header("Ritual Canvas UI")]
    [SerializeField] private Button drawButton;
    [SerializeField] private Toggle effectToggle;
    [SerializeField] private RitualDrawingComparison ritualComparison;
    [SerializeField] private DrawScript drawScript;

    [Header("Spell Unlock Icons")]
    [SerializeField] private GameObject switchSpellIcon;
    [SerializeField] private GameObject rotateSpellIcon;
    [SerializeField] private GameObject flashSpellIcon;

    [Header("Spell Info Panels")]
    [SerializeField] private GameObject switchSpellInfoPanel;
    [SerializeField] private GameObject rotateSpellInfoPanel;
    [SerializeField] private GameObject flashSpellInfoPanel;

    [Header("Health Button Stuff")]
    [SerializeField] public Button healButton;
    [SerializeField] private TextMeshProUGUI healButtonText;
    
    [Header("Page Flip Animation")]
    [SerializeField] private RectTransform leftPage;
    [SerializeField] private RectTransform rightPage;
    [SerializeField] private float flipDuration = 0.5f;
    public RectTransform LeftPage => leftPage;
    public RectTransform RightPage => rightPage;
    public float FlipDuration => flipDuration;
    private Vector3 leftPageOriginalScale;
    private Vector3 rightPageOriginalScale;
    public Vector3 LeftPageOriginalScale => leftPageOriginalScale;
    public Vector3 RightPageOriginalScale => rightPageOriginalScale;

    /// Change this later
    public static string selectedItemName = "";
    private bool interactingWithInteractable = false;

    [Header("CHANGE THIS LATER")]
    [SerializeField] private GameObject hideThis;
    public static bool dirty = false;

    private bool damagePrompted = false;
    private bool showingWinLoseScreen = false;
    public bool ShowingWinLoseScreen {
        get => showingWinLoseScreen;
        set => showingWinLoseScreen = value;
    }
    private void Update()
    {
        if (dirty)
        {
            itemSelectText.text = $"Select an item to use: {selectedItemName}";
            dirty = false;
        }


        //debugging to test flash spell easily --- can be removed at any time
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            CastFlashSpell();
        }
    }
    ///
    
    [SerializeField] private Button confirmButton;

    private string correctItemName = "";

    [Header("Interaction Prompt UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    private void Start()
    {
        //Get the original scale for left and right pages for the animation
        leftPageOriginalScale = leftPage.localScale;
        rightPageOriginalScale = rightPage.localScale;
    }

    /// <summary>
    ///    Toggles the inventory UI
    /// </summary>
    /// <returns> True or false for PlayerMovement to either enable or disable player control </returns>
    public bool ToggleInventory()
    {
        bool isActive = inventoryUI.activeSelf;
        hideThis.SetActive(true);
        return !isActive ? ShowInventory() : HideInventory();
    }

    /// <summary>
    ///    Shows the inventory UI
    /// </summary>
    /// <returns> False to disable player controller </returns>
    public bool ShowInventory()
    {
        if (inventoryUI != null)
        {
            interactingWithInteractable = false;
            AudioManager.Instance.Play("Menu Open 1");

            inventoryUI.SetActive(true);
            itemSelectionUI.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        return false;
    }

    /// <summary>
    ///     Hides the inventory UI. If called from an InteractableItem closing, it will not attempt to close that prompt again.
    /// </summary>
    /// <param name="fromInteractableClose"></param>
    /// <returns> True to re-enable player controller </returns>
    public bool HideInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        JournalUIController.Instance?.CloseDetailOverlay();
        SwitchToInventory();

        if (healButton != null)
        {
            healButton.gameObject.SetActive(false);
        }

        //InventoryItem.SwitchToInventory();
        //selectedSpell = SpellType.NONE;

        //InventoryItem.ClearItemDetails();

        return true;
    }

    public void OpenEasel(string correctItemName)
    {
        ShowInventory();
        interactingWithInteractable = true;
        selectedItemName = "";
        dirty = true;
        if (itemSelectionUI != null)
        {
            itemSelectionUI.SetActive(true);
        }
        hideThis.SetActive(false);

        ClearItemDetails();
        this.correctItemName = correctItemName;
    }

    /// <summary>
    ///    Called when the player confirms using the selected item on the InteractableItem.
    /// </summary>
    public void OnConfirmUse()
    {
        // Check if it's the correct item
        if (selectedItemName == correctItemName)
        {
            GameManager.Instance.InteractedItem.PlaceItemModel();
            
            switch (selectedItemName)
            {
                case "Waltz of the Flowers":
                    GameManager.Instance.OpenNextDoor();
                    GameManager.Instance.InteractedItem.gameObject.tag = "Untagged";
                    break;
                case "Mothers Letter":
                    GameManager.Instance.OpenNextDoor();
                    GameManager.Instance.InteractedItem.gameObject.tag = "Untagged";
                    break;
                case "Canvas with eyes":
                    ShowWinScreen("Complete!");
                    GameManager.Instance.OpenNextDoor();
                    GameManager.Instance.InteractedItem.gameObject.tag = "Untagged";
                    break;
                default:
                    GameManager.Instance.OpenNextDoor();
                    GameManager.Instance.InteractedItem.gameObject.tag = "Untagged";
                    break;
            }

            AudioManager.Instance.Play("Correct Object");
            InventoryManager.Instance.DeleteSelectedItem();
            bool enableMovement = ToggleInventory();
            PlayerMovement.Instance.PlayerControllerToggle(enableMovement);
        }
        else
        {
            Debug.Log($"<color=red>Wrong item! Need {correctItemName}, used {selectedItemName}</color>");
            PlayerHealth.Instance.TakeMentalDamage();

            ToggleInventory();
            PlayerMovement.Instance.PlayerControllerToggle(true);

            AudioManager.Instance.Play("Incorrect Object 1");

            //HARD CODED DIALOGUE:
            //Replace this maybe.
            if (!damagePrompted)
            {
                IList<string> dialogueLine = new List<string>
                {
                    "Yeoouch! Maybe I should be more careful where I put these things."
                };
                StartCoroutine(DialogueManager.Instance.StartDialogue(
                    "Lika",
                    dialogueLine,
                    Resources.Load<Sprite>("CharacterPortraits/Lika")
                ));
                damagePrompted = true;
            }
        }
    }

    //----------------------------------
    

    /// <summary>
    ///    Shows the inventory UI
    /// </summary>
    /// <returns> False to disable player controller </returns>
    public void ShowLowHealthVignette()
    {
        if (lowHealthVignette != null) lowHealthVignette.SetActive(true);
    }

    public void HideLowHealthVignette()
    {
        if (lowHealthVignette != null) lowHealthVignette.SetActive(false);
    }

    public void CloseInventoryFromButton()
    {
        bool enableMovement = HideInventory(); // True
        PlayerMovement.Instance.PlayerControllerToggle(enableMovement);
    }


    /// <summary>
    ///   Loads the WinLose scene with win status and reason.
    /// </summary>
    /// <param name="reason"> Header text of win screen </param>
    public void ShowWinScreen(string reason = "You win!")
    {
        DisableEffects(); // should hide the visual effects
        WinLoseStats.hasWon = true;
        WinLoseStats.reason = reason;
        SceneManager.LoadScene("WinLose");
    }

    /// <summary>
    ///   Loads the WinLose scene with lose status and reason.
    /// </summary>
    /// <param name="reason"> Header text of lose screen </param>
    public void ShowLoseScreen(string reason = "Unknown")
    {
        DisableEffects(); // should hide the visual effects
        WinLoseStats.hasWon = false;
        WinLoseStats.reason = reason;
        SceneManager.LoadSceneAsync("WinLose", LoadSceneMode.Additive);
    }



    //Dialogue Implementation
    /// <summary>
    ///   Shows, updates and begins printing the dialogue box
    /// </summary>
    /// <param name="speakerName"> Name of the person speaking </param>
    /// <param name="pages"> List of paragraphs/pages of dialogue</param>
    ///    /// <param name="characterPortrait"> Image of the character or object relevent to the dialogue</param>
    public void StartDialogue(string speakerName, IList<string> pages, Sprite characterPortrait)
    {
        PlayerMovement.Instance.DisableMovement();
        dialogueRunning = true;
        dialogueBox.SetActive(true);
        dialogueNameText.text = speakerName;
        dialogueBodyText.text = "";
        dialoguePortrait.sprite = characterPortrait;

        if (dialogueCoroutine != null)
            StopCoroutine(dialogueCoroutine);

        dialogueCoroutine = StartCoroutine(ProcessDialogue(pages));
    }
    /// <summary>
    ///   Recursively prints pages to the textbox.
    /// </summary>
    /// <param name="pages"> List of paragraphs/pages of dialogue</param>
    private IEnumerator ProcessDialogue(IList<string> pages)
    {
        dialogueRunning = true;
        dialogueBox.SetActive(true);

        yield return null;

        //Splits the dialogue up to fit in the textbox.
        List<string> updatedPages = PaginateToFit(pages);

        for (int pageIndex = 0; pageIndex < updatedPages.Count; pageIndex++)
        {
            string currentPage = updatedPages[pageIndex];
            dialogueBodyText.text = "";

            //Prints the letters one at a time
            for (int i = 0; i < currentPage.Length; i++)
            {
                //If the user clicks, skip to the end of the message
                if (ConsumeAdvanceRequested())
                {
                    dialogueBodyText.text = currentPage;
                    break;
                }

                dialogueBodyText.text += currentPage[i];
                yield return new WaitForSeconds(dialogueSpeed);
            }

            //Keep the dialogue on the screen for a set time, unless the user clicks
            yield return StartCoroutine(WaitForAdvanceOrTimeout(dialogueTimeout));

            if (pageIndex == updatedPages.Count - 1)
                break;
        }

        dialogueBox.SetActive(false);
        dialogueRunning = false;
        PlayerMovement.Instance.EnableMovement();
    }

    /// <summary>
    ///   Splits each paragraph into one or more pages that fit in the dialogue text box.
    /// </summary>
    /// <param name="paragraphs">List of dialogue paragraphs to split.</param>
    private List<string> PaginateToFit(IList<string> paragraphs)
    {
        List<string> result = new List<string>();
        if (paragraphs == null) return result;

        foreach (var para in paragraphs)
        {
            // Skip empty paragraphs so we don't create blank pages
            if (string.IsNullOrWhiteSpace(para))
                continue;

            // Split this single paragraph into multiple "fit" pages
            result.AddRange(PaginateToFitTMP(dialogueBodyText, para));
        }

        return result;
    }

    /// <summary>
    ///   Splits a single block of text into multiple pages that fit inside a TMP text area.
    /// </summary>
    /// <param name="tmp">The TextMeshProUGUI used to measure overflow.</param>
    /// <param name="fullText">The full text to split.</param>
    private List<string> PaginateToFitTMP(TextMeshProUGUI tmp, string fullText)
    {
        List<string> pages = new();
        string remaining = fullText.Trim();

        while (!string.IsNullOrEmpty(remaining))
        {
            // Put remaining text into TMP so it can compute layout/overflow
            tmp.text = remaining;
            tmp.ForceMeshUpdate();

            // If it fits, we're done
            if (!tmp.isTextOverflowing)
            {
                pages.Add(remaining);
                break;
            }

            // Find the last character index of the last visible line
            int lastLine = tmp.textInfo.lineCount - 1;
            int split = tmp.textInfo.lineInfo[lastLine].lastCharacterIndex + 1;

            // Walk backward to split on a whitespace boundary (avoid cutting a word)
            while (split > 0 && !char.IsWhiteSpace(remaining[split - 1]))
                split--;

            // If we couldn't find whitespace, fall back to a hard split at the last visible char
            if (split <= 0)
                split = tmp.textInfo.lineInfo[lastLine].lastCharacterIndex + 1;

            // Add the visible chunk, keep the rest for the next page
            pages.Add(remaining.Substring(0, split).TrimEnd());
            remaining = remaining.Substring(split).TrimStart();
        }

        return pages;
    }

    /// <summary>
    ///   Waits until the user advances OR a timeout expires.
    /// </summary>
    /// <param name="timeout">Max time to wait before auto-advancing.</param>
    private IEnumerator WaitForAdvanceOrTimeout(float timeout)
    {
        float t = 0f;

        while (t < timeout)
        {
            // If the player clicked, stop waiting immediately
            if (ConsumeAdvanceRequested())
                yield break;

            t += Time.deltaTime;
            yield return null; // wait one frame
        }
    }

    /// <summary>
    ///   Checks if an advance was requested; if so, consumes it (resets the flag).
    /// </summary>
    /// <returns>True if an advance was requested this frame; otherwise false.</returns>
    private bool ConsumeAdvanceRequested()
    {
        if (!advanceRequested)
            return false;

        // Debug: proves the coroutine is seeing the click
        print("Advance requested");

        advanceRequested = false;
        return true;
    }

    /// <summary>
    ///   Returns whether a dialogue sequence is currently active.
    /// </summary>
    public bool IsDialogueRunning()
    {
        return dialogueRunning;
    }

    /// <summary>
    ///   Requests an advance (skip typing / move to next page) from the input system.
    /// </summary>
    public void RequestDialogueAdvance()
    {
        // Debug: proves input is hitting the correct UI instance
        print("RequestDialogueAdvance on UI instance: " + GetInstanceID());

        advanceRequested = true;
    }

    //Interaction prompt functions:
    /// <summary>
    ///   Shows the interaction prompt with given text.
    /// </summary>
    public void ShowInteractPrompt(string text)
    {
        interactionPrompt.SetActive(true);
        interactionPromptText.text = text;
    }

    /// <summary>
    ///   Hides the interaction prompt.
    /// </summary>
    public void HideInteractPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    /// <summary>
    /// Casts the switch spell
    /// </summary>
    public void CastSwitchSpell()
    {
        if (!GameManager.Instance.HasSpell(SpellType.SWITCH_ROOM)) return;

        bool switchingStatus = RoomTraversalManager.SwitchPortalDoors(Room.CurrentRoom);
        if (switchingStatus)
        {
            Debug.Log("Switch Room effect");
        }
        else
        {
            Debug.Log("Failed to switch rooms. Make sure you are in a room with multiple doors.");
        }
    }

    /// <summary>
    /// Casts the rotate room spell, requires a portal door.
    /// </summary>
    public void CastRotateSpell(PortalDoorComponent portalDoor)
    {
        if (!GameManager.Instance.HasSpell(SpellType.ROTATE_ROOM)) return;
        if (portalDoor == null) return;

        Debug.Log("Attempting to rotate rooms...");
        if (portalDoor == null)
        {
            Debug.LogError("No portal door detected for Rotate Room spell.");
            return;
        }
        bool rotatingStatus = RoomTraversalManager.RotatePortalDoors(portalDoor, Room.LookingAtRoom);
        if (rotatingStatus)
        {
            Debug.Log("Rotate Room effect");
        }
        else
        {
            Debug.Log("Failed to rotate room. Make sure the portal door is open.");
        }
    }

    /// <summary>
    /// Casts the flash spell
    /// </summary>
    public void CastFlashSpell()
    {
        if (!GameManager.Instance.HasSpell(SpellType.FLASH)) return;

        PortalCameraManager.Instance.FlashEffect();
        StartCoroutine(HandleReveal());
        if (EnemyScript.Instance != null) EnemyScript.Instance.Slowdown();
        Debug.Log("Flash effect");
    }

    private IEnumerator HandleReveal()
    {
        // slight delay for dramatic effect
        yield return new WaitForSeconds(0.2f);

        Collider[] hits = Physics.OverlapSphere(
            PlayerMovement.Instance.transform.position,
            10f
        );

        float maxAngle = 90f;
        foreach (Collider hit in hits)
        {
            Vector3 directionToObject = (hit.transform.position - PlayerMovement.Instance.transform.position).normalized;
            float angle = Vector3.Angle(PlayerMovement.Instance.transform.forward, directionToObject);

            if (angle > maxAngle)
                continue; // skip objects outside the forward cone

            InvisibleObject item = hit.GetComponentInParent<InvisibleObject>()
                                 ?? hit.GetComponentInChildren<InvisibleObject>();

            if (item == null) continue;

            if (item.isFake)
            {
                Debug.Log("UserInterface - Attempting to reveal fake object");
                item.DestroyFakeObject();
            }
            else if (item.isInvisible)
            {
                Debug.Log("UserInterface - Attempting to reveal invisible object");
                item.MakeObjectVisible();
            }
        }
        yield break;
    }

    /// <summary>
    ///     Shows the icon for the given spell.
    ///     Called by UnlockSpell when a spell is unlocked or restored on load.
    /// </summary>
    public void ShowSpellIcon(SpellType spell)
    {
        switch (spell)
        {
            case SpellType.SWITCH_ROOM:
                if (switchSpellIcon != null) switchSpellIcon.SetActive(true);
                break;
            case SpellType.ROTATE_ROOM:
                if (rotateSpellIcon != null) rotateSpellIcon.SetActive(true);
                break;
            case SpellType.FLASH:
                if (flashSpellIcon != null) flashSpellIcon.SetActive(true);
                break;
            default:
                Debug.LogWarning($"UserInterface: no icon for spell '{spell}'");
                break;
        }
    }

    /// <summary>
    ///     Hides the icon for the given spell.
    ///     Called by GameManager when a spell is reverted on checkpoint load.
    /// </summary>
    public void HideSpellIcon(SpellType spell)
    {
        switch (spell)
        {
            case SpellType.SWITCH_ROOM:
                if (switchSpellIcon != null) switchSpellIcon.SetActive(false);
                break;
            case SpellType.ROTATE_ROOM:
                if (rotateSpellIcon != null) rotateSpellIcon.SetActive(false);
                break;
            case SpellType.FLASH:
                if (flashSpellIcon != null) flashSpellIcon.SetActive(false);
                break;
        }
    }

    /// <summary>
    ///     Shows the corresponding spell info panel when a spell is unlocked and for the help menu     
    /// </summary>
    private bool spellInfoPanelVisible = false; // tracks if spell panel is visible, used to disable pause when it's open
    private bool spellInfoFromMenu = false; // tracks if the info panel was opened from the help menu or not
    public void ShowSpellInfoPanel(SpellType spell, bool fromMenu)
    {
        spellInfoPanelVisible = true;
        spellInfoFromMenu = fromMenu;
        switchSpellInfoPanel.SetActive(spell == SpellType.SWITCH_ROOM);
        rotateSpellInfoPanel.SetActive(spell == SpellType.ROTATE_ROOM);
        flashSpellInfoPanel.SetActive(spell == SpellType.FLASH);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideSpellInfoPanel()
    {
        spellInfoPanelVisible = false;
        if (!spellInfoFromMenu)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        switchSpellInfoPanel.SetActive(false);
        rotateSpellInfoPanel.SetActive(false);
        flashSpellInfoPanel.SetActive(false);
    }

    public bool IsSpellInfoPanelVisible => spellInfoPanelVisible;

    private void ResetCanvasHoverEffects(GameObject canvas)
    {
        if (canvas == null) return;
        foreach (UIHoverEffect effect in canvas.GetComponentsInChildren<UIHoverEffect>())
            effect.ResetScale();
    }

    /// <summary>
    /// Folds the given page flat, does swap action, then unfolds.
    /// </summary>
    private IEnumerator FlipPage(RectTransform page, System.Action onSwap)
    {
        Vector3 originalScale = page == leftPage ? leftPageOriginalScale : rightPageOriginalScale;

        yield return page.DOScaleX(0f, flipDuration).SetEase(Ease.InQuad).WaitForCompletion();
        onSwap?.Invoke();
        yield return page.DOScaleX(originalScale.x, flipDuration).SetEase(Ease.OutQuad).WaitForCompletion();
    }

    /// <summary>
    /// Flips to journal, right page flips forward.
    /// </summary>
    public void FlipToJournal()
    {
        StartCoroutine(FlipPage(rightPage, () => SwitchToJournal()));
    }

    /// <summary>
    /// Flips back to inventory, left page flips back.
    /// </summary>
    public void FlipToInventory()
    {
        StartCoroutine(FlipPage(leftPage, () => SwitchToInventory()));
    }
}