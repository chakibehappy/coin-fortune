using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NumberFormatter : MonoBehaviour
{
    public TMP_InputField inputField;
    public MainGame game;

    private void Start()
    {
        inputField.onValueChanged.AddListener(FormatNumber);
    }

    private void FormatNumber(string input)
    {
        if (int.TryParse(input.Replace(".", ""), out int number))
        {
            inputField.text = number.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
            game.SetBetFromKeyboard(number);
        }
    }

}
