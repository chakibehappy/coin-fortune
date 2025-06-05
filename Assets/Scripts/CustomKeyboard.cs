using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Reflection;

public class CustomKeyboard : MonoBehaviour
{
    [SerializeField] private List<GameObject> numberButtonObj;
    [SerializeField] private GameObject cancelButton;
    [SerializeField] private GameObject okButton;

    [SerializeField] private TextMeshProUGUI txtResult;

    [SerializeField] private Color32 inactiveColor;
    [SerializeField] private Color32 activeColor;

    string resultText;
    float maxNumber;

    [SerializeField] private MainGame mainGame;

    void Start()
    {
        AssignKeyboardButton();
    }

    public void SetResultText(float number, float max)
    {
        txtResult.text = MoneyFormat(number);
        maxNumber = max;
        resultText = number.ToString();
    }

    void SendResultText()
    {
        mainGame.SetBetFromKeyboard(int.Parse(resultText));
    }

    void AssignKeyboardButton()
    {
        for (int i = 0; i < numberButtonObj.Count; i++)
        {
            numberButtonObj[i].GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            EventTrigger eventTrigger = numberButtonObj[i].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            EventTrigger.Entry exit = new()
            {
                eventID = EventTriggerType.PointerUp
            };
            int index = i;
            entry.callback.AddListener((data) => { OnNumberKeyButtonDown(index); });
            exit.callback.AddListener((data) => { OnNumberKeyButtonUp(index); });
            eventTrigger.triggers.Add(entry);
            eventTrigger.triggers.Add(exit);
        }

        EventTrigger eventTriggerCancel = cancelButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryCancelDown = new EventTrigger.Entry();
        entryCancelDown.eventID = EventTriggerType.PointerDown;
        entryCancelDown.callback.AddListener((data) => { OnCancelKeyButtonDown(); });
        EventTrigger.Entry entryCancelUp = new EventTrigger.Entry();
        entryCancelUp.eventID = EventTriggerType.PointerUp;
        entryCancelUp.callback.AddListener((data) => { OnCancelKeyButtonUp(); });
        eventTriggerCancel.triggers.Add(entryCancelDown);
        eventTriggerCancel.triggers.Add(entryCancelUp);

        EventTrigger eventTriggerOK = okButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryOK = new EventTrigger.Entry();
        entryOK.eventID = EventTriggerType.PointerDown;
        entryOK.callback.AddListener((data) => { SendResultText(); });
        eventTriggerOK.triggers.Add(entryOK);
    }

    void OnNumberKeyButtonDown(int index)
    {
        SetActiveColor(numberButtonObj[index]);
        resultText += numberButtonObj[index].GetComponentInChildren<TextMeshProUGUI>().text;
        if(int.Parse(resultText) > maxNumber)
        {
            resultText = maxNumber.ToString();
        }
        txtResult.text = MoneyFormat(int.Parse(resultText));
    }
    void OnNumberKeyButtonUp(int index)
    {
        SetActiveColor(numberButtonObj[index], false);
    }


    void SetActiveColor(GameObject obj, bool isActive = true)
    {
        obj.GetComponent<Image>().color = isActive? activeColor : inactiveColor;
    }

    void OnCancelKeyButtonDown()
    {
        SetActiveColor(cancelButton);
        if (resultText.Length > 0)
        {
            string newText = resultText.Substring(0, resultText.Length - 1);
            resultText = newText;
        }
        if(resultText.Length == 0)
        {
            resultText = "0";
        }
        txtResult.text = MoneyFormat(float.Parse(resultText));
    }

    void OnCancelKeyButtonUp()
    {
        SetActiveColor(cancelButton, false);
    }

    string MoneyFormat(float number)
    {
        return number.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
    }
}
