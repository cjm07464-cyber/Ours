# SAVE_AND_STATE

GameManager, SaveSystem, 런타임 세션과 저장 파일의 경계를 정리한다.

## 1. 핵심 파일

- `Assets/Scripts/Title/GameManager.cs`
- `Assets/Scripts/Title/SaveData.cs`
- `Assets/Scripts/Title/SaveSystem.cs`
- `Assets/Scripts/Main/PlayerLoader.cs`
- 저장을 호출하는 메뉴 코드

## 2. GameManager

싱글톤 + DontDestroyOnLoad.

Runtime Bootstrap이 존재해 씬 직접 Play에서도 자동 생성될 수 있다.

중복 GameManager 오브젝트가 Scene에 있으면 Awake에서 중복 제거되는 구조를 유지한다.

### 씬 상수

현재 기준:

- `TitleSceneName = "TitleScene"`
- `ForestSceneName = "ForestScene"`
- `TownSceneName = "TownScene"`

레거시 씬명은 Normalize 과정에서 현재 명칭으로 매핑할 수 있다.

## 3. StartupSessionState

첫 실행 프롤로그를 제어하는 런타임 상태.

```text
None
ForestCompleted
NameChosen
```

관련:

- `pendingPlayerName`
- `MarkForestCompleted()`
- `SetPendingPlayerName(name)`
- `ClearStartupSession()`

이 상태는 저장 파일과 동일한 것이 아니다.

앱을 종료하면 초기화될 수 있으며, 유효 저장이 없다면 다음 실행에 Forest를 다시 보는 것이 의도된 흐름이다.

## 4. 새 게임

현재 New Game은 pending name 또는 기존 이름 입력 결과를 사용해 GameManager를 초기화한다.

현 시점 StartNewGame의 목적 씬은 `TownScene`이다.

향후 집/침실 기상 오프닝을 추가하면 이 시작 위치/씬은 별도 설계 후 변경한다.

## 5. 저장 유효성

단순 `File.Exists`만으로 Continue 가능 여부를 판단하지 않는다.

`SaveSystem.HasValidSaveData()`는 다음 종류의 실패를 false로 처리한다.

- 파일 없음
- 읽기 실패
- JSON parse 실패
- null/명백히 잘못된 최소 데이터

LoadGame도 최소한의 예외/null 방어를 유지한다.

## 6. 저장 흐름

```text
Menu Save
↓
현재 씬/위치/방향을 GameManager에 반영
↓
GameManager.GetSaveData()
↓
SaveSystem.SaveGame()
↓
JSON 저장
```

저장 파일 경로는 기존 구현을 기준으로 한다.

## 7. 불러오기 흐름

```text
Title Continue
↓
HasValidSaveData()
↓
LoadGame()
↓
GameManager.LoadFromSaveData()
↓
저장된 씬 Load
↓
PlayerLoader가 위치/방향 복원
```

씬 이름은 로드 전 Normalize 호환을 고려한다.

## 8. 현재 저장 대상

과거 구현 기준 주요 항목:

- playerName
- HP / MP
- level / exp
- 공격/방어/마법/속도/행운 계열 스탯
- gold
- currentSceneName
- playerPosition
- playerFacingDirection
- learnedSkillIds
- introPlayed 등 기존 진행 플래그
- 일부 보스 플래그가 존재할 수 있음

정확한 필드 추가/삭제 작업 전에는 `SaveData.cs` 실제 코드를 기준으로 다시 확인한다.

## 9. SaveData 수정 규칙

SaveData 필드를 추가/변경하면 최소한 같이 확인:

- SaveData 선언
- `GameManager.GetSaveData()`
- `GameManager.LoadFromSaveData()`
- 기본값/이전 저장 호환
- PlayerLoader 또는 해당 시스템 복원 코드

필드 하나만 SaveData에 추가하고 직렬화/복원 반영을 빼먹지 않는다.

## 10. 개발용 저장 삭제

GameManager에 Editor/Development Build 전용 단축키가 있다.

```text
0 또는 Numpad 0
→ SaveSystem.DeleteSaveData()
```

주의:

- 디스크 저장을 지우는 기능.
- 현재 실행 중 GameManager의 런타임 세션까지 반드시 초기화하는 기능은 아님.
- 완전 새 실행 테스트는 저장 삭제 → Play 종료 → TitleScene에서 다시 시작 권장.

## 11. 향후 안정화 후보

필요성이 생기면:

- `saveVersion`
- TryLoad 결과 타입
- temp 파일 저장 후 replace
- 백업 save
- 저장 migration

현재 기능이 안정된 상태에서 서둘러 포맷을 바꾸지 않는다.
