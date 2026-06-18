using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UserInterface : MonoBehaviour
{
    [Header("Inventory UI Setup")]
    /// <summary>
    ///     Left side panel which parents all 9 inventory slots
    /// </summary>
    [SerializeField] private Transform inventoryMenu;

    [Header("Canvas References")]
    /// <summary>
    ///     The main canvas for the inventory page
    /// </summary>
    [SerializeField] private GameObject inventoryCanvas;
    /// <summary>
    ///     The main canvas for the ritual drawing page
    /// </summary>
    [SerializeField] private GameObject ritualCanvas;
    /// <summary>
    ///     The main canvas for the journal page
    /// </summary>
    [SerializeField] private GameObject journalCanvas; 

    [Header("Button References")]
    /// <summary>
    ///     The journal button when pressed, opens the journal page
    /// </summary>
    [SerializeField] private Button journalButton; 
    /// <summary>
    ///     The back to inventory button when pressed, opens the inventory page
    /// </summary>
    [SerializeField] private Button backToInventoryButton;
    /// <summary>
    ///     The healing button when pressed, heals the player
    /// </summary>
    [SerializeField] private Button healingButton;
    /// <summary>
    ///     The ritual paper draw button when pressed, opens the ritual drawing page
    /// </summary>
    [SerializeField] private GameObject ritualPaperDrawButton;

    [Header("Inventory Item Right Side Visual UI")]
    /// <summary>
    ///     The text that displays the name of the selected item
    /// </summary>
    [SerializeField] private TextMeshProUGUI itemNameText;
    /// <summary>
    ///     The text that displays the description of the selected item
    /// </summary>
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    /// <summary>
    ///    The image that displays the icon of the selected item
    /// </summary>
    [SerializeField] private Image itemIconImage;
    /// <summary>
    ///     The action button that appears when selecting an item
    /// </summary>
    [SerializeField] private Button itemActionButton;

    public void InventorySetup()
    {
        if (journalButton != null)
        {
            journalButton.onClick.RemoveAllListeners();
            journalButton.onClick.AddListener(FlipToJournal);
            journalButton.interactable = true;
        }

        if (backToInventoryButton != null)
        {
            backToInventoryButton.onClick.RemoveAllListeners();
            backToInventoryButton.onClick.AddListener(FlipToInventory);
            backToInventoryButton.interactable = false; // start in inventory
        }

        // Clear details at start
        ClearItemDetails();

        // Start with inventory visible, others hidden
        if (inventoryCanvas != null) inventoryCanvas.SetActive(true);
        if (journalCanvas != null) journalCanvas.SetActive(false);
    }

    /// <summary>
    ///     Clears the item details from the inventory UI
    /// </summary>
    public void ClearItemDetails()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "";
        itemIconImage.gameObject.SetActive(false);
        itemActionButton.gameObject.SetActive(false);
    }

    private enum BookMode
    {
        Inventory,
        Ritual,
        Journal
    }
    private BookMode currentMode = BookMode.Inventory;

    public void SwitchToRitual()
    {
        if (currentMode == BookMode.Ritual)
        {
            Debug.Log("Already in ritual mode");
            return;
        }

        if (inventoryCanvas != null) inventoryCanvas.SetActive(false);
        if (journalCanvas != null) journalCanvas.SetActive(false);
        if (ritualCanvas != null) ritualCanvas.SetActive(true);

        // Button states
        if (journalButton != null) journalButton.interactable = true;
        if (backToInventoryButton != null) backToInventoryButton.interactable = true;

        currentMode = BookMode.Ritual;

        DrawScript.Instance.setRitualStatus(true);
    }

    public void SwitchToJournal()
    {
        if (currentMode == BookMode.Journal)
        {
            Debug.Log("Already in journal mode");
            return;
        }

        if (inventoryCanvas != null) inventoryCanvas.SetActive(false);
        if (ritualCanvas != null) ritualCanvas.SetActive(false);
        if (journalCanvas != null) journalCanvas.SetActive(true);

        // Button states
        if (journalButton != null) journalButton.interactable = false;
        if (backToInventoryButton != null) backToInventoryButton.interactable = true;

        currentMode = BookMode.Journal;

        DrawScript.Instance.setRitualStatus(false);
    }

    public void SwitchToInventory()
    {
        if (currentMode == BookMode.Inventory)
        {
            Debug.Log("Already in inventory mode");
            return;
        }

        if (inventoryCanvas != null) inventoryCanvas.SetActive(true);
        if (ritualCanvas != null) ritualCanvas.SetActive(false);
        if (journalCanvas != null) journalCanvas.SetActive(false);

        // Button states
        if (journalButton != null) journalButton.interactable = true;
        if (backToInventoryButton != null) backToInventoryButton.interactable = false;

        currentMode = BookMode.Inventory;

        DrawScript.Instance.setRitualStatus(false);
    }

    public void ShowHealingItemAction(HealingItem item)
    {
        if (interactingWithInteractable)
            return;

        itemNameText.text = "Healing item: " + item.HealingType;
        itemDescriptionText.text = "Used to heal the player";
        itemIconImage.gameObject.SetActive(true);
        itemActionButton.gameObject.SetActive(true);
        itemIconImage.sprite = item.ItemIcon;
        itemActionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use " + item.ItemName;
        itemActionButton.onClick.RemoveAllListeners();
        itemActionButton.onClick.AddListener(() => HealingItemAction(item.HealingType));
    }

    public void HealingItemAction(HealingItemType healingType)
    {        
        if (healingType == HealingItemType.PHYSICAL)
        {
            PlayerHealth.Instance.HealPhysicalHealth(20);
        } 
        else if (healingType == HealingItemType.MENTAL)
        {
            PlayerHealth.Instance.HealMentalHealth(1);
        }

        InventoryManager.Instance.DeleteSelectedItem();
        ClearItemDetails();
    }

    public void ShowStoryItemAction(string itemName, string itemDescription, Sprite itemIcon)
    {
        if (interactingWithInteractable)
        {
            selectedItemName = itemName;           
            itemSelectText.GetComponent<TextMeshProUGUI>().text = "Select an item to use: " + itemName;
        } else
        {
            itemNameText.text = itemName;
            itemDescriptionText.text = itemDescription;
            itemIconImage.gameObject.SetActive(true);
            itemIconImage.sprite = itemIcon;
        }
    }

    public void HideItemAction()
    {
        // Probably not needed?
    }
}
