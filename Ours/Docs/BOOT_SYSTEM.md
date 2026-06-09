# BOOT_SYSTEM

BootScene 인트로 / 타이틀 / 이름 입력 시스템 문서.

## 현재 상태

현재 프로젝트에서는 실제 씬 파일명은 아직 `Title`을 사용하지만, 역할은 목표 명칭 기준 `BootScene`이다.

주의:
- 실제 씬 파일명 변경은 아직 완료되지 않았다.
- 목표 명칭은 `BootScene`, `TownScene`, `BattleScene`이다.
- 씬 이름을 실제로 바꿀 때는 문자열 참조와 저장 데이터 호환 처리를 함께 확인한다.

## 담당 기능

BootScene/Title 씬이 담당하는 기능:

- 시작 크레딧 인트로
- 지구 로고 연출
- `Ours` 타이틀 표시
- 처음부터 / 이어하기 / 종료 메뉴
- 이름 입력 흐름
- 저장 데이터 유무에 따른 이어하기 활성/비활성 처리
- 이어하기 선택 시 페이드아웃 후 저장된 씬 이동

## 현재 연출 흐름

```text
검은 화면
→ Boot_BGM 재생
→ IntroCreditGroup 크레딧 1 표시
→ IntroCreditGroup 크레딧 2 표시
→ IntroCreditGroup 크레딧 3 표시
→ 지구 이미지 페이드 인
→ 지구 이미지가 작아지며 O 위치로 이동
→ UrsText 페이드 인
→ StudentCreditText 페이드 인
→ MenuGroup 표시
```

현재 크레딧 문구:

```text
Created at
────────
Pai Chai University

Directed by
────────
DAVID

Inspired by
────────
MOTHER
```

## 입력

- 방향키 위/아래: 메뉴 선택
- Z: 결정
- X: 인트로/지구 연출 스킵
- 인트로 진행 중 Z 또는 X: 현재 크레딧 연출을 빠르게 넘김
- 지구 연출 중 Z 또는 X: 지구 연출을 완료 상태로 만들고 메뉴 표시

## 현재 Canvas / UI 구조

현재 Title 씬 기준 권장 구조:

```text
Canvas
├── FadeOverlay
├── TitleManagerUI
│   ├── ContinuePanel
│   ├── NewGamePanel
│   ├── ConfirmResetPanel
│   └── IntroPanel
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

중요:
- `TitleManagerUI`는 `FadeOverlay` 다음 순서에 둔다.
- `FadeOverlay`는 시작 크레딧 배경과 이어하기 페이드아웃에 사용한다.
- 이어하기 페이드아웃 시 `BootSceneController`가 `FadeOverlay.transform.SetAsLastSibling()`으로 화면 최상단에 올린다.
- `IntroCreditGroup`은 `CanvasGroup`으로 전체 페이드 인/아웃한다.
- `CreditTopText`, `CreditBottomText`, `CreditLine` 색상은 Inspector에서 직접 조정한다.
- `TitleGroup` 안의 `StudentCreditText`는 타이틀 메뉴 하단 서명 역할이다.

## BootSceneController 주요 연결

```text
Existing Title Flow
- Title Manager
- Use Title Manager Name Input
- Town Scene Name

Audio
- Boot Bgm Source
- Play Bgm On Start

Intro
- Fade Overlay
- Intro Text
- Intro Credit Group
- Credit Top Text
- Credit Bottom Text
- Credit Line
- First Intro Text
- Second Intro Text
- Third Intro Text
- Text Fade Duration
- Text Hold Duration
- Play Intro On Start
- Allow Intro Skip

Title
- Title Group
- Earth Image
- Earth Image Graphic
- Urs Text
- Student Credit Text
- Earth Fade Duration
- Title Text Fade Duration
- Allow Earth Skip

Earth Animation
- Earth Start Anchored Position
- Earth Target Anchored Position
- Earth Start Scale
- Earth Target Scale
- Earth Move Duration
- Earth Hold Duration

Menu
- Menu Group
- Selector
- New Game Text
- Continue Text
- Quit Text
- Enabled Menu Color
- Disabled Menu Color

Continue Fade
- Continue Fade Out Duration
```

## 지구 연출 역할 분리

```text
EarthImage Animator
→ 지구 자전 프레임 애니메이션 담당

BootSceneController
→ 지구 페이드 인
→ 지구 위치 이동
→ 지구 스케일 축소
→ 스킵 시 최종 상태 적용
```

주의:
- `BootSceneController`에서 지구 자체를 RectTransform.Rotate로 굴리지 않는다.
- 자전은 `EarthImage`의 Animator가 담당한다.
- 위치/스케일/페이드는 `BootSceneController`가 담당한다.

## 처음부터 처리

처음부터 선택 시:

```text
MenuGroup 비활성
TitleGroup 비활성
Boot 타이틀 UI 알파 정리
TitleManager.BeginNewGameNameInput()
→ "이 아이의 이름은?" 패널 표시
→ 이름 확정
→ 시놉시스 시작
→ 시놉시스 종료 후 MainScene/TownScene 이동
```

주의:
- 이름 입력 흐름은 기존 `TitleManager`를 재사용한다.
- Boot_BGM이 이미 재생 중이면 시놉시스 시작 시 처음부터 다시 재생하지 않고 이어서 재생한다.
- 시놉시스 종료 후 마을 씬으로 이동할 때 기존 BGM 전환 흐름을 따른다.

## 이어하기 처리

이어하기 선택 시:

```text
SaveSystem.HasSaveData()
→ 저장 데이터가 있으면 Continue 활성화
→ Z 입력 시 FadeOverlay 페이드아웃
→ SaveSystem.LoadGame()
→ GameManager.currentSceneName이 있으면 해당 씬 로드
→ 없으면 townSceneName 로드
```

저장 데이터가 없으면:

```text
ContinueText 반투명/회색 표시
커서 선택 불가
Z 입력 실행 불가
```

## 종료 처리

- 빌드에서는 `Application.Quit()`
- 에디터에서는 Debug.Log 처리

## 씬 이름 변경 주의

목표 명칭:

```text
Title → BootScene
MainScene → TownScene
BattleScene 유지
```

실제 이름 변경 시 확인 대상:
- Build Settings 씬 목록
- `SceneManager.LoadScene(...)`
- `townSceneName`
- `defaultReturnSceneName`
- `returnSceneName`
- `currentSceneName`
- 저장 파일의 기존 `"MainScene"` 값
- 게임오버에서 그만하기 목적지
- 메뉴에서 게임종료 목적지
