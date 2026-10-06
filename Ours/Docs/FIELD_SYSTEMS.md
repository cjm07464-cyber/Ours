# FIELD_SYSTEMS

TownScene의 Player, RoomTransition, NPC, Dialogue, Event, MainMenu, Audio 기준.

## 1. 입력

일반 Town 필드:

- 방향키 — 이동 / UI 이동
- `Z` — Main Menu 열기/닫기
- `C` — 메뉴 결정
- `X` — 취소/닫기
- `0` / Numpad `0` — 개발용 저장 파일 삭제

Dialogue가 진행 중일 때 MainMenu가 열리지 않도록 `MainMenuManager`는 Dialogue 활성 상태를 확인한다.

## 2. Player

Town Player는 `PlayerController`를 사용한다.

이벤트에서 이동을 잠글 때 `SetCanMove(false)`는 다음을 함께 정리하는 현재 동작을 유지한다.

- 입력값
- AutoMove
- Rigidbody 속도
- idle 표시

StoryFlagGate 등에서 자동 복귀가 필요할 때:

```text
PlayerController.AutoMoveTo(targetPosition, speed, onComplete)
```

을 사용한다.

`IsAutoMoving` 중에는 일반 입력을 받지 않는다.

## 3. Town Camera

Town 카메라는 일반 탐험용으로 Player를 지연 없이 따라가는 구성을 우선한다.

Y-sort는 개별 NPC마다 별도 정렬 스크립트를 붙이지 않고, 프로젝트의 URP 2D Renderer Transparency Sort 설정을 우선한다.

Player/NPC SpriteRenderer는 같은 기준 Sorting Layer/Order를 사용하고, 물리 Collider는 발쪽에 두어 위/아래 겹침이 자연스럽게 보이게 한다.

## 4. RoomTransition

관련: `TownRoomTransition`.

집 내부/외부 등 한 TownScene 안의 위치 이동에 사용한다.

개념 흐름:

```text
Trigger
→ Player 잠금
→ Fade Out
→ Entry Point로 이동
→ Fade In
→ Player 해제
```

House/Outside 전환도 이 방식으로 처리한다.

## 5. PersistentUI

Town 공통 UI는 `PersistentUI` 아래에 둔다.

```text
PersistentUI
├ DialogueController
├ DialogueRunner
├ MainMenuManager
├ PhoneEventRunner
└ Canvas
   ├ DialogueUI
   ├ MainMenuUI
   └ ChoiceUI
```

기본 활성 상태:

- `PersistentUI` = ON
- `PhoneEventRunner` = ON
- `ChoiceUI` = ON
- `DialogueUI` = OFF
- `MainMenuUI` = OFF
- `ChoicePanel` = OFF

실행기를 비활성 UI 자식에 두지 않는다.

## 6. Dialogue

### DialogueController

UI Layout:

- BasicLayout
- PortraitLayout

기능:

- typewriter
- instant text
- TMP page
- portrait/name 표시
- 캐릭터별 type sound
- 종료 시 상태 cleanup

DialogueUI가 같은 프레임에 inactive → active가 되는 경우 TMP textInfo가 아직 비어 있을 수 있으므로 현재의 textInfo 준비/fallback 로직을 유지한다.

### DialogueSequence

한 Line에서 사용하는 주요 필드:

- Speaker
- Expression Id
- Text
- Show Portrait
- Show Speaker Name
- Instant Text
- Line Start Sound
- Stop Line Start Sound On Advance
- Line End Sound
- Line End Sound Volume
- Wait For Line End Sound
- Pause Bgm During Line End Sound
- Bgm Resume Fade Duration

전화 벨소리처럼 다음 줄로 넘어가는 즉시 끊어야 하는 시작음은 `Stop Line Start Sound On Advance`를 사용한다.

저장완료 징글처럼 텍스트 출력이 끝난 뒤 재생해야 하는 소리는 Line End Sound를 사용한다.

## 7. NPC

### NPCController

NPC 루트에서 필드 Sprite/방향을 관리한다.

- FacePlayer
- FaceDirection
- ResetFacing

### NPCInteraction

InteractionTrigger에서 사용한다.

필수 Context:

- NPCController
- GameEventRunner
- Primary Event Sequence

Story flag에 따른 대체 이벤트:

- Alternate Flag Id
- Alternate Event Sequence

엄마 예:

```text
Primary Event      = Mom_MorningEvent
Alternate Flag Id  = mother_morning_talk
Alternate Event    = Mom_DefaultEvent
```

## 8. Game Event

`GameEventSequence` + `GameEventRunner`를 범용 이벤트 단위로 사용한다.

Step:

- Dialogue
- GiveItem
- SetStoryFlag
- Reaction
- SystemMessage
- Choice
- SaveGame
- QuitGame

GameEventRunner는 이벤트별로 개별 존재할 수 있다.

Runner GameObject 자체가 비활성화되면 Coroutine을 시작할 수 없으므로 실행기는 ON 상태를 유지한다.

## 9. 엄마 아침 이벤트

핵심 플래그:

```text
mother_morning_talk
```

`Mom_MorningEvent` 개념:

```text
Dialogue → Mom_Morning
GiveItem → baseball_bat x1
SystemMessage → "야구방망이를 얻었다!"
SetStoryFlag → mother_morning_talk
```

아이템 획득 SystemMessage는 필요 시 BGM Pause → SFX → BGM Fade Resume 흐름을 사용할 수 있다.

플래그 설정 후 엄마에게 다시 말하면 `Mom_DefaultEvent`로 전환된다.

## 10. HouseExitGate

