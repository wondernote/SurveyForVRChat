
using UdonSharp;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PersistenceListener : UdonSharpBehaviour
{
    private bool restoredCompleted = false;

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
            restoredCompleted = true;
    }

    public bool GetRestoredCompleted()
    {
        return restoredCompleted;
    }
}
