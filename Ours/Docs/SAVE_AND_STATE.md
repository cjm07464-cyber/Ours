# SAVE_AND_STATE

GameManager, SaveSystem, StoryFlag, Inventory, 런타임 세션과 저장 파일의 경계를 정리한다.

## 1. 핵심 파일

대표:

- `Assets/Scripts/Title/GameManager.cs`
- `Assets/Scripts/Title/SaveData.cs`
- `Assets/Scripts/Title/SaveSystem.cs`
- `Assets/Scripts/Main/PlayerLoader.cs`
- `Assets/Scripts/Main/MainMenuManager.cs`
- `Assets/Scripts/Events/GameEventSequence.cs`
- `Assets/Scripts/Events/GameEventRunner.cs`

실제 경로가 이동했거나 이름이 달라졌다면 현재 프로젝트 파일을 우선한다.

## 2. GameManager

싱글톤 + DontDestroyOnLoad.

Runtime Bootstrap이 존재해 씬 직접 Play에서도 자동 생성될 수 있다.

중복 GameManager 오브젝트가 Scene에 있으면 기존 singleton 로직으로 정리한다.

GameManager가 현재 관리하는 주요 런타임 범주:

- playerName
- HP / MP
- level / exp
- 공격/방어 등 스탯
- gold
- 현재/복귀 Scene 및 Player 위치/방향
- learned skill
- Story flag
- InventoryEntry 목록
- 장착 무기 ID
- 전투 진입용 임시 상태
- StartupSessionState / pendingPlayerName

## 3. StartupSessionState

첫 실행 프롤로그용 런타임 상태.

```text
None
ForestCompleted
NameChosen
```

관련:

- pendingPlayerName
- Forest 완료/이름 선택 상태

이 값은 저장 파일과 동일하지 않다.

유효 저장이 없고 앱을 완전히 종료하면 다음 실행에 Forest를 다시 보는 흐름을 유지한다.

## 4. Story Flag

GameManager는 Story flag를 관리한다.

주요 API 개념:

- HasStoryFlag
- SetStoryFlag
- ClearStoryFlag

현재 Town 주요 플래그:

```text
mother_morning_talk
father_phone_call_done
```

용도:

- `mother_morning_talk` — 엄마 아침 이벤트 완료 / 집 출구 Gate 해제 / 엄마 기본 대화 전환
- `father_phone_call_done` — 첫 아빠 전화 완료 / MainMenu 사용 해제

새 Story flag를 추가하면 SaveData 저장/복원도 함께 확인한다.

## 5. Inventory / 장비

GameManager에 Inventory 인프라가 존재한다.

현재 개념 기능:

- AddItem
- RemoveItem
- HasItem
- GetItemCount
- Equip / Unequip
- equipped weapon id
- effective attack / defense 계산

`GameEventSequence.GiveItem`이 이벤트 중 아이템 지급에 사용된다.

엄마 이벤트의 `baseball_bat`도 이 흐름을 사용한다.

새 Inventory 필드를 추가하거나 자료구조를 바꾸면 SaveData와 함께 수정한다.

## 6. 저장 유효성

Continue 가능 여부는 단순 File.Exists보다 `SaveSystem.HasValidSaveData()` 기준을 사용한다.

다음 종류의 실패를 안전하게 처리하는 현재 방식을 유지한다.

- 파일 없음
- 읽기 실패
- JSON parse 실패
- null/명백히 잘못된 데이터

## 7. 일반 저장 흐름

저장 직전 현재 위치/씬을 GameManager에 반영한다.

```text
현재 Scene / Player 위치 / 방향
↓
GameManager runtime 상태 갱신
↓
GameManager → SaveData
↓
SaveSystem.SaveGame()
↓
JSON 저장
```

SaveData 필드 하나를 추가할 때는 선언만 하지 말고 다음을 함께 확인한다.

- GameManager → SaveData 변환
- SaveData → GameManager 복원
- 기본값/이전 저장 호환
- 해당 시스템의 실제 복원 코드

## 8. Event Step SaveGame

`GameEventSequence`에는 `SaveGame` Step이 있다.

GameEventRunner가 이 Step을 실행하면:

1. 현재 Scene/Player 위치 등 필요한 런타임 값을 갱신
2. `SaveSystem.SaveGame()` 호출
3. 다음 Event Step 진행

전화 저장 이벤트가 이 Step을 사용한다.

## 9. 메뉴 통화 저장

MainMenu의 `통화`는 단순 즉시 저장 버튼이 아니라 아빠 전화 이벤트를 실행한다.

시작 연결:

