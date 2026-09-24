# ARCHITECTURE

현재 Ours의 전체 구조를 빠르게 파악하기 위한 기준 문서.

## 1. 전체 플레이 흐름

```text
앱 시작
  ↓
TitleScene (기술적 엔트리)
  ├─ 유효 저장 있음 ──────────────→ 기존 타이틀/메뉴
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
               TownScene (현재)
                    ↓ 적 접촉
               BattleScene
                    ↓
               TownScene
```

Forest는 첫 실행 프롤로그다. 기술적인 첫 씬은 여전히 TitleScene이다.

## 2. 핵심 런타임 상태

### GameManager

역할:

- 플레이어 스탯/이름/골드/스킬 등 런타임 상태
- 저장 데이터 변환
- 전투 진입용 임시 Enemy 정보
- 씬 복귀 위치/방향
- 전투 승리/도망 Enemy ID
- 첫 실행용 startup session

현재 중요한 값:

```text
StartupSessionState
- None
- ForestCompleted
- NameChosen

pendingPlayerName
```

`StartupSessionState` / `pendingPlayerName`은 저장 데이터가 아니라 실행 중 세션 정보다.

### Runtime Bootstrap

`GameManager`는 `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`로 자동 생성 가능하다.

목적:

- Forest/Town/Battle/Title 씬을 Editor에서 직접 Play해도 GameManager 누락 방지.

씬에 기존 GameManager 오브젝트가 있으면 Awake singleton 로직으로 중복 인스턴스가 제거된다.

## 3. 씬별 책임

### ForestScene

첫 실행 프롤로그.

- 4개 맵 영역을 한 씬에서 진행
- 맵 간 Fade 전환
- Forest 전용 Player/애니메이션/발소리
- Map4 Y축 CameraFollow
- UFO/Smoke/RedSign/조명/외계인 연출
- 이름 입력
- 이름 확정 후 Forest BGM + 화면 Fade Out → TitleScene

상세: `STARTUP_FLOW.md`, `FIELD_SYSTEMS.md`

### TitleScene

- 시작 크레딧
- 지구/Ours 타이틀 연출
- New Game / Continue / Quit
- Forest 라우팅 판정
- 저장 유효성 판정에 따른 Continue 처리

주의:

- 씬 이름은 `TitleScene`.
- 스크립트 클래스 `BootSceneController`는 현재 유지.
- `TitleManager`에는 옛 이름 입력/시놉시스 로직이 남아 있음.

### TownScene

- 일반 필드 탐험
- 플레이어 이동
- 카메라 추적
- 필드 적
- A 메뉴
- 저장
- Battle 진입/복귀

### BattleScene

- EnemyData / SkillData 기반 전투
- 커맨드/스킬 선택
- 승리/도망/게임오버
- 전투 배경/스킬 이펙트

## 4. 입력 규칙

```text
방향키 = 이동 / UI 선택
C       = 확인 / 결정 / 진행
X       = 취소 / 뒤로가기
A       = 필드 메뉴
```

현재 직접 `Input.GetKeyDown()` 방식이 여러 스크립트에 남아 있다. 입력은 통일됐지만 중앙 GameInput 계층은 아직 도입하지 않았다.

## 5. 카메라 기준

`CameraFollow` 공용 스크립트에 다음 기능이 존재한다.

- 일반 smooth follow
- `instantFollow`
- `followX`
- `followY`
- `SnapToTarget()`

프로젝트 공통 방향:

- 일반 탐험 중 플레이어를 따라갈 때는 불필요한 지연 없이 붙는 카메라를 선호.
- 특수 연출에서만 smooth catch-up / focus tween을 사용.
- 카메라 GameObject나 Camera 컴포넌트를 끄지 않고, Follow 컴포넌트만 필요에 따라 enable/disable.

## 6. Fade 기준

Scene별 Fade 구현은 아직 완전히 하나의 공용 시스템으로 합치지 않았다.

Forest:

- `NightOverlay`: 분위기 전용
- `FadeOverlay`: 암전 전용

Battle/Title/Town은 기존 각 시스템을 유지한다.

성급하게 전역 FadeManager로 합치지 않는다. 실제 중복과 요구가 충분히 쌓인 뒤 통합한다.

## 7. 데이터 구조

### EnemyData

적 원본 데이터 ScriptableObject.

필드 EnemyController가 자기 EnemyData를 가지고 Battle 진입 시 GameManager에 전달한다.

### SkillData

스킬 원본 데이터 ScriptableObject.

- 스킬 이름/ID
- MP 비용
- 타입/대상/속성
- 위력
- effectPrefab
- SFX
- effectDuration 등

## 8. BGM 구조

Town:

```text
BGMManager (DontDestroyOnLoad)
Battle 진입 → PauseBGM
Town 복귀 → ResumeBGM
```

Battle:

- `Battle_BGM`은 독립 AudioSource.
- Town용 BGMManager를 붙이지 않는다.

Forest:

- Forest 전용 BGM/효과음 AudioSource.
- 이름 확정 → BGM volume 0으로 Fade 후 TitleScene.

## 9. 외부 패키지

### DOTween / DOTween Pro

설치/Setup 완료.

현재 사용 예:

- Forest 맵 Fade
- 카메라 Focus
- RedSign alpha
- Audio Fade
- Reaction icon pop
- Camera zone 복귀 연출

기존 Coroutine을 모두 DOTween으로 바꾸지 않는다. 연속적인 위치/알파/볼륨 변화에만 사용한다.

## 10. 문서 책임

- `STARTUP_FLOW.md` — Forest/Title/첫 실행
- `FIELD_SYSTEMS.md` — 플레이어/카메라/Town/적/메뉴
- `BATTLE_SYSTEM.md` — 전투
- `SAVE_AND_STATE.md` — GameManager/Save
- `RISK_REGISTER.md` — 위험한 레거시/정리 후보
