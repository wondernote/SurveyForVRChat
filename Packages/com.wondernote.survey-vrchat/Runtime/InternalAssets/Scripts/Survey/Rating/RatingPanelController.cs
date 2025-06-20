
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RatingPanelController : FadePanel
{
    private StarIconBehavior[] starIconsBehavior;

    private int selectedIndex = -1;
    private int hoveredIndex = -1;

    private SurveyManager surveyManager;
    private int questionIndex;
    private string answer = "";

    private void Start()
    {
        starIconsBehavior = GetComponentsInChildren<StarIconBehavior>();
        for(int i = 0; i < starIconsBehavior.Length; i++)
        {
            starIconsBehavior[i].Initialize(this, i);
        }
        UpdateStarStates();
    }

    public void InitializePanel(SurveyManager manager, int qIndex)
    {
        surveyManager = manager;
        questionIndex = qIndex;
    }

    public void OnStarClick(int starIndex)
    {
        if(starIndex == selectedIndex) {
            selectedIndex = -1;
        } else {
            selectedIndex = starIndex;
        }
        UpdateStarStates();

        answer = (selectedIndex >= 0) ? starIconsBehavior[selectedIndex].GetAnswerText() : "";

        surveyManager.SetAnswer(questionIndex, answer);
    }

    public void OnStarHoverEnter(int starIndex)
    {
        hoveredIndex = starIndex;
        UpdateStarStates();
    }

    public void OnStarHoverExit(int starIndex)
    {
        if(hoveredIndex == starIndex) {
            hoveredIndex = -1;
        }
        UpdateStarStates();
    }

    private void UpdateStarStates()
    {
        for (int i = 0; i < starIconsBehavior.Length; i++)
        {
            int newState = ComputeStarState(i);
            starIconsBehavior[i].SetStarState(newState);
        }
    }

    private int ComputeStarState(int starIndex)
    {
        if(hoveredIndex != -1) {
            if(hoveredIndex > selectedIndex) {
                if(starIndex <= selectedIndex)
                    return StarIconBehavior.STAR_STATE_SELECTED;
                else if(starIndex <= hoveredIndex)
                    return StarIconBehavior.STAR_STATE_HOVER;
                else
                    return StarIconBehavior.STAR_STATE_UNSELECTED;
            } else if(hoveredIndex < selectedIndex) {
                if(starIndex <= hoveredIndex)
                    return StarIconBehavior.STAR_STATE_SELECTED_HOVER;
                else if(starIndex <= selectedIndex)
                    return StarIconBehavior.STAR_STATE_SELECTED;
                else
                    return StarIconBehavior.STAR_STATE_UNSELECTED;
            } else {
                if(starIndex <= selectedIndex)
                    return StarIconBehavior.STAR_STATE_SELECTED_HOVER;
                else
                    return StarIconBehavior.STAR_STATE_UNSELECTED;
            }
        } else {
            if(starIndex <= selectedIndex)
                return StarIconBehavior.STAR_STATE_SELECTED;
            else
                return StarIconBehavior.STAR_STATE_UNSELECTED;
        }
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
        hoveredIndex = -1;
        answer = "";

        if (starIconsBehavior != null)
        {
            for (int i = 0; i < starIconsBehavior.Length; i++)
            {
                starIconsBehavior[i].SetStarState(StarIconBehavior.STAR_STATE_UNSELECTED);
            }
        }
    }
}
