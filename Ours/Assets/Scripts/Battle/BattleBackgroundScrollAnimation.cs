using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleBackgroundScrollAnimation : MonoBehaviour
{
    [Header("UI RawImage")]
    [SerializeField] private RawImage rawImage;

    [Header("0번부터 13번까지 넣기")]
    [SerializeField] private Sprite[] frames;

    [Header("프레임 교체 속도")]
    [SerializeField] private float frameDelay = 0.08f;

    [Header("반복 배율")]
    [SerializeField] private Vector2 repeat = new Vector2(3f, 2f);

    [Header("대각선 이동 속도")]
    [SerializeField] private Vector2 scrollSpeed = new Vector2(-0.05f, 0.05f);

    private Texture2D[] frameTextures;
    private int currentFrameIndex;
    private float frameTimer;
    private Vector2 offset;

    private void Reset()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Awake()
    {
        if (rawImage == null)
        {
            rawImage = GetComponent<RawImage>();
        }

        CreateTexturesFromSprites();

        if (frameTextures.Length > 0)
        {
            rawImage.texture = frameTextures[0];
        }
    }

    private void Update()
    {
        ScrollBackground();
        AnimateFrame();
    }

    private void ScrollBackground()
    {
        offset += scrollSpeed * Time.deltaTime;

        rawImage.uvRect = new Rect(
            offset.x,
            offset.y,
            repeat.x,
            repeat.y
        );
    }

    private void AnimateFrame()
    {
        if (frameTextures == null || frameTextures.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDelay)
        {
            frameTimer = 0f;

            currentFrameIndex++;

            if (currentFrameIndex >= frameTextures.Length)
            {
                currentFrameIndex = 0;
            }

            rawImage.texture = frameTextures[currentFrameIndex];
        }
    }

    private void CreateTexturesFromSprites()
    {
        if (frames == null || frames.Length == 0)
        {
            frameTextures = new Texture2D[0];
            return;
        }

        frameTextures = new Texture2D[frames.Length];

        for (int i = 0; i < frames.Length; i++)
        {
            Sprite sprite = frames[i];
            Rect rect = sprite.textureRect;

            Texture2D newTexture = new Texture2D(
                (int)rect.width,
                (int)rect.height,
                TextureFormat.RGBA32,
                false
            );

            Color[] pixels = sprite.texture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            );

            newTexture.SetPixels(pixels);
            newTexture.Apply();

            newTexture.wrapMode = TextureWrapMode.Repeat;
            newTexture.filterMode = FilterMode.Point;

            frameTextures[i] = newTexture;
        }
    }
}