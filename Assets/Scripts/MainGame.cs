using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Networking;
using Spine.Unity;
using System;
using System.Linq;

public class MainGame : MonoBehaviour
{
    public enum BuildType
    {
        Development,
        Production
    }

    public BuildType buildType = BuildType.Development;


    [Header("Other Classes")]
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private APIManager API;
    [SerializeField] private ScreenTransitionHelper transitionHelper;
    [SerializeField] private MegaBonusRound megaBonus;
    AudioManager Audio;

    [Header("Starting Screen")]
    [SerializeField] private SkeletonAnimation titleCharSpine;
    [SerializeField] private string[] titleCharAnimation;
    [SerializeField] private GameObject loginButton;
    [SerializeField] private GameObject btnStart;

    [Header("Main Screen")]
    [SerializeField] private GameObject mainGameScreenUI;
    [SerializeField] private SkeletonAnimation mainBackgroundSpine;

    [SerializeField] private GameObject mainGameObjects;
    [SerializeField] private SkeletonAnimation mainCharSpine;
    [SerializeField] private string[] mainCharAnimation;

    [SerializeField] private List<Transform> historyCoins;
    private List<string> resultHistory = new();

    [SerializeField] private GameObject btnHead;
    [SerializeField] private GameObject coinSelectionHead;
    [SerializeField] private GameObject btnTail;
    [SerializeField] private GameObject coinSelectionTail;
    [SerializeField] private GameObject spinButton;
    [SerializeField] private SkeletonGraphic spinButtonSpine;
    [SerializeField] private string[] spinButtonSpineAnimation;
    [SerializeField] private SkeletonGraphic autoPlayButtonSpine;
    [SerializeField] private string[] autoPlayButtonSpineAnimation;

    [SerializeField] private List<SkeletonAnimation> coinSpine;
    [SerializeField] private List<Transform> coinPos;
    [SerializeField] private string[] coinAnimation;
    [SerializeField] private TextMeshPro[] txtCoinMultiplier;

    [SerializeField] private Vector3 punchScaleAmount = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Player Ballance")]
    bool successLogin = false;
    [SerializeField] private decimal totalPlayerBalance = 100000;
    [SerializeField] private TextMeshProUGUI txtBalance;


    [Header("Bet Section")]
    [SerializeField] private List<int> chipBase = new List<int>();
    private int chipBaseIndex = 0;
    [SerializeField] private int confirmedBet;

    private string selectedChoice = "Head";

    [SerializeField]
    private TMP_InputField txtBet;
    [SerializeField]
    private GameObject btnIncreaseBet;
    [SerializeField]
    private GameObject btnDecreaseBet;
    [SerializeField] private Sprite[] betPanelButtonSprite;


    [SerializeField] private SkeletonAnimation winningSpine;
    [SerializeField] private string[] winningAnimationName;
    private IEnumerator flashingTextCoroutine;
    [SerializeField] private TextMeshPro txtWinningNumber;
    [SerializeField] private TextMeshPro txtWinningNumberShadow;
    [SerializeField] private SkeletonAnimation losingUISpine;

    public bool isFlipping = false;

    [Header("Auto Play")]
    [SerializeField] private GameObject btnRandomAutoPlay;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private GameObject btnHeadAutoPlay;
    [SerializeField] private GameObject btnTailAutoPlay;
    [SerializeField] private GameObject btnHeadAutoPlayObj;
    [SerializeField] private GameObject btnTailAutoPlayObj;
    [SerializeField] private List<GameObject> autoBetNumberButtons;
    [SerializeField] private List<Sprite> numberButtonSprite;
    [SerializeField] private GameObject btnConfirmAutoPlay;
    [SerializeField] private int selectedAutoPlayCount = 10;
    [SerializeField] private int currentAutoPlayCount = 10;

    [SerializeField] private TextMeshProUGUI txtCurrentAutoPlayCount;
    [SerializeField] private GameObject btnStopAutoPlay;

    public bool isAutoPlay = false;
    bool isRandomChoice = false;
    IEnumerator autoPlayCoroutine;

    public string roundType = "normal_round";

    [Header("Bonus Round Section")]
    bool isBonusRoundMode;
    bool isMegaBonusRoundMode;
    [SerializeField] private SkeletonAnimation orbSpine;
    [SerializeField] private string[] orbAnimationName;
    [SerializeField] private GameObject indikatorUI;
    [SerializeField] private Image indikatorFill;
    [SerializeField] private Sprite[] indikatorSprites;
    [SerializeField] private TextMeshProUGUI txtWinningStreak;
    [SerializeField] private SkeletonAnimation lightningSpine;
    [SerializeField] private SkeletonAnimation textBonusSpine;
    [SerializeField] private List<SkeletonAnimation> bonusGameInfoText;
    [SerializeField] private GameObject megaBonusHint;
    [SerializeField] private float delayOnBonusHintAutoPlay = 1f;

    [Header("Purchase Mode")]
    [SerializeField] private GameObject purchaseMenuUI;
    [SerializeField] private GameObject purchaseWindow;
    [SerializeField] private GameObject[] purchaseBonusButton;
    [SerializeField] private GameObject purchaseWindowButton;
    [SerializeField] private GameObject purchaseCloseButton;
    [SerializeField] private SkeletonAnimation ticketAnim;
    [SerializeField] private string[] ticketAnimName = new string[] { "ticket transisi start", "ticket transisi idle"};
    [SerializeField] private TextMeshProUGUI txtBonusPrice, txtMegaBonusPrice;
    [SerializeField] private TextMeshProUGUI txtBonusInfo, txtMegaBonusInfo;
    bool canPurchase = true;
    int mega_try_count;
    float bonus_price, mega_bonus_price;
    int total_energy_orb;

    string encryptedJson;

    [SerializeField] private SkeletonGraphic purchaseButtonSpine;
    [SerializeField] private Color32 pressedButtonColor = new Color32(200, 200, 200, 255);

    int minimalBet, maximalBet;

    bool isPurchasing = false;
    public string player_currency;

    public float GetBonusPrice()
    {
        return bonus_price * confirmedBet; 
    }

    public float GetMegaBonusPrice()
    {
        return mega_bonus_price * confirmedBet;
    }

    void Start()
    {
        Audio = AudioManager.Instance;

        megaBonusHint.SetActive(false);
        ResetBonusGameState();
        lightningSpine.gameObject.SetActive(false);
        purchaseMenuUI.SetActive(false);
        purchaseWindow.SetActive(false);

        mainGameObjects.SetActive(false);
        StartCoroutine(PlayTitleCharacterAnimationIE());
        //keyboard.SetActive(false);
        ShowMainScreen(false);
        AsignButtonClicks();
        txtBet.enabled = false;

        EnableBetPanelButton(true);

#if UNITY_EDITOR
        StartCoroutine(GetInitialDataAPIIE());
#else
        StartCoroutine(API.GetAPIFromConfig(GetUserData));
#endif
    }


