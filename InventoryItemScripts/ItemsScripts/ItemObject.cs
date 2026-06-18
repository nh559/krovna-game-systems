using UnityEngine;

public abstract class ItemObject : MonoBehaviour
{
    public abstract Item Item { get; set; }

    [SerializeField] private string itemID;

    private void OnEnable()
    {
        RegisterCheckpointLoadListener();
    }

    private void OnDisable()
    {
        UnregisterCheckpointLoadListener();
    }

    private void Start()
    {
        RegisterCheckpointLoadListener();
        ReloadItem();
    }

    private void RegisterCheckpointLoadListener()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckpointLoad -= ReloadItem;
            GameManager.Instance.CheckpointLoad += ReloadItem;
        }
    }

    private void UnregisterCheckpointLoadListener()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckpointLoad -= ReloadItem;
        }
    }

    public virtual void PickupItem()
    {
        bool itemSuccess = InventoryManager.Instance.AddItemToInventory(Item);

        if (itemSuccess)
        {
            HideObject();
            ItemObjectManager.AddToCollectedItems(ItemID);
        }
    }

    /// <summary>
    ///     Whether this item has been collected or not
    ///     Used when the player either load into checkpoint or back into game
    /// </summary>
    public void ReloadItem()
    {
        Debug.Log("reloading item with ID: " + ItemID);
        if (ItemObjectManager.IsItemCollected(ItemID))
        {
            HideObject();
        }
        else
        {
            ShowObject();
        }
    }

    public void HideObject()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer r in allRenderers)
        {
            r.enabled = false;
        }

        Collider[] allColliders = GetComponentsInChildren<Collider>();
        
        foreach (Collider c in allColliders)
        {
            c.enabled = false;
        }
    }

    public void ShowObject()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            r.enabled = true;
        }

        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in allColliders)
        {
            c.enabled = true;
        }
    }

    public string ItemID
    {
        get { return itemID; }
        protected set { itemID = value; }
    }

    [ContextMenu("Generate Item ID")]
    public virtual void GenerateItemID()
    {
        ItemID = System.Guid.NewGuid().ToString();
    }
}
