# CURRENT_STATE

## 목표 씬 명칭

- `BootScene`: 인트로, 타이틀 메뉴, 이름 입력
- `TownScene`: 기존 `MainScene` 역할. 마을 필드
- `BattleScene`: 전투 씬

주의:
- 실제 프로젝트에는 아직 `Title`, `MainScene` 이름이 남아 있을 수 있다.
- 현재 `Title` 씬이 BootScene 역할을 수행한다.
- 씬 파일명 변경은 문자열 참조 분석 후 진행한다.

## 구현 완료

### Title / Boot 역할

- Boot_BGM 재생
- IntroCreditGroup 기반 시작 크레딧 3개 표시
  - Created at / Pai Chai University
  - Directed by / DAVID
  - Inspired by / MOTHER
- 인트로 중 Z/X 스킵
- 지구 이미지 페이드 인
- 지구 자전 Animator 적용
- 지구 이미지 축소 및 O 위치 이동
- 지구 연출 중 Z/X 스킵
- 지구 이미지 + UrsText로 Ours 타이틀 구성
- StudentCreditText 하단 표기
- 처음부터 / 이어하기 / 종료 메뉴
- 저장 파일이 있으면 이어하기 가능
- 저장 파일이 없으면 이어하기 비활성 / 선택 불가
- 이어하기 선택 시 FadeOverlay 페이드아웃 후 저장 씬 이동
- 처음부터 선택 시 기존 이름 입력 흐름 연결
- 이름 확정 후 시놉시스 표시
- Boot_BGM이 시놉시스에서 처음부터 다시 재생되지 않고 이어서 재생
- 종료 선택 시 에디터 로그 / 빌드 종료

### MainScene / Town 역할

- 플레이어 이동
- 플레이어 방향 저장 / 복원
- 카메라 추적
- 필드 적 랜덤 배회 / 추적
- 적과 접촉 시 전투 진입
- 전투 진입 전 전환 연출
- Main/Town BGM Pause / Resume
- C키 메뉴
- 메뉴에서 저장하기
- 메뉴에서 Title/Boot로 돌아가기
- 메뉴 열림 중 플레이어 / 적 정지
- BattleScene 복귀 시 페이드 인
- 전투 승리 후 해당 필드 적 10초 리스폰

### BattleScene

- EnemyData 기반 적 전투
- SkillData 기반 스킬 구조
- 스킬 목록 UI / SkillSelector
- 공격
- PK회복
- PK썬더
- PK썬더 MP 소모
- PK썬더 데미지 공식
- PK썬더 이펙트 프리팹 재생 구조
- 도망
- 스피드 기반 선공
- 경험치 / 골드 보상
- 레벨업
- Lv2 PK회복 습득
- Lv2 PK썬더 습득
- 배운 스킬 저장 / 불러오기
- 도망 후 필드 적 3초 깜빡임 / 접촉 무시
- 도망 후 적 Collider2D 비활성화
- 승리 / 도망 / 게임오버 페이드 처리
- 게임오버 패널
- 다시 일어서기 / 그만하기
- 전투 배경 애니메이션 RawImage 추가
- BattleScene UI 계층 정리
- TMP 한글 폰트 재생성 및 교체
- 한글 완성형 / 단독 자모 출력 대응

## 현재 Boot/Title Canvas 구조

```text
Canvas
├── FadeOverlay
├── TitleManagerUI
├── IntroCreditGroup
├── IntroText
└── TitleGroup
```

상세는 `Docs/BOOT_SYSTEM.md`를 기준으로 한다.

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

## 현재 미구현 / 미완성

- MainScene → TownScene 명칭 전환
- Title → BootScene 명칭 전환
- 씬 이름 변경 후 저장 데이터 호환 처리
- 마을맵 1차 완성
- 방어 커맨드
- 아이템 커맨드
- 인벤토리
- NPC 대화 시스템
- 만지면 물쥐 보스전
- 4인 파티 구조

## 현재 주의사항

- BattleManager의 Command Text는 사용하지 않는다.
- BattleScene 커맨드 선택은 CommandSelector가 담당한다.
- BattleScene 스킬 선택은 SkillSelector가 담당한다.
- BattleManager의 Command Text 인스펙터는 None으로 둔다.
- Battle_BGM에는 BGMManager를 붙이지 않는다.
- Fade Image는 FadePanel/FadeOverlay의 Image여야 한다.
- Enemy Image는 `EnemyLayer/Enemy Image` 하나만 사용한다.
- EffectLayer는 스킬 이펙트 프리팹 생성 위치다.
- BootScene/Title의 `TitleManagerUI`는 `FadeOverlay` 다음 순서에 둔다.
- BootScene/Title의 시각 연출 세부값은 Unity Inspector에서 직접 조정한다.
