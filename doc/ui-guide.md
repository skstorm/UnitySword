# 검강화 게임 UI/UX 구현 지침서

> 메인 구현계획서(`implementation-guide.md`)의 로직/뷰 분리 아키텍처를 기반으로,
> Unity 뷰 레이어의 구체적인 구현 방법을 정의한다.
> 로직 인터페이스(Command, Event, GameState)는 메인 구현계획서 참조.

---

## 기술 선택

| 항목 | 선택 | 이유 |
|------|------|------|
| UI 프레임워크 | **uGUI (Canvas)** | 모바일 최적화 성숙도, 에셋/튜토리얼 풍부, 캐주얼 게임에 충분 |
| 애니메이션 | **DOTween + Coroutine** | 코드 기반 트윈이 강화 연출의 동적 파라미터에 적합 |
| 레이아웃 | **Canvas Scaler (Scale With Screen Size)** | 기준 해상도 1080×1920, Match Width Or Height (0.5) |
| 폰트 | **TextMeshPro** | 한글 지원, 아웃라인/그림자 등 이펙트 텍스트에 필수 |
| 화면 전환 | **단일 씬 + Panel 활성화/비활성화** | 씬 로딩 없이 즉시 전환, 상태 유지 용이 |

---

## 씬 / 프리팹 구조

```
Assets/
├── Scenes/
│   └── GameScene.unity          ← 단일 씬
│
├── Prefabs/
│   ├── UI/
│   │   ├── TitlePanel.prefab
│   │   ├── EnhancePanel.prefab      ← 대장간 (메인)
│   │   ├── WorkshopPanel.prefab     ← 공방
│   │   ├── AchievementPanel.prefab  ← 업적/칭호
│   │   ├── BottomNavBar.prefab      ← 하단 네비게이션
│   │   └── Popups/
│   │       ├── DestroyPopup.prefab      ← 파괴 결과 + 광고 보호권
│   │       ├── SellConfirmPopup.prefab
│   │       └── CollectConfirmPopup.prefab
│   │
│   ├── Widgets/
│   │   ├── SwordDisplay.prefab      ← 검 비주얼 + 이름
│   │   ├── GoldIndicator.prefab     ← 골드 표시
│   │   ├── EnhanceButton.prefab     ← 강화 버튼 (확률 표시 포함)
│   │   └── FragmentIndicator.prefab ← 파편 표시
│   │
│   └── Effects/
│       ├── EnhanceSuccessEffect.prefab
│       ├── EnhanceFailEffect.prefab
│       └── ScreenFlash.prefab
│
├── ScriptableObjects/
│   └── AnimationConfigs/
│       ├── EnhanceAnim_Lv01_05.asset
│       ├── EnhanceAnim_Lv06_09.asset
│       ├── EnhanceAnim_Lv10_14.asset
│       ├── EnhanceAnim_Lv15_17.asset
│       └── EnhanceAnim_Lv18_20.asset
│
└── Art/
    ├── Sprites/
    │   ├── Swords/               ← 검 일러스트 (21종)
    │   ├── UI/                   ← 버튼, 프레임, 아이콘
    │   └── Backgrounds/          ← 대장간, 공방 배경
    ├── Fonts/
    └── Audio/
        ├── SFX/
        └── BGM/
```

### GameScene 하이어라키

```
GameScene
├── [Main Camera]
├── [EventSystem]
├── GameManager (GameBinding.cs)
│
├── UICanvas (Canvas — Screen Space Overlay)
│   ├── TitlePanel
│   ├── MainPanel
│   │   ├── EnhancePanel
│   │   ├── WorkshopPanel (P2)
│   │   └── AchievementPanel (P3)
│   ├── BottomNavBar
│   └── PopupLayer
│       ├── DestroyPopup
│       └── ...
│
└── EffectCanvas (Canvas — Screen Space Overlay, Sort Order +1)
    ├── ScreenFlash
    ├── ScreenShake (카메라가 아닌 Canvas 오프셋으로 처리)
    └── ParticleLayer
```

---

## 화면 흐름 & 네비게이션

