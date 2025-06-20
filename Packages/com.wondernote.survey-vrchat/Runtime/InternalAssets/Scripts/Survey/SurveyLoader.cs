
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SurveyLoader : UdonSharpBehaviour
{
    [UdonSynced] private string syncedSurveyJson;
    [UdonSynced] private bool syncedLoadError;
    [SerializeField] private SurveyConfig surveyConfig;

    private VRCUrl receiveUrl;
    private string downloadedJson;

    [SerializeField] private SurveyManager surveyManager;

    [SerializeField] private GameObject startingContainer;
    [SerializeField] private GameObject loadingContainer;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject errorContainer;
    [SerializeField] private TextMeshProUGUI error;

    [SerializeField] private GameObject previewQuad;
    [SerializeField] private GameObject depthMaskQuad;

    public void Start()
    {
        previewQuad.SetActive(false);
        depthMaskQuad.transform.localRotation = Quaternion.identity;
        loadingContainer.SetActive(true);

        receiveUrl = surveyConfig.GetReceiveUrl();
        string url = (receiveUrl != null) ? receiveUrl.Get() : "";

        if (!string.IsNullOrEmpty(url)) {
            if (Networking.IsOwner(gameObject)) {
                VRCStringDownloader.LoadUrl(receiveUrl, this.GetComponent<UdonBehaviour>());
            }
        } else {
            startingContainer.SetActive(false);
            loadingText.SetActive(false);
            errorContainer.SetActive(true);
            error.text = "\"アンケートコード\" が設定されていません。\n設定を確認してください。";
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload download)
    {
        if (download.Url == receiveUrl && Networking.IsOwner(gameObject))
        {
            syncedSurveyJson = download.Result;
            RequestSerialization();

            downloadedJson = download.Result;

            surveyManager.Initialize(surveyConfig, downloadedJson);
        }
    }

    public override void OnDeserialization()
    {
        if (syncedLoadError) {
            ShowLoadErrorUI();
            return;
        }

        if (!string.IsNullOrEmpty(syncedSurveyJson)) {
            downloadedJson = syncedSurveyJson;

            if (!Networking.IsOwner(gameObject)) {
                surveyManager.Initialize(surveyConfig, downloadedJson);
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
        startingContainer.SetActive(false);
        loadingText.SetActive(false);
        errorContainer.SetActive(true);
        error.text = "アンケートの読み込みに失敗しました。\n入力コードが正しいかなどを確認してください。";
    }
}
