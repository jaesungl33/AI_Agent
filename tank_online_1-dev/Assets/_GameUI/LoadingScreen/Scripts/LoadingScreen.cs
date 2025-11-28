using Firebase.Firestore;
using Firebase.RemoteConfig;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadingScreen : UIScreenBase
{
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI loadingText, tipText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private float loadingProgress = 0f;
    private LoadingScreenParam loadingScreenParam;
    private float speed = 50;

    public override void Initialize()
    {
        EventManager.Register<LoadingScreenParam>(UpdateProgress);
        base.Initialize();
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show();
        loadingIndicator.SetActive(true);
        loadingText.text = "Loading...";
        progressBar.value = 0f;
    }

    public override void Hide()
    {
        loadingIndicator.SetActive(false);
        base.Hide();
    }

    private void FixedUpdate()
    {
        if (loadingIndicator.activeSelf && loadingScreenParam != null)
        {
            float targetProgress = loadingScreenParam.currentStep / (float)loadingScreenParam.totalSteps * 100f;
            if (loadingProgress < targetProgress)
            {
                loadingProgress += Time.deltaTime * speed;
                if (loadingProgress > targetProgress)
                {
                    loadingProgress = targetProgress;
                }
            }

            loadingText.text = $"{loadingProgress:0}%";
            progressBar.value = loadingProgress / 100f;

            if (loadingProgress >= 100f)
            {
                EventManager.TriggerEvent<UIEvent>(new UIEvent(UIIDs.Auth));
                EventManager.TriggerEvent(GamePhase.Auth);
                loadingProgress = 100f;
                Hide();
            }
        }
    }

    private void UpdateProgress(LoadingScreenParam param)
    {
        if (IsVisible == false) return;
        loadingScreenParam = param;
    }
}