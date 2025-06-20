
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;
using TMPro;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using System;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SurveyManager : UdonSharpBehaviour
{
    private SurveyConfig surveyConfig;
    private string configJson;

    [Header("Survey Item Settings")]
    [SerializeField] private GameObject surveyContainer;
    [SerializeField] private GameObject startingContainer;
    [SerializeField] private TextMeshProUGUI intro;
    [SerializeField] private TextMeshProUGUI info;
    [SerializeField] private GameObject surveyInterface;
    [SerializeField] private Transform surveyPanelParent;
    [SerializeField] private GameObject sendingContainer;
    [SerializeField] private GameObject sendingObject;
    [SerializeField] private GameObject completionObject;
    [SerializeField] private GameObject loadingContainer;

    [Header("Prefab Settings")]
    [SerializeField] private GameObject singleChoicePanelPrefab;
    [SerializeField] private GameObject freeTextPanelPrefab;
    [SerializeField] private GameObject scalePanelPrefab;
    [SerializeField] private GameObject ratingPanelPrefab;
    [SerializeField] private GameObject buttonContainerPrefab;
    [SerializeField] private GameObject circleIconPrefab;
    [SerializeField] private GameObject starIconPrefab;

    [Header("Color Item Settings")]
    private Color themeColor;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image progressBackground;
    [SerializeField] private Image progressFill;
    [SerializeField] private Image sendingSpinner;

    [Header("Warning Message Settings")]
    [SerializeField] private GameObject warningMessage;
    [SerializeField] private TextMeshProUGUI warningText;

    [Header("Progress Bar Elements")]
    [SerializeField] private Slider progressIndicator;
    [SerializeField] private TextMeshProUGUI progressRateText;
    private float targetProgress = 0f;
    private float progressSmoothSpeed = 9.0f;

    private int currentQuestion = 0;

    private bool isButtonClicked = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioManager audioManager;

    private float stepInterval = 0.08f;
    private float spinnerTimer = 0f;
    private int stepIndex = 0;
    private bool isSending = false;

    public VRCPlayerApi localPlayer;
    private bool isUserInVR = false;
    [SerializeField] private VRCPickup pickup;
    [SerializeField] private Transform uiPlaneTransform;
    [SerializeField] private  GraphicRaycaster raycaster;
    private float pickupThreshold = 0.6f;
    private bool lastPickupable;

    private string persistKey;
    private bool answered = false;

    private const float surveyTimeoutSec = 3600f;
    private bool  surveyActive = false;
    private bool  surveyFinished = false;
    private float surveyStartT = 0f;
    private bool hasStartedTimer = false;

    private bool alreadySentStarted  = false;
    private bool alreadySentFinished = false;
    private ParticipantCounter participantCounter;

    private bool isPermanentSurvey = true;

    private bool isConfigReady = false;
    private bool isUiInitialized = false;
    [SerializeField] private PersistenceListener persistenceListener;

    private DateTime startDate;
    private DateTime endDate;

    private int numQuestions = 0;
    private string[] questionTypes;
    private bool[] questionRequired;
    private string[] questionTexts;
    private string[][] questionChoices;
    private string[] scaleLeftLabels;
    private string[] scaleRightLabels;
    private string[] answers;
    private VRCUrl[] freeTextVRCUrls;

    private string[] lastSentAnswers;
    private VRCUrl[] lastSentFreeTextUrls;

    private string coverIntroText;
    private int coverTimeRequired;

    private GameObject[] questionPanels;

    private void SetSurveyFacingPlayer()
    {
        Vector3 playerPosition = localPlayer.GetBonePosition(HumanBodyBones.Head);
        Quaternion playerRotation = localPlayer.GetRotation();
        surveyContainer.transform.position = playerPosition + playerRotation * Vector3.forward * 1.3f;
        surveyContainer.transform.rotation = Quaternion.LookRotation(playerRotation * Vector3.forward);
    }

    public void Initialize(SurveyConfig _configObj, string _configJson, ParticipantCounter _participantCounter = null, bool _isPermanent = true)
    {
        loadingContainer.SetActive(true);

        isPermanentSurvey = _isPermanent;

        localPlayer = Networking.LocalPlayer;
        isUserInVR = localPlayer != null && localPlayer.IsUserInVR();

        if (isPermanentSurvey) {
            pickup.pickupable = false;
            raycaster.enabled = true;
            lastPickupable = false;
        } else if (isUserInVR) {
            pickup.pickupable = true;
            pickup.proximity = 0.4f;
        }

        surveyConfig = _configObj;
        configJson = _configJson;
        ProcessConfigJson(configJson);
        participantCounter = _participantCounter;

        isConfigReady = true;
    }

    private void InitializeSurveyUI()
    {
        if (!string.IsNullOrEmpty(persistKey) && PlayerData.HasKey(localPlayer, persistKey)) {
            answered = PlayerData.GetBool(localPlayer, persistKey);
        }

        if (answered) {
            backgroundImage.color = themeColor;
            closeButton.GetComponent<Image>().color = themeColor;
            intro.text = "このアンケートはすでに回答済みです。\nご協力ありがとうございました。";
            info.text = $"回答期間：{startDate:yyyy/MM/dd} ～ {endDate.AddDays(-1):yyyy/MM/dd}";
            startButton.gameObject.SetActive(false);
            loadingContainer.SetActive(false);
            return;
        }

        CreateSurveyUI();
        loadingContainer.SetActive(false);
    }

    private void ProcessConfigJson(string json)
    {
        DataToken tokenResult;
        if (VRCJson.TryDeserializeFromJson(json, out tokenResult))
        {
            DataToken rootToken = tokenResult.DataDictionary["data"];
            var dict = rootToken.DataDictionary;

            DataToken uuidToken = dict["surveyUuid"];
            if (uuidToken.TokenType == TokenType.String)
            {
                string uuid = uuidToken.String;
                persistKey = $"SurveyAnswered_{uuid}";
            }
            else
            {
                Debug.LogError("surveyUuid is missing or not a string");
            }

            DataToken periodToken = dict["period"];
            if (periodToken.TokenType == TokenType.DataDictionary)
            {
                var periodDict = periodToken.DataDictionary;
                string startStr = periodDict["startAt"].String;
                string endStr   = periodDict["endAt"].String;

                startDate = DateTime.ParseExact(startStr, "yyyy-MM-dd", null);
                endDate   = DateTime.ParseExact(endStr, "yyyy-MM-dd", null).AddDays(1);
            }
            else
            {
                Debug.LogError("period is not in expected format");
            }

            DataToken colorToken = dict["themeColor"];
            if (colorToken.TokenType == TokenType.DataDictionary)
            {
                var colorDict = colorToken.DataDictionary;
                float r = (float)colorDict["r"].Double;
                float g = (float)colorDict["g"].Double;
                float b = (float)colorDict["b"].Double;
                float a = (float)colorDict["a"].Double;
                themeColor = new Color(r, g, b, a);
            }
            else
            {
                Debug.LogError("themeColor is not in expected format (DataDictionary).");
            }

            DataToken coverToken = dict["cover"];
            if (coverToken.TokenType == TokenType.DataDictionary)
            {
                var coverDict = coverToken.DataDictionary;
                coverIntroText = coverDict["introText"].String;
                coverTimeRequired = (int)coverDict["timeRequired"].Double;
            }
            else
            {
                Debug.LogError("cover is not in expected format (DataDictionary).");
            }

            DataToken questionsToken = dict["questions"];
            if (questionsToken.TokenType == TokenType.DataList)
            {
                int count = questionsToken.DataList.Count;
                numQuestions = count;

                questionTypes = new string[numQuestions];
                questionRequired = new bool[numQuestions];
                questionTexts = new string[numQuestions];
                questionChoices = new string[numQuestions][];
                scaleLeftLabels = new string[numQuestions];
                scaleRightLabels = new string[numQuestions];
                answers = new string[numQuestions];
                freeTextVRCUrls = new VRCUrl[numQuestions];
                lastSentAnswers = new string[numQuestions];
                lastSentFreeTextUrls = new VRCUrl[numQuestions];

                questionPanels = new GameObject[numQuestions];

                for (int i = 0; i < count; i++)
                {
                    DataToken questionToken = questionsToken.DataList[i];
                    if (questionToken.TokenType == TokenType.DataDictionary)
                    {
                        var qDict = questionToken.DataDictionary;
                        questionTypes[i] = qDict["type"].String.Trim();
                        questionRequired[i] = qDict["required"].Boolean;
                        questionTexts[i] = qDict["text"].String;

                        DataToken choicesToken = qDict["choices"];

                        if (questionTypes[i] == "choice")
                        {
                            if (choicesToken.TokenType == TokenType.DataList)
                            {
                                int innerCount = choicesToken.DataList.Count;
                                questionChoices[i] = new string[innerCount];
                                for (int j = 0; j < innerCount; j++)
                                {
                                    questionChoices[i][j] = choicesToken.DataList[j].String;
                                }
                            }
                            else
                            {
                                Debug.LogError($"Expected DataList for choice at index {i}, got {choicesToken.TokenType}");
                            }
                        }
                        else if (questionTypes[i] == "scale" || questionTypes[i] == "rating")
                        {
                            if (choicesToken.TokenType == TokenType.DataDictionary)
                            {
                                var choicesDict = choicesToken.DataDictionary;

                                var stepsToken = choicesDict["steps"];
                                if (stepsToken.TokenType == TokenType.Double)
                                {
                                    int stepCount = (int)stepsToken.Double;
                                    questionChoices[i] = new string[stepCount];
                                    for (int j = 0; j < stepCount; j++)
                                        questionChoices[i][j] = (j + 1).ToString();
                                }
                                else
                                {
                                    Debug.LogError($"Expected Double for steps at index {i}, got {stepsToken.TokenType}");
                                }

                                if (questionTypes[i] == "scale")
                                {
                                    scaleLeftLabels[i]  = choicesDict["leftLabel"].String;
                                    scaleRightLabels[i] = choicesDict["rightLabel"].String;
                                }
                            }
                            else
                            {
                                Debug.LogError($"Expected DataDictionary for scale/rating at index {i}, got {choicesToken.TokenType}");
                            }
                        }
                        else
                        {
                            questionChoices[i] = new string[0];
                        }

                        answers[i] = "";
                        freeTextVRCUrls[i] = VRCUrl.Empty;
                        lastSentFreeTextUrls[i] = VRCUrl.Empty;
                    } else {
                        Debug.LogError("Expected each question to be a DataDictionary at index " + i);
                        continue;
                    }
                }
            } else {
                Debug.LogError("Expected 'questions' to be a DataList.");
            }
        }
        else
        {
            Debug.LogError("Failed to parse config JSON.");
        }
    }

    private void CreateSurveyUI()
    {
        backgroundImage.color = themeColor;
        startButton.GetComponent<Image>().color = themeColor;
        backButton.GetComponent<Image>().color = themeColor;
        nextButton.GetComponent<Image>().color = themeColor;
        closeButton.GetComponent<Image>().color = themeColor;
        progressBackground.color = themeColor;
        progressFill.color = themeColor;
        sendingSpinner.color = themeColor;

        DateTime now = DateTime.Now;
        bool isWithinPeriod = (now >= startDate) && (now < endDate);

        if (!isWithinPeriod) {
            if (now < startDate) {
                intro.text = "このアンケートはまだ開始されていません。";
            } else {
                intro.text = "このアンケートの受付は終了しました。";
            }

            info.text = $"回答期間：{startDate:yyyy/MM/dd} ～ {endDate.AddDays(-1):yyyy/MM/dd}";
            startButton.gameObject.SetActive(false);
            return;
        }

        intro.text = coverIntroText;
        var formatted = FormatDuration(coverTimeRequired);
        info.text = $"■ 質問数: {numQuestions}問（所要時間: {formatted}）　■ 回答はすべて匿名です\n■「信頼されていないURLを許可」をONにしてください";

        for (int i = 0; i < numQuestions; i++) {
            GameObject panel = null;
            string[] choices = questionChoices[i];

            switch (GetType(questionTypes[i]))
            {
                case "choice":
                    panel = Instantiate(singleChoicePanelPrefab, surveyPanelParent);

                    SingleChoicePanelController singleChoiceController = panel.GetComponent<SingleChoicePanelController>();
                    singleChoiceController.InitializePanel(this, i);

                    Transform optionContainer = panel.transform.Find("OptionContainer");

                    var qLabelSingle = panel.transform.Find("QuestionLabel").GetComponent<TextMeshProUGUI>();
                    if (questionRequired[i]) {
                        qLabelSingle.text = $"{questionTexts[i]} <size=60><color=#DC3545>*</color></size>";
                    } else {
                        qLabelSingle.text = questionTexts[i];
                    }

                    for (int j = 0; j < choices.Length; j++) {
                        GameObject btnContainer = Instantiate(buttonContainerPrefab, optionContainer);
                        GameObject btnObj = btnContainer.transform.Find("OptionButton").gameObject;
                        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                        btnText.text = choices[j];

                        Image btnImage = btnObj.GetComponent<Image>();
                        btnImage.color = themeColor;
                    }
                    break;

                case "freeText":
                    panel = Instantiate(freeTextPanelPrefab, surveyPanelParent);

                    FreeTextPanelController freeTextController = panel.GetComponent<FreeTextPanelController>();
                    VRCUrl prefixUrl = surveyConfig.GetFreeTextPrefix(i);
                    freeTextController.InitializePanel(this, i, prefixUrl);

                    var qLabelFree = panel.transform.Find("QuestionLabel").GetComponent<TextMeshProUGUI>();
                    if (questionRequired[i]) {
                        qLabelFree.text = $"{questionTexts[i]} <size=60><color=#DC3545>*</color></size>";
                    } else {
                        qLabelFree.text = questionTexts[i];
                    }

                    Transform frameImageTransform = panel.transform.Find("Frame/FrameImage");
                    Image frameImg = frameImageTransform.GetComponent<Image>();
                    frameImg.color = themeColor;

                    RectTransform panelRect = panel.GetComponent<RectTransform>();
                    panelRect.localScale = new Vector3(1, 1, 1);
                    break;

                case "scale":
                    panel = Instantiate(scalePanelPrefab, surveyPanelParent);

                    ScalePanelController scaleController = panel.GetComponent<ScalePanelController>();
                    scaleController.InitializePanel(this, i);

                    var qLabelScale = panel.transform.Find("QuestionLabel").GetComponent<TextMeshProUGUI>();
                    if (questionRequired[i]) {
                        qLabelScale.text = $"{questionTexts[i]} <size=60><color=#DC3545>*</color></size>";
                    } else {
                        qLabelScale.text = questionTexts[i];
                    }

                    Transform optionContainerScale = panel.transform.Find("OptionContainer");

                    Transform labelContainerL = optionContainerScale.Find("LabelContainer_L");
                    var evaluatedLabelL = labelContainerL.Find("EvaluatedLabel_L").GetComponent<TextMeshProUGUI>();
                    evaluatedLabelL.text = scaleLeftLabels[i];

                    Transform labelContainerR = optionContainerScale.Find("LabelContainer_R");
                    var evaluatedLabelR = labelContainerR.Find("EvaluatedLabel_R").GetComponent<TextMeshProUGUI>();
                    evaluatedLabelR.text = scaleRightLabels[i];

                    for (int j = 0; j < choices.Length; j++) {
                        GameObject circleIcon = Instantiate(circleIconPrefab, optionContainerScale);
                        var numberText = circleIcon.transform.Find("Number").GetComponent<TextMeshProUGUI>();
                        numberText.text = choices[j];

                        Transform circleIconTransform = circleIcon.transform.Find("CircleIcon");
                        Image circleIconImage = circleIconTransform.GetComponent<Image>();
                        circleIconImage.color = themeColor;

                        int insertIndex = labelContainerL.GetSiblingIndex() + 1 + j;
                        circleIcon.transform.SetSiblingIndex(insertIndex);
                    }
                    break;

                case "rating":
                    panel = Instantiate(ratingPanelPrefab, surveyPanelParent);

                    RatingPanelController ratingController = panel.GetComponent<RatingPanelController>();
                    ratingController.InitializePanel(this, i);

                    var qLabelRating = panel.transform.Find("QuestionLabel").GetComponent<TextMeshProUGUI>();
                    if (questionRequired[i]) {
                        qLabelRating.text = $"{questionTexts[i]} <size=60><color=#DC3545>*</color></size>";
                    } else {
                        qLabelRating.text = questionTexts[i];
                    }

                    Transform optionContainerRating = panel.transform.Find("OptionContainer");
                    for (int j = 0; j < choices.Length; j++) {
                        GameObject starIcon = Instantiate(starIconPrefab, optionContainerRating);
                        var numberText = starIcon.transform.Find("Number").GetComponent<TextMeshProUGUI>();
                        numberText.text = choices[j];

                        Transform starIconTransform = starIcon.transform.Find("StarIcon");
                        Image starIconImage = starIconTransform.GetComponent<Image>();
                        starIconImage.color = themeColor;
                    }
                    break;

                default:
                    Debug.LogError("Unknown question type: " + questionTypes[i]);
                    break;
            }

            questionPanels[i] = panel;
        }

        ShowFirstQuestionPanel();
    }

    private string FormatDuration(int totalSec)
    {
        if (totalSec == 0) {
            return "0秒";
        }
        if (totalSec <= 30) {
            return "約30秒";
        }
        if (totalSec <= 60) {
            return "約1分";
        }

        int m   = totalSec / 60;
        int rem = totalSec - m * 60;

        if (rem == 0) {
            return $"約{m}分";
        }
        if (rem <= 30) {
            return $"約{m}分半";
        }
        return $"約{m + 1}分";
    }

    private void ShowFirstQuestionPanel()
    {
        UpdateProgress(0);

        backButton.interactable = false;
        backButton.GetComponent<Image>().raycastTarget = false;

        var firstPanel = questionPanels[0];
        firstPanel.GetComponent<CanvasGroup>().alpha = 1f;
        firstPanel.SetActive(true);
    }

    public void StartSurvey()
    {
        isButtonClicked = false;

        surveyInterface.transform.localScale = new Vector3(1f, 1f, 1f);
        startingContainer.SetActive(false);

        if (!alreadySentStarted && participantCounter != null) {
            participantCounter.NotifyStarted();
            alreadySentStarted = true;
        }
    }

    public void SetAnswer(int questionIndex, string answer)
    {
        if (questionIndex < 0 || questionIndex >= numQuestions) return;
        answers[questionIndex] = answer;
    }

    public void SetFreeTextVRCUrl(int questionIndex, VRCUrl url)
    {
        if (questionIndex < 0 || questionIndex >= numQuestions) return;
        freeTextVRCUrls[questionIndex] = url;
    }

    public void GoNext()
    {
        isButtonClicked = false;

        if (string.IsNullOrEmpty(answers[currentQuestion]) && questionRequired[currentQuestion])
        {
            string message = "この質問は必須です";
            DisplayWarning(message);
            return;
        }

        SubmitAnswerForQuestion(currentQuestion);

        GameObject currentPanel = questionPanels[currentQuestion];
        Animator currentAnim = currentPanel.GetComponent<Animator>();
        currentAnim.SetTrigger("FadeOutTrigger");

        currentQuestion++;

        if (currentQuestion >= numQuestions) {
            isSending = true;
            surveyInterface.SetActive(false);
            sendingContainer.SetActive(true);
            return;
        }

        GameObject nextPanel = questionPanels[currentQuestion];
        nextPanel.SetActive(true);
        Animator nextAnim = nextPanel.GetComponent<Animator>();
        nextAnim.SetTrigger("FadeInTrigger");

        UpdateProgress(currentQuestion);

        if (currentQuestion > 0 && currentQuestion < numQuestions)
        {
            backButton.interactable = true;
            backButton.GetComponent<Image>().raycastTarget = true;
        }

        if (currentQuestion == numQuestions - 1) {
            nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "送信";
        }
    }

    private int pendingDownloads = 0;

    private void SubmitAnswerForQuestion(int questionIndex)
    {
        if (!hasStartedTimer) {
            hasStartedTimer = true;
            surveyActive = true;
            surveyStartT = Time.time;
        }

        string type = GetType(questionTypes[questionIndex]);
        VRCUrl targetUrl;

        if (type == "freeText") {
            targetUrl = freeTextVRCUrls[questionIndex];

            if (targetUrl.Equals(lastSentFreeTextUrls[questionIndex])) return;

            lastSentFreeTextUrls[questionIndex] = targetUrl;
        } else {
            string answer = answers[questionIndex];

            if ((lastSentAnswers[questionIndex] != null) && (answer == lastSentAnswers[questionIndex])) return;

            int optionIndex;
            if (string.IsNullOrEmpty(answer)) {
                optionIndex = 0;
            } else {
                optionIndex = int.Parse(answer);
            }

            targetUrl = surveyConfig.GetResponseUrl(questionIndex, optionIndex);
            lastSentAnswers[questionIndex] = answer;
        }

        pendingDownloads++;
        VRCStringDownloader.LoadUrl(targetUrl, this.GetComponent<UdonBehaviour>());
    }

    public override void OnStringLoadSuccess(IVRCStringDownload download)
    {
        pendingDownloads--;
        CheckAllDownloadsComplete();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        pendingDownloads--;
        CheckAllDownloadsComplete();
    }

    private void CheckAllDownloadsComplete()
    {
        if (pendingDownloads <= 0 && isSending) {
            ShowSubmissionMessage();
        }
    }

    private void ShowSubmissionMessage()
    {
        isSending = false;
        sendingObject.SetActive(false);
        completionObject.SetActive(true);

        PlayerData.SetBool(persistKey, true);

        surveyFinished = true;
        surveyActive   = false;

        if (!alreadySentFinished && participantCounter != null) {
            participantCounter.NotifyFinished();
            alreadySentFinished = true;
        }
    }

    public void GoBack()
    {
        isButtonClicked = false;

        GameObject currentPanel = questionPanels[currentQuestion];
        Animator currentAnim = currentPanel.GetComponent<Animator>();
        currentAnim.SetTrigger("FadeOutTrigger");

        currentQuestion--;

        GameObject prevPanel = questionPanels[currentQuestion];
        prevPanel.SetActive(true);
        Animator prevAnim = prevPanel.GetComponent<Animator>();
        prevAnim.SetTrigger("FadeInTrigger");

        UpdateProgress(currentQuestion);

        if (currentQuestion < numQuestions - 1)
        {
            nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "次へ";
        }

        if (currentQuestion == 0)
        {
            backButton.interactable = false;
            backButton.GetComponent<Image>().raycastTarget = false;
        }
    }

    public void CloseSurvey()
    {
        isButtonClicked = false;
        if (alreadySentStarted && !alreadySentFinished) {
            ResetSurvey();
        }
        surveyContainer.SetActive(false);
    }

    private void UpdateProgress(int questionIndex)
    {
        targetProgress = (float)(questionIndex + 1) / (float)numQuestions;
        progressRateText.text = $"{questionIndex + 1}/{numQuestions}";
    }

    private void Update()
    {
        if (!isUiInitialized && isConfigReady && persistenceListener.GetRestoredCompleted()) {
            isUiInitialized = true;
            InitializeSurveyUI();
        }

        if (Mathf.Abs(progressIndicator.value - targetProgress) > 0.001f)
        {
            progressIndicator.value = Mathf.Lerp(progressIndicator.value, targetProgress, Time.deltaTime * progressSmoothSpeed);
        }

        if (isSending) {
            spinnerTimer += Time.deltaTime;
            if (spinnerTimer >= stepInterval)
            {
                stepIndex++;
                float newAngle = stepIndex * -30f;
                sendingSpinner.rectTransform.rotation = Quaternion.Euler(0f, 0f, newAngle);
                spinnerTimer = 0f;
            }
        }

        if (!isPermanentSurvey && !isUserInVR && surveyContainer.activeSelf && !pickup.IsHeld && (localPlayer != null)) {
            Vector3 playerPosition = localPlayer.GetBonePosition(HumanBodyBones.Head);

            Vector3 pointOnPlane = uiPlaneTransform.position;
            Vector3 vectorToPlane = playerPosition - pointOnPlane;
            Vector3 planeNormal = uiPlaneTransform.forward;
            float perpendicularDistance = Mathf.Abs(Vector3.Dot(vectorToPlane, planeNormal));

            bool newPickupable = (perpendicularDistance <= pickupThreshold);

            if (lastPickupable != newPickupable) {
                pickup.pickupable = newPickupable;
                raycaster.enabled = !newPickupable;
                lastPickupable = newPickupable;
            }
        }

        if (surveyActive && !surveyFinished && (Time.time - surveyStartT) >= surveyTimeoutSec)
        {
            ResetSurvey();
        }
    }

    private void SetButtonSelected(Button btn, bool isSelected)
    {
        TextMeshProUGUI btnLabel = btn.GetComponentInChildren<TextMeshProUGUI>();

        if(isSelected) {
            btnLabel.color = Color.white;
        } else {
            btnLabel.color = themeColor;
        }
    }

    private string GetType(string questionType)
    {
        return questionType.Trim();
    }

    public void PlayHoverSound()
    {
        if (audioManager != null) {
            audioManager.PlayHoverSound();
        }
    }

    public void PlayClickSound()
    {
        if (audioManager != null) {
            audioManager.PlayClickSound();
        }
    }

    public void OnPointerEnter()
    {
        PlayHoverSound();
    }

    public void OnPointerExit()
    {
        isButtonClicked = false;
    }

    public void OnPointerDown()
    {
        if (!isButtonClicked) {
            PlayClickSound();
            isButtonClicked = true;
        }
    }

    public void DisplayWarning(string message)
    {
        warningText.text = message;
        Animator warningAnim = warningMessage.GetComponent<Animator>();
        warningAnim.SetTrigger("FadeOut");
    }

    public void ShowSurveyUI()
    {
        SetSurveyFacingPlayer();
        surveyContainer.SetActive(true);
    }

    public void HideSurveyUI()
    {
        surveyContainer.SetActive(false);
    }

    private void ResetSurvey()
    {
        currentQuestion = 0;
        targetProgress = 0f;
        isSending = false;
        spinnerTimer = 0f;
        stepIndex = 0;

        for (int i = 0; i < numQuestions; i++)
        {
            answers[i] = "";
            freeTextVRCUrls[i] = VRCUrl.Empty;
            lastSentAnswers[i] = null;
            lastSentFreeTextUrls[i] = VRCUrl.Empty;

            var panel = questionPanels[i];
            if (panel != null) {
                panel.SetActive(false);
                CanvasGroup cg = panel.GetComponent<CanvasGroup>();
                if (cg) cg.alpha = 0f;

                var singleCtrl = panel.GetComponent<SingleChoicePanelController>();
                if (singleCtrl != null) {
                    singleCtrl.ResetPanel();
                    continue;
                }

                var scaleCtrl = panel.GetComponent<ScalePanelController>();
                if (scaleCtrl != null) {
                    scaleCtrl.ResetPanel();
                    continue;
                }

                var ratingCtrl = panel.GetComponent<RatingPanelController>();
                if (ratingCtrl != null) {
                    ratingCtrl.ResetPanel();
                    continue;
                }

                var freeTextCtrl = panel.GetComponent<FreeTextPanelController>();
                if (freeTextCtrl != null) {
                    freeTextCtrl.ResetPanel();
                    continue;
                }
            }
        }

        ShowFirstQuestionPanel();

        nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "次へ";

        surveyActive = false;
        surveyStartT = 0f;
        hasStartedTimer = false;

        sendingContainer.SetActive(false);
        startingContainer.SetActive(true);
        surveyInterface.transform.localScale = new Vector3(0f, 0f, 0f);

        if (alreadySentStarted && !alreadySentFinished && participantCounter != null) {
            participantCounter.NotifyReset();
            alreadySentStarted = false;
            alreadySentFinished = false;
        }
    }
}
