using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] private CharacterData characterData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Direction defaultFacing = Direction.Front;

    private Direction currentFacing;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ResetFacing();
    }

    public void FacePlayer(Transform player)
    {
        if (player == null)
        {
            return;
        }

        Vector3 delta = player.position - transform.position;
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            FaceDirection(delta.x >= 0f ? Direction.Right : Direction.Left);
            return;
        }

        FaceDirection(delta.y >= 0f ? Direction.Back : Direction.Front);
    }

    public void FaceDirection(Direction direction)
    {
        currentFacing = direction;
        ApplyFacingSprite(direction);
    }

    public void ResetFacing()
    {
        FaceDirection(defaultFacing);
    }

    private void ApplyFacingSprite(Direction direction)
    {
        if (spriteRenderer == null || characterData == null)
        {
            return;
        }

        spriteRenderer.flipX = false;

        switch (direction)
        {
            case Direction.Back:
                SetSprite(characterData.backSprite);
                break;
            case Direction.Right:
                SetSprite(characterData.rightSprite);
                break;
            case Direction.Left:
                if (characterData.leftSprite != null)
                {
                    SetSprite(characterData.leftSprite);
                }
                else
                {
                    SetSprite(characterData.rightSprite);
                    spriteRenderer.flipX = characterData.rightSprite != null;
                }
                break;
            default:
                SetSprite(characterData.frontSprite);
                break;
        }
    }

    private void SetSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
