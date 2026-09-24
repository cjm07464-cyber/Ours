using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private NPCController npcController;
    [SerializeField] private GameEventRunner eventRunner;
    [SerializeField] private GameEventSequence eventSequence;

    [Header("After Flag")]
    [SerializeField] private string alternateFlagId;
    [SerializeField] private GameEventSequence alternateEventSequence;

    private Transform playerInRange;
    private bool isInteracting;

    private void Awake()
    {
        if (npcController == null)
        {
            npcController = GetComponent<NPCController>();
        }

        if (eventRunner == null)
        {
            eventRunner = GetComponent<GameEventRunner>();
        }
    }

    private void Update()
    {
        if (playerInRange == null || isInteracting)
        {
            return;
        }

        if (!GameInput.ConfirmPressed)
        {
            return;
        }

        StartInteraction();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform == playerInRange)
        {
            playerInRange = null;
        }
    }

    private void StartInteraction()
    {
        GameEventSequence selectedSequence = GetSelectedEventSequence();
        if (eventRunner == null || selectedSequence == null)
        {
            return;
        }

        isInteracting = true;
        if (eventRunner != null && playerInRange != null)
        {
            eventRunner.SetPlayerController(playerInRange.GetComponent<PlayerController>());
        }

        if (npcController != null)
        {
            npcController.FacePlayer(playerInRange);
        }

        eventRunner.Run(selectedSequence, FinishInteraction);
    }

    private GameEventSequence GetSelectedEventSequence()
    {
        if (!string.IsNullOrWhiteSpace(alternateFlagId) &&
            alternateEventSequence != null &&
            GameManager.Instance != null &&
            GameManager.Instance.HasStoryFlag(alternateFlagId))
        {
            return alternateEventSequence;
        }

        return eventSequence;
    }

    private void FinishInteraction()
    {
        if (npcController != null)
        {
            npcController.ResetFacing();
        }

        isInteracting = false;
    }
}
