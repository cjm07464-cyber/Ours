# AGENTS.md

이 문서는 이 저장소에서 작업하는 AI 코딩 에이전트용 작업 지침서다.

이 프로젝트는 장기 Unity 2D RPG 프로젝트다. 작업자는 기능 하나만 보고 수정하지 말고, 관련 시스템의 연결 흐름을 함께 확인해야 한다.

---

## 작업 시작 전 필수 읽기 순서

1. `README.md`
2. `AGENTS.md`
3. `PROJECT_GUIDE.md`
4. `TODO.md`
5. `Docs/CURRENT_STATE.md`
6. 작업과 관련된 `Docs/*.md`

---

## 기본 작업 절차

1. 요청 내용을 확인한다.
2. 필수 문서를 먼저 읽는다.
3. 관련 스크립트와 씬 구조를 분석한다.
4. 수정 전에 어떤 파일을 바꿀지 요약한다.
5. 필요한 최소 파일만 수정한다.
6. 기존 구조를 임의로 갈아엎지 않는다.
7. 수정 후 변경 파일 목록을 정리한다.
8. Unity에서 확인해야 할 Inspector 연결값과 테스트 절차를 알려준다.

---

## 절대 규칙

### 전투 입력 구조

- `BattleManager`의 `Command Text` 방식은 사용하지 않는다.
- `BattleManager` 인스펙터의 `Command Text` 칸은 반드시 `None`으로 유지한다.
- BattleScene 커맨드 입력은 `CommandSelector`가 담당한다.
- `BattleManager`는 실제 전투 행동 실행만 담당한다.
- `CommandSelector`와 `BattleManager`가 동시에 Z 입력을 처리하게 만들지 않는다.

### 전투 데이터 구조

- 플레이어 상태는 `GameManager` 기준으로 관리한다.
- 적 원본 데이터는 `EnemyData` ScriptableObject 기준으로 관리한다.
- 필드 적은 자기 `EnemyData`를 가진다.
- 전투 진입 시 `EnemyController`가 `GameManager.currentBattleEnemy`에 EnemyData를 전달한다.
- `BattleManager`는 `GameManager.currentBattleEnemy`를 우선 사용하고, 없으면 테스트용 `testEnemyData`를 사용한다.
- 구 구조인 `PlayerManager`, `PlayerStats`에 의존하는 코드를 되살리지 않는다.

### BattleScene UI 구조

현재 BattleScene Canvas 구조는 아래 역할을 기준으로 유지한다.

```text
Canvas
├── BattleBG
│   └── AnimatedBG
│       └── Raw Image
├── EnemyLayer
│   └── Enemy Image
├── BattleUI
│   ├── MessagePanel
│   ├── CommandPanel
│   └── StatusPanel
├── GameOverPanel
│   ├── GameOverDarkOverlay
│   ├── GameOverTitleText
│   ├── GameOverQuestionText
│   ├── GameOverSelector
│   ├── GameOverContinueText
│   └── QuitText
└── FadePanel
    └── FadeOverlay
```

- `BattleBG`는 전투 배경 애니메이션용이다.
- `EnemyLayer/Enemy Image`는 적 이미지 표시용이며 하나만 사용한다.
- `BattleUI`는 일반 전투 UI이다.
- `GameOverPanel`은 게임오버 전용 UI이다.
- `FadePanel`은 화면 전환용이다.
- 이 역할을 서로 섞지 않는다.
- 임시 테스트용 `Canvas/Image` 같은 오브젝트가 남아 있으면 사용 여부를 확인하고 필요 없으면 제거한다.

### BGM 구조

- MainScene의 `BGM_Manager`에는 `BGMManager`가 붙어 있다.
- `BGMManager`는 `DontDestroyOnLoad`로 유지된다.
- 전투 진입 시 Main BGM은 `PauseBGM()`으로 멈춘다.
- 전투 종료 후 MainScene 복귀 시 `ResumeBGM()`으로 이어서 재생한다.
- BattleScene의 `Battle_BGM`에는 `BGMManager`를 붙이지 않는다.
- BattleScene의 `Battle_BGM`은 일반 `AudioSource`로 둔다.
- 게임오버에서 그만하기 선택 시 `BGMManager.StopAndDestroy()` 후 Title 씬으로 이동한다.

