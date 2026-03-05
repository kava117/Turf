using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public abstract class BaseButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Image backgroundImage;
    [SerializeField] protected TextMeshProUGUI label;

    // PLACEHOLDER:
    // the following serialized fields will be phased out eventually for
    // the animations/whatever happens when you hover and click
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;
    

    private bool _isDisabled = false;

    public void SetDisabled(bool disabled)
    {
        _isDisabled = disabled;
        backgroundImage.color = disabled ? disabledColor : normalColor;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (_isDisabled) return;
        backgroundImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (_isDisabled) return;
        backgroundImage.color = normalColor;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (_isDisabled) return;
        SoundManager.Instance?.PlayClick(); // every button press has to play a click
        OnClick();                         // delegates to subclass so they can be personalized for each button
    }

    protected abstract void OnClick(); // left vague so every subclass MUST implement
}