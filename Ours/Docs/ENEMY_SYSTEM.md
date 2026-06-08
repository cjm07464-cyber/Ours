# ENEMY_SYSTEM

## 관련 파일

- `Assets/Scripts/EnemyController.cs`
- `Assets/Scripts/BattleTransitionEffect.cs`
- `Assets/Data/Enemies/*.asset`

## 필드 적 동작

- originPos 기준 일정 반경 배회
- 플레이어가 detectRadius 안에 들어오면 추적
- loseRadius 밖으로 나가면 배회로 복귀
- moveRadius 밖으로는 이동하지 않음
- 메뉴가 열려 Time.timeScale이 0이면 정지

## 전투 진입

1. 플레이어와 충돌
2. `EnemyController.StartBattle()`
3. `GameManager.currentBattleEnemy`에 EnemyData 저장
4. `returnSceneName`, `returnPlayerPosition` 저장
5. `currentBattleEnemyId` 저장
6. `BattleTransitionEffect.Play()`
7. BattleScene 이동

## 도망 후 처리

1. BattleManager가 `escapedEnemyId`에 `currentBattleEnemyId` 저장
2. TownScene/MainScene 복귀
3. EnemyController가 자기 `encounterId`와 `escapedEnemyId` 비교
4. 일치하면 3초간 이동 정지, 접촉 무시, Collider2D 비활성화, 스프라이트 깜빡임
5. 이후 정상 복귀

## 적 처치 후 10초 리스폰

1. BattleManager가 승리 시 `defeatedEnemyId`를 기록
2. TownScene/MainScene 복귀
3. EnemyController가 자기 `encounterId`와 `defeatedEnemyId`를 비교
4. 일치하면 적 스프라이트와 Collider를 끈다
5. 이동 / 추적 / 전투 진입을 막는다
6. 10초 대기
7. originPos 또는 원래 위치로 복귀
8. 적 스프라이트와 Collider를 다시 켠다
9. `defeatedEnemyId`를 비운다

## 주의사항

- encounterId는 필드 적마다 고유해야 한다.
- 필드 적 복제 시 encounterId 중복 주의.
- 장기 저장되는 적 처치 상태는 아직 구현하지 않는다. 현재 10초 리스폰은 런타임 처리다.
