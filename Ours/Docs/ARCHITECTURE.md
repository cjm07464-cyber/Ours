# ARCHITECTURE

현재 Ours의 전체 구조를 빠르게 파악하기 위한 기준 문서.

## 1. 전체 플레이 흐름

```text
앱 시작
  ↓
TitleScene (기술적 엔트리)
  ├─ 유효 저장 있음 ──────────────→ 타이틀/메뉴
  │                                 └─ Continue → 저장된 씬
  │
  └─ 유효 저장 없음
       ├─ StartupSessionState.NameChosen → 타이틀/메뉴
       └─ 그 외 → ForestScene
                    ↓
               숲 프롤로그
                    ↓
               이름 입력
                    ↓
               TitleScene
                    ↓
               New Game
                    ↓
               TownScene
                    ↓
          집/엄마/첫 아빠 전화
                    ↓
          탐험 / 메뉴 / 저장
                    ↓ 적 접촉
               BattleScene
                    ↓
               TownScene
```

Forest는 첫 실행 프롤로그다. 기술적인 첫 씬은 여전히 `TitleScene`이다.

## 2. 핵심 런타임 상태

### GameManager

역할:

- 플레이어 이름/스탯/레벨/EXP/골드 등 런타임 상태
- Inventory 및 장착 무기 상태
- Story flag 관리
- 저장 데이터 변환
- 전투 진입용 임시 Enemy 정보
- 씬 복귀 위치/방향
- 첫 실행용 startup session

현재 중요한 세션 값:

```text
StartupSessionState
- None
- ForestCompleted
- NameChosen

pendingPlayerName
```

`StartupSessionState` / `pendingPlayerName`은 저장 데이터가 아니라 실행 중 세션 정보다.

### Runtime Bootstrap

`GameManager`는 Runtime Bootstrap으로 씬 직접 Play에서도 생성될 수 있다.

목적:

- Forest/Town/Battle/Title 씬을 Editor에서 직접 Play해도 GameManager 누락 방지.

씬에 기존 GameManager 오브젝트가 있으면 singleton 로직으로 중복 인스턴스를 정리한다.

## 3. Town PersistentUI

Town의 공통 Dialogue/Menu/Choice 실행 구조는 `PersistentUI`를 기준으로 한다.

```text
PersistentUI
├ DialogueController
├ DialogueRunner
├ MainMenuManager
├ PhoneEventRunner
└ Canvas
   ├ DialogueUI
   │  ├ BasicLayout
   │  └ PortraitLayout
   ├ MainMenuUI
   │  ├ MenuPanel
   │  └ CharacterStatusSlot
   └ ChoiceUI
      └ ChoicePanel
         ├ Select
         ├ YesText
         └ NoText
```

활성 규칙:

- `PersistentUI` = ON
- `PhoneEventRunner` = ON
- `ChoiceUI` 루트 = ON
- `DialogueUI` / `MainMenuUI` / `ChoicePanel` = 필요할 때만 표시

Coroutine 실행 컴포넌트는 비활성 GameObject에 두지 않는다.

## 4. Dialogue 시스템

### CharacterData

캐릭터별 데이터:

- characterId / displayName
- 필드 방향 스프라이트
- expressionId 기반 portrait
- portrait 색상 정보
- 캐릭터별 dialogue type sound

### DialogueSequence

대사 줄을 ScriptableObject 데이터로 보관한다.

주요 Line 정보:

- speaker
- expressionId
- text
- showPortrait / showSpeakerName
- instantText
- lineStartSound
- stopLineStartSoundOnAdvance
- lineEndSound
- lineEndSoundVolume
- waitForLineEndSound
- pauseBgmDuringLineEndSound
- bgmResumeFadeDuration

### DialogueController

- BasicLayout / PortraitLayout 표시
- typewriter 출력
- TMP page 처리
- 즉시 출력
- 대화 닫기/정리

### DialogueRunner

- DialogueSequence 순차 실행
- 토큰 치환

현재 사용 토큰:

```text
{player}
{level}
{expToNextLevel}
```

`{player}`가 비어 있을 때의 fallback은 기존 구현을 유지한다.

## 5. 범용 Game Event 시스템

### GameEventSequence

현재 Step 유형:

```text
Dialogue
GiveItem
SetStoryFlag
Reaction
SystemMessage
Choice
SaveGame
QuitGame
```

### GameEventRunner

Sequence의 Step을 순차 실행한다.

Context에는 필요에 따라 다음을 연결한다.

- DialogueRunner
- DialogueController
- ChoiceUIController
- PlayerController
- ReactionIcon
- Reaction AudioSource / Sound
- Quit FadeOverlay

`GameEventRunner`는 각 이벤트 GameObject에 개별로 붙을 수 있다.

예:

```text
HouseExitGate
└ GameEventRunner

FatherPhoneCallTrigger
└ GameEventRunner

PersistentUI/PhoneEventRunner
└ GameEventRunner
```

같은 스크립트가 여러 GameObject에 존재하는 것은 정상이며, 각각 별도 Context를 가진다.

## 6. Story flag / NPC 상호작용

### NPCInteraction

NPC 상호작용 시 기본 Sequence를 실행한다.

Story flag에 따라 Alternate Sequence를 사용할 수 있다.

엄마 예:

```text
Primary      = Mom_MorningEvent
AlternateFlag = mother_morning_talk
Alternate    = Mom_DefaultEvent
```

### StoryFlagGate

필수 플래그가 없으면 통로를 막고 blocked event를 실행한다.

