using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [SerializeField] MainGame mainGame;
    AudioManager Audio;

    [SerializeField] private GameObject titleTutorialButton;

    [SerializeField] private Image menuToogleButton;
    [SerializeField] private Transform subMenuGroups;
    [SerializeField] private List<Transform> subMenuGroupPos;
    public bool isSubMenuOpen = false;

    public List<Image> subMenuButton;
    public List<Image> exitButton;

    [SerializeField] private GameObject infoMenuUI;
    [SerializeField] private GameObject historyMenuUI;
    [SerializeField] private List<GameObject> historyRowItem;
    [SerializeField] private GameObject soundMenuUI;

    [SerializeField] private GameObject bgmToogle;
    [SerializeField] private GameObject[] bgmButton;
    [SerializeField] private GameObject sfxToogle;
    [SerializeField] private GameObject[] sfxButton;

    [SerializeField] private Image autoPlayButton;
    [SerializeField] private GameObject autoPlayMenuUI;
    [SerializeField] private GameObject errorInfoUI;
    [SerializeField] private TextMeshProUGUI txtErrorInfo;

    [SerializeField] private Vector3 punchScaleAmount = new Vector3(0.1f, 0.1f, 0.1f);

    public TextMeshProUGUI txtDecimalOnTutorial;
    public TextMeshProUGUI txtMoneyOnTutorial;

    [SerializeField] private TutorialDisplay tutorialDisplay;

    void Start()
    {
        Audio = AudioManager.Instance;
        subMenuGroups.position = subMenuGroupPos[0].position;
        CloseAllSubMenu();
        AssignMenuButtons();
    }


    void AssignMenuButtons()
    {
        EventTrigger eventTriggerMenu = menuToogleButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryMenu = new()
        {
            eventID = EventTriggerType.PointerClick
        };
        entryMenu.callback.AddListener((data) => { ToogleSubMenu(); });
        eventTriggerMenu.triggers.Add(entryMenu);

        EventTrigger eventTriggerAutoPlay = autoPlayButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryAutoPlay = new()
        {
            eventID = EventTriggerType.PointerClick
        };
        entryAutoPlay.callback.AddListener((data) => { OpenAutoPlayMenu(); });
        eventTriggerAutoPlay.triggers.Add(entryAutoPlay);


        for (int i = 0; i < subMenuButton.Count; i++)
        {
            EventTrigger eventTrigger = subMenuButton[i].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            int index = i;
            entry.callback.AddListener((data) => { OnSubMenuButtonClick(index); });
            eventTrigger.triggers.Add(entry);
        }

        for (int i = 0; i < exitButton.Count; i++)
        {
            EventTrigger eventTrigger = exitButton[i].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => {
                Audio.PlayClickSfx();
                CloseAllSubMenu(); 
            });
            eventTrigger.triggers.Add(entry);
        }


        EventTrigger eventTriggerBgm = bgmToogle.AddComponent<EventTrigger>();
        EventTrigger.Entry entryBgm = new()
        {
            eventID = EventTriggerType.PointerClick
        };
        entryBgm.callback.AddListener((data) => { ToogleSoundSetting(bgmButton); });
        eventTriggerBgm.triggers.Add(entryBgm);


        EventTrigger eventTriggerSfx = sfxToogle.AddComponent<EventTrigger>();
        EventTrigger.Entry entrySfx = new()
        {
            eventID = EventTriggerType.PointerClick
        };
        entrySfx.callback.AddListener((data) => { ToogleSoundSetting(sfxButton); });
        eventTriggerSfx.triggers.Add(entrySfx);


        EventTrigger eventTriggerTutorial = titleTutorialButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryTutorial = new()
        {
            eventID = EventTriggerType.PointerClick
        };
        entryTutorial.callback.AddListener((data) => {
            isSubMenuOpen = true;
            OnSubMenuButtonClick(0); 
        });
        eventTriggerTutorial.triggers.Add(entryTutorial);
    }

    void PunchScaleObject(GameObject imageObject)
    {
        if (imageObject.transform.localScale == Vector3.one)
        {
            imageObject.transform.DOPunchScale(punchScaleAmount, 0.25f, 1, 1).OnComplete(() => {
                imageObject.transform.localScale = Vector3.one;
            });
        }
    }

    void ToogleSubMenu()
    {
        //if (mainGame.isFlipping || mainGame.isAutoPlay)
        //    return;

        Audio.PlayClickSfx();
        PunchScaleObject(menuToogleButton.gameObject);
        isSubMenuOpen = !isSubMenuOpen;
        ShowSubMenu(isSubMenuOpen);
    }

    public void SetSoundSetting(Sounds sound)
    {
        ChangeSoundToogle(bgmButton, sound.music);
        ChangeSoundToogle(sfxButton, sound.effect);
    }

    void ToogleSoundSetting(GameObject[] soundButton)
    {
        ChangeSoundToogle(soundButton, soundButton[0].activeInHierarchy);
        SaveSoundSetting();
    }

    void ChangeSoundToogle(GameObject[] soundButton, bool isActive)
    {
        soundButton[1].SetActive(isActive);
        soundButton[0].SetActive(!isActive);
    }

    void SaveSoundSetting()
    {
        Sounds soundSetting = new()
        {
            music = bgmButton[1].activeInHierarchy,
            effect = sfxButton[1].activeInHierarchy
        };
        // Debug.Log(soundSetting.effect);
        Audio.SaveAudioSetting(soundSetting);
        StartCoroutine(mainGame.SendSoundSettingIE(soundSetting));
    }


    void ShowSubMenu(bool isShow)
    {
        if (isShow)
        {
            subMenuGroups.DOLocalMoveY(subMenuGroupPos[1].localPosition.y, 0.25f);
        }
        else
        {
            subMenuGroups.DOLocalMoveY(subMenuGroupPos[0].localPosition.y, 0.25f);
        }
    }

    public void CloseSubMenu()
    {
        isSubMenuOpen = false;
        ShowSubMenu(isSubMenuOpen);
    }

    void OnSubMenuButtonClick(int i)
    {
        if (!isSubMenuOpen)
            return;

        Audio.PlayClickSfx();
        PunchScaleObject(subMenuButton[i].gameObject);

        switch (i) 
        {    
            case 0:
#if UNITY_WEBGL && !UNITY_EDITOR
                tutorialDisplay.OpenTutorial();
#else
                infoMenuUI.SetActive(true);
                //tutorialDisplay.OpenTutorial();
#endif
                break;

            case 1:
                StartCoroutine(ShowHistoryMenuIE());
                break;

            case 2:
                soundMenuUI.SetActive(true);
                break;

            case 3:
                CloseSubMenu();
                mainGame.GoToTitleScreen();
                break;

            default:
                break;
        }
    }

    void OpenAutoPlayMenu()
    {
        if (mainGame.isFlipping || mainGame.isAutoPlay)
            return;

        Audio.PlayClickSfx();
        CloseSubMenu();
        PunchScaleObject(autoPlayButton.gameObject);
        autoPlayMenuUI.SetActive(true);
    }

    public void CloseAllSubMenu()
    {
        infoMenuUI.SetActive(false);
        historyMenuUI.SetActive(false);
        soundMenuUI.SetActive(false);
        autoPlayMenuUI.SetActive(false);
        errorInfoUI.SetActive(false);
    }

    IEnumerator ShowHistoryMenuIE()
    {
        yield return StartCoroutine(mainGame.GetHistoryDataIE(ShowHistoryRows));
        historyMenuUI.SetActive(true);
    }

    void ShowHistoryRows(HistoryDataResponse response)
    {
        for (int i = 0; i < historyRowItem.Count; i++)
        {
            if(response.data.Count > i)
            {
                historyRowItem[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = response.data[i].created_date;
                //historyRowItem[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = FormatNumber(response.data[i].data.total_amount);
                historyRowItem[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = mainGame.MoneyFormat(decimal.Parse(response.data[i].data.total_amount));
                string type = response.data[i].data.detail_bet.type == "normal_round" ? "Normal" : "Jackpot";
                historyRowItem[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = type;
                //historyRowItem[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = FormatNumber(response.data[i].data.total_win);
                historyRowItem[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = mainGame.MoneyFormat(decimal.Parse(response.data[i].data.total_win));
                historyRowItem[i].transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = response.data[i].result.output;
            }
            historyRowItem[i].SetActive(response.data.Count > i);
        }
    }

    public void ShowErroInfo(string msg = "")
    {
        if(msg != "")
        {
            txtErrorInfo.text = msg;
        }
        else
        {
            txtErrorInfo.text = "Network Error";
        }
        errorInfoUI.SetActive(true);
    }

    public void ChangeTutorialTextFormat()
    {
        txtMoneyOnTutorial.text = mainGame.MoneyFormat(10000);
        txtDecimalOnTutorial.text = DecimalFormat(txtDecimalOnTutorial.text, mainGame.player_currency);
    }

    public string DecimalFormat(string text, string playerCurrency)
    {
        if(playerCurrency.ToLower() == "idr")
        {
            return text.Replace(".", ",");
        }
        else
        {
            return text.Replace(",", ".");
        }
    }
}
