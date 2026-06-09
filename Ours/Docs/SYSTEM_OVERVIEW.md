# SYSTEM_OVERVIEW

## 목표 전체 흐름

```text
BootScene
  ├─ 인트로 크레딧
  ├─ 지구 로고 / Ours 타이틀
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
- 현재 `Title` 씬이 BootScene 역할을 수행한다.
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

## BootScene / Title 구성

현재 Title 씬 기준:

```text
Title
├── Main Camera
├── EventSystem
├── BootSceneController
├── TitleManager
├── GameManager
├── Boot_BGM 또는 AudioSource
└── Canvas
    ├── FadeOverlay
    ├── TitleManagerUI
    ├── IntroCreditGroup
    │   ├── CreditTopText
    │   ├── CreditLine
    │   └── CreditBottomText
    ├── IntroText
    └── TitleGroup
        ├── EarthImage
        ├── UrsText
        ├── MenuGroup
        │   ├── MenuSelector
        │   ├── NewGameText
        │   ├── ContinueText
        │   └── QuitText
        └── StudentCreditText
```

역할:
- `FadeOverlay`: 시작 검은 배경 및 이어하기 페이드아웃
- `TitleManagerUI`: 기존 이름 입력 / 시놉시스 / 확인 패널
- `IntroCreditGroup`: 시작 크레딧 3개 표시
- `TitleGroup`: 지구 로고, Ours 텍스트, 메뉴, 하단 서명
- `EarthImage`: Animator로 지구 자전
- `BootSceneController`: 지구 페이드/이동/축소, 메뉴 선택, 이어하기 처리
- `TitleManager`: 이름 입력과 시놉시스 흐름

## TownScene 구성

현재는 기존 `MainScene` 이름이 남아 있을 수 있다.

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
BootScene/Title
→ 처음부터
→ 이름 입력
→ GameManager 새 데이터 초기화
→ 시놉시스
→ TownScene/MainScene 로드
```

### 이어하기

```text
BootScene/Title
→ SaveSystem.HasSaveData()
→ 저장 데이터 있으면 Continue 활성화
→ 이어하기 선택
→ FadeOverlay 페이드아웃
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

## 씬 이름 변경 예정

목표:

```text
Title → BootScene
MainScene → TownScene
BattleScene 유지
```

변경 시 확인:
- Build Settings
- 모든 `SceneManager.LoadScene(...)` 문자열
- 저장 데이터 `currentSceneName`
- 전투 복귀용 `returnSceneName`
- 게임오버 / 메뉴 종료 목적지
