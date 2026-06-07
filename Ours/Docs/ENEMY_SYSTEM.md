# ENEMY_SYSTEM

필드 적 / 전투 진입 시스템 상세.

---

## 관련 파일

- `Assets/Scripts/EnemyController.cs`
- `Assets/Scripts/BattleTransitionEffect.cs`
- `Assets/Data/Enemies/*.asset`

---

## 필드 적 동작

- originPos 기준 일정 반경 배회
- 플레이어가 detectRadius 안에 들어오면 추적
- loseRadius 밖으로 나가면 배회로 복귀
- moveRadius 밖으로는 이동하지 않음
- 메뉴가 열려 Time.timeScale이 0이면 정지

---

## 전투 진입

1. 플레이어와 충돌
2. `EnemyController.StartBattle()`
3. `GameManager.currentBattleEnemy`에 EnemyData 저장
4. `returnSceneName`, `returnPlayerPosition` 저장
5. `currentBattleEnemyId` 저장
6. `BattleTransitionEffect.Play()`
7. BattleScene 이동

---

## 도망 후 처리

1. BattleManager가 `escapedEnemyId`에 `currentBattleEnemyId` 저장
2. MainScene 복귀
3. EnemyController가 자기 `encounterId`와 `escapedEnemyId` 비교
4. 일치하면 3초간:
   - 이동 정지
   - 접촉 무시
   - Collider2D 비활성화
   - 스프라이트 깜빡임
5. 이후 정상 복귀

---

## 예정: 적 처치 후 10초 리스폰

전투에서 승리하면 해당 필드 적은 MainScene 복귀 후 일시적으로 사라진다.

예정 흐름:

1. BattleManager가 승리 시 `defeatedEnemyId`를 기록
2. MainScene 복귀
3. EnemyController가 자기 `encounterId`와 `defeatedEnemyId`를 비교
4. 일치하면 적 스프라이트와 Collider를 끈다
5. 이동 / 추적을 멈춘다
6. 10초 대기
7. 적 스프라이트와 Collider를 다시 켠다
8. 원래 위치 또는 originPos로 복귀 후 정상 이동 재개
9. `defeatedEnemyId`를 비운다

초기 구현은 단일 `defeatedEnemyId` 방식으로 시작한다. 나중에 적이 여러 마리거나 동시에 여러 적 처치 상태를 관리해야 하면 리스트 또는 Dictionary 방식으로 확장한다.

---

## 주의사항

- encounterId는 필드 적마다 고유해야 한다.
- encounterId가 비어 있으면 gameObject.name을 사용한다.
- 필드 적 복제 시 encounterId 중복 주의.
- 도망 후 깜빡임 중 Collider2D가 꺼져야 플레이어가 적을 밀지 않는다.
- 처치 후 리스폰 구현 시 도망 후 깜빡임 처리와 충돌하지 않게 한다.
