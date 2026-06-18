using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Correct Item")]
    [SerializeField] private string correctItemName = "{ Key }";

    [Header("Locking Door")]
    [SerializeField] private Door linkedDoor;

    [Header("Item Model")]
    [SerializeField] private GameObject itemModel;

    public string CorrectItemName => correctItemName;
    public Door LinkedDoor => linkedDoor;

    private void Awake()
    {
        if (itemModel != null) itemModel.SetActive(false);
    }

    /// <summary>
    ///     Enables the item model in the scene when the correct item is used.
    /// </summary>
    public void PlaceItemModel()
    {
        if (itemModel != null)
            itemModel.SetActive(true);
        else
            Debug.Log($"No item model assigned on '{gameObject.name}'");
    } 
}
