
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class FadePanel : UdonSharpBehaviour
{
    public virtual void FadeOutEnded()
    {
        gameObject.SetActive(false);
    }

    public virtual void FadeInEnded()
    {
    }

    public virtual void FadeOutStarted()
    {
    }

    public virtual void FadeInStarted()
    {
    }
}
