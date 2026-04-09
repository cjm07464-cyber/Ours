using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    AudioSource audioSrc;

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
    }

    public void StopBGM()
    {
        audioSrc.Stop();
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
