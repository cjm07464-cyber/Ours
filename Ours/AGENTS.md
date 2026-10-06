# AGENTS.md

이 문서는 이 저장소를 수정하는 AI 코딩 에이전트용 작업 규칙이다.

## 1. 시작 절차

작업 전 필요한 문서만 읽는다.

1. `README.md`
2. `Docs/ARCHITECTURE.md`
3. 작업 대상 문서 1개
4. 위험/정리 작업이면 `Docs/RISK_REGISTER.md`

불필요하게 모든 문서를 읽어 토큰을 소모하지 않는다.

문서와 실제 구현이 다르면 **현재 프로젝트의 코드/씬/Inspector 구조를 우선 확인**한다. 특히 Battle 관련 작업은 과거 문서만 믿고 수정하지 않는다.

## 2. 기본 작업 원칙

- 요청 범위 밖의 리팩터링 금지.
- 동작 중인 씬/시스템의 구조를 임의로 갈아엎지 않는다.
- 가능한 최소 파일만 수정한다.
- Scene/Prefab/YAML 직접 수정은 사용자가 명시적으로 요청하지 않는 한 금지.
- 기존 스크립트는 삭제/재생성하지 않는다.
- 파일명, public class명, `.meta` 보존.
- 기존 SerializedField 이름 변경은 최소화한다.
- Inspector에서 이미 저장된 값은 코드 기본값 변경으로 갱신되지 않는다는 점을 항상 고려한다.
- 새 SerializedField를 추가했다면 **어느 GameObject의 어떤 컴포넌트에 무엇을 연결해야 하는지 반드시 보고**한다.
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

현재 기본:

- 방향키 — 이동 / UI 선택
- `C` — 확인 / 결정
- `X` — 취소 / 뒤로가기
- `Z` — Town 필드 메뉴 열기 / 닫기
- `0` / Numpad `0` — 개발용 저장 파일 삭제

주의:

- MainMenu는 `Z`로 열고 닫으며, `C`로 선택한다.
- Dialogue 진행 입력과 메뉴 입력이 겹치지 않도록 `DialogueController` 활성 상태를 확인한다.
- 개발용 `0` 저장 삭제 단축키는 유지한다.
- 텍스트 입력창(TMP_InputField)에서 입력 충돌 여부를 확인한다.
- 새 Input System으로 성급하게 마이그레이션하지 않는다. 현재 입력 계층을 유지한다.

## 5. 첫 실행 / Title / Forest 규칙

기술적 엔트리 씬은 `TitleScene`이다.

- 유효 저장 없음 + 이름 선택 세션 없음 → 즉시 `ForestScene`
- 유효 저장 있음 → Forest 건너뛰고 기존 Title 흐름
- Forest에서 이름 선택 완료(`NameChosen`) → `TitleScene`
- Title의 New Game은 pending name을 사용해 현재 게임 흐름으로 진입

Forest 전용 연출을 일반 시스템으로 무리하게 승격하지 않는다.

특히:

- `ForestPrologueEvent`는 Forest 프롤로그 연출 담당.
- `ForestNameEntryController`는 Forest 이름 입력 전용.
- `TitleManager`의 옛 이름 입력/시놉시스 코드는 레거시 호환 때문에 당장 삭제하지 않는다.

## 6. Town PersistentUI 규칙

Town의 공통 UI/이벤트 실행기는 `PersistentUI` 아래에 둔다.

핵심 구성:

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
      └ ChoicePanel
