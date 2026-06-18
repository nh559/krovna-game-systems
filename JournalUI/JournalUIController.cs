using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class JournalUIController : MonoBehaviour, IPointerClickHandler
{
    public static JournalUIController Instance { get; private set; }

    [Header("Picture Slots")]
    [SerializeField] private Transform slotsParent; //the parent holding all JournalSlot children

    [Header("Pagination")]
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;

    [Header("Detail Overlay")]
    [SerializeField] private GameObject overlayCanvas;
    [SerializeField] private Image overlayItemImage;
    [SerializeField] private TextMeshProUGUI overlayTitle;
    [SerializeField] private TextMeshProUGUI overlayBody;

    [Header("Optional")]
    [SerializeField] private bool rebuildOnEnable = true;

    private int currentPage = 0;
    private JournalSlot[] slots;

    /// <summary>
    /// Hides the overlay and caches slot references on startup.
    /// </summary>
    private void Awake()
    {
        if (overlayCanvas != null) overlayCanvas.SetActive(false);
        slots = slotsParent.GetComponentsInChildren<JournalSlot>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Wires pagination buttons.
    /// </summary>
    private void Start()
    {
        if (prevPageButton != null)
        {
            prevPageButton.onClick.RemoveAllListeners();
            prevPageButton.onClick.AddListener(PrevPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(NextPage);
        }
    }

    /// <summary>
    /// Rebuilds the page when the journal tab is opened if rebuildOnEnable is set.
    /// </summary>
    private void OnEnable()
    {
        if (rebuildOnEnable)
        {
            currentPage = 0;
            RebuildPage();
        }
    }

    /// <summary>
    /// Populates all slots with entries for the current page.
    /// Slots with no entry for this page are hidden automatically by JournalSlot.Setup.
    /// </summary>
    public void RebuildPage()
    {
        IReadOnlyList<LoreEntry> entries = LoreJournal.Entries;

        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)entries.Count / slots.Length));
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        int startIndex = currentPage * slots.Length;

        for (int i = 0; i < slots.Length; i++)
            slots[i].Setup(GetEntryAt(entries, startIndex + i));

        if (prevPageButton != null) prevPageButton.interactable = currentPage > 0;
        if (nextPageButton != null) nextPageButton.interactable = currentPage < totalPages - 1;
    }

    /// <summary>
    /// Returns a lore entry at the given index, or null if out of range.
    /// </summary>
    private LoreEntry GetEntryAt(IReadOnlyList<LoreEntry> entries, int index)
    {
        return index < entries.Count ? entries[index] : null;
    }

    /// <summary>
    /// Moves to the previous page.
    /// </summary>
    private void PrevPage()
    {
        currentPage--;
        StartCoroutine(FlipThenRebuild(-1));
    }

    /// <summary>
    /// Moves to the next page.
    /// </summary>
    private void NextPage()
    {
        IReadOnlyList<LoreEntry> entries = LoreJournal.Entries;
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)entries.Count / slots.Length));
        if (currentPage >= totalPages - 1) return;
        StartCoroutine(FlipThenRebuild(1));
    }

    /// <summary>
    /// Folds the outgoing page flat, swaps content, then unfolds the new page
    /// Direction 1 = next page (right page flips), -1 = prev page (left page flips)
    /// </summary>
    private IEnumerator FlipThenRebuild(int direction)
    {
        AudioManager.Instance.Play("Book Flipping");
        if (prevPageButton != null) prevPageButton.interactable = false;
        if (nextPageButton != null) nextPageButton.interactable = false;

        UserInterface ui = UserInterface.Instance;

        RectTransform pageToFlip = direction > 0 ? ui.RightPage : ui.LeftPage;
        float originalScaleX = direction > 0 ? ui.RightPageOriginalScale.x : ui.LeftPageOriginalScale.x;

        yield return pageToFlip.DOScaleX(0f, ui.FlipDuration).SetEase(Ease.InQuad).WaitForCompletion();
        currentPage += direction;
        RebuildPage();
        yield return pageToFlip.DOScaleX(originalScaleX, ui.FlipDuration).SetEase(Ease.OutQuad).WaitForCompletion();
    }

    /// <summary>
    /// Opens the detail overlay for a given entry
    /// </summary>
    /// <param name="entry"> The lore entry to display </param>
    public void OpenDetailOverlay(LoreEntry entry)
    {
        if (overlayCanvas == null) return;

        overlayCanvas.SetActive(true);

        if (overlayItemImage != null)
        {
            overlayItemImage.sprite = entry.sprite;
            overlayItemImage.enabled = entry.sprite != null;
        }

        if (overlayTitle != null) overlayTitle.text = entry.name;
        if (overlayBody != null) overlayBody.text = entry.lore;

        Debug.Log($"[Journal] Opened detail for: {entry.name}");
    }

    /// <summary>
    /// Closes the detail overlay and returns to the journal page.
    /// </summary>
    public void CloseDetailOverlay()
    {
        if (overlayCanvas != null)
            overlayCanvas.SetActive(false);
    }

    /// <summary>
    /// Detects any click on this GameObject and closes the overlay.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.Play("Click");
        if (overlayCanvas != null && overlayCanvas.activeSelf)
            CloseDetailOverlay();
    }
}