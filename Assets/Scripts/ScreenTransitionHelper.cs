using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScreenTransitionHelper : MonoBehaviour
{
    [SerializeField] private GameObject titleScreenUI;
    [SerializeField] private Transform logoImage;
    [SerializeField] private List<GameObject> btnOnStartScreen;
    [SerializeField] private GameObject betPanelUI;

    [SerializeField] private Transform startingChar;
    [SerializeField] private Transform[] startingCharPos;
    [SerializeField] private Transform mainScreenChar;
    [SerializeField] private Transform[] mainScreenCharPos;
    [SerializeField] private Transform table;
    [SerializeField] private Transform[] tablePos;
    [SerializeField] private Transform megaBonusChar;
    [SerializeField] private Transform[] megaBonusCharPos;

    [SerializeField] private GameObject normalAndBonusRoundBg;
    [SerializeField] private GameObject megaBonusRoundBg;

    [SerializeField] private GameObject normalAndBonusRoundObjects;
    [SerializeField] private GameObject megaBonusRoundObjects;
    [SerializeField] private GameObject megaBonusUIObjects;
    [SerializeField] private Transform megaBonusText;
    [SerializeField] private Transform[] megaBonusTextPos;

    [SerializeField] private CanvasGroup mainGameCanvas;
    [SerializeField] private SkeletonAnimation landingSfx;
    [SerializeField] private List<SkeletonAnimation> spineCoins;
    [SerializeField] private SpriteRenderer frontSky;

    private void Start()
    {
        startingChar.transform.position = startingCharPos[0].position;
        mainScreenChar.transform.position = mainScreenCharPos[0].position;
        table.transform.position = tablePos[0].position;
        betPanelUI.SetActive(false);
    }

    public IEnumerator GoToMainScreenIE()
    {
        mainScreenChar.gameObject.SetActive(false);
        btnOnStartScreen.ForEach(x => x.SetActive(false));
        float delay = 0.5f;

        normalAndBonusRoundBg.SetActive(true);

        logoImage.DOScale(Vector3.zero, delay).SetEase(Ease.Linear).OnComplete(() =>
        {
            titleScreenUI.SetActive(false);
        });

        ResetSkeletonAlpha(mainScreenChar);
        startingChar.gameObject.SetActive(true);
        startingChar.DOMove(startingCharPos[1].position, delay).SetEase(Ease.Linear);
        FadeSkeleton(startingChar);
        yield return new WaitForSeconds(1f);
        startingChar.gameObject.SetActive(false);

        delay = 0.3f;

        betPanelUI.SetActive(true);
        table.gameObject.SetActive(true);
        table.DOMove(tablePos[1].position, delay).SetEase(Ease.Linear);
        
        mainScreenChar.gameObject.SetActive(true);
        FadeSkeleton(mainScreenChar, 1, delay);
        mainScreenChar.DOMove(mainScreenCharPos[1].position, delay).SetEase(Ease.Linear);
        yield return new WaitForSeconds(delay);
    }


    public IEnumerator GoToTitleScreenIE()
    {
        betPanelUI.SetActive(false);
        mainScreenChar.DOMove(mainScreenCharPos[0].position, 0.5f).SetEase(Ease.Linear);
        FadeSkeleton(mainScreenChar);
        table.DOMove(tablePos[0].position, 0.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);
        mainScreenChar.gameObject.SetActive(false);
        table.gameObject.SetActive(false);

        startingChar.gameObject.SetActive(true);
        SkeletonAnimation skeleton = startingChar.GetComponent<SkeletonAnimation>();
        skeleton.AnimationState.Data.DefaultMix = 0.0f;
        skeleton.AnimationState.AddEmptyAnimation(0, 0, 0);
        skeleton.Skeleton.SetBonesToSetupPose();
        skeleton.Skeleton.A = 1;
        startingChar.position = startingCharPos[0].position;

        titleScreenUI.SetActive(true);
        logoImage.DOScale(Vector3.one, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            btnOnStartScreen.ForEach(x => x.SetActive(true));
        });
    }

    public IEnumerator GoToMegaBonusRoundIE()
    {
        AudioManager.Instance.ChangeMegaBonusBgm();
        mainGameCanvas.DOFade(0, 1f);
        mainScreenChar.DOMoveY(mainScreenChar.transform.position.y - 2f, 1f).SetEase(Ease.Linear);
        table.GetComponent<SpriteRenderer>().DOFade(0, 1);
        spineCoins.ForEach(x => FadeSkeleton(x.transform, 0, 1));

        yield return new WaitForSeconds(1f);

        megaBonusText.position = megaBonusTextPos[0].position;
        megaBonusText.localScale = megaBonusTextPos[0].localScale;
        megaBonusUIObjects.SetActive(false);

        mainScreenChar.DOMove(mainScreenCharPos[0].position, 0.5f).SetEase(Ease.Linear);
        //FadeSkeleton(mainScreenChar);
        table.gameObject.SetActive(false);

        frontSky.DOFade(1, 0.75f);
        yield return new WaitForSeconds(0.75f);
        mainScreenChar.gameObject.SetActive(false);
        normalAndBonusRoundBg.SetActive(false);
        normalAndBonusRoundObjects.SetActive(false);
        megaBonusRoundObjects.SetActive(true);
        megaBonusRoundBg.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        frontSky.DOFade(0, 0.5f);

        SkeletonAnimation skeleton = megaBonusChar.GetComponent<SkeletonAnimation>();
        skeleton.AnimationState.Data.DefaultMix = 0.0f;
        skeleton.AnimationState.AddEmptyAnimation(0, 0, 0);
        skeleton.Skeleton.SetBonesToSetupPose();
        yield return StartCoroutine(PlayMegaBonusCharacterAnimationIE());
        
        megaBonusText.DOMove(megaBonusTextPos[1].position, 0.5f).SetEase(Ease.Linear);
        megaBonusText.DOScale(megaBonusTextPos[1].localScale, 0.5f).SetEase(Ease.Linear);

        megaBonusUIObjects.SetActive(true);
    }

    public IEnumerator BackFromMegaBonusRoundIE()
    {
        AudioManager.Instance.ChangeToNormalBgm();
        frontSky.DOFade(1, 0.25f);
        yield return new WaitForSeconds(0.25f);
        megaBonusRoundBg.SetActive(false);
        normalAndBonusRoundBg.SetActive(true);
        ResetSkeletonAlpha(normalAndBonusRoundBg.transform);
        normalAndBonusRoundObjects.SetActive(true);
        megaBonusRoundObjects.SetActive(false);
        frontSky.DOFade(0, 0.5f);        

        //yield return new WaitForSeconds(0.5f);
        
        mainScreenChar.gameObject.SetActive(true);
        FadeSkeleton(mainScreenChar, 1);
        mainGameCanvas.DOFade(1, 0.5f);
        table.gameObject.SetActive(true);
        table.GetComponent<SpriteRenderer>().DOFade(1, 0.5f);
        spineCoins.ForEach(x => FadeSkeleton(x.transform, 1, 0.5f));

        SpineHelper.PlayAnimation(mainScreenChar.GetComponent<SkeletonAnimation>(), "idle", true);
        ResetSkeletonAlpha(mainScreenChar);
        mainScreenChar.DOMove(mainScreenCharPos[1].position, 0.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator PlayMegaBonusCharacterAnimationIE()
    {
        SkeletonAnimation anim = megaBonusChar.GetComponent<SkeletonAnimation>();
        float resetDelay = 0.1f;
        float delay = SpineHelper.GetAnimationDuration(anim, "Entrance") - resetDelay;
        float fxDelay = 0.75f;
        anim.skeleton.A = 0;
        SpineHelper.PlayAnimation(anim, "Entrance", false);
        yield return new WaitForSeconds(resetDelay);
        anim.skeleton.A = 1;

        yield return new WaitForSeconds(fxDelay);
        landingSfx.gameObject.SetActive(true);
        SpineHelper.PlayAnimation(landingSfx, "special fx", false);

        yield return new WaitForSeconds(delay - fxDelay);
        SpineHelper.PlayAnimation(anim, "pose 1-idle", true);
    }

    public void FadeSkeleton(Transform obj, float targetValue = 0, float delay = 0.5f)
    {
        SkeletonAnimation skeleton = obj.GetComponent<SkeletonAnimation>();
        if (targetValue == 1)
        {
            skeleton.Skeleton.A = 0;
        }
        DOTween.To(() => skeleton.Skeleton.A, x => skeleton.Skeleton.A = x, targetValue, delay);
    }

    public void ResetSkeletonAlpha(Transform obj)
    {
        SkeletonAnimation skeleton = obj.GetComponent<SkeletonAnimation>();
        skeleton.Skeleton.A = 1;
    }
}