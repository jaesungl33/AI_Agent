using TMPro;
using UnityEngine;

public class GameVersion : MonoBehaviour
{
    public TMP_Text versionText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        versionText.text = Application.version;
    }
}