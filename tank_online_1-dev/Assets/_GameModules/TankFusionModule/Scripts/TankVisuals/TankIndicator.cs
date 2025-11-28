using System;
using UnityEngine;
public class TankIndicator : MonoBehaviour
{
    //menu header tank indicator
    [Header("Tank Indicator")]
    [SerializeField] private Transform indicatorPanel;
    [SerializeField] private SpriteRenderer indicatorRange;
    [SerializeField] private Transform indicatorAim;
    [SerializeField] private Transform indicatorCircle;

     [SerializeField] private Transform indicatorMyTank;

    public void SetIndicatorSkillRotate(Vector2 aimVector = default, float range = 0)
    {
        if (indicatorPanel != null)
        {
            indicatorPanel.forward = new Vector3(aimVector.x, 0, aimVector.y);
            indicatorPanel.gameObject.SetActive(range > 0);
        }
        if (indicatorRange != null)
        {
            // float width = radius;
            // transform.localScale = new Vector3(width, 1, 1);
            indicatorRange.size = new Vector2(indicatorRange.size.x, range);
        }
        // if (indicatorCircle != null)
        // {
        //     //indicatorCircle.localScale = new Vector3(range, range, range);
        //     indicatorCircle.localScale = new Vector3(0, 0, 0);
        // }
    }
    public void SetIndicatorAim(float rangeRatio, float radius, float rangeMax = 0)
    {
        if (indicatorAim != null)
        {
            float distance = rangeMax * rangeRatio;
            indicatorAim.localPosition = new Vector3(0, distance, 0);
            indicatorAim.localScale = new Vector3(radius, radius, radius);
            indicatorAim.gameObject.SetActive(rangeRatio > 0 && rangeMax > 0);
        }
    }

    public void PresetIndicator(Vector2 aimVector = default, float range = 0)
    {
        if (indicatorPanel.forward != new Vector3(aimVector.x, 0, aimVector.y))
            indicatorPanel.forward = new Vector3(aimVector.x, 0, aimVector.y);
        if (indicatorRange != null)
            indicatorRange.size = new Vector2(indicatorRange.size.x, range);
        indicatorPanel.gameObject.SetActive(range > 0);
    }
    public void ActiveIndicator(bool active)
    {
        if (indicatorPanel?.gameObject.activeSelf != active) indicatorPanel?.gameObject.SetActive(active);
        if (indicatorRange?.gameObject.activeSelf != active) indicatorRange?.gameObject.SetActive(active);
        //if (indicatorCircle?.gameObject.activeSelf != active) indicatorCircle?.gameObject.SetActive(active);
    }
    public void ShowMyTankIndicator()
    {
        //if (indicatorMyTank?.gameObject?.activeSelf == false) indicatorMyTank?.gameObject?.SetActive(true);
    }
}
