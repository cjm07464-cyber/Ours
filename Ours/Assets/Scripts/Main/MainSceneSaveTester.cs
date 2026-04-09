using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneSaveTester : MonoBehaviour
{
    public Transform playerTransform;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SaveNow();
        }
    }

    private void SaveNow()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager가 없습니다.");
            return;
        }

        GameManager.Instance.currentSceneName = SceneManager.GetActiveScene().name;

        if (playerTransform != null)
        {
            GameManager.Instance.playerPosition = playerTransform.position;
        }

        SaveSystem.SaveGame();
    }
}