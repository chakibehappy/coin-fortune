using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using System.Collections;
using Spine;

public class MegaBonusOrbCollider : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation glowingEffectAnim;
    [SerializeField] private SkeletonAnimation[] coinCapturedAnim;
    [SerializeField] private Vector3 punchScaleAmount = new Vector3(0.025f, 0.025f, 0.025f);
    [SerializeField] private TextMeshPro multiplierText;

    Vector3 originalScale;
    int coinNumber = 0;

    IEnumerator glowingEffectCoroutine;
    [SerializeField] private string[] effectAnimationName = new string[] { "", "" };
    [SerializeField] private string[] coinAnimationName = new string[] { "", "" };

    [SerializeField] private string[] orbAnimationName = new string[] { "start", "idle", "dissapear" };
    SkeletonAnimation orb;

    private void Start()
    {
        orb = GetComponent<SkeletonAnimation>();
        originalScale = transform.localScale;
    }

    public void ActivateOrb(MegaBonusPose pose, int i = 0)
    {
        for (int x = 0; x < coinCapturedAnim.Length; x++)
        {
            coinCapturedAnim[x].gameObject.SetActive(false);
        }
        coinNumber = 0;
        multiplierText.text = "x" + pose.activeOrb[i].multiplication.ToString();
        orb = GetComponent<SkeletonAnimation>();
        GetComponent<Collider2D>().enabled = true;
        StartCoroutine(PlayStartOrbAnimationIE());
    }

    public void ShowText(MegaBonusPose pose, int i = 0)
    {
        multiplierText.text = "x" + pose.activeOrb[i].multiplication.ToString();
        multiplierText.gameObject.SetActive(true);
    }

    IEnumerator PlayStartOrbAnimationIE()
    {
        SpineHelper.PlayAnimation(orb, orbAnimationName[0], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(orb, orbAnimationName[0]));
        SpineHelper.PlayAnimation(orb, orbAnimationName[1], true);
        multiplierText.gameObject.SetActive(true);
    }

    IEnumerator PlayEndOrbAnimationIE()
    {
        GetComponent<Collider2D>().enabled = false;
        SpineHelper.PlayAnimation(orb, orbAnimationName[2], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(orb, orbAnimationName[2]));
        gameObject.SetActive(false);
    }

    public void KillOrb()
    {
        StartCoroutine(PlayEndOrbAnimationIE());
        ResetCoinCaptured();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            AudioManager.Instance.PlaySfx(8, 9);
            StartCoroutine(PlayCoinAbsorbedAnimationIE());
            collision.GetComponent<Collider2D>().enabled = false;
            collision.gameObject.SetActive(false);
        }
    }

    IEnumerator PlayCoinAbsorbedAnimationIE()
    {
        ShowGlowingEffect();
        PunchScaleObject();
        coinNumber++;
        if(coinNumber > coinCapturedAnim.Length)
        {
            PlayCoinSpinAnimation();
        }
        else
        {
            ShowCoinCaptured(coinNumber - 1);
        }
        yield return null;
    }

    void ShowGlowingEffect()
    {
        if(glowingEffectCoroutine != null)
        {
            StopCoroutine(glowingEffectCoroutine);
            glowingEffectCoroutine = null;
        }
        glowingEffectCoroutine = ShowGlowingEffectIE();
        StartCoroutine(glowingEffectCoroutine);
    }
    IEnumerator ShowGlowingEffectIE()
    {
        glowingEffectAnim.gameObject.SetActive(true);
        SpineHelper.PlayAnimation(glowingEffectAnim, effectAnimationName[0], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(glowingEffectAnim, effectAnimationName[0]));
        glowingEffectAnim.gameObject.SetActive(false);
    }

    void ShowCoinCaptured(int index)
    {
        coinCapturedAnim[index].gameObject.SetActive(true);
        FadeSkeleton(coinCapturedAnim[index], 1);
        SpineHelper.PlayAnimation(coinCapturedAnim[index], coinAnimationName[0], true);
    }

    void HideCoinCaptured(int index)
    {
        FadeSkeleton(coinCapturedAnim[index]);
    }

    void ResetCoinCaptured()
    {
        for (int i = 0; i < coinCapturedAnim.Length; i++)
        {
            HideCoinCaptured(i);
        }
    }

    void PlayCoinSpinAnimation()
    {
        for (int i = 0; i < coinCapturedAnim.Length; i++)
        {
            SpineHelper.PlayAnimation(coinCapturedAnim[i], coinAnimationName[1], true);
        }
    }

    void PunchScaleObject()
    {
        transform.DOPunchScale(punchScaleAmount, 0.25f, 1, 1).OnComplete(() => {
            transform.localScale = originalScale;
        });
    }

    public void FadeSkeleton(SkeletonAnimation skeleton, float targetValue = 0, float delay = 0.5f)
    {
        if (targetValue == 1)
        {
            skeleton.Skeleton.A = 0;
        }
        DOTween.To(() => skeleton.Skeleton.A, x => skeleton.Skeleton.A = x, targetValue, delay).OnComplete(() =>
        {
            skeleton.gameObject.SetActive(targetValue == 1);
        });
    }
}