```
앱 시작 → TitlePanel (게임 시작 버튼)
              │
              ▼
         MainPanel 활성화 + TitlePanel 비활성화
              │
              ├── EnhancePanel (기본 활성)
              ├── WorkshopPanel (탭 전환)
              └── AchievementPanel (탭 전환)
              │
         BottomNavBar (3탭: 대장간 / 공방 / 업적)
```

### 화면 전환 매니저

```csharp
public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject _titlePanel;
    [SerializeField] private GameObject _enhancePanel;
    [SerializeField] private GameObject _workshopPanel;
    [SerializeField] private GameObject _achievementPanel;

    private GameObject _currentPanel;

    public void ShowTitle() => SwitchTo(_titlePanel);
    public void ShowEnhance() => SwitchTo(_enhancePanel);
    public void ShowWorkshop() => SwitchTo(_workshopPanel);
    public void ShowAchievement() => SwitchTo(_achievementPanel);

    private void SwitchTo(GameObject panel)
    {
        _currentPanel?.SetActive(false);
        panel.SetActive(true);
        _currentPanel = panel;
    }
}
```

- 전환 시 페이드 애니메이션은 P4에서 추가 (P1은 즉시 전환)
- 팝업은 별도 레이어에서 활성화 (메인 화면 위에 오버레이)

---

## 화면별 레이아웃 상세

### 1. 타이틀 화면 (TitlePanel)

```
┌─────────────────────────┐
│                         │
│                         │
│      [게임 로고/타이틀]    │  ← 상단 40%
│       "검강화"           │
│                         │
│                         │
├─────────────────────────┤
│                         │
│     [ 게임 시작 ]        │  ← 중앙 버튼
│                         │
│     [ 설정 ⚙ ]          │  ← 작은 버튼
│                         │
└─────────────────────────┘
```

| 요소 | 컴포넌트 | 앵커 | 비고 |
|------|---------|------|------|
| 타이틀 텍스트 | TMP_Text | 상단 중앙 | 폰트 크기 72, Bold |
| 게임 시작 버튼 | Button + TMP_Text | 중앙 | 크기 400×100, 메인 액센트 색상 |
| 설정 버튼 | Button + Image | 중앙 하단 | 크기 80×80, 아이콘 |
| 배경 | Image | Stretch | 대장간 분위기 일러스트 |

- 게임 시작 → `ScreenManager.ShowEnhance()` + `GameBinding`에서 엔진 초기화
- P1: 설정 버튼은 비활성 placeholder

### 2. 대장간 화면 (EnhancePanel) — 핵심 화면

```
┌─────────────────────────┐
│ 💰 12,500G     🔷 파편 23 │  ← 상단 바 (골드 + 파편)
├─────────────────────────┤
│                         │
│     [ 검 비주얼 ]        │  ← 검 이미지 (화면 중앙)
│     "+12 쿠사나기"       │  ← 검 이름 + 강화 단계
│                         │
│   성공률: 35%  비용: 270G  │  ← 확률/비용 (검 바로 아래)
│                         │
├─────────────────────────┤
│  [🛡 부적 x2] [📜 주문서 x1] │  ← 아이템 슬롯 (P2, 적용 상태 표시)
│  [📺 부스터 +5%]              │  ← 광고 부스터 (적용 중일 때만 표시)
├─────────────────────────┤
│                         │
│  ┌─────────────────────┐│
│  │    🔨 강 화          ││  ← 메인 버튼 (크고 눈에 띄게)
│  │    성공률 35%         ││
│  └─────────────────────┘│
│                         │
│  [💰 판매 2,800G] [📦 수집] │  ← 보조 버튼 (작게, 수집은 +10↑)
│                         │
│  [📺 골드 받기] [📺 부스터] │  ← 광고 버튼 (골드 부족 시 강조)
│                         │
├─────────────────────────┤
│ [대장간] [공방] [업적]    │  ← 하단 네비게이션
└─────────────────────────┘
```

