# FIELD_SYSTEMS

TownScene과 일반 탐험에 필요한 Player, Camera, Menu, Enemy, BGM 기준.

## 1. 입력

일반 필드:

- 방향키 — 이동
- `A` — 메뉴 열기/닫기
- `C` — 메뉴 결정
- `X` — 취소/뒤로가기

텍스트 입력 중 필드 단축키가 오작동하지 않도록 주의한다.

## 2. Player

Town과 Forest Player 구현은 현재 별도다.

Forest에서 검증된 이동/카메라 감각을 Town에 적용하더라도 스크립트를 성급하게 통합하지 않는다.

### Town PlayerController 주의

현재 구조에는 Rigidbody2D가 있으면서 Transform 직접 이동을 사용하는 부분이 있을 수 있다.

동작 중이라면 카메라 통일 작업과 함께 물리 이동까지 동시에 리팩터링하지 않는다. 충돌/전투 진입 회귀 위험이 있다.

## 3. CameraFollow

공용 `CameraFollow`는 다음 기능을 가진다.

- target
- smooth follow
- instant follow
- followX / followY
- offset
- `SnapToTarget()`

일반 탐험 카메라의 목표 감각:

- 플레이어가 먼저 움직이고 카메라가 뒤늦게 따라오는 느낌을 피한다.
- 씬 로드/텔레포트 직후에는 Fade In 전에 `SnapToTarget()`으로 미리 맞춘다.
- 평상시 이동은 필요하면 instant follow 사용.

Map4의 Y-only Zone 연출은 Forest 전용이며 Town에 강제로 복사하지 않는다.

## 4. Main Menu

관련:

- `Assets/Scripts/Main/MainMenuManager.cs`

기본 입력:

```text
A = 열기/닫기
C = 실행
X = 취소/닫기
방향키 = 항목 이동
```

메뉴가 열리면 기존 구조상 `Time.timeScale = 0`을 사용할 수 있다.

Player/Enemy가 timeScale 0 상태에서 입력/이동하지 않는지 유지한다.

현재 메뉴 UI가 런타임 생성 방식이라면, 디자인 전면 개편 전까지 작동 중인 구조를 무리하게 Prefab 기반으로 바꾸지 않는다.

## 5. 저장

메뉴 저장 시 최소 흐름:

```text
현재 씬명
Player 위치
Player 방향
↓
GameManager 반영
↓
SaveSystem.SaveGame()
```

저장 구조 상세: `SAVE_AND_STATE.md`

## 6. Field Enemy

관련:

- `EnemyController.cs`
- `EnemyData` assets
- `BattleTransitionEffect.cs`

일반 흐름:

```text
배회
↓ Player detect radius
추적
↓ 접촉
GameManager에 EnemyData/encounter 정보 저장
↓
BattleTransitionEffect
↓
BattleScene
```

### encounterId

각 필드 적 인스턴스마다 고유해야 한다.

복제 시 ID 중복에 주의한다.

### 도망 후

- 해당 enemy ID를 `escapedEnemyId`로 식별.
- 약 3초간 이동/접촉 무시.
- Collider 비활성/깜빡임 등 기존 로직 유지.

### 승리 후

- 해당 enemy ID를 `defeatedEnemyId`로 식별.
- Town 복귀 시 일시적으로 숨김.
- 약 10초 후 원위치/정상 상태 복귀.

현재 이 적 상태는 장기 저장용 시스템이 아니라 런타임 처리다.

## 7. BGM

Town:

- `BGM_Manager` + `BGMManager`.
- Battle 진입 시 `PauseBGM()`.
- Town 복귀 시 `ResumeBGM()`.

게임 종료/Title 복귀 시 persistent BGM이 남지 않도록 기존 정리 메서드를 사용한다.

Battle_BGM에는 Town용 BGMManager를 붙이지 않는다.

## 8. Scene Return

Battle 진입 전에 보통 아래 정보를 보관한다.

- return scene
- return player position
- current battle enemy
- encounter ID

Battle 종료 후 Town으로 복귀하면 PlayerLoader/SceneFadeIn 등 기존 복원 흐름을 사용한다.

## 9. 앞으로 공통화해도 좋은 것

필요가 명확해졌을 때만 진행:

- 범용 DialogueController
- 일반 탐험 CameraFollow 규칙
- 공통 입력 wrapper
- 공통 Fade 서비스

현재는 각 씬이 정상 동작하는 것이 우선이다.
