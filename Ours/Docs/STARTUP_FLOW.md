# STARTUP_FLOW

ForestScene과 TitleScene, 새 게임 시작 상태를 다룬다.

## 1. 현재 엔트리 규칙

Build에서 첫 씬은 `TitleScene`을 기준으로 한다.

TitleScene 시작 직후 기존 타이틀 연출을 무조건 시작하지 않는다.

```text
SaveSystem.HasValidSaveData()
StartupSessionState
```

를 확인한다.

### 유효 저장 있음

- ForestScene 스킵.
- 기존 Title 크레딧/타이틀/메뉴 정상 진행.
- Continue 사용 가능.

### 유효 저장 없음 + NameChosen 아님

- 타이틀 BGM/인트로를 보여주기 전에 ForestScene으로 라우팅.

### NameChosen

- 저장 파일이 없어도 Forest 재진입 금지.
- Title 크레딧/타이틀/메뉴 진행.
- New Game 선택 시 `pendingPlayerName`으로 시작.

실행을 완전히 종료하고 저장도 하지 않았다면 startup session은 사라진다. 다음 실행에는 Forest를 다시 보는 것이 의도된 동작이다.

## 2. TitleScene

주요 스크립트:

- `Assets/Scripts/Title/BootSceneController.cs`
- `Assets/Scripts/Title/TitleManager.cs`
- `Assets/Scripts/Title/GameManager.cs`
- `Assets/Scripts/Title/SaveSystem.cs`

### BootSceneController

현재 TitleScene의 주 컨트롤러지만 파일/클래스명은 과거 명칭을 유지한다.

담당:

- 첫 실행 Forest 라우팅
- 크레딧/지구/타이틀 연출
- 메뉴 선택
- Continue
- pending name New Game 처리

**파일/클래스를 TitleSceneController로 rename하지 않는다.** Inspector Missing Script 위험이 있다.

### TitleManager

과거 이름 입력/시놉시스 기능이 남아 있다.

Forest 이름 입력은 `ForestNameEntryController`가 담당하므로 새 Forest 흐름을 위해 TitleManager를 확장하지 않는다.

레거시 코드는 실제 참조를 확인하기 전 삭제하지 않는다.

## 3. ForestScene 구조

개념적 Hierarchy:

```text
ForestScene
├ Main Camera
├ Player
│  ├ PlayerNightLight
│  └ ReactionIcon
├ Grid / Tilemaps
├ MapPoints
├ CameraPoints
│  ├ CameraPoint_1..4
│  └ UFOFocusPoint
├ Transitions
│  ├ Map1To2
│  ├ Map2To3
│  └ Map3To4
├ UFO
│  ├ UFO_Smoke
│  └ world RedSign marker
├ Alien
│  └ LightFocusPoint
├ Map4Event
│  ├ AlienEventTrigger
│  └ Map4CameraFollowZone
├ Canvas
│  ├ NightOverlay
│  ├ RedSign (UI)
│  ├ NameEntryPanel
│  └ FadeOverlay
└ EventSystem
```

실제 sibling 순서는 화면 가림 우선순위를 확인해 유지한다.

## 4. Forest 시작

시작 연출:

```text
FadeOverlay alpha 1
Player 이동 잠금
↓
낙하 SFX
↓ 0.5초
불시착 SFX
↓
두 SFX가 모두 끝난 뒤
FadeOverlay 1 → 0
↓
Player 이동 허용
```

`NightOverlay`는 별도 밤 분위기 레이어이며 Fade에 사용하지 않는다.

## 5. Forest Player

`ForestPlayerController`는 Town PlayerController와 별도다.

걷기 스프라이트 배열:

```text
0 = idle
1 = 한쪽 발
2 = 중간
3 = 반대쪽 발

걷기 = 1 → 2 → 3 → 2
정지 = 0
```

발소리:

- frame 1 → Footstep A
- frame 3 → Footstep B
- frame 2 / 정지 → 없음

짧은 one-shot WAV를 사용한다.

## 6. Map 전환

`ForestMapTransition`.

```text
Player lock
FadeOverlay 0 → 1
↓ 완전 검정
Player/Camera 이동
↓
FadeOverlay 1 → 0
Player unlock
```