| 영역 | 비율 | 내용 |
|------|------|------|
| 상단 바 | 8% | 골드, 파편 (P2), 장인 레벨 아이콘 (P2) |
| 검 디스플레이 | 35% | 검 이미지 + 이름 + 확률/비용 텍스트 + 강화 이펙트 영역 |
| 아이템 슬롯 | 7% | 부적/주문서 슬롯 (P2), 광고 부스터 적용 표시 |
| 액션 영역 | 30% | 강화 메인 버튼 (크게) + 판매/수집 보조 버튼 (작게) + 광고 버튼 |
| 하단 네비 | 10% | 3탭 네비게이션 |
| 여백 | 10% | 상하 SafeArea 패딩 |

#### 검 디스플레이 (SwordDisplay)

```csharp
public class SwordDisplay : MonoBehaviour
{
    [SerializeField] private Image _swordImage;
    [SerializeField] private TMP_Text _swordNameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Transform _effectAnchor;  // 이펙트 재생 위치

    public void UpdateSword(Sword sword, int level)
    {
        _swordImage.sprite = LoadSwordSprite(level);
        _swordNameText.text = sword.Name;
        _levelText.text = level > 0 ? $"+{level}" : "";
    }
}
```

#### 강화 버튼 (EnhanceButton)

메인 강화 버튼은 화면에서 가장 크고 눈에 띄는 요소. 확률과 비용을 버튼 내부에 표시.

```csharp
public class EnhanceButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _rateText;   // "35%" 또는 "???"
    [SerializeField] private TMP_Text _costText;   // "270G"
    [SerializeField] private Image _buttonBg;      // 골드 부족 시 회색 처리

    public void UpdateInfo(int level, double rate, int cost, int gold, double bonusRate)
    {
        // 기본 확률 표시
        if (level < 15)
        {
            double totalRate = rate + bonusRate;
            _rateText.text = bonusRate > 0
                ? $"{rate * 100:F0}% + {bonusRate * 100:F0}%"  // "35% + 5%"
                : $"{rate * 100:F0}%";
        }
        else
        {
            _rateText.text = bonusRate > 0 ? "??? + 부스터" : "???";
        }

        _costText.text = $"{cost:N0}G";

        bool canAfford = gold >= cost;
        _button.interactable = canAfford;
        _buttonBg.color = canAfford ? _activeColor : _disabledColor;
    }

    [SerializeField] private Color _activeColor = new Color(1f, 0.6f, 0f);   // #FF9800
    [SerializeField] private Color _disabledColor = new Color(0.46f, 0.46f, 0.46f); // #757575
}
```

#### 강화 선택지 표시 규칙

| 조건 | 표시 버튼 |
|------|----------|
| +0 (나무검) | [강화] 만 표시 (판매/수집 불가) |
| +1 ~ +9 | [강화] [판매] |
| +10 ~ +20 | [강화] [판매] [수집] |
| 골드 부족 | [강화] 비활성 (회색) + [📺 골드 받기] 버튼 강조 (펄스 애니메이션) |
| pendingAdProtection (P2) | 모든 버튼 비활성, 파괴 팝업 표시 |

#### 광고 버튼 표시 규칙

| 버튼 | 위치 | 표시 조건 | 비고 |
|------|------|----------|------|
| 📺 골드 받기 | 액션 영역 하단 | 항상 표시 (골드 부족 시 강조) | 200골드 고정, 무제한 |
| 📺 부스터 | 액션 영역 하단 | 부스터 미적용 상태일 때 표시 | +5%p, 1회용 |

- 광고 부스터 적용 중일 때: 부스터 버튼 대신 아이템 슬롯 영역에 "📺 부스터 +5%" 배지 표시
- P1에서는 광고 연동 없이 버튼 UI만 배치 (탭 시 "준비 중" 토스트)

#### 아이템 슬롯 표시 규칙 (P2)

