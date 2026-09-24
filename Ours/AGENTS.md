# AGENTS.md

이 문서는 이 저장소를 수정하는 AI 코딩 에이전트용 작업 규칙이다.

## 1. 시작 절차

작업 전 필요한 문서만 읽는다.

1. `README.md`
2. `Docs/ARCHITECTURE.md`
3. 작업 대상 문서 1개
4. 위험/정리 작업이면 `Docs/RISK_REGISTER.md`

불필요하게 모든 문서를 읽어 토큰을 소모하지 않는다.

## 2. 기본 작업 원칙

- 요청 범위 밖의 리팩터링 금지.
- 동작 중인 씬/시스템의 구조를 임의로 갈아엎지 않는다.
- 가능한 최소 파일만 수정한다.
- Scene/Prefab/YAML 직접 수정은 사용자가 명시적으로 요청하지 않는 한 금지.
- 기존 스크립트는 삭제/재생성하지 않는다.
- 파일명, public class명, `.meta` 보존.
- 기존 SerializedField 이름 변경은 최소화한다.
- Inspector에서 이미 저장된 값은 코드 기본값 변경으로 갱신되지 않는다는 점을 항상 고려한다.
- 수정 후 `dotnet build Ours.sln` 실행.

## 3. 현재 씬 기준

정식 명칭:

- `ForestScene`
- `TitleScene`
- `TownScene`
- `BattleScene`

레거시 문자열 호환은 `GameManager.NormalizeSceneName()` 등에서 처리할 수 있다.

- `Title` / `BootScene` → `TitleScene`
- `MainScene` → `TownScene`

`BootSceneController.cs` / `BootSceneController` 클래스명은 **Inspector 안전을 위해 현재 유지**한다. 씬 이름이 `TitleScene`이라고 해서 파일/클래스를 임의로 rename하지 않는다.

## 4. 입력 규칙

게임 전체 기본:

- 방향키 — 이동/UI 선택
- `C` — 확인/결정/대화 진행
- `X` — 취소/뒤로가기
- `A` — 필드 메뉴

주의:

- 개발용 `0` 저장 삭제 단축키는 유지.
- 텍스트 입력창(TMP_InputField)에서 C/X/A 입력 충돌 여부를 확인한다.
- 새 Input System으로 마이그레이션하지 않는다. 현재 입력 방식은 기존 `Input` API를 유지한다.

## 5. 첫 실행 / Title / Forest 규칙

기술적 엔트리 씬은 `TitleScene`이다.

- 유효 저장 없음 + 이름 선택 세션 없음 → 즉시 `ForestScene`
- 유효 저장 있음 → Forest 건너뛰고 기존 Title 흐름
- Forest에서 이름 선택 완료(`NameChosen`) → `TitleScene`
- Title의 New Game은 pending name을 사용해 현재는 `TownScene`으로 시작

Forest 전용 연출을 일반 시스템으로 무리하게 승격하지 않는다.

특히:

- `ForestPrologueEvent`는 Forest 프롤로그 연출 담당.
- `ForestNameEntryController`는 Forest 이름 입력 전용.
- `TitleManager`의 옛 이름 입력/시놉시스 코드는 레거시 호환 때문에 당장 삭제하지 않는다.

## 6. Forest 카메라 규칙

- Map1~3: 고정 카메라.
- Map4: `Map4CameraFollowZone`에서 Y축만 추적.
- Zone 진입: smooth catch-up → 따라잡으면 instant follow.
- X축은 `CameraPoint_4.x` 고정.
- Zone 이탈: fixed point로 부드럽게 복귀.
- UFO 이벤트: CameraFollow만 끄고 Main Camera/Camera 컴포넌트는 끄지 않는다.

`CameraFollow`는 일반 기능을 보존한다.

- `SnapToTarget()` 존재.
- `instantFollow`, `followX`, `followY` 옵션 존재.
- Town 등 일반 탐험 씬에서 재사용 가능.

## 7. Forest 페이드/밤 규칙

역할 분리:

- `NightOverlay` — Forest 밤 분위기. 코드 제어하지 않는 고정 남색 반투명 Image.
- `FadeOverlay` — 검정 화면 전환 전용.

FadeOverlay:

- Forest 시작: `1 → 0`
- 맵 전환: `0 → 1 → 0`
- 이름 확정: `0 → 1`

`nightOverlayAlpha` 필드가 남아 있어도 호환용이다. FadeOverlay 복귀값으로 다시 사용하지 않는다.

## 8. 전투 규칙

- `BattleManager`는 행동 실행 중심.
- 커맨드 UI 입력: `CommandSelector`.
- 스킬 UI 입력: `SkillSelector`.
- `BattleManager.commandText` 레거시 경로는 사용하지 않는다.
- BattleManager Inspector의 `Command Text`는 `None` 유지.
- `EnemyData`가 적 원본 데이터.
- `SkillData`가 스킬 원본 데이터.
- 필드 `EnemyController`가 전투 진입 전 `GameManager.currentBattleEnemy` 등을 설정한다.

BattleScene 관련 정리 작업은 `Docs/RISK_REGISTER.md` 확인 후 진행한다.

## 9. 저장 / GameManager 규칙

저장 수정 시 같이 확인:

- `GameManager.cs`
- `SaveData.cs`
- `SaveSystem.cs`
- `PlayerLoader.cs`
- 저장을 호출하는 메뉴 코드

현재:

- `GameManager`는 Runtime Bootstrap으로 어느 씬에서 직접 Play해도 생성 가능.
- `SaveSystem.HasValidSaveData()` 사용.
- `StartupSessionState`와 `pendingPlayerName`은 런타임 세션 정보이며 저장 파일과 별개.
- 저장 데이터가 없고 앱을 종료하면 다음 실행에 Forest를 다시 보는 것이 의도된 동작.

## 10. BGM 규칙

- Town의 `BGMManager`는 전투 진입 시 Pause, 복귀 시 Resume.
- Battle_BGM에 Town용 `BGMManager`를 붙이지 않는다.
- Forest는 독립 AudioSource들을 사용하며 프롤로그 연출과 결합되어 있다.
- 이름 확정 후 Forest → Title 전환 시 Forest BGM은 화면 Fade와 함께 0으로 줄어든다.

## 11. 작업 완료 보고

길게 설명하지 말고 아래만 보고한다.

```text
수정 파일:
- ...

핵심 변경:
- ...

Inspector에서 확인할 것:
- ...

빌드:
- 경고 N / 오류 N

주의 또는 충돌 가능성:
- 있을 때만 작성
```