엄마와 대화하지 않고 집을 나가려는 경우:

```text
Required Flag = mother_morning_talk
Blocked Event = Mom_ExitBlockedEvent
Return Point  = HouseExitReturnPoint
```

blocked event 후 Player의 `AutoMoveTo()`를 사용해 집 안쪽으로 되돌린다.

### StoryEventTrigger

위치 진입으로 GameEventSequence를 실행한다.

첫 아빠 전화 Trigger는 완료 플래그 `father_phone_call_done`을 사용한다.

## 7. Main Menu / Choice / 전화 저장

Main Menu 입력:

```text
Z = 열기/닫기
방향키 = 이동
C = 결정
X = 닫기
```

`father_phone_call_done` 이후 메뉴가 사용 가능하다.

현재 메뉴 항목:

- 상태
- 장비
- 가방
- 통화
- 닫기

### ChoiceUI

`ChoiceUIController`가 Yes/No 선택을 담당한다.

```text
ChoiceUI        = ON
ChoicePanel     = 기본 OFF
Select          = TMP ">"
YesText         = 예
NoText          = 아니오
```

Cursor/Select 이동 시 `SFXManager`를 통해 이동 SFX를 재생할 수 있다.

### 전화 저장

`MainMenuManager`의 통화 항목은 `PhoneEventRunner`를 통해 전화용 `GameEventSequence`를 실행한다.

개념 흐름:

```text
Dad_SaveCall_Event
├ Dialogue → Intro
└ Choice
   ├ YES → SaveGame
   │        → 저장완료 Dialogue
   │        → Choice
   │           ├ YES → 마지막 Dialogue → QuitGame
   │           └ NO  → 계속 플레이 Dialogue
   └ NO  → 저장 없이 통화 종료 Dialogue
```

`QuitGame`은 화면/BGM Fade 후 Build에서 종료한다. Editor에서는 개발 로그만 남긴다.

## 8. Town Room / Player

Town은 한 씬 안에서 집 내부/외부 등 위치를 `TownRoomTransition`으로 전환한다.

- trigger 진입
- Fade
- Player 위치 이동
- Fade in

Player는 이벤트 중 `SetCanMove(false)`로 입력/속도를 정리한다.

StoryFlagGate 차단 복귀에는 `PlayerController.AutoMoveTo()`를 사용한다.

## 9. 씬별 책임

### ForestScene

첫 실행 프롤로그.

- 4개 맵 영역
- 전용 이동/카메라/사운드
- UFO/Alien 이벤트
- 이름 입력
- Forest → Title 전환

상세: `STARTUP_FLOW.md`

### TitleScene

- 시작 크레딧/타이틀
- New Game / Continue / Quit
- Forest 라우팅
- 저장 유효성 판정

### TownScene

- 일반 탐험
- 집/외부 RoomTransition
- NPC / Dialogue / Story Event
- Z Main Menu
- 통화 저장
- Inventory / 장착 상태
- 필드 적 및 Battle 진입

### BattleScene

현재 전투 진입 구조를 다시 분석하기 전까지 기존 `BATTLE_SYSTEM.md`를 안전선으로만 사용한다.

Battle 관련 수정 전에는 실제 `EnemyController`, `BattleTransitionEffect`, `GameManager`, `BattleManager`, Inspector 연결을 다시 추적한다.

## 10. 데이터 구조

### ItemData / Inventory

GameManager는 InventoryEntry 목록과 장착 무기 ID를 관리한다.

- Add / Remove / Has / Count
- Equip / Unequip
- effective attack / defense 계산

아이템 지급 이벤트는 `GiveItem` Step을 사용한다.

### EnemyData / SkillData

전투 원본 데이터.

Battle 진입 구조는 별도 분석 후 정리한다.

## 11. Audio 구조

### BGMManager

Town BGM 관리.

- Play / Stop
- Pause
- Resume
- Resume with Fade
- Fade Out

저장완료 징글처럼 Dialogue Line End Sound가 재생될 때:

```text
텍스트 출력 완료
→ BGM Pause
→ Line End Sound 재생
→ 입력 잠금
→ Sound 종료
→ BGM Resume + Fade-in
```

### SFXManager

공용 2D SFX AudioSource를 사용한다.

- Reaction SFX
- 메뉴 Cursor 이동음
- Choice Select 이동음
- 기타 일반 효과음

`PlayOneShot(AudioClip, volumeScale)`로 특정 클립만 볼륨을 조절할 수 있다.

캐릭터 타자음은 Dialogue 전용 AudioSource 흐름을 유지한다.

## 12. Fade 기준

Fade는 아직 하나의 전역 서비스로 통합하지 않는다.

- Forest: Forest 전용 FadeOverlay
- Town: TownEventCanvas/FadeOverlay 및 각 이벤트의 Context
- QuitGame: GameEventRunner의 Quit Fade Overlay
- Battle: 기존 Battle Fade

동작 중인 Fade를 이유 없이 공통 Manager로 재설계하지 않는다.

## 13. 문서 책임

- `STARTUP_FLOW.md` — Forest/Title/첫 실행
- `FIELD_SYSTEMS.md` — Town Player/NPC/Dialogue/Event/Menu
- `BATTLE_SYSTEM.md` — 전투 (실제 구현 재분석 전 안전선)
- `SAVE_AND_STATE.md` — GameManager/Save/StoryFlag/Inventory
- `RISK_REGISTER.md` — 위험한 레거시/정리 후보
