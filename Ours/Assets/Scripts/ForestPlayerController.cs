using UnityEngine;

public class ForestPlayerController : MonoBehaviour
{
    private enum FacingDirection
    {
        Down,
        Left,
        Right,
        Up
    }
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Sprites")]
    [SerializeField] private Sprite[] downSprites = new Sprite[4];
    [SerializeField] private Sprite[] leftSprites = new Sprite[4];
    [SerializeField] private Sprite[] rightSprites = new Sprite[4];
    [SerializeField] private Sprite[] upSprites = new Sprite[4];
    [SerializeField] private float animationInterval = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip footstepClipA;
    [SerializeField] private AudioClip footstepClipB;

    private static readonly int[] WalkFrameSequence = { 1, 2, 3, 2 };

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private Vector2 lastMovement = Vector2.right;
    private FacingDirection facingDirection = FacingDirection.Right;
    private float animationTimer;
    private int walkFrameIndex;
    private bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyIdleSprite();
    }

    private void Update()
    {
        ReadInput();
        UpdateFacingDirection();
        UpdateSpriteAnimation();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        Vector2 nextPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            movement = Vector2.zero;
            ApplyIdleSprite();
        }
    }

    private void ReadInput()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
            return;
        }

        movement = GameInput.MovementVector;

        if (movement != Vector2.zero)
        {
            lastMovement = movement;
        }
    }

    private void UpdateFacingDirection()
    {
        Vector2 direction = movement != Vector2.zero ? movement : lastMovement;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            facingDirection = direction.x < 0f ? FacingDirection.Left : FacingDirection.Right;
        }
        else if (direction.y > 0f)
        {
            facingDirection = FacingDirection.Up;
        }
        else
        {
            facingDirection = FacingDirection.Down;
        }
    }

    private void UpdateSpriteAnimation()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (movement == Vector2.zero)
        {
            animationTimer = 0f;
            walkFrameIndex = 0;
            ApplyIdleSprite();
            return;
        }

        animationTimer += Time.deltaTime;
        if (animationTimer >= animationInterval)
        {
            animationTimer = 0f;
            walkFrameIndex = (walkFrameIndex + 1) % WalkFrameSequence.Length;
            PlayFootstepIfNeeded(WalkFrameSequence[walkFrameIndex]);
        }

        ApplySprite(GetCurrentSpriteArray(), WalkFrameSequence[walkFrameIndex]);
    }

    private void ApplyIdleSprite()
    {
        ApplySprite(GetCurrentSpriteArray(), 0);
    }

    private void PlayFootstepIfNeeded(int frame)
    {
        if (footstepAudioSource == null)
        {
            return;
        }

        AudioClip clip = null;
        if (frame == 1)
        {
            clip = footstepClipA;
        }
        else if (frame == 3)
        {
            clip = footstepClipB;
        }

        if (clip != null)
        {
            footstepAudioSource.PlayOneShot(clip);
        }
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
}
