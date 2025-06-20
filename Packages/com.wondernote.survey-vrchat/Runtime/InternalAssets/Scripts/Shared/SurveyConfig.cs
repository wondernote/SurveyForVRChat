
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[AddComponentMenu("設定")]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SurveyConfig : UdonSharpBehaviour
{
    [SerializeField] private string surveyCode;
    [SerializeField, HideInInspector] private VRCUrl receiveUrl;
    [SerializeField, HideInInspector] private VRCUrl[] responseUrls;
    [SerializeField, HideInInspector] private VRCUrl[] freeTextPrefixUrls;

    private const int MaxQuestions = 20;
    private const int MaxValue = 10;
    private const int SlotsPerQ = MaxValue + 1;

    #if UNITY_EDITOR && !COMPILER_UDONSHARP
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(surveyCode))
        {
            receiveUrl = VRCUrl.Empty;
            responseUrls = new VRCUrl[0];
            return;
        }

        receiveUrl = new VRCUrl($"https://wondernote.net/api/surveys/{surveyCode}");

        int total = MaxQuestions * SlotsPerQ;
        if (responseUrls == null || responseUrls.Length != total) {
            responseUrls = new VRCUrl[total];
        }

        int idx = 0;
        for (int q = 1; q <= MaxQuestions; q++)
        {
            string qStr = q.ToString("D2");
            string basePath = $"{surveyCode}/r/Q{qStr}";
            responseUrls[idx++] = new VRCUrl($"https://wondernote.net/api/surveys/{basePath}");

            for (int v = 1; v <= MaxValue; v++) {
                responseUrls[idx++] = new VRCUrl($"https://wondernote.net/api/surveys/{basePath}/{v}");
            }
        }

        freeTextPrefixUrls = new VRCUrl[MaxQuestions];
        for (int q = 1; q <= MaxQuestions; q++)
        {
            string qStr = q.ToString("D2");
            string prefixStr = $"https://wondernote.net/api/surveys/{surveyCode}/r/Q{qStr}/___\n";

            freeTextPrefixUrls[q - 1] = new VRCUrl(prefixStr);
        }
    }
    #endif

    public VRCUrl GetReceiveUrl()
    {
        return receiveUrl;
    }

    public VRCUrl GetResponseUrl(int questionIndex, int value)
    {
        if (questionIndex < 0 || (questionIndex + 1) > MaxQuestions || value < 0 || value > MaxValue) {
            return null;
        }

        int idx = questionIndex * SlotsPerQ + value;
        return responseUrls[idx];
    }

    public VRCUrl GetFreeTextPrefix(int questionIndex)
    {
        if (questionIndex < 0 || (questionIndex + 1) > MaxQuestions){
            return null;
        }

        return freeTextPrefixUrls[questionIndex];
    }
}