Fade 완료 전 다음 맵 Camera/Player 위치를 노출하지 않는다.

Map3To4에서 CameraFollow를 즉시 켜지 않는다. Map4의 Follow는 전용 Zone이 담당한다.

## 7. Map4 Camera

`Map4CameraFollowZone`.

기본:

- CameraPoint_4 고정.

Zone 진입:

- X = CameraPoint_4.x 고정.
- Y만 Player를 smooth catch-up.
- Y가 threshold 이내로 따라잡으면 instant follow.
- 이후 Player Y와 지연 없이 동기화.

Zone 이탈:

- instant follow 해제.
- CameraPoint_4.y로 부드럽게 복귀.
- 복귀 중 재진입하면 복귀 Tween 취소 후 다시 catch-up.

## 8. UFO / Alien 프롤로그

담당: `ForestPrologueEvent`.

큰 흐름:

```text
AlienEventTrigger
↓
Player lock
CameraFollow OFF
↓
Camera Y → UFOFocusPoint
PlayerNightLight Y → LightFocusPoint
↓
짧은 reactionDelay
ReactionIcon + SFX
↓
Smoke / RedSign
↓
UFO shutdown SFX + RedSign 소등
↓
Alien Up → Side → Front
↓
긴장 BGM
↓
BGM 종료 직전 Walk sprite + 실제 아래 이동
↓
BGM 종료 시점 암전
↓
NameEntry
```

### Smoke

한 사이클:

```text
smoke1 → smoke2 → smoke3 → OFF
```

- smoke2/smoke3은 local Y offset 사용.
- OFF 시 sprite null + base localPosition 복귀.
- cycles는 Inspector 조율.

### RedSign

월드 SpriteRenderer가 아니라 Canvas의 UI Image를 사용해 NightOverlay 위에서 밝게 보이게 한다.

- 월드 위치 marker를 UI가 추적하는 구조 사용 가능.
- Smoke 동안 alpha blink.
- Smoke 종료 후 shutdown SFX 길이에 맞춰 1 → 0.

### Alien timing

현재 핵심 필드:

- Up/Side/Front durations
- `frontAudioDelay`
- `tensionFadeDuration`
- `tensionClimaxLeadTime`
- walk pose/move duration/offset

연출 의도:

- Front 후 잠깐 정적.
- 긴장음 시작.
- 끝나기 직전에 발을 내밀며 실제로 Player 방향(화면 아래)으로 움직임.
- 음악 종료 시점에는 이동 완료를 기다리지 않고 암전 가능.

세부 값은 Inspector에서 귀로 조율한다.

## 9. Night / Fade / 2D Light

### NightOverlay

- Forest 분위기 전용.
- 남색 반투명.
- 코드 Fade 대상 아님.

### FadeOverlay

- 검정 암전 전용.
- 평상시 alpha 0.

### 2D Light

- `ForestNightGlobalLight`: Global Light 2D로 푸른 밤 톤.
- `PlayerNightLight`: Player 자식 Point Light 2D.
- 외계인 이벤트에서 PlayerNightLight transform Y를 Alien의 LightFocusPoint로 이동해 손전등처럼 보이게 한다.

NightOverlay와 Global Light가 둘 다 너무 강하면 이중으로 어두워질 수 있으므로 최종 톤은 Inspector에서 조절한다.

## 10. 이름 입력

담당: `ForestNameEntryController`.

흐름:

```text
이 아이의 이름은?
↓ 입력
이게 좋을까?
↓ Yes / No
```

Yes:

- `GameManager.MarkForestCompleted()`
- `SetPendingPlayerName(name)`
- 화면 Fade Out
- Forest BGM volume Fade to 0
- 완전 암전 후 `TitleScene`

이름 입력 폰트는 별도 TMP Font Asset을 사용할 수 있다.

## 11. Forest 완료 기준

ForestScene은 기능 추가 대상보다 **완료된 프롤로그 씬**으로 취급한다.

이후 수정은 아래만 허용하는 방향을 권장한다.

- 버그 수정
- 볼륨/타이밍/색/위치 미세 조율
- Dialogue UI 완성 후 후퇴 방지 문구 연결

대규모 구조 변경은 피한다.
