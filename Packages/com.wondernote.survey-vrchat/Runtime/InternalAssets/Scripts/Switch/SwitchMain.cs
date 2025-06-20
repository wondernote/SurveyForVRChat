
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.StringLoading;
using TMPro;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SwitchMain : UdonSharpBehaviour
{
    [UdonSynced] private string syncedSurveyJson;
    [UdonSynced] private bool syncedSurveyActive;
    [UdonSynced] private bool syncedLoadError;

    [SerializeField] private SurveyConfig surveyConfig;
    [SerializeField] private SurveySwitchSettings switchSettings;
    [SerializeField] private ParticipantCounter counter;

    private VRCUrl receiveUrl;
    private string downloadedJson;
    private bool isDownloaded = false;

    [SerializeField] private GameObject errorUI;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Collider interactCollider;

    [SerializeField] private SwitchAudioManager switchAudioManager;

    private bool restrictToWhitelist = false;
    private string[] authorizedStaff;

    [SerializeField] private GameObject counterUI;

    private bool isPermanentSurvey = false;

    [SerializeField] private GameObject surveyPrefab;

    private GameObject surveyInstance;
    private SurveyManager surveyManager;

    private bool surveyActive = false;

    [SerializeField] private Renderer iconRenderer;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    public void Start()
    {
        interactCollider.enabled = false;

        receiveUrl = surveyConfig.GetReceiveUrl();
        string url = (receiveUrl != null) ? receiveUrl.Get() : "";

        if (!string.IsNullOrEmpty(url)) {
            if (Networking.IsOwner(gameObject)) {
                VRCStringDownloader.LoadUrl(receiveUrl, this.GetComponent<UdonBehaviour>());
            }
        } else {
            errorText.text = "\"アンケートコード\" が設定されていません。設定を確認してください。";
            errorUI.SetActive(true);
            interactCollider.enabled = false;
        }

        restrictToWhitelist = switchSettings.restrictToWhitelist;
        authorizedStaff = switchSettings.authorizedStaff;
    }

    public override void OnStringLoadSuccess(IVRCStringDownload download)
    {
        if (download.Url == receiveUrl && Networking.IsOwner(gameObject))
        {
            syncedSurveyJson = download.Result;
            RequestSerialization();

            downloadedJson = download.Result;
            isDownloaded = true;
            interactCollider.enabled = true;
        }
    }

    public override void OnDeserialization()
    {
        if (syncedLoadError)
        {
            ShowLoadErrorUI();
            return;
        }

        if (!isDownloaded && !string.IsNullOrEmpty(syncedSurveyJson))
        {
            downloadedJson = syncedSurveyJson;
            isDownloaded = true;
            interactCollider.enabled = true;
        }

        if (!isDownloaded) return;

        if (surveyActive != syncedSurveyActive)
        {
            surveyActive = syncedSurveyActive;
            if (surveyActive) {
                OnSurveyTurnOn();
            } else {
                OnSurveyTurnOff();
            }
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        syncedLoadError = true;
        RequestSerialization();

        ShowLoadErrorUI();
    }

    private void ShowLoadErrorUI()
    {
        errorText.text = "アンケートの読み込みに失敗しました。入力コードが正しいかなどを確認してください。";
        errorUI.SetActive(true);
        interactCollider.enabled = false;
    }

    public override void Interact()
    {
        if (!isDownloaded) return;

        bool authorized = true;

        if (restrictToWhitelist && authorizedStaff != null && authorizedStaff.Length > 0) {
            authorized = false;
            string localName = Networking.LocalPlayer.displayName.Trim();

            foreach (string name in authorizedStaff)
            {
                if (localName.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    authorized = true;
                    break;
                }
            }
        }

        if (!authorized) {
            switchAudioManager.PlayClickSound();
            errorText.text = "アンケートを操作する権限がありません";
            errorUI.SetActive(true);
            return;
        }

        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        surveyActive = !surveyActive;
        syncedSurveyActive = surveyActive;

        if (surveyActive) {
            switchAudioManager.PlaySwitchOnSound();
            OnSurveyTurnOn();
        } else {
            switchAudioManager.PlaySwitchOffSound();
            OnSurveyTurnOff();
        }

        RequestSerialization();
    }

    private void SetIconMaterial(Material mat, int matIndex)
    {
        Material[] mats = iconRenderer.materials;
        mats[matIndex] = mat;
        iconRenderer.materials = mats;
    }

    public void OnSurveyTurnOn()
    {
        surveyActive = true;
        SetIconMaterial(onMaterial, 0);
        counterUI.SetActive(true);

        if (Networking.IsOwner(gameObject)) return;

        if (surveyInstance == null) {
            surveyInstance = Instantiate(surveyPrefab);
            surveyInstance.SetActive(true);
            surveyInstance.transform.localScale = new Vector3(1f, 1f, 1f);

            surveyManager = surveyInstance.GetComponentInChildren<SurveyManager>();
            surveyManager.Initialize(surveyConfig, downloadedJson, counter, isPermanentSurvey);
            surveyManager.ShowSurveyUI();
        } else {
            if (surveyManager != null) {
                surveyManager.ShowSurveyUI();
            }
        }
    }

    public void OnSurveyTurnOff()
    {
        surveyActive = false;
        SetIconMaterial(offMaterial, 0);
        counterUI.SetActive(false);

        if (surveyManager != null) {
            surveyManager.HideSurveyUI();
        }
    }
}
