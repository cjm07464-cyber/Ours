# FUTURE_PLAN

장기 개발 계획.

---

## 단기 예정 작업

- MainScene 복귀 시 페이드 인 구현
- PK썬더 1차 구현
- 적 처치 후 10초 리스폰 구현
- BattleManager의 구 CommandText 방식 정리
- PlayerManager 구 구조 제거 여부 확인

---

## 1. 플레이어 방향 저장 / 복원

목표:

- 저장 시 플레이어가 바라보는 방향까지 저장
- 불러오기 시 위치와 방향을 모두 복원

예상 수정:

- GameManager
- SaveData
- PlayerController
- PlayerLoader
- SaveSystem 검증

---

## 2. 전투 커맨드 확장

- 방어
- 아이템
- 스킬 목록
- Special 기능 결정
- PK썬더
- 스킬 MP 소모
- 적 마법 공격
- 회피 / 명중 계산

---

## 3. NPC 대화 시스템

목표:

- NPC와 대화
- 예 / 아니오 선택지
- 분기 대사
- 튜토리얼 대사
- 보스전 진입 대화

구조 후보:

- ScriptableObject DialogueData
- JSON / CSV 대사 데이터
- DialogueManager
- DialogueUI

---

## 4. 보스전

대상:

- 만지면 물쥐

필요:

- EnemyData 생성
- 대화 후 전투 진입
- 처치 후 ratBossDefeated 플래그 저장
- 보스 처치 후 필드 상태 변화

---

## 5. 4인 파티

목표:

- 최종적으로 파티원 4명
- 각자 HP / MP / 스킬 / 상태 보유
- 모든 파티원이 전투불능이면 게임오버

주의:

- 현재 GameManager는 주인공 1명 기준이다.
- 나중에 PartyMember 데이터 구조로 분리 필요.
