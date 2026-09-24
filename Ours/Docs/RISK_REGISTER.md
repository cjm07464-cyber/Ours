# RISK_REGISTER

현재 프로젝트에서 **이름만 보고 삭제/리팩터링하면 위험한 부분**을 모아둔다.

우선순위는 "당장 수정"이 아니라 "건드릴 때 조심"이다.

## High — 작은 안전 수정 후보

### BattleManager GameOver coroutine 중복

과거 코드 점검에서 게임오버 조건이 반복 평가될 경우 coroutine이 중복 실행될 가능성이 확인됐다.

권장:

- bool guard / coroutine handle 방식의 최소 방어.
- 게임오버 전체 구조 리팩터링은 분리.

### BattleManager commandText 레거시

현재 실제 커맨드 입력은 `CommandSelector`가 담당한다.

`commandText` 관련 코드가 남아 있어도 Inspector에서는 None을 유지한다.

삭제 전 반드시 실제 실행 경로와 참조 여부 확인.

## Medium — 구조 개선 후보지만 지금 건드리기 위험

### Town PlayerController + Rigidbody2D

Transform 직접 이동과 Rigidbody2D가 혼용된 구조가 있을 수 있다.

물리적으로 더 올바른 구조로 바꾸고 싶더라도:

- Collision
- Enemy 접촉
- Battle 진입
- PlayerLoader 위치 복원

회귀가 발생할 수 있으므로 마감 중에는 대규모 변경 금지.

### EnemyController 의존성 탐색

Start에서 Player/BattleEffect 등을 `Find` 계열로 찾는 코드가 있을 수 있다.

장기적으로 Inspector/DI 구조가 깔끔하지만 현재 동작 중이면 기능 작업과 동시에 바꾸지 않는다.

### MainMenuManager Player 탐색

Tag/이름/FindObjectOfType 계열에 의존할 수 있다.

메뉴 개편 때 별도 정리.

## Title / Startup 레거시

### BootSceneController 이름

씬은 `TitleScene`이지만 클래스/파일은 `BootSceneController`.

**rename 금지 우선.**

이유: Inspector Missing Script 위험.

### TitleManager 레거시 이름 입력

ForestNameEntryController가 새 이름 입력을 담당하지만 TitleManager의 예전 NewGame 이름 입력/시놉시스가 남아 있다.

현재 Title 흐름에서 직접 호출되는지 확인 전 삭제 금지.

### Scene 내 GameManager

Runtime Bootstrap이 추가된 이후 TitleScene 등 기존 씬의 GameManager 오브젝트가 중복일 수 있다.

장기 삭제 후보지만:

- 씬 GameManager Inspector에 특별한 설정값이 없는지 확인
- Runtime bootstrap 새 인스턴스로 동일하게 동작하는지 QA

후 제거한다.

## Forest 호환 필드

### nightOverlayAlpha

NightOverlay/FadeOverlay 역할 분리 후 기존 `nightOverlayAlpha`가 호환용으로 남을 수 있다.

현재 FadeOverlay 복귀값으로 다시 사용하지 않는다.

### Forest 연출 SerializedField

ForestPrologueEvent는 Inspector 연결이 많다.

- 카메라
- Follow
- Light
- Smoke
- RedSign
- Audio
- Alien Sprite/타이밍
- NameEntry

이 필드 이름을 정리 목적으로 변경하지 않는다.

## Legacy/Test 정리 후보

실제 참조 확인 후 릴리즈 전에 판단:

- `PlayerManager`
- `MainSceneSaveTester`
- `GitTest`
- 오래된 PlayerStats 계열
- BattleTransitionEffect의 과도한 Debug.Log / 불필요 public field
- 사용하지 않는 과거 Title UI prefab/script

**파일명이 테스트처럼 보여도 바로 삭제하지 않는다.** Scene/Prefab 참조를 먼저 확인한다.

## MainMenu runtime UI

런타임에서 UI를 생성하는 구조는 유지보수 관점에서 정리 후보지만, 현재 정상 동작 중이라면 Title/Town polish와 동시에 교체하지 않는다.

## 카메라 일반화 주의

Forest Map4의 Zone 기반 Y-only 카메라는 특수 연출이다.

일반 Town 카메라에 필요한 것은 보통:

- 씬 시작 전 Snap
- 평상시 instant follow

Forest 특수 Zone 로직을 공용 카메라에 억지로 포함시키지 않는다.

## 입력 중앙화

C/X/A 통일은 완료됐지만 각 스크립트의 직접 Input 호출이 남아 있다.

향후 키 리바인딩/게임패드가 실제 요구가 될 때 `GameInput` 또는 새 Input System 전환을 검토한다.

지금은 단순 중앙화만을 위한 대규모 수정은 하지 않는다.