| 상태 | 표시 |
|------|------|
| 부적/주문서 미보유 | 슬롯 비활성 (회색), 보유 수량 "x0" |
| 부적/주문서 보유 | 슬롯 활성, 탭하여 적용 토글 |
| 부적 적용 중 | 🛡 아이콘 글로우 + "보호 ON" 배지 |
| 주문서 적용 중 | 📜 아이콘 글로우 + "+5%" 배지 |
| 부스터+주문서 동시 | 확률 표시에 "+10%" 배지 (최대치 표시) |

### 3. 공방 화면 (WorkshopPanel) — P2

3개 섹션을 **세로 스크롤** 또는 **상단 탭**으로 구성:

```
┌─────────────────────────┐
│ [숙련도] [교환소] [도감]  │  ← 탭 바
├─────────────────────────┤
│                         │
│  탭별 콘텐츠 영역         │  ← 스크롤 가능
│                         │
├─────────────────────────┤
│ [대장간] [공방] [업적]    │  ← 하단 네비게이션
└─────────────────────────┘
```

**장인 숙련도 탭:**
- 레벨 표시 (예: "Lv.4 숙련 장인")
- 경험치 프로그레스 바 (현재/다음 레벨까지)
- 다음 레벨 보상 미리보기 텍스트
- 공방 배경 이미지 (레벨에 따라 변화 — P4)

**파편 교환소 탭:**
- 보유 파편 수 상단 표시
- 교환 아이템 3종 리스트 (보호의 부적 / 축복의 주문서 / 골드 주머니)
- 각 아이템: 아이콘 + 이름 + 비용 + [교환] 버튼 + 보유 수량

**컬렉션 도감 탭:**
- 그리드 레이아웃 (3열)
- 수집한 검: 검 이미지 + 이름 + 별 (중복 수 ★★★)
- 미수집 검: 실루엣 + "???"
- 상단에 완성도 표시 ("3/11 — 27%")

### 4. 업적/칭호 화면 (AchievementPanel) — P3

```
┌─────────────────────────┐
│ [업적] [칭호]            │  ← 탭 바
├─────────────────────────┤
│                         │
│  업적/칭호 리스트         │  ← 스크롤 가능
│                         │
├─────────────────────────┤
│ [대장간] [공방] [업적]    │  ← 하단 네비게이션
└─────────────────────────┘
```

**업적 탭:** 세로 리스트. 각 항목: 아이콘 + 조건 텍스트 + 달성 여부 (체크/미달성)
**칭호 탭:** 세로 리스트. 각 항목: 칭호명 + [장착] 버튼 (현재 장착 중 표시)

### 5. 팝업

#### 파괴 경험 (P1 vs P2)

**P1 (MVP):** 팝업 없이 인라인 처리. 파괴 연출 후 즉시 나무검 리셋.

```
[파괴 연출 재생] → 파괴 텍스트 ("파괴!" 빨간색)
    → "성공했다면 💰 6,360G 였는데..." 도발 메시지 (1.5초 표시)
    → 나무검으로 자동 리셋 (ConfirmDestroyCommand 자동 dispatch)
```

- P1에서는 광고 보호권이 없으므로 선택지 불필요
- 도발 메시지는 검 디스플레이 영역 하단에 페이드 인/아웃
- 판매가는 `SwordDataTable`에서 다음 레벨의 `SellPrice` 참조

**P2 (광고 보호권 추가 후):** 파괴 팝업으로 전환.

#### 파괴 팝업 (DestroyPopup) — P2

```
┌───────────────────────┐
│                       │
│   +14 듀랑달이         │
│   파괴되었습니다...     │
│                       │
│ 성공했다면 판매가:      │  ← "성공했다면" 도발 메시지
│ 💰 6,360G 였는데...    │
│                       │
│ ┌───────────────────┐ │
│ │ 📺 광고 보고 복구  │ │  ← 광고 보호권 (일일 2회)
│ │   (남은 횟수: 1/2) │ │  ← 잔여 횟수 표시
│ └───────────────────┘ │
│ ┌───────────────────┐ │
│ │    파괴 확정       │ │  ← ConfirmDestroyCommand
│ └───────────────────┘ │
└───────────────────────┘
```