```

규칙:

- `PersistentUI` 루트는 활성 상태를 유지한다.
- `MainMenuManager`, `DialogueRunner`, `DialogueController`처럼 실행을 담당하는 컴포넌트를 비활성 UI 자식에 두지 않는다.
- `PhoneEventRunner` GameObject도 활성 상태를 유지한다. 비활성 GameObject에서는 Coroutine을 시작할 수 없다.
- `DialogueUI`, `MainMenuUI`, `ChoicePanel`은 필요할 때만 표시한다.
- `ChoiceUI` 루트는 활성 상태, `ChoicePanel`은 기본 비활성 상태를 기준으로 한다.
- `MainMenuManager.Main Menu UI`에는 반드시 `MainMenuUI`를 연결한다. `DialogueUI`를 잘못 연결하지 않는다.

## 7. Dialogue / Event 시스템 규칙

### Dialogue

주요 구성:

- `CharacterData` — 이름, 필드 스프라이트, 표정 portrait, 캐릭터별 타자음.
- `DialogueSequence` — 대사 줄 데이터.
- `DialogueController` — Basic/Portrait UI, 타자 출력, 페이지 처리.
- `DialogueRunner` — DialogueSequence 실행 및 토큰 치환.

현재 토큰에는 최소 다음이 사용된다.

- `{player}`
- `{level}`
- `{expToNextLevel}`

DialogueLine에는 시작/종료 사운드 관련 옵션이 존재한다.

- `Line Start Sound`
- `Stop Line Start Sound On Advance`
- `Line End Sound`
- `Line End Sound Volume`
- `Wait For Line End Sound`
- `Pause Bgm During Line End Sound`
- `Bgm Resume Fade Duration`

캐릭터 타자음과 일반 SFX를 한 AudioSource로 합치지 않는다.

### Game Event

주요 구성:

- `GameEventSequence` — 이벤트 Step 데이터.
- `GameEventRunner` — Step 실행기.
- `StoryEventTrigger` — 위치 진입 등으로 이벤트 시작.
- `StoryFlagGate` — 스토리 플래그 조건에 따라 통과/차단.
- `NPCInteraction` — NPC 상호작용 및 플래그별 대체 대화.

현재 Step 유형:

- `Dialogue`
- `GiveItem`
- `SetStoryFlag`
- `Reaction`
- `SystemMessage`
- `Choice`
- `SaveGame`
- `QuitGame`

`GameEventRunner`는 이벤트별 GameObject에 여러 개 존재해도 정상이다. 예: HouseExitGate, FatherPhoneCallTrigger, PhoneEventRunner. 단, 각 Runner의 Inspector 참조는 그 이벤트 목적에 맞게 별도로 연결한다.

## 8. Town 스토리 이벤트 기준

### 엄마 아침 이벤트

핵심 플래그:

```text
mother_morning_talk
```

- 엄마 첫 대화/아이템 지급 완료 후 플래그 설정.
- 이후 엄마 상호작용은 기본 대화 Sequence로 전환.
- 집 출구의 `StoryFlagGate`는 이 플래그가 없으면 차단 이벤트를 실행한다.
- 차단 후 Player는 `AutoMoveTo()`로 ReturnPoint까지 이동한다.

### 첫 아빠 전화

핵심 플래그:

```text
father_phone_call_done
```

- 집 밖 첫 전화 이벤트 완료 후 설정.
- 이 플래그가 Town 메인 메뉴 사용 조건이다.

## 9. Main Menu / 전화 저장 규칙

`MainMenuManager`:

- `Z` — 메뉴 열기/닫기
- 방향키 — 항목 이동
- `C` — 선택
- `X` — 닫기

메뉴 항목에는 현재 상태/장비/가방/통화/닫기가 있다.

통화 선택 시:

- `Phone Event Runner` → `PhoneEventRunner`의 `GameEventRunner`
- `Phone Event Sequence` → 전화 저장 흐름의 시작 `GameEventSequence`

전화 저장 흐름은 Dialogue → Choice → SaveGame → 추가 Choice → 계속/종료 분기로 구성한다.

`QuitGame` Step은 BGM과 검정 FadeOverlay를 페이드한 뒤 Build에서 `Application.Quit()`을 호출한다. Editor에서는 종료 대신 개발 로그를 남긴다.

## 10. 저장 / GameManager 규칙

저장 수정 시 같이 확인:

- `GameManager.cs`
- `SaveData.cs`
- `SaveSystem.cs`
- `PlayerLoader.cs`
- 저장을 호출하는 메뉴/이벤트 코드

현재:

- `GameManager`는 Runtime Bootstrap으로 어느 씬에서 직접 Play해도 생성 가능.
- `SaveSystem.HasValidSaveData()` 사용.
- `StartupSessionState`와 `pendingPlayerName`은 런타임 세션 정보이며 저장 파일과 별개.
- Story flag / Inventory / 장착 무기 정보도 GameManager/SaveData 흐름과 함께 확인한다.
- `SaveGame` Event Step은 현재 씬/플레이어 위치를 반영한 뒤 `SaveSystem.SaveGame()`을 호출한다.
- 개발용 `0` 키는 디스크의 savefile 삭제용이며 현재 실행 중 Runtime 상태 전체를 즉시 초기화하는 기능은 아니다.

## 11. BGM / SFX 규칙

- Town의 `BGMManager`는 Pause/Resume 및 Fade 기능을 제공한다.
- 저장완료 징글처럼 특정 Line End Sound가 재생될 때 BGM을 Pause하고, 종료 후 Fade-in으로 Resume할 수 있다.
- `SFXManager.Instance.PlayOneShot()`을 공용 UI/Reaction SFX에 사용한다.
- 개별 효과음이 유독 큰 경우 전체 AudioSource 볼륨이 아니라 `volumeScale`로 해당 클립만 조정한다.
- 메뉴 Cursor와 Choice Select 이동음은 실제 선택 인덱스가 변경될 때만 재생한다.
- Battle BGM에는 Town용 `BGMManager`를 붙이지 않는다.

## 12. 전투 규칙

현재 전투 문서는 실제 전투 진입 흐름 재점검 전이므로, Battle 관련 수정은 **실제 코드/씬 참조를 먼저 분석**한 뒤 진행한다.

현재 안전선:

- `BattleManager`는 행동 실행 중심.
- 커맨드 UI 입력: `CommandSelector`.
- 스킬 UI 입력: `SkillSelector`.
- `BattleManager.commandText` 레거시 경로는 사용하지 않는다.
- BattleManager Inspector의 `Command Text`는 `None` 유지.
- `EnemyData` / `SkillData`는 원본 데이터 역할을 한다.

BattleScene 관련 정리 작업은 `Docs/RISK_REGISTER.md` 확인 후 진행한다.

## 13. 작업 완료 보고

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
