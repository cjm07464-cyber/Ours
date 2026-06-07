# SYSTEM_OVERVIEW

전체 시스템 관계 요약.

---

## 전체 흐름

```text
Title
  ↓
MainScene
  ↓ 적 접촉
BattleTransitionEffect
  ↓
BattleScene
  ├─ 승리 / 도망 → MainScene
  └─ 게임오버 → 다시 일어서기 / 그만하기
```

---

## 주요 싱글톤 / 유지 오브젝트

### GameManager

- 플레이어 상태 보관
- 저장 데이터 반영
- 전투 임시 데이터 보관
- DontDestroyOnLoad

### BGMManager

- MainScene BGM 유지
- 전투 진입 시 Pause
- MainScene 복귀 시 Resume
- Title 복귀 시 StopAndDestroy 가능
- DontDestroyOnLoad

---

## MainScene 구성

- Player
  - PlayerController
  - PlayerLoader
- Enemy_Dog
  - EnemyController
  - EnemyData 연결
- BGM_Manager
  - AudioSource
  - BGMManager
- MainMenuManager
  - C키 메뉴
  - 저장 / 게임종료

---

## BattleScene 구성

```text
BattleScene
├── Main Camera
├── Global Light 2D
├── Battle Manager
├── Canvas
│   ├── BattleBG
│   │   └── AnimatedBG
│   │       └── Raw Image
│   ├── EnemyLayer
│   │   └── Enemy Image
│   ├── BattleUI
│   │   ├── MessagePanel
│   │   ├── CommandPanel
│   │   └── StatusPanel
│   ├── GameOverPanel
│   └── FadePanel
├── PlayerManager
├── Battle_BGM
└── rdog
```

주의:
- `PlayerManager`는 구 구조일 가능성이 있으므로 현재 사용 여부를 확인한다.
- 실제 전투 플레이어 상태는 `GameManager` 기준으로 관리한다.
- `rdog`가 테스트 오브젝트라면 실제 사용 여부를 확인하고 정리한다.

---

## BattleScene UI 렌더 순서

```text
BattleBG
→ EnemyLayer
→ BattleUI
→ GameOverPanel
→ FadePanel
```

---

## 데이터 흐름

### 전투 진입

```text
EnemyController
→ GameManager.currentBattleEnemy
→ BattleTransitionEffect
→ BattleScene
→ BattleManager.ResolveEnemyData()
```

### 도망

```text
BattleManager.EscapeRoutine()
→ GameManager.escapedEnemyId 저장
→ MainScene 복귀
→ EnemyController.EscapeIgnoreRoutine()
```

### 승리 후 리스폰 예정

```text
BattleManager.VictoryRoutine()
→ GameManager.defeatedEnemyId 저장 예정
→ MainScene 복귀
→ EnemyController가 자기 encounterId와 비교
→ 해당 적 숨김
→ 10초 후 리스폰
```

### 저장

```text
MainMenuManager
→ GameManager.GetSaveData()
→ SaveSystem.SaveGame()
→ savefile.json
```

### 불러오기

```text
SaveSystem.LoadGame()
→ GameManager.LoadFromSaveData()
→ SceneManager.LoadScene()
→ PlayerLoader가 위치 적용
```