- `EnhanceFailEvent.AdProtectionAvailable == false`면 광고 버튼 숨기고 파괴 확정만 표시
- 광고 버튼 탭 → `WatchAdCommand(protection)` dispatch
- 파괴 확정 탭 → `ConfirmDestroyCommand` dispatch
- 팝업 외부 터치로 닫기 불가 (반드시 선택)
- 광고 보호권 잔여 횟수를 버튼 내에 표시 (일일 2회 제한의 투명성)

#### 판매 확인 팝업

```
┌───────────────────────┐
│  +12 쿠사나기를         │
│  판매하시겠습니까?       │
│                       │
│  💰 2,800G 획득        │
│  (투자 대비 130% 회수)  │  ← 회수율 표시로 판단 근거 제공
│                       │
│  [취소]  [판매하기]     │
└───────────────────────┘
```

- P1부터 구현 (판매는 P1 핵심 기능)
- 회수율은 `SwordDataTable`의 `RecoveryRate` 참조

#### 수집 확인 팝업 (P2)

```
┌───────────────────────┐
│  +12 쿠사나기를         │
│  수집하시겠습니까?       │
│                       │
│  📦 컬렉션에 등록       │
│  (골드 보상 없음)       │  ← 판매와의 트레이드오프 명시
│                       │
│  [취소]  [수집하기]     │
└───────────────────────┘
```

- +10 이상에서만 수집 버튼 표시
- 수집 시 골드 없음을 명시하여 실수 방지

---

## 하단 네비게이션 (BottomNavBar)

```csharp
public class BottomNavBar : MonoBehaviour
{
    [SerializeField] private Button _forgeTab;
    [SerializeField] private Button _workshopTab;
    [SerializeField] private Button _achievementTab;
    [SerializeField] private Image[] _tabIcons;
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _inactiveColor;

    public event Action<int> OnTabSelected;

    private void Start()
    {
        _forgeTab.onClick.AddListener(() => SelectTab(0));
        _workshopTab.onClick.AddListener(() => SelectTab(1));
        _achievementTab.onClick.AddListener(() => SelectTab(2));
        SelectTab(0);
    }

    private void SelectTab(int index)
    {
        for (int i = 0; i < _tabIcons.Length; i++)
            _tabIcons[i].color = i == index ? _activeColor : _inactiveColor;
        OnTabSelected?.Invoke(index);
    }
}
```

- P1: 공방/업적 탭은 탭 가능하지만 "준비 중" placeholder 표시
- 탭 아이콘: 대장간(망치), 공방(집), 업적(트로피)

---

## 공용 UI 컴포넌트

### GoldIndicator

상단 바의 골드 표시. `GoldChangeEvent` 구독하여 자동 갱신.

```csharp
public class GoldIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text _goldText;

    public void UpdateGold(int amount)
    {
        _goldText.text = $"{amount:N0}G";
    }

    // 골드 변동 시 숫자 카운팅 연출 (DOTween)
    public void AnimateChange(int from, int to, float duration = 0.3f)
    {
        DOTween.To(() => from, x => {
            from = x;
            _goldText.text = $"{x:N0}G";
        }, to, duration);
    }
}
```

### FragmentIndicator (P2)

파편 보유량 표시. 동일 구조.

### ToastMessage

일시적 알림 (업적 달성, 레벨업 등). 화면 상단에서 슬라이드 인 → 2초 후 페이드 아웃.

---

## 연출 타임라인 상세

> 메인 구현계획서의 `IEnhanceAnimationController` 인터페이스를 구현하는 상세 타임라인.
> 모든 시간값은 `EnhanceAnimationConfig` ScriptableObject에서 읽음.

### 강화 시도 (PlayEnhanceAttempt)

```
[0.0s] 강화 버튼 비활성화
[0.0s] 검 이미지에 글로우 이펙트 시작 (펄스)
[0.0s ~ SuspenseDuration] 서스펜스 연출
  - +1~+5: 화면 살짝 어둡게 (배경 알파 0.3)
  - +6~+9: 어둡게 + 검 주변 파란 파티클
  - +10~+14: 어둡게 + 보라 파티클 + 약한 화면 흔들림
  - +15~+17: 어둡게 + 금색 파티클 + 강한 흔들림 + 진동
  - +18~+20: 어둡게 + 무지개 파티클 + 최강 흔들림 + 진동
[SuspenseDuration] 판정 결과 분기 → PlaySuccess 또는 PlayDestroy
```