### 페이드 / 게임오버 UI 구조

- `FadePanel/FadeOverlay`는 화면 전환용이다.
- `GameOverPanel/GameOverDarkOverlay`는 게임오버 화면 배경용이다.
- `FadeOverlay`와 `GameOverDarkOverlay`를 혼동하지 않는다.
- `BattleManager`의 `Fade Image`에는 `FadePanel/FadeOverlay`의 Image를 연결해야 한다.
- `GameOverDarkOverlay`를 `Fade Image`에 연결하지 않는다.
- `FadePanel`은 Canvas의 가장 아래쪽에 두어 모든 UI 위에 렌더링되게 한다.
- 게임오버 선택 후 페이드아웃할 때 `GameOverPanel`을 먼저 끄지 않는다.
- 완전 암전된 뒤에 필요한 UI를 끄고 씬 전환한다.

### 저장 시스템

- 저장 시스템 수정 시 반드시 `GameManager.cs`, `SaveData.cs`, `SaveSystem.cs`, `PlayerLoader.cs`를 함께 확인한다.
- `SaveData`에 필드를 추가하면 `GameManager.GetSaveData()`와 `GameManager.LoadFromSaveData()`에도 반영한다.
- 현재 플레이어 방향 저장 / 복원은 아직 미구현이다.
- 게임오버의 “다시 일어서기”는 마지막 저장 데이터를 불러오는 방식으로 처리한다.

### Enemy 시스템

- `EnemyController`는 필드 적 이동, 추적, 전투 진입을 담당한다.
- 적과 접촉해 전투에 들어갈 때 `currentBattleEnemyId`를 저장한다.
- 도망 시 `escapedEnemyId`를 사용해 해당 적을 3초간 접촉 무시 / 깜빡임 / 이동 정지 처리한다.
- 도망 후 깜빡임 중에는 적의 Collider2D도 꺼서 플레이어가 밀지 못하게 한다.
- 승리 후에는 해당 적을 MainScene 복귀 시 숨기고 일정 시간 후 리스폰시키는 구조를 구현할 예정이다.
- `encounterId`는 필드 적마다 고유해야 한다.

### 폰트 구조

- TMP 한글 폰트는 완성형 한글과 단독 자모를 모두 포함해야 한다.
- 권장 Unicode Range: `0020-007E,3130-318F,AC00-D7A3`
- 더 넉넉하게 만들 경우: `0020-007E,1100-11FF,3130-318F,AC00-D7A3`
- 새 TMP Font Asset을 만들면 BattleScene, TitleScene, MainMenuManager의 폰트 경로를 함께 확인한다.

---

## Unity Inspector 체크가 필요한 경우

### BattleManager

- `Message Panel` → `Canvas/BattleUI/MessagePanel`
- `Command Panel` → `Canvas/BattleUI/CommandPanel`
- `Status Panel` → `Canvas/BattleUI/StatusPanel`
- `Enemy Image` → `Canvas/EnemyLayer/Enemy Image`
- `Message Text`
- `Command Text = None`
- `Status Name / HP / MP Text`
- `Game Over Panel`
- `Game Over Selector`
- `Game Over Continue Text`
- `Game Over Quit Text`
- `Test Enemy Data`
- `Fade Image` → `Canvas/FadePanel/FadeOverlay`
- `Battle Bgm Source` → `Battle_BGM`의 AudioSource

### CommandSelector

- `Selector`
- `Options[0]` = 공격
- `Options[1]` = 방어
- `Options[2]` = Special
- `Options[3]` = 스킬
- `Options[4]` = 아이템
- `Options[5]` = 도망
- `Battle Manager`

### EnemyController

- `Enemy Data`
- `Battle Scene Name`
- `Encounter Id`
- `Battle Transition Effect`

---

## 작업 완료 보고 형식

```text
수정한 파일:
- ...

핵심 변경:
- ...

Unity에서 확인할 Inspector 연결:
- ...

테스트 방법:
1. ...
2. ...

주의:
- ...
```
