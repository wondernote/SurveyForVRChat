
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
    private bool multiple = false;
    private int minSelections = 0;
    private int maxSelections = 0;
    private bool[] selectedOptions;
    private int selectedCount = 0;

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
        selectedOptions = new bool[optionButtonsBehavior.Length];

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

    public void InitializePanel(SurveyManager manager, int qIndex, bool isMultiple, int min, int max)
    {
        surveyManager = manager;
        questionIndex = qIndex;
        multiple = isMultiple;
        minSelections = min;
        maxSelections = max;
    }

    public void OnOptionButtonClicked(int index)
    {
        if (multiple) {
            OnMultipleOptionButtonClicked(index);
            return;
        }

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

    private void OnMultipleOptionButtonClicked(int index)
    {
        if (selectedOptions == null) {
            selectedOptions = new bool[optionButtonsBehavior.Length];
        }

        if (index >= 7) {
            surveyManager.DisplayWarning("この項目は選べません");
            return;
        }

        if (selectedOptions[index]) {
            selectedOptions[index] = false;
            selectedCount--;
            optionButtonsBehavior[index].SetSelected(false);
        } else {
            if (maxSelections > 0 && selectedCount >= maxSelections) {
                if (minSelections > 0 && minSelections == maxSelections) {
                    surveyManager.DisplayWarning($"{maxSelections}つ選んでください");
                } else {
                    surveyManager.DisplayWarning($"選べるのは{maxSelections}つまでです");
                }
                return;
            }

            selectedOptions[index] = true;
            selectedCount++;
            optionButtonsBehavior[index].SetSelected(true);
        }

        UpdateMultipleAnswer();
    }

    private void UpdateMultipleAnswer()
    {
        if (selectedCount <= 0) {
            answer = "";
            surveyManager.SetAnswer(questionIndex, answer);
            return;
        }

        int bitmask = 0;

        int count = selectedOptions.Length;
        if (count > 7) count = 7;

        for (int i = 0; i < count; i++)
        {
            if (selectedOptions[i]) {
                bitmask |= 1 << i;
            }
        }

        answer = bitmask.ToString();
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
        selectedCount = 0;

        if (optionButtonsBehavior != null)
        {
            if (selectedOptions == null || selectedOptions.Length != optionButtonsBehavior.Length) {
                selectedOptions = new bool[optionButtonsBehavior.Length];
            }

            for (int i = 0; i < optionButtonsBehavior.Length; i++)
            {
                selectedOptions[i] = false;
                optionButtonsBehavior[i].SetSelected(false);
            }
        }
    }
}
