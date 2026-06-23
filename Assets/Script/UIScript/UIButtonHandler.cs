using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    Vector3 originalScale;
    public float hoverScale = 1.1f;
    public bool hoverable = true;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Hover mulai
        if (AudioManager.instance != null)
        {
            AudioManager.instance.hoverButton();
        }
        if (!hoverable) return;
        transform.localScale = originalScale * hoverScale;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.hoverButton();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hover selesai
        if (!hoverable) return;
        transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Tombol ditekan
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Tombol dilepas
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Tombol diklik
        if(AudioManager.instance != null)
        {
            AudioManager.instance.clickButton();
        }
    }
}