    void GetUserData()
    {
        StartCoroutine(GetInitialDataAPIIE());
    }

    void ResetBonusGameState()
    {
        coinSpine[0].gameObject.SetActive(false);
        coinSpine[2].gameObject.SetActive(false);
        coinSpine[0].transform.position = coinPos[1].position;
        coinSpine[2].transform.position = coinPos[1].position;
        
        if(SpineHelper.GetCurrentAnimationName(mainBackgroundSpine) != "all no meteor")
            SpineHelper.PlayAnimation(mainBackgroundSpine, "all no meteor", true);
        
        textBonusSpine.gameObject.SetActive(false);
        bonusGameInfoText.ForEach(obj => { obj.gameObject.SetActive(false); });
    }

    IEnumerator PlayTitleCharacterAnimationIE()
    {
        float resetDelay = 0.1f;
        titleCharSpine.skeleton.A = 0;
        SpineHelper.PlayAnimation(titleCharSpine, titleCharAnimation[0], false);
        yield return new WaitForSeconds(resetDelay);
        titleCharSpine.skeleton.A = 1;
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(titleCharSpine, titleCharAnimation[0]) - resetDelay);
        SpineHelper.PlayAnimation(titleCharSpine, titleCharAnimation[1], true);
    }

    IEnumerator TriggerLoginIE()
    {
        UnityWebRequest request = UnityWebRequest.Get(API.TriggerLoginAPI());
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + request.error);
        }
        else
        {
            UnityWebRequest request2 = UnityWebRequest.Get(API.GetDataUserAPI());
            yield return request2.SendWebRequest();
            string responseJson = request2.downloadHandler.text;
            Log(responseJson);
            UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
            totalPlayerBalance = decimal.Parse(response.data.player.player_balance);
            txtBalance.text = MoneyFormat(totalPlayerBalance);
            successLogin = true;
            menuManager.SetSoundSetting(response.data.game.sounds);
            Audio.SaveAudioSetting(response.data.game.sounds);
            megaBonus.SetPrizeList(response);
            chipBase = response.data.game.chip_base.ToList();
            minimalBet = response.data.game.limit_bet.minimal;
            maximalBet = response.data.game.limit_bet.maximal;
            
            confirmedBet = chipBase[chipBaseIndex];
            txtBet.text = MoneyFormat(confirmedBet);
        }
    }

    public IEnumerator SendSoundSettingIE(Sounds soundSetting)
    {
        string jsonBody = "{";
        jsonBody += $"\"effect\":{soundSetting.effect.ToString().ToLower()},";
        jsonBody += $"\"music\":{soundSetting.music.ToString().ToLower()},";
        jsonBody += "\"language\":\"ID\"}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest request = new UnityWebRequest(API.GetSettingPropertiesAPI(), "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Log(request.error);
        }
        else
        {
            Log(request.downloadHandler.text);
        }
    }

    public IEnumerator GetHistoryDataIE(Action<HistoryDataResponse> nextAction = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(API.GetHistoryTransactionAPI());
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + request.error);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            Log(responseJson);
            HistoryDataResponse response = JsonUtility.FromJson<HistoryDataResponse>(responseJson);
            if (resultHistory == null || resultHistory.Count == 0)
            {
                if (response.data.Count > 0)
                {
                    for (int i = 0; i < response.data.Count; i++)
                    {
                        if (i < 3)
                            resultHistory.Add(response.data[i].result.output);
                        else
                            break;
                    }
                }
                ShowHistoryCoinResult();
            }
            nextAction?.Invoke(response);
        }
    }

    void AddNewResultHistory(string result)
    {
        resultHistory.Insert(0, result);
        if (resultHistory.Count > historyCoins.Count)
        {
            resultHistory.RemoveAt(resultHistory.Count - 1);
        }
    }

    void ShowHistoryCoinResult()
    {
        if (resultHistory.Count <= 0)
            return;

        for (int i = 0; i < historyCoins.Count; i++)
        {
            if (i < resultHistory.Count)
            {
                string[] coinResults = resultHistory[i].Split(",");
                for (int j = 0; j < coinResults.Length; j++)
                {
                    bool isHead = coinResults[j] == "Head";
                    historyCoins[i].GetChild(j).GetChild(0).gameObject.SetActive(isHead);
                    historyCoins[i].GetChild(j).GetChild(1).gameObject.SetActive(!isHead);
                }

                historyCoins[i].GetChild(0).gameObject.SetActive(true);
                historyCoins[i].GetChild(1).gameObject.SetActive(coinResults.Length > 1);
                historyCoins[i].GetChild(2).gameObject.SetActive(coinResults.Length > 1);
            }
        }
    }

    IEnumerator GetUserDataIE()
    {
        UnityWebRequest request = UnityWebRequest.Get(API.GetDataUserAPI());
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + request.error);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            Log(responseJson);
            UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
            
            bonus_price = response.data.game.prize_detail.purchase_bonus;
            mega_bonus_price = response.data.game.prize_detail.purchase_mega;

            chipBase = response.data.game.chip_base.ToList();
            minimalBet = response.data.game.limit_bet.minimal;
            maximalBet = response.data.game.limit_bet.maximal;
            player_currency = response.data.player.player_currency;

            confirmedBet = chipBase[chipBaseIndex];
            txtBet.text = MoneyFormat(confirmedBet);

            totalPlayerBalance = decimal.Parse(response.data.player.player_balance);
            txtBalance.text = MoneyFormat(totalPlayerBalance);
            txtBonusPrice.text = MoneyFormat(GetBonusPrice());
            txtMegaBonusPrice.text = MoneyFormat(GetMegaBonusPrice());
            
            string playerLanguage = response.data.player.player_language.ToLower();
            LanguageManager.Instance.SetLanguage(playerLanguage);
            ChangeBonusPriceText();

            StartCoroutine(PlayOrbAnimationIE(response.data.game.energy_bar, response.data.game.total_energy_orb));
            total_energy_orb = response.data.game.total_energy_orb;
            //roundType = response.data.game.current_round;
            roundType = "normal_round";
            successLogin = true;
            menuManager.SetSoundSetting(response.data.game.sounds);
            Audio.SaveAudioSetting(response.data.game.sounds);
            megaBonus.SetPrizeList(response);
            menuManager.ChangeTutorialTextFormat();
        }
    }

    void ChangeBonusPriceText()
    {
        txtBonusInfo.GetComponent<LocalizedText>().SetFormatArgs(bonus_price);
        txtMegaBonusInfo.GetComponent<LocalizedText>().SetFormatArgs(mega_bonus_price);
    }

    IEnumerator PlayOrbAnimationIE(int energyBar, int totalEnergyBar, bool playLightning = false)
    {
        indikatorUI.SetActive(true);
        txtWinningStreak.text = energyBar.ToString();
        float multiplier = (float)energyBar / (float)totalEnergyBar;
        int orbAnimIndex = Mathf.RoundToInt(multiplier * orbAnimationName.Length);
        int activeAnimIndex = Mathf.Min(orbAnimIndex, orbAnimationName.Length - 1);

        if (playLightning)
            yield return StartCoroutine(LightningToIndikatorIE());

        SpineHelper.PlayAnimation(orbSpine, orbAnimationName[activeAnimIndex], true);
        indikatorFill.DOFillAmount(multiplier, 0.5f).SetEase(Ease.Linear);
        if (playLightning)
        {
            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(lightningSpine, "Explosion") - 0.5f);
            lightningSpine.gameObject.SetActive(false);
        }
    }

    IEnumerator LightningToIndikatorIE()
    {
        Audio.PlaySfx(1);
        lightningSpine.gameObject.SetActive(true);
        SpineHelper.PlayAnimation(lightningSpine, "Explosion", false);
        yield return new WaitForSeconds(0.5f);
    }

    public string MoneyFormat(int number)
    {
        if (player_currency.ToLower() == "idr")
            return player_currency + " " + number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        else
            return player_currency + " " + number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);
    }
    public string MoneyFormat(float number)
    {
        if (player_currency.ToLower() == "idr")
            return player_currency + " " + number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        else
            return player_currency + " " + number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);
    }

    public string MoneyFormat(decimal number)
    {
        if (player_currency.ToLower() == "idr")
            return player_currency + " " + number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        else
            return player_currency + " " + number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);
    }

    void AsignButtonClicks()
    {   
        EventTrigger eventTriggerStart = btnStart.AddComponent<EventTrigger>();
        EventTrigger.Entry entryStart = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryStart.callback.AddListener((data) => { GoToGameScreen(); });
        eventTriggerStart.triggers.Add(entryStart);

        // Assign increase and decrease button
        EventTrigger eventTriggerPlusBet = btnIncreaseBet.AddComponent<EventTrigger>();
        EventTrigger.Entry entryPlusBet = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryPlusBet.callback.AddListener((data) => { ChangeBet(btnIncreaseBet, true); });
        eventTriggerPlusBet.triggers.Add(entryPlusBet);
        
        EventTrigger eventTriggerMinusBet = btnDecreaseBet.AddComponent<EventTrigger>();
        EventTrigger.Entry entryMinusBet = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryMinusBet.callback.AddListener((data) => { ChangeBet(btnDecreaseBet, false); });
        eventTriggerMinusBet.triggers.Add(entryMinusBet);

        /*
        EventTrigger eventTriggerKeyboard = txtBet.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryKeyboard = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryKeyboard.callback.AddListener((data) => { ShowInputKeyboard(); });
        eventTriggerKeyboard.triggers.Add(entryKeyboard);
        */

        // on random choie auto play
        EventTrigger.Entry entryRandom = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryRandom.callback.AddListener((data) => {
            SelectRandomChoice();
        });
        EventTrigger eventTriggerRandomAutoPlay = btnRandomAutoPlay.AddComponent<EventTrigger>();
        eventTriggerRandomAutoPlay.triggers.Add(entryRandom);

        // Assign Head, Tails, and Bet Button
        EventTrigger.Entry entryHead = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryHead.callback.AddListener((data) => { SelectCoinSide(btnHead, "Head"); });
        EventTrigger eventTriggerHead = btnHead.AddComponent<EventTrigger>();
        eventTriggerHead.triggers.Add(entryHead);
        // on auto head auto play
        EventTrigger eventTriggerHeadAutoPlay = btnHeadAutoPlay.AddComponent<EventTrigger>();
        eventTriggerHeadAutoPlay.triggers.Add(entryHead);

        EventTrigger.Entry entryTails = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryTails.callback.AddListener((data) => { SelectCoinSide(btnTail, "Tail"); });
        EventTrigger eventTriggerTails = btnTail.AddComponent<EventTrigger>();
        eventTriggerTails.triggers.Add(entryTails);
        // on auto tail auto play
        EventTrigger eventTriggerTailsAutoPlay = btnTailAutoPlay.AddComponent<EventTrigger>();
        eventTriggerTailsAutoPlay.triggers.Add(entryTails);

        EventTrigger eventTriggerBet = spinButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryBet = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryBet.callback.AddListener((data) => { OnFlipCoinDown(); });
        eventTriggerBet.triggers.Add(entryBet);

        AssignAutoPlayButtons();
        AssignPurchaseMenuButtons();
    }

    void SelectRandomChoice()
    {
        isRandomChoice = true;
        btnRandomAutoPlay.GetComponent<Image>().color = Color.white;
        btnHeadAutoPlayObj.SetActive(false);
        btnTailAutoPlayObj.SetActive(false);
    }

    void AssignPurchaseMenuButtons()
    {
        for (int i = 0; i < purchaseBonusButton.Length; i++)
        {
            EventTrigger eventTrigger = purchaseBonusButton[i].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            int index = i;
            entry.callback.AddListener((data) => { OnPurchaseMenuButtonDown(index); });
            eventTrigger.triggers.Add(entry);
        }

        EventTrigger.Entry entryPurchase = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryPurchase.callback.AddListener((data) => { TooglePurchaseWindow(); });
        EventTrigger eventTriggerAutoPlay = purchaseWindowButton.AddComponent<EventTrigger>();
        eventTriggerAutoPlay.triggers.Add(entryPurchase);


        EventTrigger.Entry exitPurchase = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        exitPurchase.callback.AddListener((data) => { TooglePurchaseWindow(); });
        EventTrigger eventTriggerExit = purchaseCloseButton.AddComponent<EventTrigger>();
        eventTriggerExit.triggers.Add(exitPurchase);
    }

    void OnPurchaseMenuButtonDown(int index)
    {
        if(!canPurchase)
            return;

        isPurchasing = true;
        menuManager.CloseAllSubMenu();
        StartCoroutine(BlinkSpineObjectIE(purchaseBonusButton[index].GetComponent<SkeletonGraphic>()));
        canPurchase = false;
        Audio.PlayClickSfx();
        EnableBetPanelButton(false);
        StartCoroutine(SendPurchaseBonusRequestIE(index));
    }

    public void ReceiveEncryptedData(string encryptedData)
    {
        encryptedJson = $"{{\"data\":\"{encryptedData}\"}}";
    }

    IEnumerator SendPurchaseBonusRequestIE(int index)
    {
        bool isSuccess = false;
        string msg = "";
        string button = index == 0 ? "Buy Bonus" : "Buy Mega";
        string type = index == 0 ? "purchase_bonus" : "purchase_mega";
        
        DataToSend data = new()
        {
            data = JsonUtility.ToJson(new BetDataToSend
            {
                total_amount = confirmedBet,
                button_bet = new ButtonBet
                {
                    button = button,
                    amount = confirmedBet,
                    type = type
                }
            })
        };
        string jsonData = JsonUtility.ToJson(data);

#if UNITY_WEBGL && !UNITY_EDITOR
        jsonData = JsonUtility.ToJson(new BetDataToSend
        {
            total_amount = confirmedBet,
            button_bet = new ButtonBet
            {
                button = button,
                amount = confirmedBet,
                type = type
            }
        });
        string jsCommand = $"encryptDataAndSendBack('{jsonData}', '{API.encryptionKey}', '{gameObject.name}', 'ReceiveEncryptedData')";
        Application.ExternalCall("eval", jsCommand);
#else
        var encryptRequest = new UnityWebRequest(API.GetEncryptDataAPI(), "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        encryptRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        encryptRequest.downloadHandler = new DownloadHandlerBuffer();
        encryptRequest.SetRequestHeader("Content-Type", "application/json");

        yield return encryptRequest.SendWebRequest();

        if (encryptRequest.result == UnityWebRequest.Result.ConnectionError || encryptRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + encryptRequest.error);
        }
        else
        {

            EncryptedResponse response = JsonUtility.FromJson<EncryptedResponse>(encryptRequest.downloadHandler.text);
            encryptedJson = $"{{\"data\":\"{response.encrypted_data}\"}}";
        }
#endif

        using UnityWebRequest betRequest = new(API.GetSendBetDataAPI(), "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(encryptedJson);
        betRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        betRequest.downloadHandler = new DownloadHandlerBuffer();
        betRequest.SetRequestHeader("Content-Type", "application/json");
        yield return betRequest.SendWebRequest();
        if (betRequest.result != UnityWebRequest.Result.Success)
        {
            Log("Error: " + betRequest.error);
            string responseJson = betRequest.downloadHandler.text;
            ErrorBetResponse betResponse = JsonUtility.FromJson<ErrorBetResponse>(responseJson);
            isSuccess = betResponse.status;
            msg = betResponse.message;
        }
        else
        {
            string responseJson = betRequest.downloadHandler.text;
            //Debug.Log(responseJson);
            Log(responseJson);
            BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);
            isSuccess = betResponse.status;
            roundType = betResponse.data.type;
            mega_try_count = betResponse.data.try_count;
            StartCoroutine(PlayOrbAnimationIE(betResponse.data.energy_bar, betResponse.data.total_energy_orb, false));
            txtBalance.text = MoneyFormat(decimal.Parse(betResponse.data.current_balance));
        }

        if(isSuccess)
        {
            Audio.PlaySfx(2, 3);
            purchaseWindow.SetActive(false);
            if (index == 1) // purchase mega bonus
            {
                canPurchase = true;
                isFlipping = true;
            }
            
            ticketAnim.gameObject.SetActive(true);
            SpineHelper.PlayAnimation(ticketAnim, ticketAnimName[0], false);
            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(ticketAnim, ticketAnimName[0]));
            SpineHelper.PlayAnimation(ticketAnim, ticketAnimName[1], true);
            yield return new WaitForSeconds(1f);
            ticketAnim.gameObject.SetActive(false);

            if (index == 1) // purchase mega bonus
            {
                StartCoroutine(SendBetRequest());
            }
            else
            {
                megaBonusHint.SetActive(true);
            }
        }
        else
        {
            menuManager.ShowErroInfo(msg);
            canPurchase = true;
        }
    }

    void TooglePurchaseWindow()
    {
        if (isFlipping || isAutoPlay)
            return;
        StartCoroutine(BlinkSpineObjectIE(purchaseButtonSpine));
        Audio.PlayClickSfx();
        purchaseWindow.SetActive(!purchaseWindow.activeInHierarchy);
    }

    IEnumerator BlinkSpineObjectIE(SkeletonGraphic spine)
    {
        spine.color = pressedButtonColor;
        yield return new WaitForSeconds(0.1f);
        spine.color = Color.white;
    }

    void OnFlipCoinDown()
    {
        if (!isAutoPlay)
            FlipCoin();
    }

    void AssignAutoPlayButtons()
    {
        for (int i = 0; i < autoBetNumberButtons.Count; i++)
        {
            EventTrigger eventTrigger = autoBetNumberButtons[i].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            int index = i;
            int autoBetNumber = int.Parse(autoBetNumberButtons[index].GetComponentInChildren<TextMeshProUGUI>().text);
            entry.callback.AddListener((data) => { OnAutoPlayNumberButtonDown(index, autoBetNumber); });
            eventTrigger.triggers.Add(entry);
        }

        EventTrigger.Entry entryAutoPlay = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryAutoPlay.callback.AddListener((data) => { ConfirmAutoPlay(); });
        EventTrigger eventTriggerAutoPlay = btnConfirmAutoPlay.AddComponent<EventTrigger>();
        eventTriggerAutoPlay.triggers.Add(entryAutoPlay);

        EventTrigger.Entry entryStopAutoPlay = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryStopAutoPlay.callback.AddListener((data) => { OnStopAutoPlayDown(); });
        EventTrigger eventTriggerStopAutoPlay = btnStopAutoPlay.AddComponent<EventTrigger>();
        eventTriggerStopAutoPlay.triggers.Add(entryStopAutoPlay);
        
    }

    void OnAutoPlayNumberButtonDown(int index, int autobetNumber)
    {
        Audio.PlayClickSfx();
        selectedAutoPlayCount = autobetNumber;
        currentAutoPlayCount = autobetNumber;
        SetActiveColor(index);
    }

    void OnStopAutoPlayDown(bool playSound = true)
    {
        if(playSound)
            Audio.PlayClickSfx();
        isAutoPlay = false;
        btnStopAutoPlay.SetActive(false);
        txtCurrentAutoPlayCount.gameObject.SetActive(false);
        StopAutoPlay();
    }

    void SetActiveColor(int index)
    {
        for (int i = 0; i < autoBetNumberButtons.Count; i++)
        {
            autoBetNumberButtons[i].GetComponent<Image>().sprite = i == index ? numberButtonSprite[0] : numberButtonSprite[1];
        }
    }

    void ConfirmAutoPlay()
    {
        purchaseWindow.SetActive(false);
        PunchScaleObject(btnConfirmAutoPlay);
        menuManager.CloseAllSubMenu();
        isAutoPlay = true;
        StopAutoPlay();
        autoPlayCoroutine = StartAutoPlayIE();
        StartCoroutine(autoPlayCoroutine);
    }

    IEnumerator StartAutoPlayIE()
    {
        currentAutoPlayCount = selectedAutoPlayCount;
        btnStopAutoPlay.SetActive(true);
        autoPlayButtonSpine.gameObject.SetActive(true);
        spinButtonSpine.gameObject.SetActive(false);

        txtCurrentAutoPlayCount.text = currentAutoPlayCount.ToString();
        txtCurrentAutoPlayCount.gameObject.SetActive(true);
        for (int i = 0; i < selectedAutoPlayCount; i++)
        {
            // originalBet = confirmedBet;
            if (isAutoPlay)
            {
                if (isRandomChoice)
                {
                    SelectRandomCoin();
                }
                isFlipping = true;
                StartCoroutine(PlaySpinButtonAnimationIE());
                yield return StartCoroutine(SendBetRequest());
                currentAutoPlayCount--;
                txtCurrentAutoPlayCount.text = currentAutoPlayCount.ToString();
            }
            else
            {
                StopAutoPlay();
                break;
            }
        }
        isAutoPlay = false;
        btnStopAutoPlay.SetActive(false);
        txtCurrentAutoPlayCount.gameObject.SetActive(false);
        autoPlayButtonSpine.gameObject.SetActive(false);
        spinButtonSpine.gameObject.SetActive(true);
    }

    public void StopAutoPlay()
    {
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
        autoPlayButtonSpine.gameObject.SetActive(false);
        spinButtonSpine.gameObject.SetActive(true);
    }

    void GoToGameScreen()
    {
        Audio.PlayClickSfx();
        PunchScaleObject(btnStart);
        StartCoroutine(GoToGameScreenIE());
        SpineHelper.PlayAnimation(mainCharSpine, mainCharAnimation[0], true);
    }
    IEnumerator GoToGameScreenIE()
    {
        menuManager.isSubMenuOpen = false;
        yield return new WaitForSeconds(0.25f);
        mainGameObjects.SetActive(true);
        if (!successLogin)
            StartCoroutine(GetInitialDataAPIIE());
        yield return StartCoroutine(transitionHelper.GoToMainScreenIE());
        ShowMainScreen();
    }

    public void GoToTitleScreen()
    {
        StartCoroutine(GoToTitleScreenIE());
    }
    IEnumerator GoToTitleScreenIE()
    {
        ShowMainScreen(false);
        yield return StartCoroutine(transitionHelper.GoToTitleScreenIE());
        StartCoroutine(PlayTitleCharacterAnimationIE());
    }

    IEnumerator GetInitialDataAPIIE()
    {
        if (buildType == BuildType.Development)
        {
            yield return StartCoroutine(TriggerLoginIE());
        }
        yield return StartCoroutine(GetUserDataIE());
        yield return StartCoroutine(GetHistoryDataIE());
        btnStart.SetActive(true);
    }

    public void ShowMainScreen(bool isShow = true)
    {
        purchaseMenuUI.SetActive(isShow);
        mainGameScreenUI.SetActive(isShow);
    }

    void SelectRandomCoin()
    {
        bool coinIsHeads = UnityEngine.Random.value < 0.5f;
        string ranCoin = coinIsHeads ? "Head" : "Tail";
        selectedChoice = ranCoin;
        ShowSelectedCoin(ranCoin == "Head");
        //Debug.Log(ranCoin);
    }


    void SelectCoinSide(GameObject coinObj, string coinSide = "Head")
    {
        if (isAutoPlay || isFlipping)
            return;
        isRandomChoice = false;

        Audio.PlaySfx(0);
        Audio.PlaySfx(8, 9);

        PunchScaleObject(coinObj);
        selectedChoice = coinSide;
        ShowSelectedCoin(coinSide == "Head");
    }

    void PunchScaleObject(GameObject imageObject, bool overWriteSize = false, float scaleSize = 0)
    {
        Vector3 punchScale = overWriteSize ? new Vector3(scaleSize, scaleSize, scaleSize) : punchScaleAmount;
        if (imageObject.transform.localScale == Vector3.one)
        {
            imageObject.transform.DOPunchScale(punchScale, 0.25f, 1, 1).OnComplete(() => {
                imageObject.transform.localScale = Vector3.one;
            });
        }
    }

    void ShowSelectedCoin(bool isHead)
    {
        btnRandomAutoPlay.GetComponent<Image>().color = inactiveColor;
        coinSelectionHead.SetActive(isHead);
        coinSelectionTail.SetActive(!isHead);
        btnHeadAutoPlayObj.SetActive(isHead);
        btnTailAutoPlayObj.SetActive(!isHead);
    }

    void ChangeBet(GameObject btnObj, bool isIncrease = true)
    {
        if (isFlipping || isAutoPlay || isMegaBonusRoundMode || megaBonusHint.activeInHierarchy)
            return;

        Audio.PlayClickSfx();
        PunchScaleObject(btnObj);
        if (isIncrease){
            chipBaseIndex++;
            if (chipBaseIndex >= chipBase.Count)
                chipBaseIndex = 0;
        }
        else{
            chipBaseIndex--;
            if (chipBaseIndex < 0)
                chipBaseIndex = chipBase.Count - 1;
        }
        confirmedBet = chipBase[chipBaseIndex];
        txtBet.text = MoneyFormat(confirmedBet);
        txtBonusPrice.text = MoneyFormat(GetBonusPrice());
        txtMegaBonusPrice.text = MoneyFormat(GetMegaBonusPrice());
    }

    IEnumerator PlayBonusRoundAnimationIE()
    {
        indikatorUI.SetActive(false);
        SpineHelper.PlayAnimation(mainBackgroundSpine, "all", true);
        Audio.PlaySfx(1);
        yield return StartCoroutine(PlayLightningAnimationIE());
        StartCoroutine(ShowTextBonusAnimationIE());

        coinSpine[0].gameObject.SetActive(true);
        coinSpine[2].gameObject.SetActive(true);

        SpineHelper.PlayAnimation(coinSpine[0], SpineHelper.GetCurrentAnimationName(coinSpine[1]), true);
        SpineHelper.PlayAnimation(coinSpine[2], SpineHelper.GetCurrentAnimationName(coinSpine[1]), true);

        coinSpine[0].transform.DOJump(coinPos[0].transform.position, 0.5f, 1, 0.25f);
        coinSpine[2].transform.DOJump(coinPos[2].transform.position, -0.5f, 1, 0.25f);
        yield return new WaitForSeconds(0.35f);
        lightningSpine.gameObject.SetActive(false);
;    }


    IEnumerator PlayLightningAnimationIE()
    {
        lightningSpine.gameObject.SetActive(true);
        SpineHelper.PlayAnimation(lightningSpine, "Explosion", false);
        yield return new WaitForSeconds(0.25f);
        SpineHelper.PlayAnimation(orbSpine, "no rings", true);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator ShowTextBonusAnimationIE()
    {
        textBonusSpine.gameObject.SetActive(true);
        SpineHelper.PlayAnimation(textBonusSpine, "bonus start", false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(textBonusSpine, "bonus start"));
        SpineHelper.PlayAnimation(textBonusSpine, "bonus idle", true);
    }

    public void FlipCoin()
    {
        if(confirmedBet < minimalBet || confirmedBet > maximalBet)
        {
            menuManager.ShowErroInfo("Minimum Bet " + MoneyFormat(minimalBet) + " \r\ndan Max Bet " + MoneyFormat(maximalBet));
            return;
        }

        if (isFlipping || confirmedBet <= 0)
            return;
        isFlipping = true;

        megaBonusHint.SetActive(false);
        if (!isAutoPlay)
        {
            StartCoroutine(PlaySpinButtonAnimationIE());
        }
        else
        {
            StartCoroutine(PlayAutoPlayButtonAnimationIE());
        }

        // originalBet = confirmedBet;
        StartCoroutine(SendBetRequest());
    }

    IEnumerator PlaySpinButtonAnimationIE()
    {
        megaBonusHint.SetActive(false);
        Audio.PlaySfx(2, 3);
        SpineHelper.PlayAnimation(spinButtonSpine, spinButtonSpineAnimation[1], true);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(spinButtonSpine, spinButtonSpineAnimation[1]));
    }

    IEnumerator PlayAutoPlayButtonAnimationIE()
    {
        megaBonusHint.SetActive(false);
        Audio.PlaySfx(2, 3);
        SpineHelper.PlayAnimation(autoPlayButtonSpine, spinButtonSpineAnimation[1], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(autoPlayButtonSpine, autoPlayButtonSpineAnimation[1]));
        SpineHelper.PlayAnimation(autoPlayButtonSpine, spinButtonSpineAnimation[0], true);
    }

    IEnumerator SendBetRequest()
    {
        if (SpineHelper.GetCurrentAnimationName(mainCharSpine) != mainCharAnimation[0])
        {
            SpineHelper.PlayAnimation(mainCharSpine, mainCharAnimation[0], true);
        }

        bool isError = false;
        bool isWinning = false;

        EnableBetPanelButton(false);
        if (roundType == "mega_bonus_round")
        {
            yield return StartCoroutine(StartMegaBonusIE(confirmedBet, mega_try_count));
            StartCoroutine(PlayOrbAnimationIE(0, total_energy_orb, false));
        }
        else
        {
            if (!isPurchasing)
            {
                totalPlayerBalance -= confirmedBet;
                txtBalance.text = MoneyFormat(totalPlayerBalance);
            }
            //Debug.Log(confirmedBet);

            isBonusRoundMode = roundType == "bonus_round";

            string button = roundType == "bonus_round" ?
                selectedChoice + "," + selectedChoice + "," + selectedChoice :
                selectedChoice;
            
            DataToSend data = new()
            {
                data = JsonUtility.ToJson(new BetDataToSend
                {
                    total_amount = confirmedBet,
                    button_bet = new ButtonBet
                    {
                        button = button,
                        amount = confirmedBet,
                        type = roundType
                    }
                })
            };
            string jsonData = JsonUtility.ToJson(data);
            
            //Debug.Log(jsonData); 

#if UNITY_WEBGL && !UNITY_EDITOR
            jsonData = JsonUtility.ToJson(new BetDataToSend
            {
                total_amount = confirmedBet,
                button_bet = new ButtonBet
                {
                    button = button,
                    amount = confirmedBet,
                    type = roundType
                }
            });
            string jsCommand = $"encryptDataAndSendBack('{jsonData}', '{API.encryptionKey}', '{gameObject.name}', 'ReceiveEncryptedData')";
            Application.ExternalCall("eval", jsCommand);
#else
            var encryptRequest = new UnityWebRequest(API.GetEncryptDataAPI(), "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            encryptRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            encryptRequest.downloadHandler = new DownloadHandlerBuffer();
            encryptRequest.SetRequestHeader("Content-Type", "application/json");

            yield return encryptRequest.SendWebRequest();

            if (encryptRequest.result == UnityWebRequest.Result.ConnectionError || encryptRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Log("Error: " + encryptRequest.error);
                isError = true;
                SpineHelper.PlayAnimation(spinButtonSpine, spinButtonSpineAnimation[0], true);
                if (!isPurchasing)
                {
                    totalPlayerBalance += confirmedBet;
                    txtBalance.text = MoneyFormat(totalPlayerBalance);
                }
                menuManager.ShowErroInfo();
            }
            else
            {
                EncryptedResponse response = JsonUtility.FromJson<EncryptedResponse>(encryptRequest.downloadHandler.text);
                encryptedJson = $"{{\"data\":\"{response.encrypted_data}\"}}";
            }
#endif
            int currentBet = confirmedBet;

            if (!isError)
            {
                using UnityWebRequest betRequest = new(API.GetSendBetDataAPI(), "POST");
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(encryptedJson);
                betRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                betRequest.downloadHandler = new DownloadHandlerBuffer();
                betRequest.SetRequestHeader("Content-Type", "application/json");
                yield return betRequest.SendWebRequest();

                if (betRequest.result != UnityWebRequest.Result.Success)
                {
                    string responseJson = betRequest.downloadHandler.text;
                    BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);

                    Log("Error: " + betRequest.error);
                    isError = true;
                    SpineHelper.PlayAnimation(spinButtonSpine, spinButtonSpineAnimation[0], true);
                    menuManager.ShowErroInfo(betResponse.message);
                    if (isAutoPlay)
                    {
                        OnStopAutoPlayDown(false);
                    }
                }
                else
                {
                    string responseJson = betRequest.downloadHandler.text;
                    //Debug.Log(responseJson);
                    Log(responseJson);
                    BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);
                    
                    if (isBonusRoundMode)
                    {
                        Audio.ChangeToBonusBgm();
                        string[] jackpotResult = betResponse.data.result.Split(",");

                        yield return StartCoroutine(PlayBonusRoundAnimationIE());
                        StartCoroutine(ShowFlippedCoinResultIE(jackpotResult[1] == "Head", 1));
                        yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.15f));
                        StartCoroutine(ShowFlippedCoinResultIE(jackpotResult[0] == "Head", 0));
                        yield return StartCoroutine(ShowFlippedCoinResultIE(jackpotResult[2] == "Head", 2));
                    }
                    else
                    {
                        yield return StartCoroutine(ShowFlippedCoinResultIE(betResponse.data.result == "Head"));
                    }

                    if (!isAutoPlay)
                    {
                        spinButtonSpine.gameObject.SetActive(true);
                        SpineHelper.PlayAnimation(spinButtonSpine, spinButtonSpineAnimation[0], true);
                    }

                    isWinning = betResponse.data.result.Contains(selectedChoice);
                    //int totalToWin = Mathf.RoundToInt(betResponse.data.prize * currentBet);
                    decimal totalToWin = decimal.Parse(betResponse.data.total_win);
                    totalPlayerBalance += totalToWin;

                    if (isWinning)
                    {
                        if (isBonusRoundMode)
                        {
                            string[] jackpotResult = betResponse.data.result.Split(",");
                            float winCount = 0;
                            for (int i = 0; i < jackpotResult.Length; i++)
                            {

                                txtCoinMultiplier[i].transform.position = new Vector3(coinPos[i].position.x, coinPos[i].position.y - 1, coinPos[i].transform.position.z);
                                if (jackpotResult[i] == selectedChoice)
                                    winCount++;
                            }
                            float multiplier = betResponse.data.prize / winCount;
                            for (int i = 0;i < jackpotResult.Length; i++)
                            {
                                if (jackpotResult[i] == selectedChoice)
                                {
                                    txtCoinMultiplier[i].text = multiplier.ToString("F2").Replace(".00", "") + "x";
                                    txtCoinMultiplier[i].gameObject.SetActive(true);
                                }
                                txtCoinMultiplier[i].transform.DOMove(coinPos[i].position, 0.25f).SetEase(Ease.Linear);
                            }
                            yield return new WaitForSeconds(0.25f);
                            
                            for (int i = 0; i < txtCoinMultiplier.Length; i++)
                            {
                                txtCoinMultiplier[i].transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.5f, 1, 1).SetEase(Ease.Linear);
                            }
                            yield return new WaitForSeconds(0.5f);
                            
                            for (int i = 0; i < txtCoinMultiplier.Length; i++)
                            {
                                txtCoinMultiplier[i].transform.DOMove(txtWinningNumber.transform.position, 0.25f).SetEase(Ease.Linear);
                            }
                            yield return new WaitForSeconds(0.25f);

                            for (int i = 0; i < txtCoinMultiplier.Length; i++)
                            {
                                txtCoinMultiplier[i].text = betResponse.data.prize + "x";
                                txtCoinMultiplier[i].transform.DOPunchScale(Vector3.one, 0.75f, 1, 1).SetEase(Ease.Linear);
                            }
                            yield return new WaitForSeconds(0.75f);

                            for (int i = 0; i < txtCoinMultiplier.Length; i++)
                            {
                                txtCoinMultiplier[i].transform.position = coinPos[i].position;
                                txtCoinMultiplier[i].gameObject.SetActive(false);
                            }
                        }
                        ShowWinningNumberText(true, totalToWin);
                    }

                    if (!isBonusRoundMode)
                    {
                        ShowWinningUI(isWinning);
                    }
                    else
                    {
                        StartCoroutine(ShowWinningAnimationIE(isWinning));
                        if (isWinning)
                        {
                            Audio.PlaySfx(7);
                            Audio.PlaySfx(12);
                            int winNumber = 0;
                            string[] splitCoins = betResponse.data.result.Split(",");
                            for (int i = 0; i < splitCoins.Length; i++)
                            {
                                if (splitCoins[i] == selectedChoice)
                                    winNumber++;
                            }
                            bonusGameInfoText[winNumber - 1].gameObject.SetActive(true);
                            SpineHelper.PlayAnimation(bonusGameInfoText[winNumber - 1], bonusGameInfoText[winNumber - 1].name + " start", false);
                            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(bonusGameInfoText[winNumber - 1], bonusGameInfoText[winNumber - 1].name + " start"));
                            SpineHelper.PlayAnimation(bonusGameInfoText[winNumber - 1], bonusGameInfoText[winNumber - 1].name + " idle", true);
                        }
                        else
                        {
                            Audio.PlaySfx(4);
                            StartCoroutine(ShowLosingInfoIE());
                        }
                    }

                    if (!isWinning)
                    {
                        if (currentBet > totalPlayerBalance)
                        {
                            currentBet = (int)totalPlayerBalance;
                            txtBet.text = MoneyFormat(currentBet);
                        }
                    }

                    AddNewResultHistory(betResponse.data.result);
                    ShowHistoryCoinResult();

                    //Debug.Log(betResponse.data.current_balance);
                    txtBalance.text = MoneyFormat(decimal.Parse(betResponse.data.current_balance));
                    //txtBalance.text = betResponse.data.current_balance;

                    float delay = 3f;
                    if (!isBonusRoundMode)
                    {
                        if (isWinning)
                            delay = SpineHelper.GetAnimationDuration(winningSpine, winningAnimationName[0]) + SpineHelper.GetAnimationDuration(winningSpine, winningAnimationName[1]);
                        else
                            delay = SpineHelper.GetAnimationDuration(losingUISpine, "try again start") + SpineHelper.GetAnimationDuration(losingUISpine, "try again idle");
                    }
                    yield return new WaitForSeconds(delay / 2);

                    if (!isBonusRoundMode)
                        StartCoroutine(PlayOrbAnimationIE(betResponse.data.energy_bar, betResponse.data.total_energy_orb, isWinning));
                    else
                        StartCoroutine(PlayOrbAnimationIE(0, betResponse.data.total_energy_orb, false));

                    roundType = betResponse.data.type;
                    if (roundType == "mega_bonus_round")
                    {
                        yield return StartCoroutine(StartMegaBonusIE(confirmedBet, betResponse.data.try_count));
                        StartCoroutine(PlayOrbAnimationIE(0, total_energy_orb, false));
                    }
                    else
                    {                        
                        Audio.ChangeToNormalBgm();
                        megaBonusHint.SetActive(roundType == "bonus_round");
                    }


                    canPurchase = true;
                    isPurchasing = false;
                }
            }
        }
        
        if (txtWinningNumber.gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(0.5f);
            ShowWinningNumberText(false);
        }
        
        ResetBonusGameState();
        EnableBetPanelButton(true);

        // SpineHelper.PlayAnimation(mainCharSpine, mainCharAnimation[0], true);
        StartCoroutine(WaitForNextTurnIE(isWinning));

        winningSpine.gameObject.SetActive(false);
        losingUISpine.gameObject.SetActive(false); 
        if (roundType == "bonus_round" && isAutoPlay)
        {
            yield return new WaitForSeconds(delayOnBonusHintAutoPlay);
        }
        isFlipping = false;
    }

    IEnumerator WaitForNextTurnIE(bool isWinning)
    {
        if (!isWinning)
        {
            yield return new WaitForSeconds(isWinning ? 1f : 0.5f);
            if (SpineHelper.GetCurrentAnimationName(mainCharSpine) != mainCharAnimation[0])
            {
                SpineHelper.PlayAnimation(mainCharSpine, mainCharAnimation[0], true);
            }
        }
    }

    IEnumerator StartMegaBonusIE(int bet, int tryCount)
    {
        isMegaBonusRoundMode = true;
        ShowWinningNumberText(false);
        ShowMainScreen(false);

        yield return StartCoroutine(transitionHelper.GoToMegaBonusRoundIE());
        megaBonus.StartMegaBonusRoundSession(bet, tryCount);

        while (megaBonus.megaBonusIsActive)
        {
            yield return null;
        }
        isMegaBonusRoundMode = false;
        ResetBonusGameState();

        yield return StartCoroutine(transitionHelper.BackFromMegaBonusRoundIE());
        roundType = "normal_round";
        ShowMainScreen(true);
        indikatorUI.SetActive(true);
        EnableBetPanelButton(true);
    }

    void ShowWinningUI(bool isWin = true)
    {
        StartCoroutine(ShowWinningAnimationIE(isWin));
        if (isWin)
        {
            Audio.PlaySfx(7);
            winningSpine.gameObject.SetActive(true);
            StartCoroutine(ShowWinningInfoIE());
        }
        else
        {
            Audio.PlaySfx(4);
            StartCoroutine(ShowLosingInfoIE());
        }
    }

    IEnumerator ShowWinningAnimationIE(bool isWin = true)
    {
        SpineHelper.PlayAnimation(mainCharSpine, mainCharAnimation[isWin ? 3 : 1], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(mainCharSpine, mainCharAnimation[3]));
        SpineHelper.PlayAnimation(mainCharSpine, mainCharAnimation[isWin? 4 : 2], true);
    }

    IEnumerator ShowWinningInfoIE()
    {
        SpineHelper.PlayAnimation(winningSpine, winningAnimationName[0], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(winningSpine, winningAnimationName[0])/2);
        SpineHelper.PlayAnimation(winningSpine, winningAnimationName[1], true);
    }

    IEnumerator ShowLosingInfoIE()
    {
        losingUISpine.gameObject.SetActive(true);
        SpineHelper.PlayAnimation(losingUISpine, "try again start", false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(losingUISpine, "try again start")/2);
        SpineHelper.PlayAnimation(losingUISpine, "try again idle", true);
    }

    IEnumerator ShowFlippedCoinResultIE(bool isHead, int index = 1)
    {
        float spanTime = 1f;
        string spinAnim = coinAnimation[isHead ? 2 : 3];
        float delay = (SpineHelper.GetAnimationDuration(coinSpine[index], spinAnim) + 0.5f) - spanTime;
        SpineHelper.PlayAnimation(coinSpine[index], spinAnim, false);
        yield return new WaitForSeconds(1f);
        Audio.PlaySfx(11);
        yield return new WaitForSeconds(delay - 1f);
        SpineHelper.PlayAnimation(coinSpine[index], coinAnimation[isHead ? 0 : 1], true);
    }

    public void ShowInputKeyboard()
    {
        if (isFlipping || isAutoPlay || isMegaBonusRoundMode || megaBonusHint.activeInHierarchy)
            return;

        //keyboard.GetComponent<CustomKeyboard>().SetResultText(confirmedBet, totalPlayerBalance);
        //keyboard.SetActive(true);
    }

    public void SetBetFromKeyboard(int number)
    {
        //keyboard.SetActive(false);
        confirmedBet = number;
        txtBet.text = MoneyFormat(confirmedBet);
        txtBonusPrice.text = MoneyFormat(GetBonusPrice());
        txtMegaBonusPrice.text = MoneyFormat(GetMegaBonusPrice());
    }

    public void ShowWinningNumberText(bool isShow = true, decimal number = 0)
    {
        txtWinningNumber.gameObject.SetActive(true);
        txtWinningNumber.transform.localScale = isShow ? Vector3.zero : Vector3.one;
        if(number > 0)
        {
            //txtWinningNumber.text = "Rp " + MoneyFormat(number);
            //txtWinningNumberShadow.text = "Rp " + MoneyFormat(number);
            StartCoroutine(AnimateNumberIE(0, (float)number, 1f));
        }
        txtWinningNumber.transform.DOScale(isShow ? Vector3.one : Vector3.zero, 0.25f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (!isShow)
            {
                StopFlashingText();
                txtWinningNumber.gameObject.SetActive(false);
            }
            else
            {
                FlashingNumberText();
            }
        });
    }

    IEnumerator AnimateNumberIE(float startValue, float endValue, float duration)
    {
        DOTween.To(() => startValue, x => startValue = x, endValue, duration).OnUpdate(() =>
        {
            txtWinningNumber.text = MoneyFormat(startValue);
            txtWinningNumberShadow.text = MoneyFormat(startValue);
        });
        yield return new WaitForSeconds(1f);
        PunchScaleObject(txtWinningNumber.gameObject, true, 0.5f);
    }

    void StopFlashingText()
    {
        if (flashingTextCoroutine != null)
            StopCoroutine(flashingTextCoroutine);
        flashingTextCoroutine = null;
    }
    void FlashingNumberText()
    {
        StopFlashingText();
        flashingTextCoroutine = FlashingNumberTextIE();
        StartCoroutine(flashingTextCoroutine);
    }

    IEnumerator FlashingNumberTextIE(float duration = 1f, float delay = 0.5f)
    {
        while (txtWinningNumber.gameObject.activeInHierarchy)
        {
            Material sharedMaterial = txtWinningNumber.GetComponent<TMP_Text>().fontSharedMaterial;
            sharedMaterial.SetFloat("_LightAngle", 6.2f);
            DOTween.To(() => sharedMaterial.GetFloat("_LightAngle"), x => sharedMaterial.SetFloat("_LightAngle", x), 0, duration).SetEase(Ease.Linear);
            yield return new WaitForSeconds(delay + duration);
        }
    }

    public void Log(string message)
    {
        if(buildType == BuildType.Development)
        {
            Debug.Log(message);
        }
    }

    public void EnableBetPanelButton(bool isEnabled = true)
    {
        //txtBet.enabled = isEnabled;
        btnIncreaseBet.GetComponent<Image>().sprite = betPanelButtonSprite[isEnabled ? 0 : 1];
        btnDecreaseBet.GetComponent<Image>().sprite = betPanelButtonSprite[isEnabled ? 2 : 3];

        menuManager.subMenuButton[1].raycastTarget = isEnabled;
        menuManager.subMenuButton[1].color = isEnabled ? Color.white : new Color32(200, 200, 200, 255);
        menuManager.subMenuButton[3].raycastTarget = isEnabled;
        menuManager.subMenuButton[3].color = isEnabled ? Color.white : new Color32(200, 200, 200, 255);
    }
}