
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class FreeTextPanelController : FadePanel
{
    private SurveyManager surveyManager;
    private int questionIndex;
    private string answer = "";

    private VRCUrl presetPrefixUrl;
    [SerializeField] private VRCUrlInputField inputField;
    [SerializeField] private Text actualText;
    [SerializeField] private GameObject dummyPrompt;
    [SerializeField] private GameObject dummyInputField;
    [SerializeField] private GameObject dummyPlaceholder;
    [SerializeField] private Text dummyText;
    private string urlPrefix;
    private VRCUrl lastUrl;

    private VRCPlayerApi localPlayer;
    private float originalJumpImpulse = 0.0f;

    private bool isActive = false;

    bool hasInput = false;
    private float inputAreaHeight = 635f;
    private bool isHovered = false;

    private float debounceTime = 0.05f;
    private float lastInputTime = 0f;
    private bool debounceActive = false;

    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoveredSprite;
    [SerializeField] private Sprite focusedSprite;

    public void InitializePanel(SurveyManager manager, int qIndex, VRCUrl _url)
    {
        surveyManager = manager;
        questionIndex = qIndex;
        presetPrefixUrl = _url;
    }

    private void Start()
    {
        inputField.SetUrl(presetPrefixUrl);
        localPlayer = (VRCPlayerApi)surveyManager.GetProgramVariable("localPlayer");
        originalJumpImpulse = localPlayer.GetJumpImpulse();
        lastUrl = presetPrefixUrl;
        urlPrefix = presetPrefixUrl.Get();

        if (questionIndex == 0 && actualText != null) {
            actualText.color = new Color32(50, 50, 50, 255);
        }
    }

    public void ClickField()
    {
        if (!isActive) {
            surveyManager.PlayClickSound();
            isActive = true;

            dummyInputField.SetActive(false);

            if (inputField.GetUrl().Get() == urlPrefix) {
                dummyPrompt.SetActive(true);
                hasInput = false;
            } else {
                dummyPrompt.SetActive(false);
                hasInput = true;
            }

            inputField.selectionColor = new Color32(168, 206, 255, 192);
            frameImage.sprite = focusedSprite;

            localPlayer.Immobilize(true);
            localPlayer.SetJumpImpulse(0.0f);
        }
    }

    public void DeselectField()
    {
        isActive = false;

        inputField.SetUrl(lastUrl);

        dummyInputField.SetActive(true);
        frameImage.sprite = normalSprite;
        string currentUrl = inputField.GetUrl().Get();
        answer = currentUrl.Substring(urlPrefix.Length);
        dummyText.text = answer;

        if (string.IsNullOrEmpty(answer)) {
            dummyPlaceholder.SetActive(true);
        } else {
            dummyPlaceholder.SetActive(false);
        }

        inputField.selectionColor = new Color32(168, 206, 255, 0);

        localPlayer.Immobilize(false);
        localPlayer.SetJumpImpulse(originalJumpImpulse);

        surveyManager.SetFreeTextVRCUrl(questionIndex, inputField.GetUrl());
    }

    public void PointerEnter()
    {
        isHovered = true;

        if (!isActive)
        {
            surveyManager.PlayHoverSound();
            inputField.ActivateInputField();
            frameImage.sprite = hoveredSprite;
        }
    }

    public void PointerExit()
    {
        isHovered = false;

        if (!isActive)
        {
            inputField.DeactivateInputField();
            frameImage.sprite = normalSprite;
        }
    }

    public void OnValueChanged()
    {
        lastInputTime = Time.time;
        debounceActive = true;
    }

    private void ProcessDebouncedInput()
    {
        string currentUrl = inputField.GetUrl().Get();
        bool hasValidUrl = currentUrl.StartsWith(urlPrefix) && currentUrl.Length >= urlPrefix.Length;

        if (!hasValidUrl) {
            inputField.SetUrl(lastUrl);
            DeselectField();
            if (isHovered) {
                PointerEnter();
            }
        }

        SendCustomEventDelayedFrames(nameof(DeferredCheck), 1);
    }

    public void DeferredCheck()
    {
        float preferredHeight = inputField.textComponent.preferredHeight;

        if (preferredHeight > inputAreaHeight) {
            inputField.SetUrl(lastUrl);

            string message = "最大行数に達しました";
            surveyManager.DisplayWarning(message);
        }

        lastUrl = inputField.GetUrl();
        string unPrefixedUrl = lastUrl.Get().Substring(urlPrefix.Length);
        hasInput = !string.IsNullOrEmpty(unPrefixedUrl);

        if (hasInput) {
            dummyPrompt.SetActive(false);
        } else {
            dummyPrompt.SetActive(true);
        }
    }

    private void Update()
    {
        if (debounceActive && Time.time - lastInputTime >= debounceTime)
        {
            ProcessDebouncedInput();
            debounceActive = false;
        }

        if (isActive && !hasInput && !debounceActive) {
            string lastUrlStr = inputField.textComponent.text;
            string unPrefixedUrl = lastUrlStr.Substring(urlPrefix.Length);

            if (string.IsNullOrEmpty(unPrefixedUrl)) {
                dummyPrompt.SetActive(true);
            } else {
                dummyPrompt.SetActive(false);
            }
        }
    }

    public override void FadeInEnded()
    {
        actualText.color = new Color32(50, 50, 50, 255);
    }

    public override void FadeOutStarted()
    {
        actualText.color = new Color32(50, 50, 50, 0);
    }

    public void ResetPanel()
    {
        inputField.SetUrl(presetPrefixUrl);
        lastUrl = presetPrefixUrl;
        answer = "";
        hasInput = false;

        dummyText.text = "";
        dummyPlaceholder.SetActive(true);
        dummyPrompt.SetActive(false);

        isActive = false;
    }
}
