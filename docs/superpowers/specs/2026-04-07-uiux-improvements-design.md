# UI/UX 개선 디자인 스펙

> 기존 직접 분석 + brainstorming 스킬 기반 분석 종합 결과

---

## 1. 시스템 변경: 부스터 시스템 전체 삭제

### 삭제 대상

| 항목 | 영향 범위 |
|------|----------|
| 광고 부스터 (광고 시청 → +5%p) | WatchAdCommand의 booster 타입, AdRewardLogic의 부스터 처리, EnhanceView 부스터 버튼 |
| 축복의 주문서 (파편 15개 → +5%p) | ExchangeCommand의 주문서 교환, UseItemCommand의 주문서 사용, Inventory.BlessingScrolls |
| IModifier 인터페이스 / ActiveModifiers | GameState.ActiveModifiers, EnhanceLogic.GetEffectiveRate()의 modifier 루프 |
| 강화 버튼의 "부스터 적용 중" 표시 | EnhanceButton.UpdateInfo()의 bonusRate 파라미터 |
| 아이템 슬롯의 주문서 슬롯 | EnhancePanel 아이템 슬롯 영역 |
| 부스터+주문서 중복 적용 관련 테스트 | P2-24, P2-26의 부스터 관련 테스트 케이스 |

### 잔존 파편 사용처

| 아이템 | 파편 비용 | 효과 |
|--------|----------|------|
| 보호의 부적 | 30개 | 다음 강화 실패 시 파괴 대신 단계 유지 (1회) |
| 골드 주머니 | 10개 | 500골드 획득 |

### 영향받는 문서

- `plan.md` — 광고 시스템 표의 부스터 행 삭제, 용어 정리 표에서 축복의 주문서/광고 부스터 삭제, 파편 사용처 테이블 축소
- `implementation-guide.md` — IModifier/ActiveModifiers 관련 코드 전부, EnhanceCommand의 modifier 적용 로직, 커맨드 종류 표에서 부스터 관련 삭제, 이벤트 페이로드에서 UseItemEvent의 주문서 타입 삭제, GameState의 ActiveModifiers 필드 삭제
- `ui-guide.md` — 아이템 슬롯 영역 삭제, 광고 버튼 표시 규칙에서 부스터 삭제, 확률 표시에서 bonusRate 관련 삭제
- `tasks.md` — P2-3 UseItemCommand에서 주문서 제거, P2-6 삭제, P2-24 삭제, P2-26에서 부스터 테스트 삭제

---

## 2. P1 골드 데드락 해결

### 문제

골드 0 + 나무검(판매 불가) 상태에서 강화 불가. P1에서 광고 미연동이므로 탈출 수단이 없음.

### 해결: AdService 모듈화 (인터페이스 + 구현체 교체)

Repository 패턴(`IStorageRepository` → InMemory/Local/Synced)과 동일한 구조로, 광고 서비스를 인터페이스로 추상화하고 환경별 구현체를 DI로 교체한다.

```
IAdService (인터페이스)
│   ShowRewardAd(adType, onSuccess, onFailed)
│
├── RealAdService     — 본환경: 실제 AdMob 리워드 광고 재생, 시청 완료 시 onSuccess 콜백
└── DevAdService      — 개발모드: 광고 표시 없이 즉시 onSuccess 콜백 반환
```

- `IAdService` 인터페이스는 `App` 어셈블리에 정의 (Unity 의존성 있으므로 GameCore 밖)
- `WatchAdCommand` 등 로직은 광고 표시 방법을 모름 — 뷰가 `IAdService`를 호출하고, 성공 콜백에서 커맨드를 dispatch
- DI 시점에 구현체만 교체하면 로직 코드 변경 없음

```csharp
public interface IAdService
{
    void ShowRewardAd(string adType, Action onSuccess, Action onFailed);
    bool IsReady(string adType);  // 광고 로딩 완료 여부
}

// 개발 모드 — 광고 없이 즉시 성공
public class DevAdService : IAdService
{
    public void ShowRewardAd(string adType, Action onSuccess, Action onFailed)
        => onSuccess?.Invoke();
    public bool IsReady(string adType) => true;
}

// 본환경 — 실제 AdMob 리워드 광고
public class RealAdService : IAdService { /* AdMob SDK 연동 */ }
```

| 환경 | 구현체 | 동작 |
|------|--------|------|
| P1 개발 | `DevAdService` | 버튼 탭 → 즉시 200골드 지급 |
| P2+ 본환경 | `RealAdService` | 버튼 탭 → 리워드 광고 → 시청 완료 → 200골드 지급 |

---

## 3. 대장간 레이아웃 개선

### 기존 문제 (직접 분석)

- 액션 영역 30%에 버튼 6개 과밀 → 터치 영역 48dp 미달 위험
- 확률/비용이 검 아래 + 강화 버튼 내부에 이중 표시
- 강화 연타 중 바로 아래 판매 버튼 오조작 위험
- 아이템 슬롯 7%(134px)에 3개 요소 → 가독성 부족
- 광고 버튼 상시 노출 → "광고 게임" 인식 위험

### 확정 레이아웃

