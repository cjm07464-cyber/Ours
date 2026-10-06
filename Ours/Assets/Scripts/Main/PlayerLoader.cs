using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLoader : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.TryConsumeBattleReturnRequest(
                SceneManager.GetActiveScene().name,
                out Vector2 battleReturnPosition,
                out Vector2 battleReturnFacing))
        {
            RestorePlayerState(battleReturnPosition, battleReturnFacing);
            return;
        }

        if (!GameManager.Instance.ConsumeSavedPlayerRestoreRequest())
        {
            return;
        }

        RestorePlayerState(
            GameManager.Instance.playerPosition,
            GameManager.Instance.playerFacingDirection);
    }

    private void RestorePlayerState(Vector2 position, Vector2 facingDirection)
    {
        transform.position = position;

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetCanMove(true);
            playerController.ApplyFacingDirection(facingDirection);
        }
    }
}
