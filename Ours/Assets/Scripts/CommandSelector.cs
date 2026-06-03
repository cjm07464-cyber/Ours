using UnityEngine;

public class CommandSelector : MonoBehaviour
{
    public RectTransform selector;         // 커서 오브젝트
    public RectTransform[] options;        // Option1~6
    public BattleManager battleManager;

    private int currentIndex = 0;
    private int columnCount = 3; // 2행 3열 구조

    void Start()
    {
        MoveSelectorTo(currentIndex);
    }

    void OnEnable()
    {
        currentIndex = 0;
        MoveSelectorTo(currentIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if ((currentIndex + 1) % columnCount != 0)
            {
                currentIndex++;
            }

            MoveSelectorTo(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentIndex % columnCount != 0)
            {
                currentIndex--;
            }

            MoveSelectorTo(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int nextIndex = currentIndex + columnCount;
            if (nextIndex < options.Length)
            {
                currentIndex = nextIndex;
            }

            MoveSelectorTo(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int nextIndex = currentIndex - columnCount;
            if (nextIndex >= 0)
            {
                currentIndex = nextIndex;
            }

            MoveSelectorTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ExecuteCurrentCommand();
        }
    }

    private void ExecuteCurrentCommand()
    {
        if (battleManager == null)
        {
            Debug.LogWarning("CommandSelector: BattleManager가 연결되지 않았습니다.");
            return;
        }

        switch (currentIndex)
        {
            case 0: // 공격
                battleManager.OnAttackCommand();
                break;

            case 1: // 방어
                Debug.Log("방어는 아직 구현되지 않았습니다.");
                break;

            case 2: // Special
                Debug.Log("Special은 아직 구현되지 않았습니다.");
                break;

            case 3: // 스킬 = PK회복
                battleManager.OnHealCommand();
                break;

            case 4: // 아이템
                Debug.Log("아이템은 아직 구현되지 않았습니다.");
                break;

            case 5: // 도망
                battleManager.OnEscapeCommand();
                break;
        }
    }

    private void MoveSelectorTo(int index)
    {
        if (selector == null || options == null || options.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= options.Length || options[index] == null)
        {
            return;
        }

        Vector3 basePos = options[index].position;
        selector.position = new Vector3(basePos.x - 90f, basePos.y, basePos.z);
    }
}