# AGENTS.md

이 문서는 이 저장소에서 작업하는 AI 코딩 에이전트용 작업 지침서다.

## 작업 시작 전 필수 읽기 순서

1. `README.md`
2. `AGENTS.md`
3. `PROJECT_GUIDE.md`
4. `TODO.md`
5. `Docs/CURRENT_STATE.md`
6. 작업과 관련된 `Docs/*.md`

## 기본 작업 절차

1. 요청 내용을 확인한다.
2. 필수 문서를 먼저 읽는다.
3. 관련 스크립트와 씬 구조를 분석한다.
4. 수정 전에 어떤 파일을 바꿀지 요약한다.
5. 필요한 최소 파일만 수정한다.
6. 기존 구조를 임의로 갈아엎지 않는다.
7. 수정 후 변경 파일 목록을 정리한다.
8. Unity에서 확인해야 할 Inspector 연결값과 테스트 절차를 알려준다.

## 절대 규칙

### 씬 명칭 기준

향후 목표 명칭은 아래를 기준으로 한다.

- `BootScene`: 게임 실행 첫 씬. 인트로, 타이틀 메뉴, 이름 입력 담당.
- `TownScene`: 기존 `MainScene` 역할. 마을 필드 담당.
- `BattleScene`: 전투 씬. 이름 유지.

주의:
- 현재 프로젝트에는 기존 `Title`, `MainScene` 이름이 남아 있을 수 있다.
- 실제 씬 파일명 변경은 관련 문자열 참조를 분석한 뒤 진행한다.
- `"MainScene"`, `"Title"` 같은 하드코딩 문자열을 바꿀 때는 `GameManager`, `BattleManager`, `EnemyController`, `MainMenuManager`, `SceneFadeIn`, `PlayerLoader`, `SaveData`, `SaveSystem`을 함께 확인한다.
- 저장 파일에 옛 씬 이름이 남아 있을 수 있으므로 호환 처리도 고려한다.

### BootScene 작업 규칙

- BootScene은 Codex가 완성 연출까지 만들지 않는다.
- Codex는 기능 뼈대만 구현한다.
- 지구 이미지, 로고 위치, 폰트, 페이드 타이밍, BGM, 세부 연출은 사용자가 Unity Editor에서 직접 다듬는다.
- 지구본 회전/축소/이동은 Inspector에서 조정 가능한 SerializedField 기반으로 만든다.
- 저장 데이터가 없으면 이어하기는 반투명 표시하고 선택 불가 처리한다.
- 처음부터 선택 시 기존 이름 입력 흐름을 재사용한다.
- 종료는 빌드에서는 Application.Quit, 에디터에서는 로그 처리한다.

### 전투 입력 구조

- `BattleManager`의 `Command Text` 방식은 사용하지 않는다.
- `BattleManager` 인스펙터의 `Command Text` 칸은 반드시 `None`으로 유지한다.
- BattleScene 커맨드 입력은 `CommandSelector`가 담당한다.
- SkillPanel 입력은 `SkillSelector`가 담당한다.
- `CommandSelector`와 `SkillSelector` 역할을 섞지 않는다.
- `BattleManager`는 실제 전투 행동 실행만 담당한다.

### 전투 데이터 구조

- 플레이어 상태는 `GameManager` 기준으로 관리한다.
- 적 원본 데이터는 `EnemyData` ScriptableObject 기준으로 관리한다.
- 스킬 원본 데이터는 `SkillData` ScriptableObject 기준으로 관리한다.
- 필드 적은 자기 `EnemyData`를 가진다.
- 전투 진입 시 `EnemyController`가 `GameManager.currentBattleEnemy`에 EnemyData를 전달한다.
- 구 구조인 `PlayerManager`, `PlayerStats`에 의존하는 코드를 되살리지 않는다.

### BattleScene UI 구조

```text
Canvas
├── BattleBG
│   └── AnimatedBG
│       └── Raw Image
├── EnemyLayer
│   └── Enemy Image
├── EffectLayer
├── BattleUI
│   ├── MessagePanel
│   ├── CommandPanel
│   ├── SkillPanel
│   └── StatusPanel
├── GameOverPanel
└── FadePanel
    └── FadeOverlay
```

- `EffectLayer`는 SkillData의 effectPrefab을 런타임 생성하는 부모다.
- `BattleUI`, `GameOverPanel`, `FadePanel` 역할을 섞지 않는다.

### BGM 구조

- TownScene의 `BGM_Manager`에는 `BGMManager`가 붙어 있다.
- 전투 진입 시 Town BGM은 `PauseBGM()`으로 멈춘다.
- 전투 종료 후 TownScene 복귀 시 `ResumeBGM()`으로 이어서 재생한다.
- BattleScene의 `Battle_BGM`에는 `BGMManager`를 붙이지 않는다.
- 게임오버에서 그만하기 선택 시 `BGMManager.StopAndDestroy()` 후 BootScene/Title 씬으로 이동한다.
- BootScene 인트로 BGM은 별도 AudioSource로 둘 수 있으며 기존 BGMManager와 충돌하지 않게 한다.

### 저장 시스템

- 저장 시스템 수정 시 반드시 `GameManager.cs`, `SaveData.cs`, `SaveSystem.cs`, `PlayerLoader.cs`를 함께 확인한다.
- `SaveData`에 필드를 추가하면 `GameManager.GetSaveData()`와 `GameManager.LoadFromSaveData()`에도 반영한다.
- 플레이어 방향 저장 / 복원은 구현되어 있다.
- 배운 스킬 ID 목록은 저장 대상이다.

### Enemy 시스템

- 도망 시 `escapedEnemyId`를 사용해 해당 적을 3초간 접촉 무시 / 깜빡임 / 이동 정지 처리한다.
- 승리 후에는 `defeatedEnemyId`를 사용해 해당 적을 TownScene 복귀 시 숨기고 10초 후 리스폰한다.
- `encounterId`는 필드 적마다 고유해야 한다.

## Unity Inspector 체크가 필요한 경우

### BattleManager

- `Message Panel` → `Canvas/BattleUI/MessagePanel`
- `Command Panel` → `Canvas/BattleUI/CommandPanel`
- `Skill Panel` → `Canvas/BattleUI/SkillPanel`
- `Status Panel` → `Canvas/BattleUI/StatusPanel`
- `Enemy Image` → `Canvas/EnemyLayer/Enemy Image`
- `Command Text = None`
- `Skill Selector`
- `PK Heal Skill`
- `PK Thunder Skill`
- `Effect Layer` → `Canvas/EffectLayer`
- `Fade Image` → `Canvas/FadePanel/FadeOverlay`
- `Battle Bgm Source` → `Battle_BGM`의 AudioSource

### EnemyController

- `Enemy Data`
- `Battle Scene Name`
- `Encounter Id`
- `Battle Transition Effect`
- `Defeated Respawn Seconds`

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

주의:
- ...
```
