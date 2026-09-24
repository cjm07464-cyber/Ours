using UnityEngine;

public class ForestBacktrackBlocker : MonoBehaviour
{
    [SerializeField] private ForestPlayerController playerController;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private string message = "일단 앞으로 나아가보자.";

    private bool dialogueActive;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryShowDialogue(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryShowDialogue(collision.collider);
    }

    private void TryShowDialogue(Collider2D other)
    {
        if (dialogueActive || other == null || !other.CompareTag("Player"))
        {
            return;
        }

        if (dialogueController != null && dialogueController.IsOpen)
        {
            return;
        }

        dialogueActive = true;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        if (dialogueController == null)
        {
            UnlockPlayer();
            return;
        }

        dialogueController.Show(message, UnlockPlayer);
    }

    private void UnlockPlayer()
    {
        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }

        dialogueActive = false;
    }
}
