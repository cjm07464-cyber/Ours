# CURRENT_STATE

현재 구현 상태와 주의사항 정리.

---

## 구현 완료

### Title

- 새 게임 시작
- 이름 입력
- 저장 파일이 있으면 이어하기 패널 표시
- 저장 파일이 없으면 새 게임 패널 표시
- 저장 초기화 확인 패널

### MainScene

- 플레이어 이동
- 카메라 추적
- 필드 적 랜덤 배회 / 추적
- 적과 접촉 시 전투 진입
- 전투 진입 전 전환 연출
- Main BGM Pause / Resume
- C키 메뉴
- 메뉴에서 저장하기
- 메뉴에서 Title로 돌아가기
- 메뉴 열림 중 플레이어 / 적 정지

### BattleScene

- EnemyData 기반 적 전투
- 공격
- PK회복
- 도망
- 스피드 기반 선공
- 경험치 / 골드 보상
- 레벨업
- Lv2 PK회복 습득
- 도망 후 필드 적 3초 깜빡임 / 접촉 무시
- 도망 후 적 Collider2D 비활성화
- 승리 / 도망 / 게임오버 페이드 처리
- 게임오버 패널
- 다시 일어서기 / 그만하기
- 전투 배경 애니메이션 RawImage 추가
- BattleScene UI 계층 정리
- TMP 한글 폰트 재생성 및 교체
- 한글 완성형 / 단독 자모 출력 대응

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

---

## 현재 미구현

- MainScene 복귀 시 페이드 인
- PK썬더 스킬
- 적 처치 후 필드에서 사라졌다가 10초 후 리스폰
- 플레이어 방향 저장 / 복원
- 방어 커맨드
- 아이템 커맨드
- 인벤토리
- NPC 대화 시스템
- 만지면 물쥐 보스전
- 4인 파티 구조

---

## 현재 주의사항

- BattleManager의 Command Text는 사용하지 않는다.
- BattleScene 커맨드 선택은 CommandSelector가 담당한다.
- BattleManager의 Command Text 인스펙터는 None으로 둔다.
- Battle_BGM에는 BGMManager를 붙이지 않는다.
- Fade Image는 FadePanel/FadeOverlay의 Image여야 한다.
- GameOverDarkOverlay를 Fade Image로 연결하면 안 된다.
- GameOverPanel은 기본 비활성화 상태여야 한다.
- GameOverPanel을 페이드아웃 전에 먼저 끄면 뒤의 BattleScene 화면이 보인다.
- Enemy Image는 `EnemyLayer/Enemy Image` 하나만 사용한다.
- BattleBG는 MessagePanel 안에 두지 않는다.
