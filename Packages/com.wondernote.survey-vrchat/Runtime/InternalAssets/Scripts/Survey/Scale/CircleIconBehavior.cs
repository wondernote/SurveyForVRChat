
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CircleIconBehavior : UdonSharpBehaviour
{
    private ScalePanelController parentController;
    private int index;
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image btnImage;

    private Color unselectedColor;
    private Color selectedColor = Color.white;

    [SerializeField] private Sprite unselectedSprite;
    [SerializeField] private Sprite unselectedHighlightedSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite selectedHighlightedSprite;

    private bool isSelected = false;
    private bool isHovered = false;
    private bool isClicked = false;

    private void Start()
    {
        unselectedColor = btnImage.color;
    }

    public void Initialize(ScalePanelController controller, int buttonIndex)
    {
        parentController = controller;
        index = buttonIndex;
    }

    public void SetSelected(bool sel)
    {
        isSelected = sel;

        if (isHovered) {
            btnImage.sprite = isSelected ? selectedHighlightedSprite : unselectedHighlightedSprite;
        } else {
            btnImage.sprite = isSelected ? selectedSprite : unselectedSprite;
        }
    }

    public void OnCircleIconClick()
    {
        isClicked = false;
    }

    public void HandlePointerEnter()
    {
        isHovered = true;
        parentController.PlayHoverSound();
        btnImage.sprite = isSelected ? selectedHighlightedSprite : unselectedHighlightedSprite;
    }

    public void HandlePointerExit()
    {
        isHovered = false;
        btnImage.sprite = isSelected ? selectedSprite : unselectedSprite;
    }

    public string GetAnswerText()
    {
        return textComponent.text;
    }

    public void OnPointerDown()
    {
        if (!isClicked) {
            parentController.PlayClickSound();
            parentController.OnCircleIconClicked(index);
            isClicked = true;
        }
    }
}
