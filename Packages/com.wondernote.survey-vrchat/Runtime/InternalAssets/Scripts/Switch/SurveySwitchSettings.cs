
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SurveySwitchSettings : UdonSharpBehaviour
{
    [SerializeField] public bool restrictToWhitelist = false;
    [SerializeField] public string[] authorizedStaff = new string[3];
}
