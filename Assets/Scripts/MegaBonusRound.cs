using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using Spine.Unity;
using System;
using UnityEngine.Networking;
using System.Linq;
using System.Net;

public class MegaBonusRound : MonoBehaviour
{
    AudioManager Audio;
    [SerializeField] private MainGame game;
    [SerializeField] private MenuManager menu;
    [SerializeField] private APIManager API;

    [SerializeField] private TextMeshProUGUI txtBalance;
    [SerializeField] private GameObject megaBonusCanvas;

    [SerializeField] private GameObject[] fountainButton;
    [SerializeField] private SkeletonGraphic[] fountainButtonSpine;
    [SerializeField] private string[] buttonAnimationName = new string[] { "flip button idle", "flip button push", "flip button disable" };
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinCount = 10;
    [SerializeField] private TextMeshProUGUI txtTryCountText;
    int tryCount = 5;

    [SerializeField] private TextMeshPro txtWinningNumber;
    [SerializeField] private TextMeshPro txtWinningNumberShadow;

    [SerializeField] private Transform leftTopMaxPos;
    [SerializeField] private Transform rightTopMaxPos;
    [SerializeField] private Transform BottomPos;
    [SerializeField] private Transform[] projectionPos;

    [SerializeField] private Transform leftInitPos, rightInitPos;
    [SerializeField] private float jumpPower = 0.5f;

    [SerializeField] private SkeletonAnimation charAnim;
    [SerializeField] private string[] charAnimationName;
    [SerializeField] private List<GameObject> orbAnimObject;
    [SerializeField] private string[] orbAnimationName;
    [SerializeField] private List<MegaBonusPose> megaBonusPose;

    [SerializeField] private int activePose = 0;
    int previousPose = 0;
    IEnumerator poseChangingCoroutine;
    IEnumerator poseAnimationChangeCoroutine;
    float currentPrize = 0;
    decimal totalToWin = 0;

    bool megaBonusEnd = false;
    bool canShootCoin = true;
    [SerializeField] float poseChangeDelay = 3f;

    IEnumerator flashingTextCoroutine;
    int confirmedBet = 1000;
    decimal totalMoneyEarned = 0;

    public bool megaBonusIsActive = true;
    decimal current_balance;

    [SerializeField] private SkeletonAnimation[] splashSpine;
    [SerializeField] private Vector3 punchScaleAmount = new Vector3(0.15f, 0.15f, 0.15f);

    string encryptedJson;
    List<float> prizeList = new List<float>();
    public void SetPrizeList(UserDataResponse response)
    {
        prizeList = response.data.game.prize_detail.mega_bonus_round.win.model_1.prize.ToList();
    }

    void Start()
    {
        Audio = AudioManager.Instance;
        HideAllOrbs();
        charAnim.skeletonDataAsset.GetAnimationStateData().SetMix(charAnimationName[1], charAnimationName[2], 0.5f);
        charAnim.skeletonDataAsset.GetAnimationStateData().SetMix(charAnimationName[2], charAnimationName[3], 0.5f);
        charAnim.skeletonDataAsset.GetAnimationStateData().SetMix(charAnimationName[3], charAnimationName[1], 0.5f);
        AssignUIButtons();
    }

    void HideAllOrbs()
    {
        orbAnimObject.ForEach((x) =>
        {
            x.SetActive(false);
            x.transform.GetChild(0).gameObject.SetActive(false);
        });
    }

    void AssignUIButtons()
    {
        for (int i = 0; i < fountainButton.Length; i++)
        {
            int index = i;
            EventTrigger eventTrigger = fountainButton[index].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entryButton = new()
            {
                eventID = EventTriggerType.PointerClick
            };
            entryButton.callback.AddListener((data) => { TriggerCoinShoot(index); });
            eventTrigger.triggers.Add(entryButton);
        }
    }

    public void StartMegaBonusRoundSession(int bet = 1000, int try_count = 5)
    {
        megaBonusIsActive = true;
        megaBonusEnd = false;
        confirmedBet = bet;
        totalMoneyEarned = 0;
        txtWinningNumber.text = game.MoneyFormat(0);
        txtWinningNumberShadow.text = game.MoneyFormat(0);
        canShootCoin = true;
        ToogleButtonStateAnimation();
        tryCount = try_count;
        activePose = UnityEngine.Random.Range(0, 2);

        txtTryCountText.text = tryCount.ToString();
        StartCoroutine(ActivateMegaBonusPoseIE());
        StartChangingPose();
    }

