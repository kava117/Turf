using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Abstract base for all buttons in the game.
// Handles hover, click, and disabled visual states.
// Every subclass must implement OnClick().
public abstract class BaseButton : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Image            backgroundImage;
    [SerializeField] protected TextMeshProUGUI  label;

    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color hoverColor    = new Color(0.85f, 0.85f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f);

    private bool _isDisabled;

    public void SetInteractable(bool interactable)
    {
        _isDisabled = !interactable;
        if (backgroundImage != null)
            backgroundImage.color = _isDisabled ? disabledColor : normalColor;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (_isDisabled) return;
        if (backgroundImage != null) backgroundImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (_isDisabled) return;
        if (backgroundImage != null) backgroundImage.color = normalColor;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (_isDisabled) return;
        SoundManager.Instance?.PlayClick();
        OnClick();
    }

    protected abstract void OnClick();
}
