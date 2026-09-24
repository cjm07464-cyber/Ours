# Ours

Unity 2D RPG 프로젝트.

이 저장소의 Markdown 문서는 **현재 동작을 빠르게 파악하고 안전하게 수정하기 위한 실전 개발 문서**다. 과거 문서처럼 기능별 설명을 지나치게 잘게 나누지 않고, 실제 작업에 필요한 기준만 유지한다.

## 현재 씬

- `ForestScene` — 첫 실행 프롤로그. 숲 이동, UFO/외계인 연출, 이름 입력.
- `TitleScene` — 기술적 시작 씬. 크레딧/타이틀/메뉴/이어하기.
- `TownScene` — 일반 필드 탐험, 메뉴, 필드 적, 저장.
- `BattleScene` — 전투.

과거 명칭 `BootScene`, `Title`, `MainScene`은 레거시 호환 코드나 클래스명에 일부 남아 있을 수 있다. **씬 파일명은 현재 명칭을 기준으로 한다.**

## 기본 입력

- 방향키 — 이동 / UI 선택
- `C` — 확인 / 선택 / 결정 / 대화 진행
- `X` — 취소 / 뒤로가기
- `A` — 필드 메뉴 열기 / 닫기
- `0` / Numpad `0` — 개발용 저장 파일 삭제(Editor/Development Build 전용)

## 문서 읽기 순서

AI/Codex 작업 시 전부 읽지 말고 아래 순서만 따른다.

1. `AGENTS.md`
2. `Docs/ARCHITECTURE.md`
3. 작업 대상에 맞는 문서 1개
4. 구조 변경 또는 위험 작업이면 `Docs/RISK_REGISTER.md`

작업별 문서:

- 첫 실행 / Forest / Title: `Docs/STARTUP_FLOW.md`
- Town / 카메라 / 메뉴 / 필드 적: `Docs/FIELD_SYSTEMS.md`
- 전투: `Docs/BATTLE_SYSTEM.md`
- 저장 / GameManager / 런타임 상태: `Docs/SAVE_AND_STATE.md`
- 우선순위: `ROADMAP.md`


## 문서 교체 방법

이번 문서 세트는 구 문서를 부분 수정한 것이 아니라 **전면 재작성본**이다. 저장소 루트에서 기존 개발 문서를 백업한 뒤 아래 구 문서를 제거하고 이 ZIP의 내용을 그대로 덮어쓴다.

삭제 대상(구 문서):

- `PROJECT_GUIDE.md`
- `TODO.md`
- `Docs/CURRENT_STATE.md`
- `Docs/BOOT_SYSTEM.md`
- `Docs/ENEMY_SYSTEM.md`
- `Docs/FUTURE_PLAN.md`
- `Docs/MENU_SYSTEM.md`
- `Docs/SAVE_SYSTEM.md`
- `Docs/SYSTEM_OVERVIEW.md`
- 구 `Docs/BATTLE_SYSTEM.md`는 새 파일로 교체

새 기준은 `README.md`, `AGENTS.md`, `ROADMAP.md`, `Docs/*.md` 6개다.

## 현재 개발 원칙

- 작동 중인 시스템을 이유 없이 재설계하지 않는다.
- 씬/Prefab/YAML 직접 수정보다 Inspector 연결을 우선한다.
- 기존 `.cs` 파일은 삭제 후 재생성하지 않고 수정한다. `.meta`, 파일명, public class명 보존을 우선한다.
- SerializedField 이름 변경은 Inspector 연결 손실 위험이 있으므로 반드시 필요할 때만 한다.
- 기능 수정 후 `dotnet build Ours.sln`으로 컴파일을 확인한다.
- 시각적 위치, 볼륨, 타이밍은 가능한 Inspector에서 조율한다.

## 최근 큰 변경

- 첫 실행 흐름이 `TitleScene → 필요 시 ForestScene → TitleScene → TownScene` 구조로 변경됨.
- `GameManager` Runtime Bootstrap 추가.
- `SaveSystem.HasValidSaveData()` 기반 유효 저장 판정 추가.
- Forest 전체 프롤로그/카메라/사운드/DOTween 연출 구현.
- 공통 조작키를 `C/X/A` 규칙으로 통일.
- Forest의 밤 표현은 `NightOverlay`와 `FadeOverlay` 역할을 분리하고, 2D Light도 사용 가능.

자세한 현재 구조는 `Docs/ARCHITECTURE.md`를 기준으로 한다.
