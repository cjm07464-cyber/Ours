# Ours

Unity 2D RPG 프로젝트.

## Unity Version

- Unity 2022.3.60f1

## 씬 명칭 기준

향후 목표 씬 명칭은 아래를 기준으로 한다.

- `BootScene`: 게임 실행 시 가장 먼저 나오는 씬. 인트로, 타이틀 메뉴, 이름 입력을 담당한다.
- `TownScene`: 기존 `MainScene` 역할. 마을, 플레이어 이동, 필드 적, 저장 메뉴를 담당한다.
- `BattleScene`: 전투 전용 씬. 이름 유지.

주의: 현재 프로젝트에는 기존 씬 이름(`Title`, `MainScene`)이 남아 있을 수 있다. 실제 씬 파일명 변경은 문자열 참조와 저장 데이터를 함께 확인한 뒤 진행한다.

## 작업 시작 전 읽을 문서 순서

1. `AGENTS.md`
2. `PROJECT_GUIDE.md`
3. `TODO.md`
4. `Docs/CURRENT_STATE.md`
5. 작업과 관련된 `Docs/*.md`

## 주요 문서

- `AGENTS.md`: Codex / AI 에이전트 전용 작업 규칙
- `PROJECT_GUIDE.md`: 전체 개발 원칙과 작업 흐름
- `TODO.md`: 우선순위 작업 목록
- `Docs/SYSTEM_OVERVIEW.md`: 전체 시스템 관계 요약
- `Docs/BOOT_SYSTEM.md`: BootScene 인트로 / 타이틀 / 이름 입력 상세
- `Docs/BATTLE_SYSTEM.md`: 전투 시스템 상세
- `Docs/ENEMY_SYSTEM.md`: 필드 적 / 전투 진입 / 리스폰 상세
- `Docs/MENU_SYSTEM.md`: TownScene 메뉴 상세
- `Docs/SAVE_SYSTEM.md`: 저장 / 불러오기 상세
- `Docs/FUTURE_PLAN.md`: 장기 개발 계획
