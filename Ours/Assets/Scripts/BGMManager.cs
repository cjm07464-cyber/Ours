using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    AudioSource audioSrc;
    Tween resumeFadeTween;
    float volumeBeforePause = 1f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSrc = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);

        if (GameManager.Instance != null && GameManager.Instance.HasTownOpeningRequest())
        {
            StopBGM();
        }
    }

    public void PlayBGM()
    {
        if (audioSrc != null && !audioSrc.isPlaying)
        {
            audioSrc.Play();
        }
    }

    public void StopBGM()
    {
        if (audioSrc != null)
        {
            audioSrc.Stop();
        }
    }

    public void PauseBGM()
    {
        if (audioSrc != null && audioSrc.isPlaying)
        {
            resumeFadeTween?.Kill();
            volumeBeforePause = audioSrc.volume;
            audioSrc.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (audioSrc != null)
        {
            audioSrc.UnPause();
        }
    }

    public void ResumeBGMWithFade(float duration)
    {
        if (audioSrc == null)
        {
            return;
        }

        resumeFadeTween?.Kill();

        float targetVolume = Mathf.Max(0f, volumeBeforePause);

        audioSrc.volume = 0f;
        audioSrc.UnPause();

        float safeDuration = Mathf.Max(0f, duration);
        if (safeDuration <= 0f)
        {
            audioSrc.volume = targetVolume;
            return;
        }

        resumeFadeTween = audioSrc
            .DOFade(targetVolume, safeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => resumeFadeTween = null);
    }
    public void StopAndDestroy()
    {
        if (audioSrc != null)
        {
            audioSrc.Stop();
        }

        if (Instance == this)
        {
            Instance = null;
        }

        Destroy(gameObject);
    }

    public void FadeOut(float speed = 1.5f)
    {
        StartCoroutine(FadeOutRoutine(speed));
    }

    IEnumerator FadeOutRoutine(float speed)
    {
        while (audioSrc.volume > 0)
        {
            audioSrc.volume -= Time.unscaledDeltaTime * speed;
            yield return null;
        }
        audioSrc.Stop();
    }



}