    public void StartChangingPose()
    {
        StopChangingPose();
        poseChangingCoroutine = ChangeMegaBonusPoseIE();
        StartCoroutine(poseChangingCoroutine);
    }

    public void StopChangingPose()
    {
        if (poseChangingCoroutine != null)
        {
            StopCoroutine(poseChangingCoroutine);
            poseChangingCoroutine = null;
        }
    }

    IEnumerator ChangeMegaBonusPoseIE()
    {
        while (!megaBonusEnd)
        {
            float delay = 0;
            if (SpineHelper.GetCurrentAnimationName(charAnim) != megaBonusPose[activePose].animationName)
            {
                StartCoroutine(ExchangePoseIE());
                delay += SpineHelper.GetAnimationDuration(orbAnimObject[0].GetComponent<SkeletonAnimation>(), orbAnimationName[0]);
                delay += SpineHelper.GetAnimationDuration(orbAnimObject[0].GetComponent<SkeletonAnimation>(), orbAnimationName[2]);
            }

            yield return new WaitForSeconds(poseChangeDelay + delay);
            previousPose = activePose;
            activePose++;
            if (activePose == megaBonusPose.Count - 1)
            {
                activePose = 0;
            }
        }
    }


    IEnumerator ExchangePoseIE()
    {
        MegaBonusPose pose = megaBonusPose[previousPose];
        for (int i = 0; i < pose.activeOrb.Count; i++)
        {
            SkeletonAnimation orb = pose.activeOrb[i].orbAnim;
            orb.GetComponent<MegaBonusOrbCollider>().KillOrb();
        }
        float delay = SpineHelper.GetAnimationDuration(orbAnimObject[0].GetComponent<SkeletonAnimation>(), orbAnimationName[2]);
        yield return new WaitForSeconds(delay);
        poseAnimationChangeCoroutine = ActivateMegaBonusPoseIE();
        StartCoroutine(poseAnimationChangeCoroutine);
    }

    IEnumerator ActivateMegaBonusPoseIE()
    {
        MegaBonusPose pose = megaBonusPose[activePose];
        SpineHelper.PlayAnimation(charAnim, pose.animationName, true);

        List<int> orbMultiplier = new();
        orbMultiplier.Add((int)(prizeList.Max()/coinCount));
        if(pose.activeOrb.Count > 1)
        {
            orbMultiplier.Add(1);
        }
        if (pose.activeOrb.Count > 2)
        {
            orbMultiplier.Add(UnityEngine.Random.Range(2, orbMultiplier[0]));
        }
        List<int> newOrbMultiplier = orbMultiplier.OrderBy(x => UnityEngine.Random.value).ToList();

        for (int i = 0; i < pose.activeOrb.Count; i++)
        {
            pose.activeOrb[i].multiplication = newOrbMultiplier[i];
            SkeletonAnimation orb = pose.activeOrb[i].orbAnim;
            orb.gameObject.SetActive(true);
            orb.GetComponent<MegaBonusOrbCollider>().ActivateOrb(pose, i);
        }
        float delay = SpineHelper.GetAnimationDuration(orbAnimObject[0].GetComponent<SkeletonAnimation>(), orbAnimationName[0]);
        yield return new WaitForSeconds(delay);
    }


    void TriggerCoinShoot(int side)
    {
        if (!canShootCoin || megaBonusEnd)
            return;
        canShootCoin = false;
        Audio.PlaySfx(2, 3);
        StartCoroutine(PlayButtonPushAnimationIE(side));
        StopChangingPose();
        StartCoroutine(PlayCoinProjectionAnimationIE(side));
    }

    IEnumerator PlayButtonPushAnimationIE(int side)
    {
        SpineHelper.PlayAnimation(fountainButtonSpine[side], buttonAnimationName[1], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(fountainButtonSpine[side], buttonAnimationName[1]));
        ToogleButtonStateAnimation();
    }