### 강화 성공 (PlaySuccess)

```
[0.0s] 배경 밝아짐 (Flash 화이트, 0.1s)
[0.1s] 검 이미지 스케일 펀치 (1.0 → 1.3 → 1.0, 0.3s)
[0.1s] 성공 텍스트 팝업 ("강화 성공!" + 초록색, 스케일 0→1, 바운스)
[0.1s] 성공 파티클 재생 (EffectColor, EffectScale 적용)
[0.4s] 검 이미지 교체 (새 검 스프라이트)
[0.4s] 레벨 텍스트 갱신 (숫자 카운트업 연출)
[0.8s] 성공 텍스트 페이드 아웃
[1.0s] 강화 버튼 재활성화
```

### 강화 실패 — 파괴 (PlayDestroy)

#### P1 (인라인 처리 — 팝업 없음)

```
[0.0s] 화면 빨간 플래시 (0.15s)
[0.0s] 검 이미지 흔들림 (좌우 진동, 0.3s)
[0.15s] 파괴 텍스트 ("파괴!" + 빨간색, 스케일 펀치)
[0.3s] 검 이미지 페이드 아웃
[0.5s] "성공했다면 💰 6,360G 였는데..." 도발 메시지 페이드 인
[1.5s] 도발 메시지 페이드 아웃
[1.8s] 나무검 페이드 인 (자동 리셋, ConfirmDestroyCommand 자동 dispatch)
[2.0s] 강화 버튼 재활성화
```

#### P2+ (파괴 팝업 — 광고 보호권 선택지)

```
[0.0s] 화면 빨간 플래시 (0.15s)
[0.0s] 검 이미지 흔들림 (좌우 진동, 0.3s)
[0.15s] 파괴 텍스트 ("파괴!" + 빨간색, 스케일 펀치)
[0.3s] 검 이미지 페이드 아웃 + 파편 파티클 (깨지는 느낌)
[0.3s] 진동 (EnableHaptic일 때)
[0.5s] "성공했다면" 도발 메시지 페이드 인
[0.8s] 파괴 팝업 표시 (광고 보호권 선택지)
       → 광고 복구 선택 시: 검 복구 연출 (파편 역재생 느낌, 0.5s) → 현재 단계 유지
       → 파괴 확정 선택 시: 나무검 페이드 인 (리셋)
```

### 판매 (PlaySell)

```
[0.0s] 검 이미지 위로 슬라이드 아웃 (0.3s)
[0.1s] 골드 아이콘 이펙트 (동전 날아가는 파티클)
[0.3s] GoldIndicator 카운팅 연출
[0.5s] 나무검으로 교체 (페이드 인)
```

### 수집 (PlayCollect)

```
[0.0s] 검 이미지에 금색 테두리 글로우
[0.2s] "컬렉션 등록!" 텍스트 팝업
[0.3s] 검 이미지가 축소되며 화면 우측으로 날아감 (도감으로 가는 느낌)
[0.6s] 나무검으로 교체 (페이드 인)
```

### P1 vs P2 vs P4 구현 범위

| 연출 요소 | P1 (Basic) | P2 (광고/아이템) | P4 (Rich) |
|----------|-----------|----------------|----------|
| 서스펜스 | 딜레이 + 어둡게만 | P1과 동일 | 파티클 + 흔들림 + 진동 |
| 성공 이펙트 | 초록 텍스트 + 스케일 | P1과 동일 | 폭발 파티클 + 사운드 |
| 파괴 이펙트 | 빨간 텍스트 + 도발 메시지 + 자동 리셋 | 파괴 팝업 (광고 보호권 선택) | 파편 파티클 + 사운드 + 진동 |
| 판매 | 확인 팝업 + 골드 카운팅 | P1과 동일 | 동전 파티클 + 사운드 |
| 아이템 상태 | 없음 | 글로우 + 배지 표시 | 적용 시 이펙트 연출 |
| 화면 흔들림 | 없음 | 없음 | ScreenShakeIntensity 적용 |
| 사운드 | 없음 | 없음 | SuccessSound / DestroySound |
| 연속 연출 | 없음 | 없음 | 콤보 불꽃, 연속 파괴 어둡게 |

