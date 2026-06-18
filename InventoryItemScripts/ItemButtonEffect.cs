using UnityEngine;
using UnityEngine.EventSystems;

public class ItemButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject clickCircle;

    private void Awake()
    {
        if (clickCircle != null) clickCircle.SetActive(false);
    }

    /// <summary>
    ///     Shows the circle when the mouse enters the button.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (clickCircle != null) clickCircle.SetActive(true);
    }

    /// <summary>
    ///     Hides the click circle when the mouse leaves the button.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (clickCircle != null) clickCircle.SetActive(false);
    }

    /// <summary>
    ///     Shows the click circle when the button is pressed.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickCircle != null) clickCircle.SetActive(true);
    }

    /// <summary>
    ///     Hides the click circle when the button is released.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (clickCircle != null) clickCircle.SetActive(false);
    }
}