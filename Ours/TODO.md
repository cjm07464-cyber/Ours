# TODO

## 최우선

- [x] BootScene 인트로/타이틀 기능 뼈대 구현
- [x] BootScene 지구 로고 연출용 UI 세팅
- [x] BootScene 이어하기 비활성화 UI 구현
- [x] BootScene 인트로/지구 연출 스킵 구현
- [x] BootScene 이어하기 페이드아웃 후 로드 구현
- [ ] Title/MainScene → BootScene/TownScene 명칭 전환 계획 수립
- [ ] Title/MainScene → BootScene/TownScene 실제 씬 이름 변경
- [ ] 씬 이름 변경 후 저장 데이터 호환 처리
- [ ] 마을맵 1차 완성
- [ ] BattleManager 코드 정리
- [ ] 방어 커맨드 구현
- [ ] 아이템 커맨드 기본 구조 구현
- [ ] 만지면 물쥐 보스 EnemyData 생성

## BootScene / 타이틀

- [x] 기존 Title 씬을 BootScene 역할로 정리
- [x] 검은 화면에서 BGM 재생
- [x] IntroCreditGroup 기반 크레딧 표시
- [x] `Created at / Pai Chai University` 페이드 인/아웃
- [x] `Directed by / DAVID` 페이드 인/아웃
- [x] `Inspired by / MOTHER` 페이드 인/아웃
- [x] 인트로 중 Z/X 스킵
- [x] 지구 이미지 페이드 인
- [x] 지구 자전 Animator 적용
- [x] 지구 이미지 축소 및 O 위치 이동
- [x] 지구 연출 중 Z/X 스킵
- [x] 지구 이미지 + `urs` 텍스트로 `Ours` 타이틀 구성
- [x] StudentCreditText 하단 표기
- [x] 처음부터 / 이어하기 / 종료 메뉴
- [x] 저장 데이터 없을 때 이어하기 반투명 및 선택 불가
- [x] 처음부터 선택 시 기존 이름 입력 흐름 연결
- [x] 이어하기 선택 시 저장 데이터 로드
- [x] 이어하기 선택 시 FadeOverlay 페이드아웃
- [x] 종료 선택 시 Application.Quit 처리
- [ ] 실제 씬 파일명 `Title` → `BootScene` 변경
- [ ] BootScene 관련 문자열 참조 정리

## 전투 시스템

- [x] EnemyData 기반 전투
- [x] 공격
- [x] PK회복
- [x] 도망
- [x] 게임오버 패널
- [x] 페이드 아웃 / 페이드 인
- [x] 전투 배경 애니메이션 RawImage 추가
- [x] BattleScene UI 계층 정리
- [x] TMP 한글 폰트 재생성 및 교체
- [x] SkillData ScriptableObject 구조
- [x] 스킬 목록 UI / SkillSelector 구현
- [x] Lv2 PK회복 습득
- [x] Lv2 PK썬더 습득
- [x] PK썬더 MP 소모 처리
- [x] PK썬더 데미지 공식 적용
- [x] MainScene/TownScene 복귀 시 페이드 인 구현
- [x] 승리 후 필드 적 숨김 및 10초 리스폰 구현
- [x] 스킬 이펙트 프리팹 재생 구조
- [ ] PK썬더 이펙트 크기/속도 최종 조정
- [ ] PK썬더 SFX 추가
- [ ] PK회복 MP 소모 추가
- [ ] 방어 구현
- [ ] 아이템 구현
- [ ] 적 마법 공격 구현
- [ ] 회피 / 명중 / 행운 계산식 정리

## 성장 시스템

- [x] 경험치 획득
- [x] 레벨업
- [x] Lv2 PK회복 습득
- [x] Lv2 PK썬더 습득
- [x] 배운 스킬 ID 저장
- [ ] 경험치 테이블 확정
- [ ] 레벨업 스탯 증가량 확정
- [ ] 레벨별 스킬 습득 구조 확장

## 저장 시스템

- [x] 이름 저장
- [x] HP / MP 저장
- [x] 레벨 / 경험치 저장
- [x] 골드 저장
- [x] 위치 저장
- [x] 플레이어 바라보는 방향 저장 / 복원
- [x] 배운 스킬 ID 저장
- [ ] 인벤토리 저장
- [ ] 필드 적 장기 처치 상태 저장
- [ ] 씬 이름 변경 후 저장 데이터 호환 처리

## TownScene / 마을

- [ ] 기존 MainScene을 TownScene 역할로 정리
- [ ] 실제 씬 파일명 `MainScene` → `TownScene` 변경
- [ ] 마을맵 1차 배치
- [ ] 길 / 집 / 나무 / 장식 배치
- [ ] 충돌 영역 정리
- [ ] 플레이어 시작 위치 재조정
- [ ] 필드 적 위치 재조정
- [ ] 마을 BGM 확인
- [ ] 저장/불러오기 위치 테스트
- [ ] 전투 진입/복귀 테스트

## NPC / 대화

- [ ] Dialogue System 설계
- [ ] NPC 대사 데이터 외부화
- [ ] 예 / 아니오 분기
- [ ] 튜토리얼 NPC 구현
- [ ] 보스 대화 후 전투 진입

## 장기 계획

- [ ] 4인 파티 구조
- [ ] 파티 전원 전투불능 시 게임오버
- [ ] 파티원별 HP / MP / 스킬
- [ ] 장비 / 아이템
- [ ] 퀘스트 플래그
- [ ] 보스전 연출
