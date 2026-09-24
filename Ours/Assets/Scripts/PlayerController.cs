using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum FacingDirection
    {
        Down,
        Left,
        Right,
        Up
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] upSprites = new Sprite[4];
    [SerializeField] private Sprite[] downSprites = new Sprite[4];
    [SerializeField] private Sprite[] leftSprites = new Sprite[4];
    [SerializeField] private Sprite[] rightSprites = new Sprite[4];
    [SerializeField] private float animationInterval = 0.09f;

    private static readonly int[] WalkFrameSequence = { 1, 2, 3, 2 };

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private FacingDirection facingDirection = FacingDirection.Right;
    private float animationTimer;
    private int walkFrameIndex;
    private bool canMove = true;
    private Vector2 autoMoveTarget;
    private float autoMoveSpeed;
    private Action autoMoveComplete;

    public bool IsAutoMoving { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        ApplyIdleSprite();
    }

    private void Update()
    {
        ReadInput();
        UpdateSpriteAnimation();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (IsAutoMoving)
        {
            UpdateAutoMove();
            return;
        }

        Vector2 nextPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            IsAutoMoving = false;
            autoMoveComplete = null;
            moveInput = Vector2.zero;
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            ResetWalkAnimation();
            ApplyIdleSprite();
        }
    }

    public void ApplyFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        facingDirection = GetFacingDirection(direction);
        moveInput = Vector2.zero;
        ResetWalkAnimation();
        ApplyIdleSprite();
        UpdateGameManagerFacing();
    }

    public Vector2 GetDirection()
    {
        return moveInput;
    }

    public void AutoMoveTo(Vector2 targetPosition, float speed, Action onComplete = null)
    {
        if (rb == null)
        {
            onComplete?.Invoke();
            return;
        }

        autoMoveTarget = targetPosition;
        autoMoveSpeed = Mathf.Max(0.01f, speed);
        autoMoveComplete = onComplete;
        IsAutoMoving = true;
    }

    private void ReadInput()
    {
        if (Time.timeScale == 0f || IsAutoMoving || !canMove)
        {
            if (!IsAutoMoving)
            {
                moveInput = Vector2.zero;
            }

            return;
        }

        moveInput = GameInput.MovementVector;

        if (moveInput != Vector2.zero)
        {
            facingDirection = GetFacingDirection(moveInput);
            UpdateGameManagerFacing();
        }
    }

    private void UpdateAutoMove()
    {
        Vector2 currentPosition = rb.position;
        Vector2 toTarget = autoMoveTarget - currentPosition;

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            FinishAutoMove();
            return;
        }

        Vector2 direction = toTarget.normalized;
        moveInput = direction;
        facingDirection = GetFacingDirection(direction);
        UpdateGameManagerFacing();

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            autoMoveTarget,
            autoMoveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);

        if ((autoMoveTarget - nextPosition).sqrMagnitude <= 0.0001f)
        {
            FinishAutoMove();
        }
    }

    private void FinishAutoMove()
    {
        IsAutoMoving = false;
        moveInput = Vector2.zero;
        ResetWalkAnimation();
        ApplyIdleSprite();

        Action callback = autoMoveComplete;
        autoMoveComplete = null;
        callback?.Invoke();
    }

    private FacingDirection GetFacingDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            return direction.x < 0f ? FacingDirection.Left : FacingDirection.Right;
        }

        return direction.y < 0f ? FacingDirection.Down : FacingDirection.Up;
    }

    private void UpdateSpriteAnimation()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (moveInput == Vector2.zero)
        {
            ResetWalkAnimation();
            ApplyIdleSprite();
            return;
        }

        animationTimer += Time.deltaTime;
        if (animationTimer >= animationInterval)
        {
            animationTimer = 0f;
            walkFrameIndex = (walkFrameIndex + 1) % WalkFrameSequence.Length;
        }

        ApplySprite(GetCurrentSpriteArray(), WalkFrameSequence[walkFrameIndex]);
    }

    private void ResetWalkAnimation()
    {
        animationTimer = 0f;
        walkFrameIndex = 0;
    }

    private void ApplyIdleSprite()
    {
        ApplySprite(GetCurrentSpriteArray(), 0);
    }

    private Sprite[] GetCurrentSpriteArray()
    {
        switch (facingDirection)
        {
            case FacingDirection.Left:
                return leftSprites;
            case FacingDirection.Right:
                return rightSprites;
            case FacingDirection.Up:
                return upSprites;
            default:
                return downSprites;
        }
    }

    private void ApplySprite(Sprite[] sprites, int preferredIndex)
    {
        if (spriteRenderer == null || sprites == null || sprites.Length == 0)
        {
            return;
        }

        int index = Mathf.Clamp(preferredIndex, 0, sprites.Length - 1);
        Sprite sprite = sprites[index];

        if (sprite == null)
        {
            sprite = FindFallbackSprite(sprites);
        }

        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private Sprite FindFallbackSprite(Sprite[] sprites)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                return sprites[i];
            }
        }

        return null;
    }

    private void UpdateGameManagerFacing()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        switch (facingDirection)
        {
            case FacingDirection.Left:
                GameManager.Instance.playerFacingDirection = Vector2.left;
                break;
            case FacingDirection.Right:
                GameManager.Instance.playerFacingDirection = Vector2.right;
                break;
            case FacingDirection.Up:
                GameManager.Instance.playerFacingDirection = Vector2.up;
                break;
            default:
                GameManager.Instance.playerFacingDirection = Vector2.down;
                break;
        }
    }
}
