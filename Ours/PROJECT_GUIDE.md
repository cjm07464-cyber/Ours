# PROJECT_GUIDE

이 문서는 프로젝트 전체 개발 원칙과 시스템 간 의존성을 정리한다.

---

## 프로젝트 기본 방향

- Unity 2D RPG 프로젝트
- MainScene에서 탐험
- EnemyData 기반 전투 진입
- BattleScene에서 전투 진행
- 저장 / 불러오기 지원
- 게임오버와 페이드 연출 지원
- 전투 배경 애니메이션 지원
- 추후 NPC 대화, 보스전, 아이템, 4인 파티 구조 확장 예정

---

## 작업 원칙

- 기존 구조를 임의로 갈아엎지 않는다.
- 기능 단위로 필요한 최소 파일만 수정한다.
- 시스템 간 의존성을 확인하고 수정한다.
- 씬 파일 변경 시 Inspector 연결값을 반드시 확인한다.
- 저장 구조 변경 시 관련 파일을 모두 함께 수정한다.
- AI / Codex 작업 전에는 `AGENTS.md`를 먼저 읽는다.

---

## 시스템 간 관계

### Title → MainScene

- Title에서 새 게임 또는 이어하기 선택
- GameManager가 유지됨
- SaveSystem으로 저장 데이터 로드 가능
- MainScene으로 이동

### MainScene → BattleScene

- PlayerController가 플레이어 이동 담당
- EnemyController가 필드 적 이동 / 추적 담당
- 적과 충돌 시 EnemyController가 전투 정보를 GameManager에 저장
- BattleTransitionEffect가 전투 진입 연출 재생
- Main BGM은 Pause
- BattleScene으로 이동

### BattleScene → MainScene

- BattleManager가 전투 진행
- 승리 / 도망 시 페이드아웃 후 MainScene 복귀
- Main BGM은 Resume
- 도망 시 escapedEnemyId로 해당 적 3초 접촉 무시
- 승리 시 추후 defeatedEnemyId로 해당 적 숨김 / 리스폰 처리 예정

### BattleScene → Title

- 게임오버에서 그만하기 선택 시 Title 씬 이동
- BGMManager.StopAndDestroy() 호출

---

## 입력 체계

- 방향키: 이동 / UI 선택
- Z: 확인
- X: 취소 / 뒤로가기
- C: 메뉴 열기 / 닫기

---

## 현재 BattleScene Canvas 구조

```text
Canvas
├── BattleBG
│   └── AnimatedBG
│       └── Raw Image
├── EnemyLayer
│   └── Enemy Image
├── BattleUI
│   ├── MessagePanel
│   ├── CommandPanel
│   └── StatusPanel
├── GameOverPanel
│   ├── GameOverDarkOverlay
│   ├── GameOverTitleText
│   ├── GameOverQuestionText
│   ├── GameOverSelector
│   ├── GameOverContinueText
│   └── QuitText
└── FadePanel
    └── FadeOverlay
```

렌더 순서는 아래를 기준으로 한다.

```text
전투 배경
→ 적 이미지
→ 일반 전투 UI
→ 게임오버 UI
→ 페이드 오버레이
```

---

## 현재 주의할 구조

### BattleScene 커맨드

- CommandSelector가 입력 담당
- BattleManager는 행동 실행 담당
- Command Text는 사용하지 않음

### 저장

- 저장 데이터는 SaveData 기준
- GameManager는 런타임 상태 보유
- PlayerLoader는 MainScene 시작 시 playerPosition 적용
- 플레이어 방향 저장은 아직 미구현

### 페이드

- FadePanel/FadeOverlay는 화면 전환 연출
- GameOverPanel/GameOverDarkOverlay는 게임오버 배경
- 둘을 혼동하지 않는다

### TMP 폰트

- 한글 완성형만 넣으면 단독 자음/모음이 깨질 수 있다.
- 권장 범위: `0020-007E,3130-318F,AC00-D7A3`

---

## 작업 요청 시 권장 문구

```text
작업 시작 전에 AGENTS.md, README.md, PROJECT_GUIDE.md, TODO.md, 그리고 관련 Docs 문서를 반드시 먼저 읽어.
현재 구조를 분석하고, 수정 계획을 먼저 제시한 뒤 필요한 파일만 수정해.
```
