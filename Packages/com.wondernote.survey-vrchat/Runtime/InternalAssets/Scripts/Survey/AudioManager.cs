
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace WonderNote.Survey
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AudioManager : UdonSharpBehaviour
    {
        [SerializeField] private AudioSource hoverAudioSource;
        [SerializeField] private AudioSource clickAudioSource;
        private VRCPlayerApi localPlayer;
        private bool isUserInVR = false;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            isUserInVR = localPlayer != null && localPlayer.IsUserInVR();

            if (isUserInVR) {
                hoverAudioSource.volume = 0.1f;
                clickAudioSource.volume = 0.6f;
            }
        }

        public void PlayHoverSound()
        {
            hoverAudioSource.Play();

            if (isUserInVR) {
                localPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.01f, 0.2f, 0.01f);
            }
        }

        public void PlayClickSound()
        {
            clickAudioSource.Play();
        }
    }
}
