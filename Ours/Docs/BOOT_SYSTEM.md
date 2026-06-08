# BOOT_SYSTEM

BootScene 인트로 / 타이틀 / 이름 입력 시스템 문서.

## 목적

BootScene은 게임 실행 시 가장 먼저 나오는 씬이다.

담당:
- 제작자/출품 문구 인트로
- 지구 로고 연출
- Ours 타이틀 표시
- 처음부터 / 이어하기 / 종료 메뉴
- 이름 입력 흐름
- 저장 데이터 유무에 따른 이어하기 활성/비활성 처리

## 핵심 원칙

- Codex는 기능 뼈대만 구현한다.
- 지구 이미지, 로고 위치, 폰트, 색, 페이드 시간, 이동 곡선은 사용자가 Unity Editor에서 직접 조정한다.
- 모든 주요 UI 오브젝트는 Inspector에서 연결 가능해야 한다.
- 씬 이름 문자열 변경은 저장/불러오기와 전투 복귀 흐름을 함께 확인한 뒤 진행한다.

## 목표 연출 흐름

1. 검은 화면에서 BGM 재생
2. `Directed by 조정민` 페이드 인
3. 잠시 대기
4. `Directed by 조정민` 페이드 아웃
5. `배재대 개인작품 출품작` 페이드 인
6. 잠시 대기
7. `배재대 개인작품 출품작` 페이드 아웃
8. 지구 이미지가 화면 중앙에서 등장
9. 지구 이미지가 회전
10. 지구 이미지가 서서히 작아지며 왼쪽 위 O 위치로 이동
11. `urs` 텍스트가 나타나 지구 이미지와 함께 `Ours` 타이틀 구성
12. 메뉴 표시
    - 처음부터
    - 이어하기
    - 종료

## 이어하기 처리

- `SaveSystem.HasSaveData()`가 true이면 이어하기 활성
- false이면 이어하기 비활성
- 비활성 상태:
  - 반투명 또는 회색 표시
  - 커서 선택 불가
  - Z 입력으로 실행 불가
- 메뉴 이동 시 이어하기가 비활성이라면 건너뛴다.

## 처음부터 처리

처음부터 선택 시:
1. 기존 저장 데이터가 있으면 필요 시 덮어쓰기 확인
2. 이름 입력 패널 표시
3. 이름 입력 완료
4. GameManager 새 게임 데이터 초기화
5. TownScene 또는 기존 MainScene으로 이동

주의:
- 현재 이름 입력 흐름이 기존 TitleManager에 구현되어 있다면 재사용한다.
- 기존 기능을 갈아엎지 말고 BootSceneController에서 연결하거나 기존 TitleManager를 확장한다.

## 종료 처리

- 빌드에서는 `Application.Quit()`
- 에디터에서는 `Debug.Log("Quit")` 정도로 처리

## 권장 하이어라키

```text
BootScene
├── Main Camera
├── EventSystem
├── BootSceneController
├── Boot_BGM
└── Canvas
    ├── FadeOverlay
    ├── IntroText
    ├── TitleGroup
    │   ├── EarthImage
    │   ├── UrsText
    │   └── MenuGroup
    │       ├── Selector
    │       ├── NewGameText
    │       ├── ContinueText
    │       └── QuitText
    └── NameInputPanel
```

## BootSceneController 권장 필드

```text
Intro
- Image fadeOverlay
- TextMeshProUGUI introText
- float textFadeDuration
- float textHoldDuration

Title
- RectTransform earthImage
- TextMeshProUGUI ursText
- GameObject titleGroup
- GameObject menuGroup
- RectTransform selector
- TextMeshProUGUI newGameText
- TextMeshProUGUI continueText
- TextMeshProUGUI quitText

Earth Animation
- Vector2 earthStartAnchoredPosition
- Vector2 earthTargetAnchoredPosition
- Vector3 earthStartScale
- Vector3 earthTargetScale
- float earthMoveDuration
- float earthRotationSpeed

Scene
- string townSceneName
```

## 입력

- 방향키 위/아래: 메뉴 선택
- Z: 결정
- X: 이름 입력 또는 확인 패널에서 뒤로가기 가능하면 처리

## 주의사항

- BootScene 작업 중 BattleScene, TownScene 전투 복귀 흐름을 깨지 않는다.
- `BGMManager`와 Boot_BGM이 동시에 꼬이지 않게 한다.
- Title/MainScene 이름을 실제로 바꿀 경우 모든 하드코딩 문자열을 검색한다.
- 기존 저장 파일이 `"MainScene"`을 저장하고 있을 수 있으므로 호환 처리가 필요할 수 있다.
