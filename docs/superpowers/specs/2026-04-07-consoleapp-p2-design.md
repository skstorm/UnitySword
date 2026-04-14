# ConsoleApp P2 기능 반영 설계

## 개요

기존 P1 전용 ConsoleApp에 P2 기능(마스터리, 파편, 교환, 아이템 사용, 수집)을 전부 반영하고, JSON 기반 세션 저장/로드를 추가한다.

## 파일 구조

현재 `Program.cs`(318줄) 단일 파일을 역할별로 분리한 뒤 P2 기능을 추가한다.

```
ConsoleApp/
  Program.cs          — 진입점, CSV/JSON 로드, 게임 루프
  ConsoleRenderer.cs  — 상태 표시, 이벤트별 색상 출력
  MenuHandler.cs      — 메인 메뉴 + 상점 서브메뉴 입력 처리
  SaveManager.cs      — PlayerData ↔ JSON 저장/로드
```

## 게임 루프

1. `SaveManager`가 `save.json` 있으면 로드, 없으면 초기 상태 생성
2. `ConsoleRenderer`가 현재 상태 표시 (검, 골드, 파편, 마스터리, 인벤토리)
3. `MenuHandler`가 입력 받아 Command 실행
4. 이벤트 발생 시 `ConsoleRenderer`가 색상별 출력
5. 상태 변경 시 `SaveManager`가 자동 저장
6. 반복

## 메뉴 구조

### 메인 메뉴

```
=== 검 강화 ===
[현재 검: 나무검 +0] [골드: 200] [파편: 5] [마스터리: Lv.2]
[보호부적: 1개] [보호 활성: 없음]

1. 강화 (비용: 100G)
2. 판매 (50G 획득)
3. 수집 (도감에 등록)     ← 수집 가능 검일 때만 표시
4. 아이템 사용             ← 보호부적 보유 시만 표시
5. 상점
Q. 저장 후 종료
```

### 상점 서브메뉴

```
=== 상점 === [파편: 5]
1. 보호부적 구매 (30 파편)
2. 골드 주머니 구매 (10 파편 → 500G)
0. 돌아가기
```

### 파괴 대기 상태

기존과 동일. 파괴 대기 중일 때 `3. 파괴 확정`이 표시된다.

### 메뉴 표시 규칙

- 강화/판매/상점/종료: 항상 표시. 실행 불가 시 사유 메시지 출력.
- 수집: 현재 검의 `Collectible` 플래그가 true일 때만 표시.
- 아이템 사용: 보호부적 1개 이상 보유 시만 표시.

## 저장/로드 시스템

### 저장 파일

`save.json` — ConsoleApp 실행 디렉토리에 생성.

### 저장 시점

Command 실행 후 상태가 변경될 때마다 자동 저장. 종료(Q) 시 별도 저장 불필요.

### 저장 내용

GameState에서 영속 필요한 PlayerData 부분:

```json
{
  "gold": 1500,
  "fragments": 12,
  "isFirstRun": false,
  "statistics": { "totalAttempts": 50, "totalDestroys": 3, "totalSells": 2, "consecutiveSuccess": 0, "consecutiveFail": 1, "weeklyHighestLevel": 7, "totalGoldSpent": 3000 },
  "masteryData": { "level": 2, "currentExp": 5 },
  "inventory": { "protectionAmulet": 1, "goldPouch": 0 },
  "collection": { "collected": { "10": true, "15": true }, "totalCollectible": 5 },
  "achievements": [],
  "titles": []
}
```

### 로드 흐름

1. `save.json` 존재 → 역직렬화 → `PlayerData` 복원 → `CreateInitialState` 호출
2. 파일 없음 → `PlayerData(isFirstRun: true)` → `CreateInitialState`가 200G 지급

### 에러 처리

파일 손상 시 경고 메시지 출력 후 기본 상태로 시작.

## 이벤트 출력

| 이벤트 | 색상 | 출력 예시 |
|--------|------|----------|
| EnhanceSuccessEvent | Green | `★ 강화 성공! +3 → +4 (다음 비용: 150G)` |
| EnhanceFailEvent | Red | `✗ 강화 실패! 파괴됨!` / `✗ 강화 실패! 보호됨` |
| GoldChangeEvent | Yellow | `💰 -100G (잔액: 1400G)` |
| DestroyConfirmedEvent | DarkRed | `🗡 +5 나무검 파괴됨` |
| FragmentGainEvent | Cyan | `◆ 파편 +3 획득 (보유: 8)` |
| ExchangeEvent | Magenta | `🛒 보호부적 구매 (파편 -30)` |
| UseItemEvent | Blue | `🛡 보호부적 활성화!` |
| CollectEvent | Green | `📖 +10 검 도감에 등록!` |
| CollectionCompleteEvent | Yellow | `🏆 도감 완성!` |
| MasteryExpEvent | DarkYellow | `⚡ 마스터리 경험치 +1` |
| MasteryLevelUpEvent | Yellow | `⚡ 마스터리 Lv.3 달성! (비용 할인 10%)` |

## csproj 변경

`mastery_levels.csv`를 출력 디렉토리에 복사하도록 추가:

```xml
<Content Include="..\Project\Assets\Resources\Data\mastery_levels.csv" 
         CopyToOutputDirectory="PreserveNewest" Link="Data\mastery_levels.csv" />
```

## autotest 모드

기존 P1 시나리오 유지. P2 기능은 수동 플레이로 확인.

## 범위 외

- Unity 뷰 연동
- P3 기능 (랭킹, 업적, 리플레이 등)
- autotest P2 시나리오 확장
