using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler
{
    private Item itemInSlot = null;
    public Item ItemInSlot
    {
        get { return itemInSlot; }
        set { itemInSlot = value; }
    }

    /// <summary>
    ///     The image component for the item icon in the inventory slot
    /// </summary>
    [SerializeField] private Image itemIcon;

    private static readonly Color selectedColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color deselectedColor = new Color(1f, 1f, 1f, 0.5f);

    private Image slotBackground;

    private void Awake()
    {
        slotBackground = GetComponent<Image>();
        SetSelected(false);
    }

    public void SetInventoryItemSlotIcon(Sprite icon)
    {
        itemIcon.sprite = icon;
        itemIcon.gameObject.SetActive(true);
    }

    public void ClearInventoryItemSlotIcon()
    {
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
        SetSelected(false);
    }

    /// <summary>
    ///     Sets the slot background to selected or deselected color.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (slotBackground != null)
            slotBackground.color = selected ? selectedColor : deselectedColor;
    }

    /// <summary>
    ///     When we click on one of the inventory item slots, display item
    /// </summary>
    /// <param name="eventData"> The event data from the pointer click </param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemInSlot == null) return;
        
        AudioManager.Instance.Play("Click");
        InventoryManager.Instance.SetSelectedItem(itemInSlot);
    }

}