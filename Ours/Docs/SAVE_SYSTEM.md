# SAVE_SYSTEM

저장 / 불러오기 시스템 상세.

---

## 관련 파일

- `Assets/Scripts/Title/GameManager.cs`
- `Assets/Scripts/Title/SaveData.cs`
- `Assets/Scripts/Title/SaveSystem.cs`
- `Assets/Scripts/Main/PlayerLoader.cs`
- `Assets/Scripts/Main/MainMenuManager.cs`

---

## 저장 흐름

1. 메뉴에서 저장하기 선택
2. 현재 씬 이름과 플레이어 위치를 GameManager에 반영
3. `SaveSystem.SaveGame()` 호출
4. `GameManager.GetSaveData()`로 SaveData 생성
5. JSON으로 `Application.persistentDataPath/savefile.json` 저장

---

## 불러오기 흐름

1. `SaveSystem.HasSaveData()` 확인
2. `SaveSystem.LoadGame()` 호출
3. JSON을 SaveData로 변환
4. `GameManager.LoadFromSaveData()` 호출
5. 저장된 씬으로 이동
6. MainScene 시작 시 `PlayerLoader`가 `GameManager.playerPosition` 적용

---

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
- introPlayed
- ratBossDefeated

---

## 아직 저장되지 않는 것

- 플레이어가 바라보는 방향
- 플레이어 현재 애니메이션 상태
- 필드 적 개별 처치 상태
- 인벤토리
- 스킬 목록
- 파티원 정보

---

## 다음 작업

- 플레이어 방향 저장 / 복원 추가
- SaveData에 playerDirX / playerDirY 추가
- GameManager에 playerFacingDirection 추가
- PlayerController에서 마지막 방향 저장
- PlayerLoader에서 방향에 맞는 스프라이트 적용
- 스킬 목록을 저장할지 여부 결정
- 필드 적 처치 상태 저장 여부 결정
