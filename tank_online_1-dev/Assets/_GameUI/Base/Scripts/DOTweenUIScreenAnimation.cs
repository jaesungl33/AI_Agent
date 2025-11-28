using UnityEngine;
using DG.Tweening;

public class DOTweenUIScreenAnimation : UIScreenAnimation
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 0.3f;

    public override void PlayShowAnimation(System.Action onComplete = null)
    {
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, duration).OnComplete(() => onComplete?.Invoke());
    }

    public override void PlayHideAnimation(System.Action onComplete = null)
    {
        canvasGroup.DOFade(0, duration).OnComplete(() => onComplete?.Invoke());
    }
}
