# BATTLE_SYSTEM

전투 시스템 상세 문서.

---

## 관련 파일

- `Assets/Scripts/Battle/BattleManager.cs`
- `Assets/Scripts/CommandSelector.cs`
- `Assets/Scripts/EnemyController.cs`
- `Assets/Scripts/BattleTransitionEffect.cs`
- `Assets/Data/Enemies/*.asset`

---

## BattleScene UI 구조

현재 BattleScene Canvas 구조:

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

역할:

- `BattleBG`: 전투 배경 애니메이션 담당
- `AnimatedBG/Raw Image`: 실제 배경 프레임 표시
- `EnemyLayer/Enemy Image`: 적 스프라이트 표시
- `BattleUI`: 일반 전투 UI 묶음
- `GameOverPanel`: 게임오버 UI
- `FadePanel/FadeOverlay`: 씬 전환 페이드

주의:
- 전투 배경은 MessagePanel 안에 두지 않는다.
- EnemyImage는 하나만 사용한다.
- FadeOverlay와 GameOverDarkOverlay를 혼동하지 않는다.

---

## 전투 진입 흐름

1. MainScene의 적 오브젝트가 플레이어와 충돌한다.
2. `EnemyController`가 자기 `EnemyData`를 `GameManager.currentBattleEnemy`에 저장한다.
3. 복귀 씬 / 복귀 위치 / 적 ID를 GameManager에 저장한다.
4. `BattleTransitionEffect.Play()`를 호출한다.
5. Main BGM은 Pause된다.
6. 전투 진입 연출이 끝나면 BattleScene으로 이동한다.
7. BattleManager가 `GameManager.currentBattleEnemy`를 읽어서 전투를 시작한다.

---

## BattleManager 역할

- 전투 상태 관리
- 적 HP / MP 런타임 관리
- 플레이어 공격
- 적 공격
- PK회복
- 도망
- 승리 보상
- 레벨업
- 패배 / 게임오버
- 페이드 아웃 / 페이드 인
- BGM 정리
- MainScene / Title 씬 전환

---

## CommandSelector 역할

- BattleScene 커맨드 UI 커서 이동
- Z 입력 처리
- 선택된 커맨드에 따라 BattleManager의 public 함수 호출

---

## 커맨드 상태

| 커맨드 | 상태 |
|---|---|
| 공격 | 구현 |
| 방어 | 미구현 |
| Special | 미구현. PK썬더 1차 구현 후보 |
| 스킬 / PK회복 | 구현 |
| 아이템 | 미구현 |
| 도망 | 구현 |

---

## 메시지 진행

- 공격 메시지, 적 공격 메시지, 도망 메시지, 승리 메시지는 Z 입력으로 넘긴다.
- 자동 대기만 필요한 경우 기존 WaitMessage 계열 함수를 사용할 수 있다.
- `WaitForConfirm()`은 Z를 새로 누를 때까지 기다리는 용도다.

---

## 게임오버 흐름

1. 모든 플레이어가 전투불능 상태가 된다.
2. `모두 쓰러졌다...` 메시지가 나온다.
3. Z 입력 시 현재 화면을 유지한 채 FadeOut.
4. Battle_BGM이 서서히 줄어들다 정지한다.
5. 암전 중 GameOverPanel을 준비한다.
6. FadeIn으로 게임오버 선택 화면을 보여준다.
7. 다시 일어서기 선택 시 마지막 저장 데이터를 불러온다.
8. 그만하기 선택 시 BGMManager를 정리하고 Title로 이동한다.

---

## 예정 스킬: PK썬더

1차 구현 후보:

- 연결 커맨드: `Special`
- 습득 레벨: Lv2 예정
- MP 소모: 4 예정
- 타입: 마법 공격
- 대상: 적 1체
- 데미지 공식 예정:

```csharp
Mathf.Max(3, GameManager.Instance.magicAttack * 2 - enemyData.magicDefense)
```

예정 흐름:

1. Special 선택
2. Lv2 미만이면 “아직 PK썬더를 사용할 수 없다.”
3. MP 부족이면 “MP가 부족하다.”
4. 사용 가능하면 MP 4 소모
5. 데미지 적용
6. 적 HP 0 이하이면 VictoryRoutine
7. 적이 살아 있으면 EnemyTurnRoutine

---

## 페이드 주의사항

- FadePanel은 화면 전환용이다.
- GameOverDarkOverlay는 게임오버 화면 배경용이다.
- GameOverPanel을 페이드아웃 전에 끄지 않는다.
- FadePanel은 Canvas 가장 아래쪽에 둔다.
- MainScene 복귀 시 페이드 인은 아직 미구현이다.
