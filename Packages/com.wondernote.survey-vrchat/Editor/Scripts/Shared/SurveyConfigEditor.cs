#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SurveyConfig))]
public class SurveyConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var behavior = (SurveyConfig)target;

        SerializedObject so = new SerializedObject(behavior);
        so.Update();

        EditorGUILayout.LabelField("◆アンケートコード", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("surveyCode"),
            new GUIContent("コードを入力")
        );

        so.ApplyModifiedProperties();

        GUILayout.Space(10f);

        var settings = behavior.GetComponent<SurveySwitchSettings>();
        if (settings != null)
        {
            settings.hideFlags |= HideFlags.HideInInspector;
            EditorUtility.SetDirty(settings);

            SerializedObject soSet = new SerializedObject(settings);
            soSet.Update();

            EditorGUILayout.LabelField("◆アンケートを実施できるユーザー", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                soSet.FindProperty("restrictToWhitelist"),
                new GUIContent("制限する")
            );

            GUILayout.Space(5f);

            if (settings.restrictToWhitelist)
            {
                EditorGUILayout.PropertyField(
                    soSet.FindProperty("authorizedStaff"), new GUIContent("許可するユーザー名 (Display Name)"), true);
                EditorGUILayout.HelpBox("VRChatのユーザー名 (Display Name) を正確に入力してください。\n入力欄が足りない場合はサイズを変更してください。", MessageType.Info);
            }

            soSet.ApplyModifiedProperties();
        }
    }
}
#endif
