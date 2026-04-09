using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkEffect : MonoBehaviour
{
    public float blinkSpeed = 2f;
    private Image image;
    private bool isBlinking = false;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void StartBlinking()
    {
        isBlinking = true;
    }

    public void StopBlinking()
    {
        isBlinking = false;
        var color = image.color;
        color.a = 1f;
        image.color = color;
    }

    private void Update()
    {
        if (!isBlinking || image == null) return;

        float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f; // 0~1
        var color = image.color;
        color.a = Mathf.Lerp(0.3f, 1f, alpha); // 깜빡임 범위 조절
        image.color = color;
    }
}