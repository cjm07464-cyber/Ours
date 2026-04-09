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

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        ApplyToGameManager(data);

        Debug.Log("불러오기 완료");
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
