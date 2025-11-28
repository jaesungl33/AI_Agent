using UnityEngine;

public abstract class UIScreenAnimation : MonoBehaviour
{
    public abstract void PlayShowAnimation(System.Action onComplete = null);
    public abstract void PlayHideAnimation(System.Action onComplete = null);
}
