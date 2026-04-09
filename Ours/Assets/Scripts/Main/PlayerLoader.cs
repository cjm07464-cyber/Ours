using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoader : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
        {
            transform.position = GameManager.Instance.playerPosition;
        }
    }
}
