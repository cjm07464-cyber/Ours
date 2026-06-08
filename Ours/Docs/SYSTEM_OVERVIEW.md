# SYSTEM_OVERVIEW

## 목표 전체 흐름

```text
BootScene
  ├─ 인트로 연출
  ├─ 처음부터 → 이름 입력 → TownScene
  ├─ 이어하기 → 저장된 씬
  └─ 종료

TownScene
  ↓ 적 접촉
BattleTransitionEffect
  ↓
BattleScene
  ├─ 승리 / 도망 → TownScene
  └─ 게임오버 → 다시 일어서기 / 그만하기
```

주의:
- 현재 프로젝트에는 기존 `Title`, `MainScene` 씬 이름이 남아 있을 수 있다.
- 목표 명칭은 `BootScene`, `TownScene`, `BattleScene`이다.

## 주요 싱글톤 / 유지 오브젝트

### GameManager

- 플레이어 상태 보관
- 저장 데이터 반영
- 전투 임시 데이터 보관
- 배운 스킬 ID 목록 보관
- 플레이어 바라보는 방향 보관
- 전투 승리/도망 적 ID 보관
- DontDestroyOnLoad

### BGMManager

- TownScene BGM 유지
- 전투 진입 시 Pause
- TownScene 복귀 시 Resume
- BootScene/Title 복귀 시 StopAndDestroy 가능
- DontDestroyOnLoad

## BootScene 구성 목표

```text
BootScene
├── Main Camera
├── EventSystem
├── BootSceneController
├── Boot_BGM
└── Canvas
    ├── FadeOverlay
    ├── IntroText
    ├── TitleGroup
    │   ├── EarthImage
    │   ├── UrsText
    │   └── MenuGroup
    │       ├── Selector
    │       ├── NewGameText
    │       ├── ContinueText
    │       └── QuitText
    └── NameInputPanel
```

## TownScene 구성

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
- SceneFadeIn
  - BattleScene 복귀 시 페이드 인

## BattleScene 구성

```text
BattleScene
├── Main Camera
├── Global Light 2D
├── Battle Manager
├── Canvas
│   ├── BattleBG
│   ├── EnemyLayer
│   ├── EffectLayer
│   ├── BattleUI
│   ├── GameOverPanel
│   └── FadePanel
├── Battle_BGM
└── 기타 임시/구 오브젝트
```

## 데이터 흐름

### 새 게임

```text
BootScene
→ 처음부터
→ 이름 입력
→ GameManager 새 데이터 초기화
→ TownScene 로드
```

### 이어하기

```text
BootScene
→ SaveSystem.HasSaveData()
→ 저장 데이터 있으면 Continue 활성화
→ SaveSystem.LoadGame()
→ 저장된 씬 로드
```

### 전투 진입

```text
EnemyController
→ GameManager.currentBattleEnemy
→ GameManager.currentBattleEnemyId
→ BattleTransitionEffect
→ BattleScene
→ BattleManager.ResolveEnemyData()
```

### 스킬 사용

```text
CommandSelector
→ SkillPanel 열기
→ SkillSelector
→ BattleManager.OnSkillSelected()
→ SkillData 참조
→ 필요 시 EffectLayer에 effectPrefab 생성
→ 데미지/회복 처리
```
