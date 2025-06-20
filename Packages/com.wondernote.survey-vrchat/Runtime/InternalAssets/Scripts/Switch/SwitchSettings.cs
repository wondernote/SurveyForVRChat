
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SwitchSettings : UdonSharpBehaviour
{
    // [SerializeField] private GameObject settingsPanel;
    // [SerializeField] private Renderer iconRenderer;
    // [SerializeField] private Material onMaterial;
    // [SerializeField] private Material offMaterial;
    // [SerializeField] private Material disabledMaterial;

    // private bool panelIsOpen = false;

    // public override void Interact()
    // {
    //     TogglePanel();
    // }

    // public void TogglePanel()
    // {
    //     panelIsOpen = !panelIsOpen;
    //     settingsPanel.SetActive(panelIsOpen);

    //     Material[] mats = iconRenderer.materials;

    //     if (panelIsOpen) {
    //         SetIconMaterial(onMaterial, 2);
    //     } else {
    //         SetIconMaterial(offMaterial, 2);
    //     }
    // }

    // private void SetIconMaterial(Material mat, int matIndex)
    // {
    //     Material[] mats = iconRenderer.materials;
    //     mats[matIndex] = mat;
    //     iconRenderer.materials = mats;
    // }
}
