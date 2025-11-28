using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DamageVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro damageText;
    [SerializeField] private List<DamageText> pools;
    [SerializeField] private float scaleDuration = 0.2f, moveDuration = 1f;
    [SerializeField] private float minY = 5f, maxY=10f;
    [SerializeField] private float mixX = -5f, maxX = 5f;
    [SerializeField] private float minScale = 1.2f, maxScale = 1.8f;

    private void Awake()
    {
        pools = new List<DamageText>();
        damageText.gameObject.SetActive(false);
    }

    public void ShowText(string text, Color color)
    {
        if (damageText == null)
        {
            Debug.LogError("Damage text is not assigned.");
            return;
        }
        DamageText dt = null;
        dt = pools.Find(dt => !dt.isActive);
        if (dt == null)
        {
            dt = new DamageText();
            dt.TextMeshPro = Instantiate(damageText, transform);
            pools.Add(dt);
        }

        dt.Active(text, color);

        // Apply movement and scale effects using Sequence
        dt.TextMeshPro.transform.DOKill();

        float randomScale = Random.Range(minScale, maxScale);

        Sequence sequence = DOTween.Sequence();
        float randomX = Random.Range(mixX, maxX);
        sequence.Append(dt.TextMeshPro.transform.DOScale(Vector3.one * randomScale, scaleDuration).SetEase(Ease.OutQuad));
        sequence.Join(dt.TextMeshPro.transform.DOLocalMove(new Vector3(transform.localPosition.x + randomX, transform.localPosition.y + Random.Range(minY, maxY), 0), moveDuration).SetEase(Ease.OutQuad));
        sequence.Append(dt.TextMeshPro.transform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.InQuad));
        sequence.OnComplete(() =>
        {
            dt.Hide();
        });
    }

    [System.Serializable]
    private class DamageText
    {
        public bool isActive;
        public TextMeshPro TextMeshPro;

        public void Hide()
        {
            isActive = false;
            TextMeshPro.gameObject.SetActive(false);
        }

        public void Active(string text, Color color)
        {
            isActive = true;
            TextMeshPro.gameObject.SetActive(true);
            TextMeshPro.text = text;
            TextMeshPro.color = color;// Reset position
            TextMeshPro.transform.localPosition = Vector3.zero;
            TextMeshPro.transform.localScale = Vector3.one;
        }
    }
}
