#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;

[CustomEditor(typeof(SurveySwitchSettings))]
public class SurveySwitchSettingsHideEditor : Editor
{
    public override void OnInspectorGUI()
    {
    }
}
#endif
