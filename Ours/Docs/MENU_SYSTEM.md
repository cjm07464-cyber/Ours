# MENU_SYSTEM

TownScene/MainScene 메뉴 시스템 상세.

## 관련 파일

- `Assets/Scripts/Main/MainMenuManager.cs`
- `Assets/Scripts/Main/PlayerController.cs` 또는 `Assets/Scripts/PlayerController.cs`
- `Assets/Scripts/EnemyController.cs`
- `Assets/Scripts/Title/SaveSystem.cs`

## 입력

- C: 메뉴 열기 / 닫기
- X: 메뉴 닫기
- 방향키: 항목 선택
- Z: 결정

## 메뉴 항목

- 스탯
- 가방
- 저장하기
- 게임종료

## 동작

- 메뉴가 열리면 `Time.timeScale = 0`
- 메뉴가 닫히면 `Time.timeScale = 1`
- PlayerController는 `Time.timeScale == 0`이면 입력을 무시한다.
- EnemyController도 `Time.timeScale == 0`이면 이동 / 추적을 멈춘다.

## 저장하기

- 현재 씬 이름을 GameManager에 반영
- 현재 플레이어 위치를 GameManager에 반영
- 현재 플레이어 바라보는 방향을 GameManager에 반영
- SaveSystem.SaveGame() 호출
- 저장 완료 메시지를 표시

## 게임종료

- BootScene/Title 씬으로 이동
- Main/Town BGM 꼬임이 생기지 않도록 BGMManager 정리 필요

## 씬 명칭 주의

현재 메뉴 문서에서는 TownScene을 기준으로 설명하지만, 실제 프로젝트에는 아직 MainScene 이름이 남아 있을 수 있다.
씬 이름을 바꿀 때는 MainMenuManager의 게임종료 목적지, 저장 씬 이름, Build Settings를 함께 확인한다.
