using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandSelector : MonoBehaviour
{
    public RectTransform selector;         // 커서 오브젝트 (Image)
    public RectTransform[] options;        // 선택지들 (Option1~6의 RectTransform)
    public int selectedCommandIndex = 0;
    private int currentIndex = 0;
    private int columnCount = 3; // 한 줄에 3개 (2행 3열 구조)
    public BattleManager battleManager;
    public GameObject commandPanel;
    void Start()
    {
        MoveSelectorTo(currentIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 오른쪽으로 이동
            if ((currentIndex + 1) % columnCount != 0)
                currentIndex = (currentIndex + 1) % options.Length;

            MoveSelectorTo(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // 왼쪽으로 이동
            if (currentIndex % columnCount != 0)
                currentIndex = (currentIndex - 1 + options.Length) % options.Length;

            MoveSelectorTo(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // 아래로 이동
            int nextIndex = currentIndex + columnCount;
            if (nextIndex < options.Length)
                currentIndex = nextIndex;

            MoveSelectorTo(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // 위로 이동
            int nextIndex = currentIndex - columnCount;
            if (nextIndex >= 0)
                currentIndex = nextIndex;

            MoveSelectorTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentIndex == 0 || currentIndex == 3) // 공격 or 스킬
            {
                commandPanel.SetActive(false);
                this.enabled = false;

                // 대상 선택 시작
                battleManager.StartCoroutine(battleManager.SelectEnemyTarget());
            }
            else
            {
                Debug.Log("이 커맨드는 아직 구현되지 않았거나 대상 선택이 필요하지 않음");
            }
        }

    }

    void MoveSelectorTo(int index)
    {
        Vector3 basePos = options[index].position;
        selector.position = new Vector3(basePos.x - 90f, basePos.y, basePos.z); // ← 살짝 더 왼쪽으로 띄움
    }


}