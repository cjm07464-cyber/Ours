# MENU_SYSTEM

MainScene 메뉴 시스템 상세.

---

## 관련 파일

- `Assets/Scripts/Main/MainMenuManager.cs`
- `Assets/Scripts/Main/PlayerController.cs`
- `Assets/Scripts/EnemyController.cs`
- `Assets/Scripts/Title/SaveSystem.cs`

---

## 입력

- C: 메뉴 열기 / 닫기
- X: 메뉴 닫기
- 방향키: 항목 선택
- Z: 결정

---

## 메뉴 항목

- 스탯
- 가방
- 저장하기
- 게임종료

---

## 동작

- 메뉴가 열리면 `Time.timeScale = 0`
- 메뉴가 닫히면 `Time.timeScale = 1`
- PlayerController는 `Time.timeScale == 0`이면 입력을 무시한다.
- EnemyController도 `Time.timeScale == 0`이면 이동 / 추적을 멈춘다.

---

## 저장하기

- 현재 씬 이름을 GameManager에 반영
- 현재 플레이어 위치를 GameManager에 반영
- SaveSystem.SaveGame() 호출
- 저장 완료 메시지를 표시

---

## 게임종료

- Title 씬으로 이동
- Main BGM 꼬임이 생기지 않도록 BGMManager 정리 필요

---

## 폰트

MainMenuManager는 런타임으로 메뉴 UI를 생성하므로 코드에서 TMP Font Asset을 Resources.Load로 불러온다.

예시:

```csharp
menuFont = Resources.Load<TMP_FontAsset>("Fonts/NeoDGM_KR SDF");
```

폰트 파일 위치는 아래처럼 둔다.

```text
Assets/TextMesh Pro/Resources/Fonts/NeoDGM_KR SDF.asset
```