```
┌─────────────────────────┐
│ 💰 12,500G       🔷 파편 23 │  ← 상단 바
├─────────────────────────┤
│                         │
│     [ 검 비주얼 ]        │  ← 검 이미지 (화면 중앙)
│     "+12 천총운검"       │
│     한국 신화            │
│                         │
├─────────────────────────┤
│  🛡 부적 x2 보유중        │  ← 부적 인디케이터 (보유시만)
├─────────────────────────┤
│  [💰 판매 2,310G] [📦 수집] │  ← 보조 버튼 (강화 위)
├─────────────────────────┤
│  ┌─────────────────────┐│
│  │    🔨 강 화          ││  ← 메인 버튼 (최하단, 크게)
│  │    20% · 600G       ││
│  └─────────────────────┘│
│  [📺 골드 받기 (+200G)]   │  ← 광고 버튼 1개만
├─────────────────────────┤
│ [대장간] [공방] [업적]    │  ← 하단 네비게이션
└─────────────────────────┘
```

### 해결된 문제

| 기존 문제 | 해결 방법 |
|----------|----------|
| 버튼 6개 과밀 | 부스터 삭제로 4개 → 3개(+광고1), 아이템 슬롯 영역 삭제 |
| 확률/비용 이중 표시 | 강화 버튼 내부에만 표시 ("20% · 600G") |
| 강화/판매 오조작 | 판매/수집을 강화 버튼 **위**로 이동, 물리적 분리 |
| 아이템 슬롯 7% 부족 | 슬롯 삭제, 부적만 한 줄 인디케이터로 표시 |
| 광고 상시 노출 | 광고 버튼 1개만 (골드 받기), 부스터 버튼 삭제 |

### 상태별 버튼 표시 규칙

| 상태 | 표시 |
|------|------|
| +0 나무검 | 판매/수집 숨김, 강화 + 골드받기만 |
| +1 ~ +9 | 판매 표시, 수집 숨김 |
| +10 ~ +20 | 판매 + 수집 둘 다 표시 |
| 골드 부족 | 강화 비활성(회색), 골드받기 강조(펄스) |
| 연출 중 | 전체 버튼 + 하단 네비 비활성 |
| pendingAdProtection (P2) | 전체 비활성 + 파괴 팝업 오버레이, 네비도 비활성 |

---

## 4. 화면 전이 누락 보완

### ❶ 타이틀 복귀 (Android 뒤로가기)

대장간에서 뒤로가기 시 확인 팝업:
```
"현재 진행이 초기화됩니다.
 타이틀로 돌아가시겠습니까?"
 [취소]  [확인]
```
- 확인 → 타이틀로 전환 (현재 검 리셋)
- 취소 → 팝업 닫기, 대장간 유지

### ❷ 연출 중 입력 차단

- 강화 연출 재생 시작 → **전체 화면 Raycast blocker overlay 활성화**
- 강화 버튼, 판매/수집, 광고 골드, 하단 네비 모두 차단
- Android 뒤로가기도 무시
- 연출 완료 → blocker 해제, 모든 버튼 재활성화

### ❸ 팝업 중 뒤로가기

| 팝업 | 뒤로가기 동작 |
|------|-------------|
| 판매 확인 | 취소 (팝업 닫기) |
| 수집 확인 (P2) | 취소 (팝업 닫기) |
| 파괴 팝업 (P2) | 무시 (반드시 선택) |
| 타이틀 복귀 확인 | 취소 (팝업 닫기) |

### ❹ pendingAdProtection 중 탭 전환

- 파괴 팝업은 **모달** — 해소(광고 복구 or 파괴 확정) 전까지 하단 네비 비활성
- 팝업 외부 터치 차단 (기존 사양 유지)

### ❺ 앱 백그라운드/포그라운드

- 백그라운드 전환 시: PlayerData push (기존 사양 유지)
- **포그라운드 복귀 시:**
  - 연출 재생 중이었으면 → 연출 즉시 스킵, 결과 상태 반영
  - 팝업 중이었으면 → 팝업 유지 (유저가 선택해야 하므로)
  - 일반 상태 → 화면 그대로 유지

### ❻ 게임 시작 로딩

- P1 (InMemoryRepository): 즉시 전환 (로딩 없음)
- P2+ (LocalStorage/Firebase): 로딩 스피너 표시 → 데이터 로드 완료 → 대장간 전환

---

## 5. 문서 영향 범위 요약

| 문서 | 변경 내용 |
|------|----------|
| `plan.md` | 부스터 관련 삭제 (광고 시스템 표, 용어 정리, 파편 사용처) |
| `implementation-guide.md` | IModifier/ActiveModifiers 삭제, EnhanceCommand modifier 로직 삭제, 커맨드/이벤트 표 수정, GameState 필드 수정 |
| `ui-guide.md` | 대장간 레이아웃 전면 교체, 아이템 슬롯 삭제, 화면 전이 규칙 6건 추가, 상태별 버튼 표시 규칙 추가 |
| `tasks.md` | P2-6(주문서 로직) 삭제, P2-24(부스터 적용) 삭제, P2-26 부스터 테스트 삭제, 화면 전이 관련 태스크 추가 |
