using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Sprites")]
    public Sprite downA; // 1
    public Sprite downB; // 2
    public Sprite up;    // 3 (flip)
    public Sprite leftA; // 4
    public Sprite leftB; // 5
    public Sprite diagDownA; // 6
    public Sprite diagDownB; // 7
    public Sprite diagUpA;   // 8
    public Sprite diagUpB;   // 9

    Vector2 inputDir;
    SpriteRenderer sr;

    float animTimer;
    bool animToggle;
    public float animInterval = 0.2f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        ReadInput();
        Move();
        Animate();
    }

    void ReadInput()
    {
        inputDir = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void Move()
    {
        transform.position += (Vector3)inputDir * moveSpeed * Time.deltaTime;
    }

    void Animate()
    {
        if (inputDir == Vector2.zero)
            return;

        animTimer += Time.deltaTime;
        if (animTimer >= animInterval)
        {
            animTimer = 0f;
            animToggle = !animToggle;
        }

        UpdateSprite();
    }

    void UpdateSprite()
    {
        float x = inputDir.x;
        float y = inputDir.y;

        // 대각선 우선
        if (Mathf.Abs(x) > 0.1f && Mathf.Abs(y) > 0.1f)
        {
            if (y < 0)
                sr.sprite = animToggle ? diagDownA : diagDownB;
            else
                sr.sprite = animToggle ? diagUpA : diagUpB;

            sr.flipX = x > 0;
        }
        // 좌우
        else if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            sr.sprite = animToggle ? leftA : leftB;
            sr.flipX = x > 0;
        }
        // 상하
        else
        {
            if (y < 0)
            {
                sr.sprite = animToggle ? downA : downB;
                sr.flipX = false;
            }
            else
            {
                sr.sprite = up;
                sr.flipX = animToggle;
            }
        }
    }

    // 🔹 Enemy가 추적할 때 사용할 정보
    public Vector2 GetDirection()
    {
        return inputDir;
    }
}
