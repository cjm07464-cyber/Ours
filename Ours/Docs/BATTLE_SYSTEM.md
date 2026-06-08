# BATTLE_SYSTEM

## 관련 파일

- `Assets/Scripts/Battle/BattleManager.cs`
- `Assets/Scripts/CommandSelector.cs`
- `Assets/Scripts/SkillSelector.cs`
- `Assets/Scripts/SkillData.cs`
- `Assets/Scripts/EnemyController.cs`
- `Assets/Scripts/BattleTransitionEffect.cs`
- `Assets/Data/Enemies/*.asset`
- `Assets/Data/Skills/*.asset`
- `Assets/Prefabs/Effects/*.prefab`

## BattleScene UI 구조

```text
Canvas
├── BattleBG
├── EnemyLayer
├── EffectLayer
├── BattleUI
│   ├── MessagePanel
│   ├── CommandPanel
│   ├── SkillPanel
│   └── StatusPanel
├── GameOverPanel
└── FadePanel
```

역할:
- `BattleBG`: 전투 배경 애니메이션 담당
- `EnemyLayer/Enemy Image`: 적 스프라이트 표시
- `EffectLayer`: 스킬 이펙트 프리팹 런타임 생성 위치
- `BattleUI`: 일반 전투 UI 묶음
- `GameOverPanel`: 게임오버 UI
- `FadePanel/FadeOverlay`: 씬 전환 페이드

## 구현된 스킬

### PK회복

- 습득 레벨: Lv2
- 타입: Heal
- 대상: Self
- 효과: HP 회복

### PK썬더

- 습득 레벨: Lv2
- 타입: MagicAttack
- 대상: SingleEnemy
- 속성: Thunder
- MP 소모: 4
- 데미지 공식:

```csharp
Mathf.Max(3, GameManager.Instance.magicAttack * 2 - enemyData.magicDefense)
```

- SkillData의 effectPrefab으로 이펙트 재생 가능
- SFX는 SkillData.sfx로 연결 가능

## SkillData 구조

주요 필드:
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

## 스킬 이펙트 흐름

```text
SkillSelector
→ BattleManager.OnSkillSelected(skill)
→ PlayerSkillRoutine(skill)
→ SkillData.effectPrefab이 있으면 EffectLayer 아래 Instantiate
→ SkillData.sfx 재생
→ effectDuration 대기
→ effectPrefab Destroy
→ 데미지/회복 메시지 출력
```

PKThunderEffect 프리팹은 하이어라키에 상시 배치하지 않는다.
Project의 `Assets/Prefabs/Effects`에 프리팹으로 두고 필요할 때 생성한다.

## 게임오버 흐름

1. 모든 플레이어가 전투불능 상태가 된다.
2. `모두 쓰러졌다...` 메시지가 나온다.
3. Z 입력 시 현재 화면을 유지한 채 FadeOut.
4. Battle_BGM이 서서히 줄어들다 정지한다.
5. 암전 중 GameOverPanel을 준비한다.
6. FadeIn으로 게임오버 선택 화면을 보여준다.
7. 다시 일어서기 선택 시 마지막 저장 데이터를 불러온다.
8. 그만하기 선택 시 BGMManager를 정리하고 BootScene/Title로 이동한다.
