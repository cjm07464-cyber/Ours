using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/savefile.json";

    public static bool HasSaveData()
    {
        return File.Exists(savePath);
    }

    public static bool HasValidSaveData()
    {
        if (!HasSaveData())
        {
            return false;
        }

        if (!TryReadSaveData(out SaveData data))
        {
            return false;
        }

        return IsValidSaveData(data);
    }

    public static void SaveGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager가 없어서 저장할 수 없습니다.");
            return;
        }

        SaveData data = GameManager.Instance.GetSaveData();
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        Debug.Log("저장 완료: " + savePath);
    }
    public static void LoadGame()
    {
        if (!HasSaveData())
        {
            Debug.LogWarning("세이브 파일이 없습니다.");
            return;
        }

        if (!TryReadSaveData(out SaveData data))
        {
            return;
        }

        if (!IsValidSaveData(data))
        {
            Debug.LogWarning("세이브 데이터가 올바르지 않아 불러오기를 중단합니다.");
            return;
        }

        data.currentSceneName = GameManager.NormalizeSceneName(data.currentSceneName);

        ApplyToGameManager(data);

        Debug.Log("불러오기 완료");
    }

    private static bool TryReadSaveData(out SaveData data)
    {
        data = null;

        try
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"세이브 파일을 읽거나 파싱할 수 없습니다: {exception.Message}");
            return false;
        }

        if (data == null)
        {
            Debug.LogWarning("세이브 데이터가 비어 있습니다.");
            return false;
        }

        return true;
    }

    private static bool IsValidSaveData(SaveData data)
    {
        if (data == null)
        {
            return false;
        }

        if (data.level <= 0 || data.maxHP <= 0 || data.currentHP < 0 || data.maxMP < 0 || data.currentMP < 0)
        {
            return false;
        }

        return true;
    }

    private static void ApplyToGameManager(SaveData data)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager 없음");
            return;
        }

        GameManager.Instance.LoadFromSaveData(data);
    }
    public static void DeleteSaveData()
    {
        if (HasSaveData())
        {
            File.Delete(savePath);
            Debug.Log("세이브 파일 삭제 완료");
        }
        else
        {
            Debug.LogWarning("삭제할 세이브 파일이 없습니다.");
        }
    }
}
