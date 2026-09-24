using UnityEngine;

public class StoryFlagGate : MonoBehaviour
{
    [Header("Condition")]
    [SerializeField] private string requiredFlagId;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameEventRunner eventRunner;
    [SerializeField] private GameEventSequence blockedEventSequence;
    [SerializeField] private Transform blockedReturnPoint;
    [SerializeField] private Collider2D blockingCollider;

    [Header("Options")]
    [SerializeField] private bool requireFlag = true;
    [SerializeField] private float returnMoveSpeed = 2.5f;

    private bool isBlocking;

    private void Start()
    {
        RefreshBlockingCollider();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isBlocking || !other.CompareTag("Player"))
        {
            return;
        }

        RefreshBlockingCollider();

        if (IsConditionMet())
        {
            return;
        }

        if (playerController == null)
        {
            playerController = other.GetComponent<PlayerController>();
        }

        StartBlockEvent();
    }

    private bool IsConditionMet()
    {
        if (string.IsNullOrWhiteSpace(requiredFlagId))
        {
            return true;
        }

        bool hasFlag = GameManager.Instance != null &&
                       GameManager.Instance.HasStoryFlag(requiredFlagId);

        return requireFlag ? hasFlag : !hasFlag;
    }

    private void StartBlockEvent()
    {
        isBlocking = true;
        SetPlayerCanMove(false);

        if (eventRunner != null && blockedEventSequence != null)
        {
            eventRunner.Run(blockedEventSequence, FinishBlockEvent);
            return;
        }

        FinishBlockEvent();
    }

    private void FinishBlockEvent()
    {
        RefreshBlockingCollider();

        if (playerController != null && blockedReturnPoint != null)
        {
            SetPlayerCanMove(false);
            playerController.AutoMoveTo(blockedReturnPoint.position, returnMoveSpeed, FinishReturnMove);
            return;
        }

        FinishReturnMove();
    }

    private void FinishReturnMove()
    {
        SetPlayerCanMove(true);
        isBlocking = false;
    }

    private void RefreshBlockingCollider()
    {
        if (blockingCollider != null)
        {
            blockingCollider.enabled = !IsConditionMet();
        }
    }

    private void SetPlayerCanMove(bool canMove)
    {
        if (playerController != null)
        {
            playerController.SetCanMove(canMove);
        }
    }

}