엄마와 대화하지 않고 집을 나가는 것을 막는다.

`StoryFlagGate` 핵심 Inspector:

```text
Required Flag Id       = mother_morning_talk
Player Controller      = Player
Event Runner           = HouseExitGate GameEventRunner
Blocked Event Sequence = Mom_ExitBlockedEvent
Blocked Return Point   = HouseExitReturnPoint
Blocking Collider      = PhysicalBlocker
Require Flag           = ON
Return Move Speed      ≈ 2.5
```

`Mom_ExitBlockedEvent`:

```text
Reaction
Dialogue → Mom_ExitBlocked
```

HouseExitGate의 GameEventRunner Reaction Context에는:

- Mom/ReactionIcon
- SFX_Manager AudioSource
- 느낌표 SFX
- Wait Duration

을 연결한다.

ReactionIcon은 기본 비활성 상태여도 Runner가 이벤트 중 활성/비활성 처리한다.

## 11. 첫 아빠 전화

집 밖 첫 이벤트 Trigger:

```text
Events/FatherPhoneCallTrigger
```

구성:

- BoxCollider2D Trigger
- StoryEventTrigger
- GameEventRunner

핵심:

```text
Event Sequence     = FatherPhoneCallEvent
Completed Flag Id  = father_phone_call_done
Trigger On Enter   = ON
```

이벤트 완료 후 `father_phone_call_done` 플래그가 설정되며 Town MainMenu를 사용할 수 있다.

## 12. Main Menu

`MainMenuManager`는 `PersistentUI` 루트에 둔다.

MainMenuUI 구조:

```text
MainMenuUI
├ MenuPanel
│  ├ Cursor
│  ├ StatusText
│  ├ EquipmentText
│  ├ BagText
│  ├ PhoneText
│  └ CloseText
└ CharacterStatusSlot
```

입력:

```text
Z = Open/Close
Up/Down = 이동
C = 결정
X = 닫기
```

메뉴 잠금 플래그:

```text
father_phone_call_done
```

`CharacterStatusSlotUI`는 현재 Player 이름/현재 HP/현재 MP를 표시한다.

메뉴 Cursor가 실제 다른 항목으로 이동할 때만 Cursor Move Sound를 재생한다.

## 13. ChoiceUI

구조:

```text
ChoiceUI
└ ChoicePanel
   ├ Select     (TMP ">")
   ├ YesText
   └ NoText
```

`ChoiceUIController` Inspector:

- Choice Panel → ChoicePanel
- Cursor → Select
- Yes Text → YesText
- No Text → NoText
- Cursor Move Sound → 선택 이동 SFX

Yes/No 인덱스가 실제 변경될 때만 이동 SFX를 재생한다.

## 14. 메뉴 통화 / 저장

`PersistentUI/PhoneEventRunner`:

- GameObject = ON
- `GameEventRunner` 부착

Context:

- Dialogue Runner
- Dialogue Controller
- Choice UI Controller
- Player Controller
- Lock Player During Event = ON
- Quit Fade Overlay = TownEventCanvas/FadeOverlay
- Quit Fade Duration ≈ 1.5

`MainMenuManager`:

```text
Phone Event Runner   = PhoneEventRunner/GameEventRunner
Phone Event Sequence = Dad_SaveCall_Event
```

전화 저장 이벤트의 현재 개념 흐름:

```text
Intro Dialogue
→ Choice 1: 저장할지

YES
→ SaveGame
→ 저장 완료 Dialogue
→ Choice 2: 오늘은 여기까지 할지
   YES → 마지막 Dialogue → QuitGame
   NO  → 격려 Dialogue → 게임 계속

NO
→ 짧은 종료 Dialogue
→ 게임 계속
```

전화 시작 벨소리는 첫 Line의 `Line Start Sound`로 재생하고 `Stop Line Start Sound On Advance`를 켠다.

저장완료 징글은 저장완료 텍스트가 끝까지 표시된 뒤 Line End Sound로 재생한다.

```text
텍스트 출력 완료
→ Town BGM Pause
→ 저장완료 Sound 재생
→ Sound 동안 다음 Dialogue 진행 잠금
→ Sound 종료
→ Town BGM Resume + Fade-in
```

개별 징글 볼륨은 `Line End Sound Volume`로 조절한다.

## 15. SFX

공용:

```text
Audio/SFX_Manager
- AudioSource
- SFXManager
```

일반 UI/Reaction 효과음은:

```text
SFXManager.Instance.PlayOneShot(...)
```

을 재사용한다.

현재 사용 예:

- Reaction 느낌표
- MainMenu Cursor 이동
- Choice Select 이동
- 아이템/저장 관련 효과음

캐릭터 타자음용 AudioSource는 별도로 유지한다.

## 16. Field Enemy / Battle 진입

기존 관련 파일:

- `EnemyController.cs`
- `EnemyData`
- `BattleTransitionEffect.cs`

현재 전투 진입/복귀 구조는 다음 Battle 작업에서 실제 코드와 Inspector 연결을 다시 분석한다.

이 문서의 과거 필드 적 설명만을 근거로 EnemyController/BattleTransitionEffect를 삭제하거나 재설계하지 않는다.

## 17. BGM

Town BGM은 `BGMManager`를 사용한다.

현재 필요한 기능:

- Play / Stop
- Pause
- Resume
- Resume with Fade
- Fade Out

전화 저장/시스템 메시지처럼 짧은 징글을 강조할 때는 전체 BGM을 Stop하지 않고 Pause/Resume 흐름을 사용한다.

Battle용 BGM은 별도 AudioSource를 유지한다.
