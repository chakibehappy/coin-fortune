using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    public string labelKey;

    private TMP_Text textComponent;
    private object[] formatArgs;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {

            if (LanguageManager.Instance.IsLanguageReady())
            {
                UpdateText();
            }
            else
            {
                LanguageManager.Instance.OnLanguageReady += UpdateText;
            }

            LanguageManager.Instance.OnLanguageChanged += UpdateText;
        }
    }

    public void SetFormatArgs(params object[] args)
    {
        formatArgs = args;
        UpdateText();
    }


    void UpdateText()
    {
        if (textComponent == null) return;

        string raw = LanguageManager.Instance.GetLabel(labelKey);
        if (!string.IsNullOrEmpty(raw) && formatArgs != null && formatArgs.Length > 0)
        {
            textComponent.text = string.Format(raw, formatArgs);
        }
        else
        {
            textComponent.text = raw;
        }
    }

    void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }
}
