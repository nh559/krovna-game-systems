using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.2f; //How much the element's size go up when hovered over
    [SerializeField] private float animDuration = 0.15f; //How long until the element reaches full size

    private Vector3 originalScale;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }
    /// <summary>
    /// Scales the element up when the mouse enters it.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(originalScale * hoverScale, animDuration).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Returns the element to its original scale when the mouse leaves it.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(originalScale, animDuration).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Kills any active scale tween and snaps the element back to its original scale.
    /// Prevents leftover enlarged state carrying over to the next time the element is visible.
    /// </summary>
    public void ResetScale()
    {
        //Kill any running tween first to avoid conflicts during inventory item dragging
        rectTransform.DOKill();
        rectTransform.localScale = originalScale;
    }
}