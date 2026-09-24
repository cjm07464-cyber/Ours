# BATTLE_SYSTEM

BattleScene의 현재 책임과 수정 안전선을 정리한다.

## 1. 관련 파일

대표:

- `Assets/Scripts/Battle/BattleManager.cs`
- `Assets/Scripts/CommandSelector.cs`
- `Assets/Scripts/SkillSelector.cs`
- `Assets/Scripts/SkillData.cs`
- `Assets/Scripts/EnemyController.cs`
- `Assets/Scripts/BattleTransitionEffect.cs`
- `Assets/Data/Enemies/*.asset`
- `Assets/Data/Skills/*.asset`
- `Assets/Prefabs/Effects/*.prefab`

## 2. UI 구조

```text
Canvas
├ BattleBG
├ EnemyLayer
│  └ Enemy Image
├ EffectLayer
├ BattleUI
│  ├ MessagePanel
│  ├ CommandPanel
│  ├ SkillPanel
│  └ StatusPanel
├ GameOverPanel
└ FadePanel
   └ FadeOverlay
```

역할:

- `BattleBG` — 전투 배경.
- `EnemyLayer/Enemy Image` — 적 표시.
- `EffectLayer` — SkillData effectPrefab 생성 부모.
- `BattleUI` — 일반 전투 UI.
- `GameOverPanel` — 게임오버 선택.
- `FadePanel/FadeOverlay` — 전투 씬 Fade.

## 3. 입력 책임

현재 기본 입력은 C/X.

- `CommandSelector` — 커맨드 선택/결정.
- `SkillSelector` — 스킬 목록 선택/결정/취소.
- `BattleManager` — 실제 행동 실행과 전투 상태.

`BattleManager.commandText` 기반 레거시 입력 경로는 사용하지 않는다.

**Inspector의 `Command Text`는 None 유지.**

## 4. 현재 구현

- 일반 공격
- 스킬 선택
- PK회복
- PK썬더
- MP 소모
- 스피드 기반 선공
- 도망
- 경험치/골드
- 레벨업
- 스킬 습득/저장
- 승리/도망/게임오버 Fade
- 게임오버 패널
- 다시 일어서기 / 그만하기
- 전투 배경 애니메이션
- SkillData effectPrefab/SFX 재생 구조

## 5. SkillData

대표 필드:

- skillId
- skillName
- description
- learnLevel
- mpCost
- skillType
- targetType
- elementType
- power
- effectPrefab
- sfx
- effectDuration

기본 흐름:

```text
CommandSelector
→ SkillPanel
→ SkillSelector
→ BattleManager.OnSkillSelected(skill)
→ SkillData 참조
→ EffectLayer에 effectPrefab 생성
→ SFX
→ 효과 적용
```

프리팹 이펙트는 Scene에 상시 배치하지 않는다.

## 6. EnemyData

Field Enemy의 `EnemyController`가 자기 EnemyData를 가지고 전투 직전 GameManager에 전달한다.

BattleManager는 전달된 데이터로 전투 적을 구성한다.

구 PlayerManager/PlayerStats 계열을 다시 전투 원본으로 되살리지 않는다.

## 7. BGM

- Town BGM은 Battle 진입 시 Pause.
- `Battle_BGM`은 독립 AudioSource.
- Battle_BGM 오브젝트에 Town의 `BGMManager`를 붙이지 않는다.
- Town 복귀 시 persistent Town BGM Resume.

게임오버에서 Title로 빠질 때 persistent BGM 정리 흐름을 유지한다.

## 8. 게임오버

현재 흐름 개념:

```text
전투불능
↓
메시지
↓ C
Fade Out / Battle BGM 감소
↓
GameOverPanel
↓
다시 일어서기 / 그만하기
```

### 위험

게임오버 coroutine이 조건에 따라 중복 시작될 가능성이 과거 점검에서 발견됐다.

수정 시:

- bool guard 또는 단일 coroutine handle로 중복 진입을 막는 소규모 안전 패치를 우선.
- 게임오버 UX 자체를 동시에 재설계하지 않는다.

## 9. Inspector 핵심 체크

BattleManager:

- Message Panel → BattleUI/MessagePanel
- Command Panel → BattleUI/CommandPanel
- Skill Panel → BattleUI/SkillPanel
- Status Panel → BattleUI/StatusPanel
- Enemy Image → EnemyLayer/Enemy Image
- Command Text → **None**
- Skill Selector → 실제 SkillSelector
- Effect Layer → Canvas/EffectLayer
- Fade Image → FadePanel/FadeOverlay
- Battle Bgm Source → Battle_BGM AudioSource

## 10. 확장 예정

- 방어
- 아이템
- 적 마법
- 회피/명중
- 추가 스킬/상태
- 보스전
- 파티 전투

기존 CommandSelector/SkillSelector 책임을 유지한 상태에서 확장한다.
