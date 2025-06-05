using System.Collections;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    private bool enableBgm;
    [SerializeField] private AudioSource Bgm;
    [SerializeField] private AudioSource BgmBonus;
    [SerializeField] private AudioSource BgmMegaBonus;
    [SerializeField] private AudioClip[] bgmClip;
    [SerializeField] private float maxBgmVol = 0.2f;
    [SerializeField] private float bgmVolOnSfx = 0.1f;

    private bool enableSfx;
    [SerializeField] private AudioSource Sfx;
    [SerializeField] private AudioClip[] sfxClip;
    [SerializeField] private AudioClip menuClickSfxClip;

    [SerializeField] private float delayTransition = 1f;

    public static AudioManager Instance { get; private set; }
    IEnumerator changeBgmOnSfxCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            Bgm.loop = true;
            BgmBonus.loop = true;
            BgmMegaBonus.loop = true;
            BgmBonus.volume = 0;
            BgmMegaBonus.volume = 0;
            Sfx.loop = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnableBgm(bool isEnable = true)
    {
        enableBgm = isEnable;
        Bgm.volume = isEnable ? maxBgmVol : 0.0f;
        if (!Bgm.isPlaying)
            PlayBgm();
    }

    public bool IsBgmActive()
    {
        return enableBgm;
    }

    public void EnableSfx(bool isEnable = true)
    {
        enableSfx = isEnable;
    }

    public bool IsSfxActive()
    {
        return enableSfx;
    }

    public void SaveAudioSetting(Sounds soundSetting)
    {
        EnableBgm(soundSetting.music);
        EnableSfx(soundSetting.effect);
    }

    public void PlayClickSfx()
    {
        if (IsSfxActive())
        {
            Sfx.PlayOneShot(menuClickSfxClip);
            ChangeBgmVolumeOnSfxPlay(menuClickSfxClip);
        }
    }

    public void PlaySfx(int clipIndex)
    {
        if (IsSfxActive())
        {
            Sfx.PlayOneShot(sfxClip[clipIndex]);
            ChangeBgmVolumeOnSfxPlay(sfxClip[clipIndex]);
        }
    }

    public void PlaySfx(int startClipIndex, int endClipIndex)
    {
        if (IsSfxActive())
        {
            int clipIndex = Random.Range(startClipIndex, endClipIndex + 1);
            Sfx.PlayOneShot(sfxClip[clipIndex]);
            StartChangeBgmOnSfxPlay(sfxClip[clipIndex]);
        }
    }

    void StartChangeBgmOnSfxPlay(AudioClip clip)
    {
        StopChangeBgmOnSfxPlay();
        changeBgmOnSfxCoroutine = ChangeBgmVolumeOnSfxPlay(clip);
        StartCoroutine(changeBgmOnSfxCoroutine);
    }

    void StopChangeBgmOnSfxPlay()
    {
        if(changeBgmOnSfxCoroutine != null)
        {
            StopCoroutine(changeBgmOnSfxCoroutine);
        }
        changeBgmOnSfxCoroutine = null;
        ChangeAllBgmValue(maxBgmVol);
    }

    IEnumerator ChangeBgmVolumeOnSfxPlay(AudioClip clip)
    {
        if (enableBgm)
        {
            ChangeAllBgmValue(bgmVolOnSfx);
            yield return new WaitForSeconds(clip.length);
            ChangeAllBgmValue(maxBgmVol);
        }
    }

    void ChangeAllBgmValue(float vol)
    {
        Bgm.volume = Bgm.volume > 0 ? vol : 0;
        BgmBonus.volume = BgmBonus.volume > 0 ? vol : 0;
        BgmBonus.volume = BgmBonus.volume > 0 ? vol : 0;
    }

    public void PlayBgm()
    {
        Bgm.clip = bgmClip[0];
        BgmBonus.clip = bgmClip[1];
        BgmMegaBonus.clip = bgmClip[2];
        Bgm.Play();
        BgmBonus.Play();
        BgmMegaBonus.Play();
    }

    public void ChangeToBonusBgm()
    {
        if (enableBgm)
        {
            StopChangeBgmOnSfxPlay();
            Bgm.DOFade(0, delayTransition);
            BgmBonus.DOFade(maxBgmVol, delayTransition);
            BgmMegaBonus.DOFade(0, delayTransition);
        }
    }

    public void ChangeMegaBonusBgm()
    {
        if (enableBgm)
        {
            StopChangeBgmOnSfxPlay();
            Bgm.DOFade(0, delayTransition);
            BgmBonus.DOFade(0, delayTransition);
            BgmMegaBonus.DOFade(maxBgmVol, delayTransition);
        }
    }

    public void ChangeToNormalBgm()
    {
        if (enableBgm)
        {
            StopChangeBgmOnSfxPlay();
            Bgm.DOFade(maxBgmVol, delayTransition);
            BgmBonus.DOFade(0, delayTransition);
            BgmMegaBonus.DOFade(0, delayTransition);
        }
    }
}
