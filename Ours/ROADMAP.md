# ROADMAP

현재 우선순위를 위한 짧은 작업 목록. 완료된 세부 이력은 Git을 기준으로 하고, 이 문서에는 앞으로 필요한 것만 남긴다.

## P0 — 지금 진행

- [ ] `TitleScene` 전체 흐름/연출 최종 점검
- [ ] `TownScene` 일반 탐험 카메라를 플레이어에 지연 없이 붙는 방식으로 통일
- [ ] Forest → Title → New Game → Town 전체 새 게임 플로우 QA
- [ ] 유효 저장이 있을 때 Forest를 건너뛰고 Continue 가능한지 QA
- [ ] C/X/A 입력이 Title/Town/Battle에서 일관되게 동작하는지 QA

## P1 — 다음 핵심 기능

- [ ] New Game 시작 후 주인공 집/침실/엄마 기상 연출 구현
- [ ] 범용 Dialogue UI / DialogueController 구현
- [ ] Forest 후퇴 방지 문구 `일단 앞으로 나아가보자.` 연결
- [ ] Town 맵 및 NPC 1차 구성

## P1 — 안정화

- [ ] BattleManager 게임오버 Coroutine 중복 실행 guard
- [ ] BattleManager `commandText` 레거시 경로 실제 참조 확인 후 정리 여부 결정
- [ ] BattleScene `Enemy Image` Inspector 연결 재확인
- [ ] TitleScene의 씬 내 GameManager 오브젝트 제거 가능 여부 확인(Runtime Bootstrap과 중복)

## P2 — 전투 확장

- [ ] 방어 커맨드
- [ ] 아이템/가방 전투 커맨드
- [ ] PK회복 MP 비용/밸런스 확정
- [ ] PK썬더 이펙트/SFX 최종 조정
- [ ] 적 마법 공격
- [ ] 회피/명중/행운 계산식

## P2 — 저장/필드 확장

- [ ] 인벤토리 저장
- [ ] 퀘스트/보스 플래그 확장
- [ ] 필드 적 장기 상태 저장이 필요한지 결정
- [ ] 저장 포맷 버전(saveVersion) 도입 여부 결정
- [ ] 원자적 저장(temp → replace) 도입 검토

## P3 — 장기

- [ ] 보스전
- [ ] 장비/아이템 시스템
- [ ] 4인 파티
- [ ] 파티원별 HP/MP/스킬/상태
- [ ] 전체 파티 전투불능 게임오버

## 릴리즈 전 청소

`Docs/RISK_REGISTER.md`의 레거시/테스트 후보를 실제 참조 확인 후 정리한다. 작동 중인 코드를 이름만 보고 삭제하지 않는다.
