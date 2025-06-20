
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class OptionButtonBehavior : UdonSharpBehaviour
{
    [SerializeField] private GameObject optionButton;
    private Image btnImage;
    private LayoutElement optionButtonLayout;
    private SingleChoicePanelController parentController;
    private int index;
    public TextMeshProUGUI textComponent;

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
        btnImage = optionButton.GetComponent<Image>();
        optionButtonLayout = optionButton.GetComponent<LayoutElement>();

        unselectedColor = btnImage.color;
        textComponent.color = unselectedColor;
    }

    public void Initialize(SingleChoicePanelController controller, int buttonIndex)
    {
        parentController = controller;
        index = buttonIndex;
    }

    public float GetTextPreferredWidth()
    {
        RectTransform textRT = textComponent.GetComponent<RectTransform>();
        return textRT.rect.width;
    }

    public void SetButtonWidth(float width)
    {
        optionButtonLayout.preferredWidth = width;
    }

    public void SetSelected(bool sel)
    {
        isSelected = sel;
        textComponent.color = isSelected ? selectedColor : unselectedColor;

        if (isHovered) {
            btnImage.sprite = isSelected ? selectedHighlightedSprite : unselectedHighlightedSprite;
        } else {
            btnImage.sprite = isSelected ? selectedSprite : unselectedSprite;
        }
    }

    public void OnOptionButtonClick()
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
        string buttonNum = (index + 1).ToString();
        return buttonNum;
    }

    public void OnPointerDown()
    {
        if (!isClicked) {
            parentController.PlayClickSound();
            parentController.OnOptionButtonClicked(index);
            isClicked = true;
        }
    }
}
