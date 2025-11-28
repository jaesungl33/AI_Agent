using System.Collections.Generic;
using UnityEngine;

public class HPLineComp : MonoBehaviour
{
    [SerializeField] int hpPerLine = 50;
    [SerializeField] int minLine = 3;
    [SerializeField] int maxLine = 20;
    [SerializeField] private List<GameObject> hpLines = new List<GameObject>();

    public void SetByHP(int hp)
    {
        if (hpLines == null || hpLines.Count == 0 || hpPerLine <= 0) return;

        // Tính số vạch dựa trên tổng HP và hpPerLine
        int lineCount = Mathf.CeilToInt((float)hp / hpPerLine);
        lineCount = Mathf.Clamp(lineCount, minLine, Mathf.Min(maxLine, hpLines.Count));

        for (int i = 0; i < hpLines.Count; i++)
        {
            hpLines[i].SetActive(i < lineCount);
        }
    }
}