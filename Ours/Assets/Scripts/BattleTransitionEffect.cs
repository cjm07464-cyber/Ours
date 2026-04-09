using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BattleTransitionEffect : MonoBehaviour
{
    public Sprite[] frames;          // Normal Battle 프레임들
    public float frameInterval = 0.05f;
    public string battleSceneName = "BattleScene";
    AudioSource audioSrc;

    SpriteRenderer sr;
    int index;
    float timer;
    bool isPlaying = false;
    bool animationFinished = false;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSrc = GetComponent<AudioSource>();
        Debug.Log("BattleTransitionEffect Awake / sr = " + sr);

        if (sr == null)
        {
            Debug.LogError("SpriteRenderer를 찾지 못했습니다!");
            return;
        }

        sr.enabled = false;
        isPlaying = false;
    }

    void OnEnable()
    {

    }

    void Update()
    {
        if (!isPlaying) return; // 🔥 재생 중 아닐 땐 아무 것도 안 함

        // 애니메이션 재생
        if (!animationFinished)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= frameInterval)
            {
                timer = 0f;
                index++;

                if (index >= frames.Length)
                {
                    animationFinished = true;
                    return; // 🔥 여기서 바로 씬 이동 ❌
                }

                sr.sprite = frames[index];
            }
        }
        else
        {
            // 🔥 애니메이션은 끝났고, 이제 사운드만 기다림
            if (audioSrc == null || !audioSrc.isPlaying)
            {
                FinishBattleTransition();
            }
        }
    }
    void FinishBattleTransition()
    {
        isPlaying = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(battleSceneName);
    }
    // 외부에서 호출용
    public void Play()
    {
        if (isPlaying) return;

        isPlaying = true;
        animationFinished = false;
        index = 0;
        timer = 0f;

        sr.sprite = frames[0];
        sr.enabled = true;
        // 🔥 마을 BGM 정지 (또는 페이드)
        if (BGMManager.Instance != null)
            BGMManager.Instance.StopBGM();
        // 또는 FadeOut();

        
        if (audioSrc != null)
            audioSrc.Play();

        Time.timeScale = 0f;
    }
}