using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneSaveTester : MonoBehaviour
{
    public Transform playerTransform;

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (GameInput.DebugZeroPressed)
        {
            DeleteAllSaveData();
        }
#endif
    }

    private void DeleteAllSaveData()
    {
        SaveSystem.DeleteSaveData(true);
    }
}