---

## 비주얼 스타일 가이드

### 색상 팔레트

| 용도 | 색상 | Hex | 비고 |
|------|------|-----|------|
| 배경 (대장간) | 어두운 갈색 | #2C1810 | 대장간 분위기 |
| 메인 액센트 | 주황/금색 | #FF9800 | 강화 버튼, 골드 |
| 성공 | 초록 | #4CAF50 | 성공 텍스트/이펙트 |
| 실패/파괴 | 빨강 | #F44336 | 파괴 텍스트/이펙트 |
| 확률 텍스트 | 하양 | #FFFFFF | 확률 숫자 표시 |
| "???" 텍스트 | 보라 | #9C27B0 | +15↑ 미지의 확률 |
| 비활성 | 회색 | #757575 | 비활성 버튼, 미수집 검 |
| UI 프레임 | 짙은 회색 | #424242 | 정보 패널 배경 |

### 타이포그래피

| 용도 | 폰트 스타일 | 크기 | 비고 |
|------|-----------|------|------|
| 검 이름 | Bold | 36 | 강화 단계 색상 차등 |
| 강화 레벨 | Bold | 48 | "+15" 등 |
| 확률/비용 | Regular | 28 | 정보 영역 |
| 골드 숫자 | Bold | 32 | 상단 바 |
| 버튼 텍스트 | Bold | 30 | 액션 버튼 |
| 팝업 본문 | Regular | 26 | 팝업 메시지 |
| 도발 메시지 | Italic | 24 | "성공했다면..." |

### 검 이름 색상 (강화 단계별)

| 구간 | 색상 | 느낌 |
|------|------|------|
| +0~+5 | 하양 #FFFFFF | 일반 |
| +6~+9 | 파랑 #2196F3 | 레어 |
| +10~+14 | 보라 #9C27B0 | 에픽 |
| +15~+17 | 금색 #FFD700 | 레전더리 |
| +18~+19 | 빨강 #FF1744 | 미시크 |
| +20 | 무지개 (그라디언트 애니메이션) | 궁극 |

---

## 사운드 & 햅틱 설계 (P4)

> P1에서는 사운드/햅틱 없음. P4에서 추가. 에셋 ID는 `EnhanceAnimationConfig`에서 관리.

### SFX 목록

| ID | 상황 | 설명 |
|----|------|------|
| sfx_enhance_attempt | 강화 시도 | 망치 두드리는 소리 (서스펜스 중 반복) |
| sfx_success_low | +1~+9 성공 | 가벼운 금속 울림 |
| sfx_success_high | +10~+17 성공 | 웅장한 팡파레 |
| sfx_success_max | +18~+20 성공 | 폭발적 오케스트라 |
| sfx_destroy | 파괴 | 유리 깨지는 소리 |
| sfx_sell | 판매 | 동전 소리 |
| sfx_collect | 수집 | 보석함 닫히는 소리 |
| sfx_levelup | 장인 레벨업 | 레벨업 효과음 |
| sfx_button | 버튼 탭 | 가벼운 클릭음 |

### BGM

| 화면 | 분위기 |
|------|--------|
| 타이틀 | 장엄한 인트로 (루프 없음, 짧게) |
| 대장간 | 긴장감 있는 루프 (대장간 분위기, 불꽃 소리 앰비언스) |
| 공방 | 평화로운 루프 (마을 느낌) |

### 햅틱 (진동)

| 상황 | 강도 | 시간 |
|------|------|------|
| 강화 시도 서스펜스 (+10↑) | Light | 50ms × 반복 |
| 성공 | Medium | 100ms |
| 파괴 | Heavy | 200ms |

