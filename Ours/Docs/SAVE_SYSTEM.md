# SAVE_SYSTEM

저장 / 불러오기 시스템 상세.

## 관련 파일

- `Assets/Scripts/Title/GameManager.cs`
- `Assets/Scripts/Title/SaveData.cs`
- `Assets/Scripts/Title/SaveSystem.cs`
- `Assets/Scripts/Main/PlayerLoader.cs`
- `Assets/Scripts/Main/MainMenuManager.cs`

## 저장 흐름

1. 메뉴에서 저장하기 선택
2. 현재 씬 이름과 플레이어 위치를 GameManager에 반영
3. 현재 플레이어 바라보는 방향을 GameManager에 반영
4. `SaveSystem.SaveGame()` 호출
5. `GameManager.GetSaveData()`로 SaveData 생성
6. JSON으로 `Application.persistentDataPath/savefile.json` 저장

## 불러오기 흐름

1. `SaveSystem.HasSaveData()` 확인
2. `SaveSystem.LoadGame()` 호출
3. JSON을 SaveData로 변환
4. `GameManager.LoadFromSaveData()` 호출
5. 저장된 씬으로 이동
6. TownScene/MainScene 시작 시 `PlayerLoader`가 `GameManager.playerPosition` 적용
7. `PlayerLoader`가 `GameManager.playerFacingDirection` 적용

## 현재 저장되는 것

- playerName
- HP / MP
- level
- exp
- attack / defense
- magicAttack / magicDefense
- speed / luck
- gold
- currentSceneName
- playerPosition
- playerFacingDirection
- learnedSkillIds
- introPlayed
- ratBossDefeated

## 아직 저장되지 않는 것

- 플레이어 현재 애니메이션 프레임 상태
- 필드 적 개별 장기 처치 상태
- 인벤토리
- 파티원 정보

## 씬 이름 변경 주의

목표 씬 명칭:
- BootScene
- TownScene
- BattleScene

기존 저장 파일에는 `Title`, `MainScene` 같은 이름이 저장되어 있을 수 있다.

씬 이름을 실제로 변경할 경우:
- 기존 `MainScene` 저장값을 `TownScene`으로 매핑할지 결정한다.
- 기존 `Title` 이동 코드를 `BootScene`으로 바꿀지 결정한다.
- `defaultReturnSceneName`
- `returnSceneName`
- `currentSceneName`
- `SceneManager.LoadScene(...)`
- Build Settings
를 함께 확인한다.
