using TMPro;
using UnityEngine;

public class GamePlayTurretIcon : MonoBehaviour
{
    [SerializeField] private GameObject _active;
    [SerializeField] private GameObject _inactive;
    [SerializeField] private TMP_Text _txtName;
    
    public string TurretId { get; private set; }

    public void Init(string turretId, string nameTxt)
    {
        TurretId = turretId;
        _txtName.text = nameTxt;
        UpdateActive(true);
    }

    public void UpdateActive(bool active)
    {
        _active.SetActive(active);
        _inactive.SetActive(!active);
    }
}