```csharp
// 햅틱 유틸리티 (모바일 전용)
public static class HapticHelper
{
    public static void Light() => Handheld.Vibrate(); // 실제론 플러그인 사용 권장
    public static void Medium() => Handheld.Vibrate();
    public static void Heavy() => Handheld.Vibrate();
}
```

---

## 해상도 & 입력 처리

### 해상도 대응

| 항목 | 설정 |
|------|------|
| 기준 해상도 | 1080 × 1920 (9:16) |
| Canvas Scaler | Scale With Screen Size |
| Match | 0.5 (Width Or Height 균등) |
| SafeArea | 노치/펀치홀 영역 패딩 (SafeAreaPanel 컴포넌트) |

```csharp
// SafeArea 자동 패딩
public class SafeAreaPanel : MonoBehaviour
{
    private void Awake()
    {
        var rect = GetComponent<RectTransform>();
        var safeArea = Screen.safeArea;
        var min = safeArea.position;
        var max = safeArea.position + safeArea.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;
        rect.anchorMin = min;
        rect.anchorMax = max;
    }
}
```

### 태블릿 대응

- 16:9 ~ 19.5:9 범위에서 정상 동작
- 태블릿(4:3)에서는 좌우 여백 자동 추가 (Canvas Scaler가 처리)
- 검 이미지/버튼 크기는 고정 px 기반이 아닌 비율 기반

### 입력 처리

| 항목 | 처리 |
|------|------|
| 강화 연타 | 연출 재생 중 강화 버튼 비활성화. 연출 완료 후 재활성화 |
| 더블 탭 방지 | 커맨드 dispatch 후 0.1초 쿨타임 |
| 뒤로가기 (Android) | 대장간→타이틀 확인 팝업, 공방/업적→대장간 전환 |
| 팝업 중 입력 | 팝업 외부 터치 차단 (Raycast blocker) |

```csharp
// 연타 방지 — EnhanceView에서 사용
private bool _isProcessing;

private void OnEnhanceButtonClicked()
{
    if (_isProcessing) return;
    _isProcessing = true;
    _engine.Dispatch(new EnhanceCommand());
    // 연출 완료 콜백에서 _isProcessing = false
}
```

---

## Phase별 UI 구현 범위

### P1 (MVP)

- TitlePanel, EnhancePanel, BottomNavBar
- SwordDisplay, GoldIndicator, EnhanceButton (신규 레이아웃 — 강화 메인 버튼 크게)
- 판매 확인 팝업 (SellConfirmPopup)
- 파괴 시 인라인 도발 메시지 + 자동 나무검 리셋 (팝업 없음)
- BasicEnhanceAnimation (최소 연출)
- 광고 버튼 UI 배치 (📺 골드 받기, 📺 부스터) — 탭 시 "준비 중" 토스트, 실제 연동은 P2
- 공방/업적 탭은 "준비 중" placeholder
- 사운드/진동 없음
- 검 이미지: 단계 구간별 placeholder 색상 스프라이트 (정식 아트 전)

### P2

- WorkshopPanel (숙련도 + 교환소 + 도감)
- FragmentIndicator
- 아이템 슬롯 UI (부적/주문서 — 적용 상태 글로우+배지 표시)
- 파괴 팝업 (DestroyPopup) + 광고 보호권 선택지 (잔여 횟수 표시)
- 수집 확인 팝업 (CollectConfirmPopup)
- 광고 연동 (`AdService.cs` — Unity Ads / AdMob Unity 리워드 광고)
- 광고 골드/부스터 실제 동작 연결

### P3

- AchievementPanel (업적 + 칭호)
- 랭킹 UI (공방 또는 별도)
- 닉네임 표시
- ToastMessage (업적 달성, 레벨업 등)

### P4

- RichEnhanceAnimation (풀 연출)
- 사운드/BGM 연동
- 햅틱 피드백
- 검 정식 아트 적용
- 공방 외형 변화
- 화면 전환 페이드 애니메이션
- FTUE 오버레이
- 연속 성공/파괴 특수 연출
