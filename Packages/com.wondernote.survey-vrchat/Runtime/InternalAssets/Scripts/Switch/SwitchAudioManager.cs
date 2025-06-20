
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SwitchAudioManager : UdonSharpBehaviour
{
    [SerializeField] private AudioSource switchOnAudioSource;
    [SerializeField] private AudioSource switchOffAudioSource;
    [SerializeField] private AudioSource clickAudioSource;

    private VRCPlayerApi localPlayer;
    private bool isUserInVR = false;

    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
        isUserInVR = localPlayer != null && localPlayer.IsUserInVR();

        if (isUserInVR) {
            switchOnAudioSource.volume = 0.1f;
            switchOffAudioSource.volume = 0.1f;
            clickAudioSource.volume = 0.6f;
        }
    }

    public void PlaySwitchOnSound()
    {
        switchOnAudioSource.Play();
    }

    public void PlaySwitchOffSound()
    {
        switchOffAudioSource.Play();
    }

    public void PlayClickSound()
    {
        clickAudioSource.Play();
    }
}
