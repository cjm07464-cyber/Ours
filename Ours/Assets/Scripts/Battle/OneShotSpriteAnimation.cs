using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OneShotSpriteAnimation : MonoBehaviour
{
    [Header("Frame Animation")]
    [SerializeField] private Image targetImage;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameDuration = 0.06f;

    [Header("Hit Sound")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private int hitSoundFrameIndex = 2;
    [SerializeField, Min(0f)] private float hitSoundVolumeScale = 1f;

    [Header("Lifetime")]
    [SerializeField] private bool destroyOnComplete = true;

    private bool hitFrameReached;
    private bool playbackComplete;

    private void OnEnable()
    {
        ResolveRenderer();
        StartCoroutine(PlayRoutine());
    }

    public IEnumerator WaitForHitFrame()
    {
        while (!hitFrameReached && !playbackComplete)
        {
            yield return null;
        }
    }

    public IEnumerator WaitForCompletion()
    {
        while (!playbackComplete)
        {
            yield return null;
        }
    }

    private void ResolveRenderer()
    {
        if (targetImage == null)
        {
            targetImage = GetComponentInChildren<Image>(true);
        }

        if (targetImage == null && targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
    }

    private IEnumerator PlayRoutine()
    {
        hitFrameReached = false;
        playbackComplete = false;

        if (frames == null || frames.Length == 0)
        {
            CompletePlayback();
            yield break;
        }

        int hitFrameIndex = Mathf.Clamp(hitSoundFrameIndex, 0, frames.Length - 1);
        float duration = Mathf.Max(0.01f, frameDuration);

        for (int i = 0; i < frames.Length; i++)
        {
            SetSprite(frames[i]);

            if (i == hitFrameIndex)
            {
                hitFrameReached = true;
                PlayHitSound();
            }

            yield return new WaitForSeconds(duration);
        }

        CompletePlayback();
    }

    private void SetSprite(Sprite sprite)
    {
        if (targetImage != null)
        {
            targetImage.sprite = sprite;
            targetImage.enabled = true;
            return;
        }

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = sprite;
            targetSpriteRenderer.enabled = true;
        }
    }

    private void PlayHitSound()
    {
        if (hitSound == null)
        {
            return;
        }

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(hitSound, hitSoundVolumeScale);
            return;
        }

        GameObject audioObject = new GameObject("OneShot VFX Audio");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(hitSound, Mathf.Max(0f, hitSoundVolumeScale));
        Destroy(audioObject, hitSound.length + 0.1f);
    }

    private void CompletePlayback()
    {
        hitFrameReached = true;
        playbackComplete = true;

        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
    }
}
