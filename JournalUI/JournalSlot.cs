using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JournalSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Image slotImage;

    private LoreEntry entry;
    /// <summary>
    /// Assigns a lore entry to this slot and displays its sprite
    /// Hides the slot entirely if entry is null (fewer than 4 entries)
    /// </summary>
    /// <param name="loreEntry"> The entry to display, pass null to hide the slot </param>
    public void Setup(LoreEntry loreEntry)
    {
        entry = loreEntry;

        if (entry == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (slotImage != null)
        {
            slotImage.sprite = entry.sprite;
            slotImage.enabled = entry.sprite != null;
        }
    }

    /// <summary>
    /// Notifies the controller to open the detail overlay for this entry.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.Play("Click");
        if (entry != null && JournalUIController.Instance != null)
            JournalUIController.Instance.OpenDetailOverlay(entry);
    }
}
