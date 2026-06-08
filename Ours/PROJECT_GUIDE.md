# PROJECT_GUIDE

## 프로젝트 기본 방향

- Unity 2D RPG 프로젝트
- BootScene에서 인트로, 타이틀 메뉴, 이름 입력
- TownScene에서 탐험
- EnemyData 기반 전투 진입
- BattleScene에서 전투 진행
- SkillData 기반 스킬 시스템
- 저장 / 불러오기 지원
- 게임오버와 페이드 연출 지원
- 전투 배경 애니메이션 및 스킬 이펙트 지원
- 추후 NPC 대화, 보스전, 아이템, 4인 파티 구조 확장 예정

## 작업 원칙

- 기존 구조를 임의로 갈아엎지 않는다.
- 기능 단위로 필요한 최소 파일만 수정한다.
- 시스템 간 의존성을 확인하고 수정한다.
- 씬 파일 변경 시 Inspector 연결값을 반드시 확인한다.
- 저장 구조 변경 시 관련 파일을 모두 함께 수정한다.
- Codex는 기능 뼈대를 만들고, 시각 연출의 세부 위치/크기/타이밍은 Unity Editor에서 사용자가 직접 다듬는다.

## 씬 명칭 기준

- `BootScene`: 인트로, 타이틀 메뉴, 이름 입력
- `TownScene`: 마을 필드, 플레이어 이동, 필드 적, 메뉴, 저장
- `BattleScene`: 전투

주의:
- 현재 프로젝트에는 `Title`, `MainScene` 같은 기존 이름이 남아 있을 수 있다.
- 실제 씬 파일명 변경은 모든 문자열 참조를 확인한 뒤 진행한다.
- 저장 데이터에 옛 씬 이름이 남을 수 있으므로 호환 처리를 고려한다.

## 시스템 간 관계

### BootScene → TownScene

- BootScene에서 인트로 연출 후 타이틀 메뉴 표시
- 처음부터 선택 시 이름 입력 후 새 게임 데이터 초기화
- 이어하기 선택 시 SaveSystem으로 저장 데이터 로드
- 저장 데이터가 없으면 이어하기는 반투명 표시, 선택 불가
- 새 게임 또는 이어하기 성공 시 TownScene 또는 저장된 씬으로 이동

### TownScene → BattleScene

- PlayerController가 플레이어 이동 담당
- EnemyController가 필드 적 이동 / 추적 담당
- 적과 충돌 시 EnemyController가 전투 정보를 GameManager에 저장
- BattleTransitionEffect가 전투 진입 연출 재생
- Town BGM은 Pause
- BattleScene으로 이동

### BattleScene → TownScene

- 승리 / 도망 시 페이드아웃 후 TownScene 복귀
- Town BGM은 Resume
- 도망 시 escapedEnemyId로 해당 적 3초 접촉 무시
- 승리 시 defeatedEnemyId로 해당 적 숨김 후 10초 리스폰
- TownScene 복귀 시 SceneFadeIn으로 페이드 인

### BattleScene → BootScene

- 게임오버에서 그만하기 선택 시 BootScene 또는 기존 Title 씬으로 이동
- BGMManager.StopAndDestroy() 호출

## 입력 체계

- 방향키: 이동 / UI 선택
- Z: 확인
- X: 취소 / 뒤로가기
- C: 메뉴 열기 / 닫기

## 현재 BattleScene Canvas 구조

```text
Canvas
├── BattleBG
├── EnemyLayer
├── EffectLayer
├── BattleUI
│   ├── MessagePanel
│   ├── CommandPanel
│   ├── SkillPanel
│   └── StatusPanel
├── GameOverPanel
└── FadePanel
```

## 작업 요청 시 권장 문구

```text
작업 시작 전에 AGENTS.md, README.md, PROJECT_GUIDE.md, TODO.md, 그리고 관련 Docs 문서를 반드시 먼저 읽어.
현재 구조를 분석하고, 수정 계획을 먼저 제시한 뒤 필요한 파일만 수정해.
```