    void ToogleButtonStateAnimation()
    {
        SpineHelper.PlayAnimation(fountainButtonSpine[0], buttonAnimationName[canShootCoin ? 0 : 2], true);
        SpineHelper.PlayAnimation(fountainButtonSpine[1], buttonAnimationName[canShootCoin ? 0 : 2], true);
    }


    IEnumerator PlayCoinProjectionAnimationIE(int side)
    {
        // wait for API!
        yield return StartCoroutine(SendBetRequest());

        
        // Listing Empty Route :
        List<int> EmptyRoutes = new List<int>();
        for (int i = 0; i < projectionPos.Length; i++)
        {
            EmptyRoutes.Add(i);
        }
        for (int i = 0; i < megaBonusPose[activePose].activeOrb.Count; i++)
        {
            EmptyRoutes.Remove(megaBonusPose[activePose].activeOrb[i].index);
        }


        List<int> orbSequence = GetCoinCombinations(currentPrize);
        splashSpine[side].gameObject.SetActive(true);
        SpineHelper.PlayAnimation(splashSpine[side], "spray", false);

        if (orbSequence.Count > 0)
        {
            for (int i = 0; i < orbSequence.Count; i++)
            {
                MegaBonusOrb orb = megaBonusPose[activePose].activeOrb[orbSequence[i]];
                StartCoroutine(PlayCoinShootIE(side, orb.index, true));
                yield return new WaitForSeconds(0.1f);
            }
        }

        int missedCoin = coinCount - orbSequence.Count;
        if (missedCoin > 0)
        {
            for (int i = 0; i < missedCoin; i++)
            {
                int ran = EmptyRoutes[UnityEngine.Random.Range(0, EmptyRoutes.Count)];
                StartCoroutine(PlayCoinShootIE(side, ran, false));
                yield return new WaitForSeconds(0.1f);
            }
        }
        

        yield return new WaitForSeconds(1f);
        splashSpine[side].gameObject.SetActive(false);
        decimal prevMoney = totalMoneyEarned;
        // totalMoneyEarned += (confirmedBet * currentPrize);
        totalMoneyEarned += totalToWin;

        if (currentPrize > 0)
        {
            if (!txtWinningNumber.gameObject.activeInHierarchy)
            {
                ShowWinningNumberText(true, (float)totalMoneyEarned);
            }
            else
            {
                AnimateNumber((float)prevMoney, (float)totalMoneyEarned, 1f);
            }
        }

        txtBalance.text = game.MoneyFormat((float)current_balance);

        if (!megaBonusEnd)
        {
            canShootCoin = true;
            ToogleButtonStateAnimation();
            StartChangingPose();
        }
        else
        {
            Audio.PlaySfx(7);
            megaBonusCanvas.SetActive(false);
            HideAllOrbs();
            SpineHelper.PlayAnimation(charAnim, charAnimationName[4], false);
            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(charAnim, charAnimationName[4]));
            txtWinningNumber.gameObject.SetActive(false);
            megaBonusIsActive = false;
        }
    }


    public void ReceiveEncryptedData(string encryptedData)
    {
        encryptedJson = $"{{\"data\":\"{encryptedData}\"}}";
    }


    IEnumerator SendBetRequest()
    {
        string button = "coin_launcher";
        string roundType = "mega_bonus_round";

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
            //Debug.Log("Error: " + encryptRequest.error);
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
            string responseJson = betRequest.downloadHandler.text;
            BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);
            game.Log("Error: " + betRequest.error);
            menu.ShowErroInfo(betResponse.message);
        }
        else
        {
            string responseJson = betRequest.downloadHandler.text;
            Debug.Log(responseJson);
            BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);
            currentPrize = betResponse.data.prize;
            totalToWin = decimal.Parse(betResponse.data.total_win);
            megaBonusEnd = betResponse.data.type != "mega_bonus_round";
            tryCount = betResponse.data.try_count;
            txtTryCountText.text = tryCount.ToString();
            current_balance = decimal.Parse(betResponse.data.current_balance);
            
            //if(currentPrize >= 10)
            //{
            //    yield return StartCoroutine(ChangeToRarePoseIE());
            //}
        }
    }

    IEnumerator ChangeToRarePoseIE()
    {
        if (poseAnimationChangeCoroutine != null)
            StopCoroutine(poseAnimationChangeCoroutine);
        previousPose = activePose;
        activePose = 2;
        StartCoroutine(ExchangePoseIE());
        float delay = SpineHelper.GetAnimationDuration(orbAnimObject[0].GetComponent<SkeletonAnimation>(), orbAnimationName[0]);
        delay += SpineHelper.GetAnimationDuration(orbAnimObject[0].GetComponent<SkeletonAnimation>(), orbAnimationName[2]);
        yield return new WaitForSeconds(poseChangeDelay + delay);
    }


    public List<int> GetCoinCombinations(float target)
    {
        List<int> coinCombination = new List<int>();
        
        int[] numbers = new int[megaBonusPose[activePose].activeOrb.Count];
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = (int) megaBonusPose[activePose].activeOrb[i].multiplication;
        }

        int currentSum = 0;
        bool isDone = false;
        
        while (!isDone)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                if(currentSum + numbers[i] <= target)
                {
                    currentSum += numbers[i];
                    coinCombination.Add(i);
                }

                if (currentSum >= target)
                {
                    isDone = true;
                    break;
                }
            }
        }
        List<int> shuffledNumbers = coinCombination.OrderBy(x => UnityEngine.Random.value).ToList();
        return shuffledNumbers;
    }

    IEnumerator PlayCoinShootIE(int side, int orbTarget = 0, bool isHittingOrb = false)
    {
        Vector3 initPos = side == 0 ? leftInitPos.position : rightInitPos.position;
        GameObject coin = Instantiate(coinPrefab, initPos, Quaternion.identity);

        coin.GetComponent<Collider2D>().enabled = isHittingOrb;

        float randTime = 0.35f/2;
        coin.transform.DOMoveY(leftTopMaxPos.position.y, randTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(randTime);

        Vector3 targetPos = projectionPos[orbTarget].position;
        float delay = 1 + (orbTarget * 0.075f);
        coin.transform.DOJump(targetPos, jumpPower, 1, delay/2).SetEase(Ease.Linear);
        yield return new WaitForSeconds(delay/2);

        randTime = 0.5f + (orbTarget * 0.06f);
        float bottomHorizontalGap = 0.2f;
        float bottomX = side == 0 ? targetPos.x + bottomHorizontalGap : targetPos.x - bottomHorizontalGap;
        Vector3 bottomTargetPos = new Vector3(bottomX, BottomPos.position.y, targetPos.z);
        coin.transform.DOMove(bottomTargetPos, randTime/2).SetEase(Ease.Linear);
        yield return new WaitForSeconds(randTime/2);

        DOTween.Kill(coin);
        yield return new WaitForEndOfFrame();
        if (coin != null)
            Destroy(coin);
    }

    void ShowWinningNumberText(bool isShow = true, float number = 0)
    {
        txtWinningNumber.gameObject.SetActive(true);
        txtWinningNumber.transform.localScale = isShow ? Vector3.zero : Vector3.one;
        if (number > 0)
        {
            txtWinningNumber.text = game.MoneyFormat(number);
            txtWinningNumberShadow.text = game.MoneyFormat(number);
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

    void AnimateNumber(float startValue, float endValue, float duration)
    {
        PunchScaleObject(txtWinningNumber.gameObject);
        DOTween.To(() => startValue, x => startValue = x, endValue, duration).OnUpdate(() =>
        {
            txtWinningNumber.text = game.MoneyFormat(startValue);
            txtWinningNumberShadow.text = game.MoneyFormat(startValue);
        });
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


    void PunchScaleObject(GameObject imageObject)
    {
        imageObject.transform.DOPunchScale(punchScaleAmount, 1f, 1, 1).OnComplete(() => {
            imageObject.transform.localScale = Vector3.one;
        });
    }
}

[Serializable]
public class MegaBonusPose
{
    public int index;
    public string animationName;
    public List<MegaBonusOrb> activeOrb;
}

[Serializable]
public class MegaBonusOrb
{
    public int index;
    public SkeletonAnimation orbAnim;
    public float multiplication;
}