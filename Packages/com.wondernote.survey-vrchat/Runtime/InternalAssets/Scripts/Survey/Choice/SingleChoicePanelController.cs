
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SingleChoicePanelController : FadePanel
{
    private OptionButtonBehavior[] optionButtonsBehavior;

    private int selectedIndex = -1;

    private SurveyManager surveyManager;
    private int questionIndex;
    private string answer = "";

    private void Start()
    {
        optionButtonsBehavior = GetComponentsInChildren<OptionButtonBehavior>();
        for (int i = 0; i < optionButtonsBehavior.Length; i++)
        {
            optionButtonsBehavior[i].Initialize(this, i);
        }

        AdjustButtonWidths();
    }

    private void AdjustButtonWidths()
    {
        float maxWidth = 0f;
        foreach (var btn in optionButtonsBehavior)
        {
            float width = btn.GetTextPreferredWidth();
            if (width > maxWidth) {
                maxWidth = width;
            }
        }

        float padding = 400f;
        float targetWidth = maxWidth + padding;
        targetWidth = Mathf.Clamp(targetWidth, 700f, 1600f);

        foreach (var btn in optionButtonsBehavior)
        {
            btn.SetButtonWidth(targetWidth);
        }
    }

    public void InitializePanel(SurveyManager manager, int qIndex)
    {
        surveyManager = manager;
        questionIndex = qIndex;
    }

    public void OnOptionButtonClicked(int index)
    {
        if (selectedIndex == index) {
            optionButtonsBehavior[index].SetSelected(false);
            selectedIndex = -1;
        } else {
            if (selectedIndex >= 0) {
                optionButtonsBehavior[selectedIndex].SetSelected(false);
            }
            optionButtonsBehavior[index].SetSelected(true);
            selectedIndex = index;
        }

        answer = (selectedIndex >= 0) ? optionButtonsBehavior[selectedIndex].GetAnswerText() : "";

        surveyManager.SetAnswer(questionIndex, answer);
    }

    public void PlayClickSound()
    {
        surveyManager.PlayClickSound();
    }

    public void PlayHoverSound()
    {
        surveyManager.PlayHoverSound();
    }

    public void ResetPanel()
    {
        selectedIndex = -1;
        answer = "";

        if (optionButtonsBehavior != null)
        {
            foreach (var btn in optionButtonsBehavior)
            {
                btn.SetSelected(false);
            }
        }
    }
}
