using UnityEngine;
using UnityEngine.Localization;

public class LocalizeStringComponent : MonoBehaviour
{
    [SerializeField] private LocalizedString localizedText;

    void Start()
    {
        // Gán bảng và key
        localizedText.TableReference = "UI_Common";
        localizedText.TableEntryReference = "UI_Common_OK";

        // Lấy giá trị runtime
        localizedText.StringChanged += OnTextChanged;
    }

    private void OnTextChanged(string value)
    {
        Debug.Log("Localized text: " + value);
        // Ví dụ: gán vào UI Text
        GetComponent<TMPro.TextMeshProUGUI>().text = value;
    }
}
