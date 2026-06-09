# FUTURE_PLAN

## 단기 예정 작업

- Title/MainScene → BootScene/TownScene 명칭 전환 계획 수립
- 실제 씬 파일명 변경 및 문자열 참조 정리
- 저장 데이터의 기존 `MainScene` / `Title` 값 호환 처리
- 마을맵 1차 완성
- PK썬더 이펙트 크기/속도/SFX 최종 조정
- BattleManager의 구 CommandText 방식 정리
- PlayerManager 구 구조 제거 여부 확인

## 1. BootScene

현재 상태:
- 기존 `Title` 씬이 BootScene 역할을 수행 중
- IntroCreditGroup 기반 시작 크레딧 구현
- 지구 로고 연출 구현
- Ours 타이틀 메뉴 구현
- 처음부터 / 이어하기 / 종료 구현
- 이어하기 비활성화 및 페이드아웃 로드 구현

남은 작업:
- 실제 씬 파일명 `Title` → `BootScene` 변경
- 관련 문자열 참조 정리
- 저장 데이터 호환 처리 확인
- 타이틀 화면 세부 연출 최종 조정

## 2. TownScene

목표:
- 기존 MainScene을 TownScene 역할로 정리
- 실제 씬 파일명 `MainScene` → `TownScene` 변경
- 마을맵 1차 완성
- 필드 적 / 전투 진입 / 메뉴 / 저장 유지
- 향후 NPC와 보스전 진입 지점 추가

## 3. 전투 커맨드 확장

- 방어
- 아이템
- Special 기능 결정
- 스킬 MP 소모 밸런스 조정
- 적 마법 공격
- 회피 / 명중 계산
- 스킬 이펙트/SFX 추가

## 4. NPC 대화 시스템

- NPC와 대화
- 예 / 아니오 선택지
- 분기 대사
- 튜토리얼 대사
- 보스전 진입 대화

## 5. 보스전

대상:
- 만지면 물쥐

필요:
- EnemyData 생성
- 대화 후 전투 진입
- 처치 후 ratBossDefeated 플래그 저장
- 보스 처치 후 필드 상태 변화

## 6. 4인 파티

목표:
- 최종적으로 파티원 4명
- 각자 HP / MP / 스킬 / 상태 보유
- 모든 파티원이 전투불능이면 게임오버
