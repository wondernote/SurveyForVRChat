
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class StarIconBehavior : UdonSharpBehaviour
{
    private RatingPanelController parentController;
    private int index;
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image btnImage;
    [SerializeField] private Sprite unselectedSprite;
    [SerializeField] private Sprite unselectedHighlightedSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite selectedHighlightedSprite;

    public const int STAR_STATE_UNSELECTED = 0;
    public const int STAR_STATE_SELECTED = 1;
    public const int STAR_STATE_HOVER = 2;
    public const int STAR_STATE_SELECTED_HOVER = 3;

    private bool isClicked = false;

    public void Initialize(RatingPanelController controller, int buttonIndex)
    {
        parentController = controller;
        index = buttonIndex;
    }

    public void OnStarClick()
    {
        isClicked = false;
    }

    public void OnHoverEnter()
    {
        parentController.PlayHoverSound();
        parentController.OnStarHoverEnter(index);
    }

    public void OnHoverExit()
    {
        parentController.OnStarHoverExit(index);
    }

    public string GetAnswerText()
    {
        return textComponent.text;
    }

    public void SetStarState(int state)
    {
        switch(state)
        {
            case STAR_STATE_UNSELECTED:
                btnImage.sprite = unselectedSprite;
                break;
            case STAR_STATE_SELECTED:
                btnImage.sprite = selectedSprite;
                break;
            case STAR_STATE_HOVER:
                btnImage.sprite = unselectedHighlightedSprite;
                break;
            case STAR_STATE_SELECTED_HOVER:
                btnImage.sprite = selectedHighlightedSprite;
                break;
        }
    }

    public void OnPointerDown()
    {
        if (!isClicked) {
            parentController.PlayClickSound();
            parentController.OnStarClick(index);
            isClicked = true;
        }
    }
}
