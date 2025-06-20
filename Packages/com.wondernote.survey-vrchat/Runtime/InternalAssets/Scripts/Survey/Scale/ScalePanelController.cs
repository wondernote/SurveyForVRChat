
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ScalePanelController : FadePanel
{
    private CircleIconBehavior[] circleIconsBehavior;

    private int selectedIndex = -1;

    private SurveyManager surveyManager;
    private int questionIndex;
    private string answer = "";

    private void Start()
    {
        circleIconsBehavior = GetComponentsInChildren<CircleIconBehavior>();
        for (int i = 0; i < circleIconsBehavior.Length; i++)
        {
            circleIconsBehavior[i].Initialize(this, i);
        }
    }

    public void InitializePanel(SurveyManager manager, int qIndex)
    {
        surveyManager = manager;
        questionIndex = qIndex;
    }

    public void OnCircleIconClicked(int index)
    {
        if (selectedIndex == index) {
            circleIconsBehavior[index].SetSelected(false);
            selectedIndex = -1;
        } else {
            if (selectedIndex >= 0) {
                circleIconsBehavior[selectedIndex].SetSelected(false);
            }
            circleIconsBehavior[index].SetSelected(true);
            selectedIndex = index;
        }

        answer = (selectedIndex >= 0) ? circleIconsBehavior[selectedIndex].GetAnswerText() : "";

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

        if (circleIconsBehavior != null)
        {
            for (int i = 0; i < circleIconsBehavior.Length; i++)
            {
                circleIconsBehavior[i].SetSelected(false);
            }
        }
    }
}