```text
MainMenuManager
Phone Event Runner   → PhoneEventRunner/GameEventRunner
Phone Event Sequence → Dad_SaveCall_Event
```

현재 흐름:

```text
전화 Intro
↓
Choice 1: 저장할지

NO
→ 저장하지 않음
→ 전화 종료
→ Player unlock

YES
→ SaveGame
→ "....됐다. 저장되었다!"
→ 저장완료 징글
→ Choice 2: 오늘은 여기까지 할지

Choice 2 NO
→ 전화 종료
→ Player unlock

Choice 2 YES
→ 마지막 대화
→ QuitGame
```

저장과 게임 종료는 별개의 Choice다.

## 10. 저장완료 사운드와 BGM

저장완료 Dialogue Line은 시작음이 아니라 `Line End Sound`를 사용한다.

권장 설정:

```text
Line Start Sound                  = None
Line End Sound                    = 저장완료 AudioClip
Line End Sound Volume             = Inspector에서 조절
Wait For Line End Sound           = ON
Pause Bgm During Line End Sound   = ON
Bgm Resume Fade Duration          ≈ 1.0
```

실행 순서:

```text
"....됐다. 저장되었다!" 텍스트 출력 완료
↓
Town BGM Pause (재생 위치 유지)
↓
저장완료 Sound 재생
↓
Sound 재생 동안 Dialogue 진행 입력 무시
↓
Sound 종료
↓
Town BGM Resume + Fade-in
↓
다음 Dialogue 진행 가능
```

저장완료 클립만 너무 크면 공용 SFX AudioSource 볼륨을 낮추지 않고 `Line End Sound Volume`을 낮춘다.

## 11. 전화 벨소리

전화 Intro 첫 Line의 벨소리는 `Line Start Sound`를 사용한다.

```text
Instant Text = ON
Stop Line Start Sound On Advance = ON
```

사용자가 Intro Line을 빠르게 넘기면 아직 재생 중인 벨소리를 즉시 끊고 다음 Dad Dialogue로 넘어간다.

## 12. QuitGame Step

전화 저장의 두 번째 Choice YES에서 사용한다.

`GameEventRunner` Quit Game Context:

- Quit Fade Overlay
- Quit Fade Duration

실행 개념:

```text
quit 요청 상태 설정
→ Player 잠금 유지
→ BGM Fade Out
→ 검정 FadeOverlay alpha 1
→ Fade 완료
→ Editor: DEV Log
→ Build: Application.Quit()
```

Quit 요청 후 일반 Event 완료 콜백 때문에 Player가 다시 풀리지 않게 현재 guard를 유지한다.

## 13. 불러오기 흐름

```text
Title Continue
↓
HasValidSaveData()
↓
LoadGame()
↓
GameManager.LoadFromSaveData()
↓
저장된 Scene Load
↓
PlayerLoader가 위치/방향 복원
```

씬 이름은 로드 전 Normalize 호환을 고려한다.

## 14. TownScene 직접 Play 주의

Editor에서 TownScene을 직접 Play하면 정상 New Game 초기화 루트를 거치지 않았기 때문에 HP/MP/레벨 등 일부 값이 0일 수 있다.

직접 Town 테스트에서 0이 보인다는 이유만으로 기본 스탯 초기화 코드를 중복 추가하지 않는다.

정상 새 게임 흐름에서의 값을 우선 확인한다.

## 15. 개발용 저장 삭제

개발 테스트용 `0` / Numpad `0` 기능은 저장 파일 삭제용이다.

```text
0
→ SaveSystem.DeleteSaveData(...)
→ savefile.json 삭제
```

주의:

- 디스크 저장 데이터 삭제 기능이다.
- 현재 실행 중 `GameManager`의 이름/HP/StoryFlag/Inventory 등 Runtime 값을 전부 즉시 초기화하는 기능은 아니다.
- 저장 없음 상태를 정확히 테스트하려면 저장 삭제 후 Play를 종료하고 다시 시작하는 편이 안전하다.

## 16. SaveData 수정 규칙

저장 구조를 바꿀 때 최소 확인:

- SaveData 선언
- GameManager serialize
- GameManager restore
- Story flag
- Inventory / equipped weapon
- current scene / position / facing
- learned skill
- 이전 저장 데이터의 누락 필드 기본값

작동 중인 저장 포맷을 기능 작업과 동시에 대규모로 바꾸지 않는다.

## 17. 향후 안정화 후보

필요성이 생기면 검토:

- saveVersion
- TryLoad 결과 타입
- temp → replace 원자적 저장
- backup save
- migration

현재 전투 진입 구조 분석과는 별개 작업으로 유지한다.
