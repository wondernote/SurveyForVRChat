
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ParticipantCounter : UdonSharpBehaviour
{
    private const int MaxPlayers = 100;

    [UdonSynced] private bool[] isStarted = new bool[MaxPlayers];
    [UdonSynced] private int finishedCount;

    [SerializeField] private TextMeshProUGUI startedText;
    [SerializeField] private TextMeshProUGUI finishedText;

    private VRCPlayerApi originalOwner;

    void Start()
    {
        originalOwner = Networking.GetOwner(gameObject);
    }

    private void TakeOwnership()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    private void RestoreOwnership()
    {
        if (Networking.IsOwner(gameObject) && Networking.LocalPlayer != originalOwner)
            Networking.SetOwner(originalOwner, gameObject);
    }

    public override void OnOwnershipTransferred(VRCPlayerApi newOwner)
    {
        originalOwner = newOwner;
    }

    public void NotifyStarted()
    {
        int myId = Networking.LocalPlayer.playerId;
        if (myId < MaxPlayers && !isStarted[myId])
        {
            TakeOwnership();

            isStarted[myId] = true;
            RequestSerialization();
            UpdateUI();

            RestoreOwnership();
        }
    }

    public void NotifyFinished()
    {
        int myId = Networking.LocalPlayer.playerId;
        if (myId < MaxPlayers && isStarted[myId])
        {
            TakeOwnership();

            isStarted[myId] = false;
            finishedCount++;
            RequestSerialization();
            UpdateUI();

            RestoreOwnership();
        }
    }

    public void NotifyReset()
    {
        int myId = Networking.LocalPlayer.playerId;
        if (myId < MaxPlayers && isStarted[myId])
        {
            TakeOwnership();

            isStarted[myId] = false;
            RequestSerialization();
            UpdateUI();

            RestoreOwnership();
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        int pid = player.playerId;
        if (pid < MaxPlayers && isStarted[pid])
        {
            TakeOwnership();

            isStarted[pid] = false;
            RequestSerialization();
            UpdateUI();

            RestoreOwnership();
        }
    }

    public override void OnDeserialization()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        int startedCount = 0;
        foreach (bool flag in isStarted)
            if (flag) startedCount++;

        startedText.text = $"{startedCount}人";
        finishedText.text = $"{finishedCount}人";
    }
}
