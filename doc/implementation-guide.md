# 검강화 게임 구현 지침서

## 기본 원칙

1. **모듈화**: 기능별 독립 모듈 (강화, 랭킹, 칭호, 업적, 광고, 컬렉션, 장인 숙련도, 파편 등). A를 고쳤는데 B에 영향이 가지 않는 구조.
2. **로직/뷰 분리**: 로직만으로 게임을 돌릴 수 있어야 함. 테스트는 로직만으로 수행.
3. **뷰 교체 가능**: Flutter View → Unity View 교체를 목표. 로직 레이어에 Flutter 의존성 0.

---

## 프로젝트 구조

```
sword-enhance-game/
├── packages/
│   └── game_core/                    ← 순수 Dart 패키지 (Flutter 의존성 없음)
│       ├── lib/
│       │   ├── game_core.dart        ★ barrel file — 외부 공개 API 정의 (아래 참조)
│       │   ├── models/               데이터 모델
│       │   │   ├── sword.dart
│       │   │   ├── game_state.dart   세션 상태 (현재 검, 레벨, 수정자 등)
│       │   │   ├── mastery.dart        장인 숙련도
│       │   │   ├── fragment.dart     파편
│       │   │   ├── collection.dart
│       │   │   ├── achievement.dart
│       │   │   ├── title.dart        칭호
│       │   │   ├── ranking.dart
│       │   │   └── player.dart       유저 데이터 통합 (PlayerData, Statistics, Inventory, AdLimits 등)
│       │   │
│       │   ├── logic/                게임 로직
│       │   │   ├── enhance_logic.dart      강화/파괴 핵심 루프
│       │   │   ├── economy_logic.dart      골드 유입/유출, 판매
│       │   │   ├── fragment_logic.dart     파편 획득/교환
│       │   │   ├── mastery_logic.dart        장인 숙련도/경험치
│       │   │   ├── collection_logic.dart   컬렉션 등록/조회
│       │   │   ├── achievement_logic.dart  업적 달성 판정
│       │   │   ├── title_logic.dart        칭호 부여/장착
│       │   │   ├── ranking_logic.dart      랭킹 산출
│       │   │   ├── ad_reward_logic.dart    광고 보상 처리 (보호권/골드/부스터)
│       │   │   └── game_session_logic.dart 세션 관리 (시작/리셋)
│       │   │
│       │   ├── repositories/         저장소 인터페이스 (추상)
│       │   │   ├── storage_repository.dart
│       │   │   └── ranking_repository.dart
│       │   │
│       │   ├── commands/              유저 액션 커맨드
│       │   │   ├── command.dart            커맨드 기본 인터페이스
│       │   │   ├── enhance_command.dart    강화 시도
│       │   │   ├── sell_command.dart       판매
│       │   │   ├── collect_command.dart    수집
│       │   │   ├── use_item_command.dart   파편 아이템 사용
│       │   │   ├── exchange_command.dart   파편 교환
│       │   │   ├── watch_ad_command.dart   광고 시청 (보호권/골드/부스터)
│       │   │   └── confirm_destroy_command.dart  광고 보호권 거부 시 파괴 확정
│       │   │
│       │   ├── events/               상태 변경 이벤트 (출력)
│       │   │   ├── game_event.dart         기본 이벤트 인터페이스
│       │   │   ├── enhance_event.dart      EnhanceSuccessEvent, EnhanceFailEvent
│       │   │   ├── economy_event.dart      SellEvent, GoldChangeEvent
│       │   │   ├── fragment_event.dart     FragmentGainEvent, ExchangeEvent, UseItemEvent
│       │   │   ├── mastery_event.dart        MasteryLevelUpEvent
│       │   │   ├── collection_event.dart   CollectEvent, CollectionCompleteEvent
│       │   │   └── ad_event.dart           AdRewardEvent
│       │   │
│       │   ├── engine/               게임 엔진
│       │   │   ├── game_engine.dart        커맨드 수신 → 로직 실행 → 이벤트 발행
│       │   │   └── session_recorder.dart   커맨드/시드 기록 (리플레이용)
│       │   │
│       │   ├── data/                 CSV/JSON 파싱, 밸런스 데이터 로딩
│       │   │   ├── sword_data_loader.dart
│       │   │   └── mastery_data_loader.dart   장인 숙련도 테이블 파서
│       │   │
│       │   └── util/                 유틸리티
│       │       ├── time_provider.dart      시간 추상화
│       │       └── random_provider.dart    난수 추상화 (시드 기반)
│       │
│       └── test/                     로직 단위 테스트
│           ├── enhance_logic_test.dart
│           ├── economy_logic_test.dart
│           ├── fragment_logic_test.dart
│           └── ...
│
├── app_flutter/                      ← Flutter 앱 (뷰 + 인프라)
│   ├── lib/
│   │   ├── views/                    화면 위젯
│   │   │   ├── title_view.dart
│   │   │   ├── enhance_view.dart     강화 메인 화면
│   │   │   ├── workshop_view.dart       공방 화면
│   │   │   ├── achievement_view.dart 업적/칭호 화면
│   │   │   └── widgets/              공용 위젯
│   │   │       ├── sword_display_widget.dart
│   │   │       └── gold_indicator_widget.dart
│   │   │
│   │   ├── animations/               연출 모듈 (교체 가능)
│   │   │   ├── enhance_animation_controller.dart   연출 인터페이스
│   │   │   ├── enhance_animation_config.dart       단계별 연출 설정 데이터
│   │   │   ├── enhance_animation_widget.dart       연출을 렌더링하는 Flutter 위젯 (컨트롤러를 사용)
│   │   │   ├── basic_enhance_animation.dart        기본 연출 구현체 (P1)
│   │   │   └── rich_enhance_animation.dart         고도화 연출 구현체 (P4)
│   │   │
│   │   ├── bindings/                 game_core ↔ Flutter 연결
│   │   │   └── game_binding.dart     상태관리 (Provider/Riverpod 등)
│   │   │
│   │   ├── repositories/            저장소 구현체
│   │   │   ├── hive_storage_repository.dart
│   │   │   └── firebase_repository.dart
│   │   │
│   │   ├── services/                인프라 서비스
│   │   │   ├── ad_service.dart       AdMob 연동
│   │   │   ├── push_service.dart     푸시 알림
│   │   │   └── auth_service.dart     Firebase 인증
│   │   │
│   │   └── main.dart
│   │
│   └── assets/
│       ├── data/
│       │   ├── swords.csv
│       │   └── mastery_levels.csv
│       └── images/
│
└── (미래) app_unity/                 ← Unity 뷰 (game_core를 C#로 포팅)
```

---

## 파일 명명 규칙

| 분류 | 접미사 | 예시 | 설명 |
|------|--------|------|------|
| 로직 | `_logic.dart` | `enhance_logic.dart` | 게임 규칙, 계산, 판정 |
| 커맨드 | `_command.dart` | `enhance_command.dart` | 유저 액션 정의 (입력) |
| 이벤트 | `_event.dart` | `game_event.dart` | 상태 변경 알림 (출력) |
| 엔진 | 접미사 없음 | `game_engine.dart` | 커맨드 수신 → 로직 실행 → 이벤트 발행 |
| 뷰 | `_view.dart` | `enhance_view.dart` | 화면 위젯, UI 렌더링 |
| 뷰 부품 | `_widget.dart` | `sword_display_widget.dart` | 재사용 가능한 UI 컴포넌트 |
| 모델 | 접미사 없음 | `sword.dart` | 데이터 구조 정의 |
| 리포지토리 | `_repository.dart` | `storage_repository.dart` | 저장소 인터페이스/구현 |
| 서비스 | `_service.dart` | `ad_service.dart` | 외부 API 통신, 인프라 |
| 유틸리티 | `_provider.dart` 등 | `time_provider.dart` | 추상화, 헬퍼 |
| 테스트 | `_test.dart` | `enhance_logic_test.dart` | 테스트 코드 |

---

## 로직/뷰 분리 규칙

### 설계 원칙

```
Logic → View:  조정 O (이벤트로 뷰를 움직임) / 보기 X (뷰의 존재를 모름)
View → Logic:  보기 O (상태를 읽을 수 있음)   / 조정 X (직접 조작 불가)
```

### 로직 (`game_core`)
- **Flutter import 금지** — `dart:core`, `dart:async`, `dart:math`, `dart:convert`만 허용
- 모든 외부 의존성은 인터페이스(추상 클래스)로 주입
- 상태 변경은 이벤트 스트림으로 외부에 알림
- View 레이어의 존재를 모름 — 이벤트를 발행할 뿐, 누가 구독하는지 관심 없음

### 뷰 (`app_flutter`)
- **상태 읽기만 가능** — `engine.state`, `engine.events` 스트림으로 관찰
- **조작은 Command로만** — `engine.dispatch(Command)` 이외의 경로로 로직을 변경할 수 없음
- 뷰에 게임 규칙 판정 코드 없음 (예: 확률 계산, 골드 차감 등)
- **Logic 클래스 직접 import 금지** — barrel file로 구조적 강제 (아래 참조)

### barrel file (`game_core.dart`) — 외부 공개 API 제어

`app_flutter`는 `import 'package:game_core/game_core.dart'`로만 접근. Logic 클래스는 export하지 않아 뷰에서 직접 호출 불가.

```dart
// packages/game_core/lib/game_core.dart

// ✅ 외부 공개 — 뷰가 사용하는 것들
export 'models/sword.dart';
export 'models/game_state.dart';
export 'models/player.dart';
export 'models/mastery.dart';
export 'models/fragment.dart';
export 'models/collection.dart';
export 'models/achievement.dart';
export 'models/title.dart';
export 'models/ranking.dart';

export 'commands/command.dart';
export 'commands/enhance_command.dart';
export 'commands/sell_command.dart';
export 'commands/collect_command.dart';
export 'commands/use_item_command.dart';
export 'commands/exchange_command.dart';
export 'commands/watch_ad_command.dart';
export 'commands/confirm_destroy_command.dart';

export 'events/game_event.dart';
export 'events/enhance_event.dart';
export 'events/economy_event.dart';
export 'events/fragment_event.dart';
export 'events/mastery_event.dart';
export 'events/collection_event.dart';
export 'events/ad_event.dart';

export 'engine/game_engine.dart';
export 'engine/session_recorder.dart';

export 'repositories/storage_repository.dart';
export 'repositories/ranking_repository.dart';

export 'data/sword_data_loader.dart';
export 'data/mastery_data_loader.dart';

export 'util/time_provider.dart';
export 'util/random_provider.dart';

// ❌ 비공개 — Logic 클래스는 export하지 않음
// logic/enhance_logic.dart      → Command 내부에서만 사용
// logic/economy_logic.dart      → Command 내부에서만 사용
// logic/fragment_logic.dart     → Command 내부에서만 사용
// logic/mastery_logic.dart      → Command 내부에서만 사용
// logic/collection_logic.dart   → Command 내부에서만 사용
// logic/achievement_logic.dart  → Command 내부에서만 사용
// logic/title_logic.dart        → Command 내부에서만 사용
// logic/ranking_logic.dart      → Command 내부에서만 사용
// logic/ad_reward_logic.dart    → Command 내부에서만 사용
// logic/game_session_logic.dart → Command 내부에서만 사용
```

**이 구조가 강제하는 것:**
- `app_flutter`에서 `import 'package:game_core/logic/enhance_logic.dart'` → **컴파일 에러** (Dart 패키지 export 규칙)
- 뷰는 Command 객체를 생성하여 `engine.dispatch()`에 넘기는 것만 가능
- Logic 클래스는 `game_core` 내부(Command, 테스트)에서만 접근 가능

**결과:** "뷰가 로직을 볼 수는 있지만 조정할 수 없다"가 **컨벤션이 아닌 컴파일 타임에 강제**됨.

### 통신 방향
```
뷰 → Command 생성 → GameEngine.dispatch() → Command가 Logic 조합 → 상태 갱신
로직 → GameEvent 스트림 → 뷰가 구독하여 UI 반영

예시:
  뷰: engine.dispatch(EnhanceCommand())
  엔진: EnhanceCommand.execute() 내부에서 Logic 순차 호출 → 이벤트 발행
  뷰: 이벤트 구독하여 성공 이펙트 or 파괴 애니메이션 재생
```

---

## 커맨드 시스템

### 개념

유저 입력을 **커맨드 객체**로 추상화. 로직은 커맨드만 받아서 실행. 뷰는 커맨드를 생성해서 엔진에 넘길 뿐.

```
같은 초기 상태 + 같은 커맨드 시퀀스 + 같은 랜덤 시드 = 항상 같은 결과
```

### 흐름

```
[뷰] → Command 생성 → [GameEngine] → 해당 Logic 실행 → [Event] → 뷰가 구독
```

### 커맨드 종류

| 커맨드 | 설명 | 파라미터 |
|--------|------|----------|
| `EnhanceCommand` | 강화 시도 | - |
| `SellCommand` | 현재 검 판매 | - |
| `CollectCommand` | 현재 검 수집 | - |
| `UseItemCommand` | 파편 아이템 사용 (사전) | itemType (부적/주문서) |
| `ExchangeCommand` | 파편 교환 | itemType, quantity |
| `WatchAdCommand` | 광고 시청 보상 | adType (보호권/골드/부스터) |
| `ConfirmDestroyCommand` | 광고 보호권 거부 시 파괴 확정 | - |

### 커맨드 인터페이스

```dart
abstract class Command {
  final int timestamp;  // 리플레이용 실행 시점 (클라이언트 밀리초, monotonic)
  CommandResult execute(GameState state, GameContext context);
}

/// 커맨드 실행 결과 — 새로운 상태 + 발생한 이벤트 목록
class CommandResult {
  final GameState newState;       // 커맨드 실행 후 갱신된 상태
  final List<GameEvent> events;   // 이번 커맨드로 발생한 이벤트들 (뷰/리스너에 전달)
}

/// 커맨드 실행에 필요한 외부 의존성 묶음
class GameContext {
  final RandomProvider random;
  final TimeProvider time;
  final SwordDataTable swordTable;   // CSV에서 로딩된 검 데이터 테이블
  final MasteryLevelTable masteryTable;  // CSV에서 로딩된 장인 숙련도 테이블
}
```

- 커맨드는 `GameState`(현재 상태) + `GameContext`(난수/시간)를 받아 새로운 상태 + 이벤트를 반환
- `GameContext`를 통해 RandomProvider에 접근 → 결정론적 실행 보장
- timestamp는 리플레이 재생 시 커맨드 간 시간 간격 재현에 사용

### 커맨드 유효성 검증

커맨드는 실행 전에 유효성을 검증한다. 유효하지 않은 커맨드는 상태 변경 없이 에러 이벤트를 반환.

```dart
abstract class Command {
  final int timestamp;

  /// 유효성 검증 — 실행 가능 여부를 사전 판단
  /// null이면 유효, 문자열이면 거부 사유
  String? validate(GameState state, GameContext context);

  /// 실행 — validate() 통과 후에만 호출됨
  CommandResult execute(GameState state, GameContext context);
}

/// 유효하지 않은 커맨드 시도 시 발행되는 이벤트
class CommandRejectedEvent extends GameEvent {
  final String commandType;  // 거부된 커맨드 타입명
  final String reason;       // 거부 사유
}
```

**유효성 검증 규칙:**

| 커맨드 | 거부 조건 | reason |
|--------|----------|--------|
| `EnhanceCommand` | 골드 부족 | `'insufficient_gold'` |
| `EnhanceCommand` | `pendingAdProtection=true` | `'pending_ad_protection'` |
| `SellCommand` | currentLevel == 0 (나무검) | `'cannot_sell_wooden_sword'` |
| `SellCommand` | `pendingAdProtection=true` | `'pending_ad_protection'` |
| `CollectCommand` | currentLevel < 10 | `'level_too_low'` |
| `CollectCommand` | `pendingAdProtection=true` | `'pending_ad_protection'` |
| `UseItemCommand` | 해당 아이템 보유량 0 | `'no_item'` |
| `ExchangeCommand` | 파편 부족 | `'insufficient_fragments'` |
| `WatchAdCommand(protection)` | `pendingAdProtection=false` | `'no_pending_protection'` |
| `ConfirmDestroyCommand` | `pendingAdProtection=false` | `'no_pending_destruction'` |

- `pendingAdProtection=true` 상태에서는 `WatchAdCommand`와 `ConfirmDestroyCommand`만 허용. 다른 커맨드는 모두 reject.
- 뷰에서도 버튼 비활성화로 1차 방어하지만, **Logic 레이어에서 2차 방어**하여 안전성 보장.

### GameEngine

```dart
class GameEngine {
  GameState _state;
  final GameContext _context;
  final SessionRecorder? _recorder;  // P3에서 구현, P0~P1에서는 null

  // 외부 리스너 등록 가능 (동기화, 업적 체크 등)
  final StreamController<GameEvent> _eventController =
      StreamController<GameEvent>.broadcast();
  Stream<GameEvent> get events => _eventController.stream;

  GameState get state => _state;

  void dispatch(Command command) {
    // 1. 유효성 검증 — 실행 불가 시 에러 이벤트만 발행하고 종료
    final rejection = command.validate(_state, _context);
    if (rejection != null) {
      _eventController.add(CommandRejectedEvent(
        commandType: command.runtimeType.toString(),
        reason: rejection,
      ));
      return;
    }

    // 2. 커맨드 기록 (리플레이용, P3에서 활성화)
    //    validate 통과한 커맨드만 기록 — 리플레이 시 rejected 커맨드는 재생 불필요
    _recorder?.record(command);

    // 3. 로직 실행 — GameContext를 통해 난수/시간 접근
    final result = command.execute(_state, _context);

    // 4. 상태 갱신
    _state = result.newState;

    // 5. 이벤트 발행 (broadcast — 뷰, 동기화, 업적 등 다중 구독 가능)
    for (final event in result.events) {
      _eventController.add(event);
    }
  }
}
```

- `broadcast` 스트림으로 다중 리스너 지원 → P2에서 동기화/업적 리스너 추가 가능
- `SessionRecorder`는 P0~P1에서 null, P3에서 구현 후 주입
- **validate 통과한 커맨드만 기록** → 리플레이 데이터에 불필요한 reject 커맨드가 쌓이지 않음

### EnhanceCommand 내부 로직 조합

Command는 여러 Logic의 순수 함수를 순차 호출하여 결과를 조합한다. 각 Logic은 서로를 모르고, Command만이 조합 순서를 안다.

```dart
class EnhanceCommand extends Command {
  // Logic 인스턴스는 GameContext 또는 생성자에서 주입
  // (Logic이 순수 함수이므로 싱글턴으로 재사용 가능)

  @override
  String? validate(GameState state, GameContext context) {
    if (state.pendingAdProtection) return 'pending_ad_protection';
    final cost = _getEnhanceCost(state, context);
    if (!EconomyLogic().canAfford(state, cost)) return 'insufficient_gold';
    return null;
  }

  @override
  CommandResult execute(GameState state, GameContext context) {
    final events = <GameEvent>[];
    var s = state;

    // 1. 강화 비용 산출 (장인 숙련도 할인 적용)
    final baseCost = context.swordTable.getSword(s.currentLevel + 1)!.enhanceCost;
    final discount = context.masteryTable.getLevel(s.playerData.mastery.level).costDiscount;
    final cost = (baseCost * (1 - discount)).floor();

    // 2. 골드 차감
    final spendResult = EconomyLogic().spendGold(s, cost);
    s = spendResult.newState;
    events.addAll(spendResult.events);

    // 3. 확률 판정
    final enhanceLogic = EnhanceLogic();
    final targetSword = context.swordTable.getSword(s.currentLevel + 1)!;
    final rate = enhanceLogic.getEffectiveRate(s, targetSword);
    final success = enhanceLogic.roll(rate, context.random);

    // 4. 성공/실패 분기
    if (success) {
      // 4a. 성공 — 레벨업
      final successResult = enhanceLogic.handleSuccess(s, targetSword, context.swordTable);
      s = successResult.newState;
      events.addAll(successResult.events);

      // 4b. 통계 갱신 — 연속 성공, 최고 레벨 등
      s = _updateStatsOnSuccess(s);
    } else {
      // 4c. 실패 — 파괴 또는 보호
      final failResult = enhanceLogic.handleFail(s, context.swordTable, context);
      s = failResult.newState;
      events.addAll(failResult.events);

      // 4d. 실제 파괴인 경우 (보호 부적이 없었고, 광고 보호권도 미사용 대기가 아닌 경우)
      //     → 파편 지급 + 나무검 리셋은 ConfirmDestroyCommand에서 처리
      //     → pendingAdProtection=true면 여기서 멈춤 (유저 선택 대기)
      //     → pendingAdProtection=false면 (광고 보호권 자격 없음) 즉시 파괴 확정
      if (!s.pendingAdProtection && !s.hasActiveProtection) {
        final destroyResult = _finalizeDestroy(s, context);
        s = destroyResult.newState;
        events.addAll(destroyResult.events);
      }

      // 4e. 통계 갱신 — 연속 파괴, 총 파괴 횟수 등
      s = _updateStatsOnFail(s);
    }

    // 5. 장인 숙련도 경험치 +1 (성공/실패 무관)  [P2에서 활성화]
    // final masteryResult = MasteryLogic().addExp(s, context.masteryTable);
    // s = masteryResult.newState;
    // events.addAll(masteryResult.events);

    // 6. 업적 체크  [P3에서 활성화]
    // final achievementResult = AchievementLogic().check(s);
    // s = achievementResult.newState;
    // events.addAll(achievementResult.events);

    return CommandResult(newState: s, events: events);
  }

  /// 파괴 확정 처리 (광고 보호권 자격 없는 경우 즉시 실행)
  LogicResult _finalizeDestroy(GameState state, GameContext context) {
    final events = <GameEvent>[];
    var s = state;

    // 파편 지급  [P2에서 활성화]
    // final fragmentBonus = context.masteryTable.getLevel(s.playerData.mastery.level).fragmentBonus;
    // final fragResult = FragmentLogic().giveFragments(s, s.currentLevel, fragmentBonus);
    // s = fragResult.newState;
    // events.addAll(fragResult.events);

    // 나무검 리셋
    s = GameSessionLogic().resetToWoodenSword(s, context.swordTable);

    return LogicResult(s, events);
  }
}
```

**조합 순서 요약:**
1. 비용 산출 (할인 적용) → 2. 골드 차감 → 3. 확률 판정 → 4. 성공/실패 처리 → 5. 경험치 (P2) → 6. 업적 (P3)

**P2/P3 확장 방법:** 주석 처리된 블록의 주석만 해제하면 됨. Logic은 이미 순수 함수로 존재하므로 조합 순서에 끼워넣기만 하면 된다.

### 결정론적 실행

로직의 모든 비결정적 요소를 주입으로 제어:

| 비결정적 요소 | 추상화 | 리플레이 시 |
|--------------|--------|------------|
| 강화 성공/실패 판정 | `RandomProvider` (시드 기반) | 같은 시드 재사용 |
| 현재 시각 (일일 제한, 주간 리셋) | `TimeProvider` | `startTime` + `command.timestamp`로 재현 |

### 리플레이 데이터

```dart
class SessionRecord {
  final int randomSeed;           // 세션 시작 시 랜덤 시드
  final DateTime startTime;       // 세션 시작 시각 (TimeProvider 재현용)
  final GameState initialState;   // 세션 시작 시 상태 스냅샷
  final List<Command> commands;   // 커맨드 시퀀스 (타임스탬프 포함)
}

/// 리플레이 전용 TimeProvider — startTime + command.timestamp로 시각 재현
class ReplayTimeProvider implements TimeProvider {
  final DateTime _startTime;
  int _currentTimestamp = 0;

  ReplayTimeProvider(this._startTime);

  /// SessionRecorder가 각 커맨드 dispatch 전에 호출하여 시각 동기화
  void syncTo(int commandTimestamp) {
    _currentTimestamp = commandTimestamp;
  }

  @override
  DateTime now() => _startTime.add(Duration(milliseconds: _currentTimestamp));
}
```

리플레이 재생:
1. `initialState`로 GameEngine 초기화
2. `randomSeed`로 RandomProvider 초기화
3. `startTime`으로 `ReplayTimeProvider` 초기화
4. `commands`를 순서대로 순회:
   - `replayTimeProvider.syncTo(command.timestamp)` → 시각 동기화
   - `engine.dispatch(command)` → 동일한 결과 재현

**시간 재현이 필요한 이유:** 광고 보호권 일일 제한(`adProtectionUsedToday`)과 주간 랭킹 리셋은 `TimeProvider.now()`에 의존. `startTime` 없이 리플레이하면 일일 제한이 현재 날짜 기준으로 판정되어 원본과 다른 결과가 나올 수 있음.

### 활용처

| 용도 | 설명 |
|------|------|
| **리플레이** | 다른 유저의 +20 달성 과정을 재생 |
| **디버깅** | 버그 재현 — 유저의 세션 기록을 받아서 그대로 재생 |
| **밸런스 시뮬레이션** | 커맨드를 자동 생성하여 수천 세션 시뮬레이션 → 기대 수치 검증 |
| **치트 검증** | 서버에서 커맨드 시퀀스 재실행하여 결과 비교 → 조작 탐지 |
| **테스트** | FakeRandom + 커맨드 조합으로 모든 시나리오 결정적 테스트 |

---

## 추상화 규칙

### 저장소 (Repository 패턴 + 저장 모드)

```dart
// game_core (인터페이스만)
abstract class StorageRepository {
  Future<PlayerData> load();
  Future<void> save(PlayerData data);
}
```

**구현체 3종:**

| 구현체 | 용도 | 설명 |
|--------|------|------|
| `InMemoryRepository` | 테스트 | 메모리에만 저장, 앱 종료 시 소멸 |
| `HiveStorageRepository` | 로컬 모드 | Hive로 로컬 저장, 서버 통신 없음 |
| `SyncedRepository` | 서버 모드 | Hive(로컬) + Firebase(서버) 동기화 |

**저장 모드 전환:**

```dart
enum StorageMode { test, local, server }

// main.dart 또는 환경 설정에서 모드 선택
StorageRepository createRepository(StorageMode mode) {
  switch (mode) {
    case StorageMode.test:   return InMemoryRepository();
    case StorageMode.local:  return HiveStorageRepository();
    case StorageMode.server: return SyncedRepository(
      local: HiveStorageRepository(),
      remote: FirebaseRepository(),
    );
  }
}
```

- **개발/테스트**: `local` 또는 `test` 모드 → Firebase 없이 동작
- **배포**: `server` 모드 → 로컬 저장 + Firebase 동기화
- 로직(`game_core`)은 `StorageRepository` 인터페이스만 알면 됨 — 모드에 무관
- 모드 전환은 **DI(의존성 주입) 시점에 구현체만 교체** — 코드 수정 없음

**SyncedRepository 동작:**
```
save() → Hive에 즉시 저장 → Firebase에 비동기 업로드
load() → 앱 시작 시 Firebase에서 최신 데이터 pull → Hive에 반영 → Hive에서 읽기
```
- 평상시에는 Hive에서 읽기 (빠름)
- 동기화 실패 시 로컬 데이터 유지, 복귀 시 재동기화

### 시간 추상화
```dart
// game_core
abstract class TimeProvider {
  DateTime now();
}

// app_flutter
class RealTimeProvider implements TimeProvider {
  DateTime now() => DateTime.now();
}

// 테스트 — 일일 제한(보호권 2회) 등 시간 의존 로직 테스트 가능
class FakeTimeProvider implements TimeProvider {
  DateTime _now;
  DateTime now() => _now;
  void advance(Duration d) => _now = _now.add(d);
}
```

### 난수 추상화
```dart
// game_core
abstract class RandomProvider {
  double nextDouble(); // 0.0 ~ 1.0
}

// 테스트 — 강화 성공/실패를 결정적으로 테스트 가능
class FakeRandomProvider implements RandomProvider {
  double _value;
  double nextDouble() => _value;
  void setNext(double v) => _value = v;
}
```

---

## 모듈 의존 관계

```
[각 Logic 모듈] → 모두 독립 (다른 Logic을 직접 호출하지 않음)
                  입력: GameState + 파라미터 → 출력: LogicResult(newState, events)

[enhance_logic]      → (독립) 확률 판정, 성공/실패 처리
[economy_logic]      → (독립) 골드 차감/지급
[fragment_logic]     → (독립) 파편 획득/교환
[mastery_logic]      → (독립) 경험치/레벨업
[collection_logic]   → (독립) 수집 등록/완성도
[achievement_logic]  → (독립) 달성 조건 판정 (GameState 읽기만)
[ranking_logic]      → (독립) 랭킹 산출 (Statistics 읽기만)
[ad_reward_logic]    → (독립) 광고 보상 처리, 일일 제한 체크
[game_session_logic] → (독립) 초기 상태 생성, 나무검 리셋

[Command가 조합을 담당]
  EnhanceCommand  → economy + enhance + (P2: fragment, mastery) + (P3: achievement)
  SellCommand     → economy + game_session
  CollectCommand  → collection + game_session
  ExchangeCommand → fragment
  WatchAdCommand  → ad_reward + (economy for 골드 보상)
  ...
```

- `enhance_logic`이 허브 역할이지만, 각 모듈에 직접 의존하지 않음
- **모든 Logic은 순수 함수** — `(GameState, GameContext) → (GameState, List<GameEvent>)` 형태
- 모듈 간 조합은 **Command 내부**에서 각 Logic을 순차 호출하여 처리 (콜백/사이드이펙트 없음)

### Logic 순수 함수 규칙

모든 Logic 모듈은 아래 규칙을 따른다:

1. **입력**: `GameState` + 필요한 파라미터 (+ `GameContext` for 난수/시간)
2. **출력**: `LogicResult(newState, events)` — 새로운 상태 + 발생한 이벤트
3. **사이드이펙트 없음**: 콜백, 스트림, 외부 상태 변경 금지
4. **다른 Logic 호출 금지**: Logic끼리 직접 호출하지 않음. 조합은 Command의 책임

```dart
/// 모든 Logic의 반환 타입
class LogicResult {
  final GameState newState;
  final List<GameEvent> events;
  LogicResult(this.newState, [this.events = const []]);
}
```

### Logic 순수 함수 예시

```dart
/// enhance_logic.dart — 순수 함수, 사이드이펙트 없음
class EnhanceLogic {
  /// 확률 판정 (수정자 적용 포함)
  double getEffectiveRate(GameState state, Sword targetSword) {
    double rate = targetSword.successRate;
    for (final modifier in state.activeModifiers) {
      rate = modifier.apply(rate);
    }
    return rate.clamp(0.0, 1.0);
  }

  /// 강화 판정 — 성공/실패 여부만 반환 (상태 변경은 Command에서 처리)
  bool roll(double effectiveRate, RandomProvider random) {
    return random.nextDouble() < effectiveRate;
  }

  /// 성공 시 레벨업 — 새 상태 + 이벤트 반환
  LogicResult handleSuccess(GameState state, Sword newSword, SwordDataTable table) {
    final newState = state.copyWith(
      currentSword: newSword,
      currentLevel: state.currentLevel + 1,
      activeModifiers: [],  // 수정자 소모
    );
    return LogicResult(newState, [
      EnhanceSuccessEvent(
        prevLevel: state.currentLevel,
        newLevel: newState.currentLevel,
        newSwordName: newSword.name,
        goldSpent: table.getSword(newState.currentLevel)?.enhanceCost ?? 0,
      ),
    ]);
  }

  /// 실패 시 파괴/보호 분기 — 상태 + 이벤트 반환
  LogicResult handleFail(GameState state, SwordDataTable table, GameContext context) {
    if (state.hasActiveProtection) {
      // 보호의 부적이 막음 — 파괴 안 됨, 부적 소모
      final newState = state.copyWith(
        hasActiveProtection: false,
        activeModifiers: [],
      );
      return LogicResult(newState, [
        EnhanceFailEvent(
          destroyedLevel: state.currentLevel,
          destroyedSwordName: state.currentSword.name,
          fragmentsGained: 0,
          goldSpent: 0,
          adProtectionAvailable: false,
          destroyed: false,  // 파괴 안 됨
        ),
      ]);
    }
    // 실제 파괴 — pendingAdProtection 상태로 전환 (광고 보호권 대기)
    final adAvailable = _isAdProtectionAvailable(state, context);
    final newState = state.copyWith(
      pendingAdProtection: adAvailable,
      activeModifiers: [],
    );
    return LogicResult(newState, [
      EnhanceFailEvent(
        destroyedLevel: state.currentLevel,
        destroyedSwordName: state.currentSword.name,
        fragmentsGained: state.currentSword.fragmentReward,
        goldSpent: 0,
        adProtectionAvailable: adAvailable,
        destroyed: true,
      ),
    ]);
  }

  bool _isAdProtectionAvailable(GameState state, GameContext context) {
    return state.playerData.adLimits.adProtectionUsedToday < 2;
  }
}

/// economy_logic.dart — 순수 함수
class EconomyLogic {
  LogicResult spendGold(GameState state, int cost) {
    final newGold = state.playerData.gold - cost;
    final newState = state.copyWith(
      playerData: state.playerData.copyWith(gold: newGold),
    );
    return LogicResult(newState, [
      GoldChangeEvent(amount: -cost, newTotal: newGold, reason: 'enhance'),
    ]);
  }

  bool canAfford(GameState state, int cost) => state.playerData.gold >= cost;

  LogicResult addGold(GameState state, int amount, String reason) {
    final newGold = state.playerData.gold + amount;
    final newState = state.copyWith(
      playerData: state.playerData.copyWith(gold: newGold),
    );
    return LogicResult(newState, [
      GoldChangeEvent(amount: amount, newTotal: newGold, reason: reason),
    ]);
  }
}

/// fragment_logic.dart — 순수 함수
class FragmentLogic {
  LogicResult giveFragments(GameState state, int destroyedLevel, int fragmentBonus) {
    final baseFragments = state.currentSword.fragmentReward;
    final total = baseFragments + fragmentBonus;
    final newFragments = state.playerData.fragments + total;
    final newState = state.copyWith(
      playerData: state.playerData.copyWith(fragments: newFragments),
    );
    return LogicResult(newState, [
      FragmentGainEvent(amount: total, totalFragments: newFragments),
    ]);
  }
}

/// mastery_logic.dart — 순수 함수
class MasteryLogic {
  LogicResult addExp(GameState state, MasteryLevelTable table) {
    final newAttempts = state.playerData.mastery.totalAttempts + 1;
    final currentLevel = state.playerData.mastery.level;
    final newLevel = table.getLevelForExp(newAttempts);
    final newState = state.copyWith(
      playerData: state.playerData.copyWith(
        mastery: state.playerData.mastery.copyWith(
          totalAttempts: newAttempts,
          level: newLevel,
        ),
      ),
    );
    final events = <GameEvent>[];
    if (newLevel > currentLevel) {
      events.add(MasteryLevelUpEvent(newLevel: newLevel, reward: table.getReward(newLevel)));
    }
    return LogicResult(newState, events);
  }
}
```

**핵심 원칙: Logic은 "계산만", Command는 "조합"을 담당한다.**
- Logic은 자기 영역의 상태 변환만 수행하고 `LogicResult`를 반환
- Command는 여러 Logic의 결과를 순차 조합하여 최종 `CommandResult`를 구성
- 이 구조 덕분에 Logic 단위 테스트와 Command 통합 테스트를 명확히 분리 가능

---

## 테스트 전략

### 로직 테스트 (game_core)
- 모든 로직 모듈에 대해 단위 테스트 작성
- `InMemoryRepository`, `FakeTimeProvider`, `FakeRandomProvider` 사용
- CSV 데이터를 테스트용 문자열로 주입하여 밸런스 독립 테스트 가능

### 테스트 시나리오 예시
- 강화 성공/실패 → 골드 차감, 파편 지급, 경험치 증가 동시 검증
- 보호권 사용 시 파괴 대신 단계 유지
- 일일 보호권 제한 (2회) → 3회째 거부
- 골드 부족 시 강화 불가
- 판매가 = CSV 고정값과 일치
- 장인 숙련도 레벨업 시 할인율 적용

---

## 밸런스 데이터 관리

- 모든 수치(검 이름, 확률, 비용, 판매가, 파편)는 `swords.csv`에서 로딩
- 장인 숙련도 테이블은 별도 CSV 또는 JSON으로 외부화 (`mastery_levels.csv`)
- 로직은 하드코딩 수치 없이 데이터 테이블 참조만
- 밸런스 조정 = CSV 수정만으로 완료 (코드 변경 불필요)

### swords.csv 컬럼 → Sword 모델 매핑

| CSV 컬럼 | Sword 필드 | 타입 |
|----------|-----------|------|
| 강화 | level | int (+0~+20) |
| 검 이름 | name | String |
| 테마 | theme | String |
| 성공률(%) | successRate | double (0.0~1.0) |
| 강화비용 | enhanceCost | int |
| 누적투자 | totalInvestment | int |
| 판매가 | sellPrice | int |
| 회수율 | returnRate | double |
| 파괴시파편 | fragmentReward | int |
| 수집가능 | collectible | bool (Y→true, N→false) |

### mastery_levels.csv 컬럼 → MasteryLevel 모델 매핑

| CSV 컬럼 | MasteryLevel 필드 | 타입 |
|----------|----------------|------|
| 레벨 | level | int |
| 필요경험치 | requiredExp | int (누적 강화 횟수) |
| 비용할인율 | costDiscount | double (0.0~0.25) |
| 파편보너스 | fragmentBonus | int (0 또는 1) |
| 외형ID | visualId | String (공방 외형 식별자) |
| 보상설명 | rewardDescription | String |

---

## 게임 규칙 상세 (구현 시 참조)

### 확률 표시 규칙
- +1~+14: `enhance_view`에서 성공률을 숫자로 표시 (예: "35%")
- +15 이상: `enhance_view`에서 "???"로 표시
- 이 규칙은 **뷰 레이어에서만 처리** — 로직은 항상 실제 확률로 판정
- 확률 부스터 사용 시에도 "??? + 부스터 적용 중" 으로 표시 (실제 수치 비공개)

### GameEngine 초기화 흐름

```
앱 시작 → StorageRepository.load()로 PlayerData 로딩
       → game_session_logic.createInitialState(playerData, swordTable)
       → GameState 생성 (currentSword=나무검, currentLevel=0, playerData)
       → GameEngine(initialState, context) 생성
       → 뷰 렌더링 시작
```

```dart
// game_session_logic.dart
class GameSessionLogic {
  /// 앱 시작 시 초기 GameState 생성
  GameState createInitialState(PlayerData playerData, SwordDataTable swordTable) {
    final woodenSword = swordTable.getSword(0); // +0 나무검
    final data = playerData.isFirstRun
        ? playerData.copyWith(gold: 200, isFirstRun: false)
        : playerData;
    return GameState(
      currentSword: woodenSword,
      currentLevel: 0,
      playerData: data,
      activeModifiers: [],
      hasActiveProtection: false,
      pendingAdProtection: false,
    );
  }

  /// 파괴/판매/수집 후 나무검으로 리셋 (PlayerData는 유지)
  GameState resetToWoodenSword(GameState state, SwordDataTable swordTable) {
    return state.copyWith(
      currentSword: swordTable.getSword(0),
      currentLevel: 0,
      activeModifiers: [],
      hasActiveProtection: false,
      pendingAdProtection: false,
    );
  }
}
```

- `GameEngine`은 외부에서 생성된 `GameState`를 받아서 시작 — 세션 초기화 로직은 엔진 바깥
- `game_session_logic`은 순수 함수 — GameState를 받아서 새 GameState를 반환
- 커맨드 내부에서도 `resetToWoodenSword`를 호출하여 리셋 처리

### 세션 초기화 규칙
- `game_session_logic`에서 처리
- 앱 시작 시: 나무검(+0) 자동 지급, 초기 골드 200 (첫 실행 시에만)
- 파괴 시: 나무검(+0) 자동 지급 (골드 유지)
- 판매 시: 나무검(+0) 자동 지급 + 판매 골드 지급
- 수집 시: 나무검(+0) 자동 지급 + 컬렉션 등록 (골드 없음)

### 동기화 타이밍 상세
- `SyncedRepository`에서 처리
- **서버 → 로컬 (pull)**: 앱 시작 시 1회. Firebase에서 최신 데이터 가져와 로컬 덮어쓰기.
- **로컬 → 서버 (push)**: 다음 이벤트 발생 시 자동 트리거
  - `SellEvent` → push
  - `CollectEvent` → push
  - `EnhanceFailEvent` (파괴) → push
  - 앱 백그라운드 전환 → push
- 강화 성공 시에는 push 안 함 (연타 속도 유지)

### 광고 보호권 사후 적용 커맨드 흐름

광고 보호권은 **파괴 후 사후 적용**. 보호의 부적(사전)과 흐름이 다르므로 주의.

```
[보호의 부적 — 사전 적용]
UseItemCommand(부적) → 부적 소모, hasActiveProtection=true
→ EnhanceCommand → 실패해도 파괴 안 됨, hasActiveProtection=false로 복귀

[광고 보호권 — 사후 적용]
EnhanceCommand → 실패 → EnhanceFailEvent 발행
  ├── hasActiveProtection=true인 경우: 파괴 안 됨 (부적이 막음). 여기서 끝
  └── hasActiveProtection=false인 경우: 파괴 발생
       → EnhanceFailEvent.adProtectionAvailable = (일일 잔여 횟수 > 0)
       → 뷰에서 adProtectionAvailable 체크 → true면 "광고 보고 복구?" 팝업 표시
       → 유저가 광고 시청 선택 → WatchAdCommand(adType=protection) dispatch
       → 파괴 취소: GameState를 파괴 직전 상태로 복구 (레벨/검 유지)
       → AdRewardEvent(보호권) 발행
       → 유저가 거부 → 파괴 확정, 나무검 리셋 진행
```

**구현 핵심:**
- `EnhanceCommand` 실행 시 파괴가 발생하면, 파괴 직전 상태를 `GameState.pendingAdProtection`과 함께 보존
- `WatchAdCommand(protection)` 수신 시 보존된 상태로 복구
- 팝업에서 거부 시 (또는 타임아웃) 별도 `ConfirmDestroyCommand`로 파괴 확정 + 나무검 리셋
- 일일 사용 횟수는 `AdLimits.adProtectionUsedToday`로 관리

### 랭킹 연출 이벤트
- `ranking_logic`에서 기록 갱신 감지 시 이벤트 발행
- 고강화 달성(+15 이상): `HighEnhanceAlertEvent` → 뷰에서 전체 팝업
- 주간 1등 갱신: `WeeklyTopEvent` → 뷰에서 공지 배너
- 내 기록 갱신: `PersonalRecordEvent` → `push_service`에서 푸시 알림

### 주간 랭킹 리셋
- 기준 시간대: KST (UTC+9), 매주 월요일 00:00
- 구현 주체: 클라이언트 (`TimeProvider.now()`로 KST 월요일 판정)
- 리셋 범위: 주간 랭킹 점수만 리셋 (전체 랭킹, 통계, 골드 등은 유지)

### 닉네임
- MVP: Firebase 익명 로그인 UID 기반 자동 생성 (예: "대장장이#A3F2")
- 이후: 닉네임 변경 기능 추가 가능
- 랭킹에 닉네임 + 칭호 표시

---

## 연출 모듈화 (Animation System)

강화 게임에서 연출은 게임성의 핵심. P1에서 심플하게, P4에서 고도화하되, **구현체 교체만으로 연출 전체가 바뀌는 구조**로 설계.

### 아키텍처

```
[GameEvent] → [enhance_view] → [EnhanceAnimationController] → 화면 연출
                                         ↑
                                 구현체 교체만으로 연출 변경
                                 ├── BasicEnhanceAnimation (P1)
                                 └── RichEnhanceAnimation (P4)
```

### 연출 인터페이스

```dart
/// 강화 연출 컨트롤러 — 구현체를 교체하면 연출 전체가 바뀜
abstract class EnhanceAnimationController {
  /// 강화 시도 시작 연출 (망치 두드리기, 서스펜스)
  /// [level]: 현재 강화 단계, [config]: 해당 단계의 연출 설정
  Future<void> playEnhanceAttempt(int level, EnhanceAnimationConfig config);

  /// 강화 성공 연출
  Future<void> playSuccess(int prevLevel, int newLevel, EnhanceAnimationConfig config);

  /// 강화 실패(파괴) 연출
  Future<void> playDestroy(int destroyedLevel, EnhanceAnimationConfig config);

  /// 판매 연출
  Future<void> playSell(int level, int goldGained);

  /// 수집 연출
  Future<void> playCollect(int level, String swordName);

  /// 연출 스킵 (장인 숙련도 Lv.3 이상, 저단계 스킵 옵션)
  bool canSkip(int level);
}
```

### 단계별 연출 설정 데이터

```dart
/// 단계 구간별 연출 파라미터 — 데이터로 관리하여 코드 수정 없이 조절 가능
class EnhanceAnimationConfig {
  final int levelFrom;          // 적용 시작 단계
  final int levelTo;            // 적용 끝 단계
  final Duration suspenseDuration;  // 판정 전 뜸 들이기 시간
  final double screenShakeIntensity; // 화면 흔들림 강도 (0.0 = 없음)
  final String effectColorHex;  // 이펙트 색상
  final double effectScale;     // 이펙트 크기 배율
  final bool enableHaptic;      // 진동 사용 여부
  final String? successSoundId; // 성공 사운드 에셋 ID
  final String? destroySoundId; // 파괴 사운드 에셋 ID
}
```

**기본 프리셋 (코드 또는 JSON으로 관리):**

| 구간 | 서스펜스 | 흔들림 | 이펙트 색상 | 이펙트 크기 | 진동 |
|------|---------|--------|-----------|-----------|------|
| +1~+5 | 0.5초 | 없음 | 회색 | 1.0x | OFF |
| +6~+9 | 1.0초 | 약 | 파랑 | 1.5x | ON |
| +10~+14 | 1.5초 | 중 | 보라 | 2.0x | ON |
| +15~+17 | 2.5초 | 강 | 금색 | 3.0x | ON |
| +18~+20 | 3.0초 | 최강 | 무지개 | 4.0x | ON |

### P1 기본 구현체 (BasicEnhanceAnimation)

- 서스펜스: 단순 딜레이 + 화면 살짝 어둡게
- 성공: 초록 텍스트 + 간단한 스케일 애니메이션
- 파괴: 빨간 텍스트 + 페이드아웃
- 사운드/진동: 없음 (P4에서 추가)
- **핵심: 인터페이스만 지키면 됨. 내부 구현은 최소한으로.**

### P4 고도화 구현체 (RichEnhanceAnimation)

- 서스펜스: 화면 어두워짐 → 불꽃 파티클 → 빛 집중 → 결과
- 성공: 단계별 차등 폭발 이펙트 + 팡파레 사운드 + 화면 흔들림
- 파괴: 검 갈라짐 파티클 + 유리 깨지는 사운드 + 강한 진동
- 연속 성공/파괴 시 추가 연출 레이어 (불꽃 콤보, 어두운 화면 등)

### 연출과 뷰의 연결

```dart
// enhance_view.dart — 이벤트 구독 시 연출 컨트롤러에 위임
ref.listen(gameEventProvider, (prev, next) {
  next.whenData((event) async {
    if (event is EnhanceSuccessEvent) {
      final config = animationConfigs.forLevel(event.newLevel);
      await animationController.playSuccess(event.prevLevel, event.newLevel, config);
      // 연출 완료 후 UI 상태 갱신
    } else if (event is EnhanceFailEvent) {
      final config = animationConfigs.forLevel(event.destroyedLevel);
      await animationController.playDestroy(event.destroyedLevel, config);
    }
  });
});
```

### Riverpod 연동

```dart
// 연출 구현체를 Provider로 등록 — DI 시점에 교체 가능
final enhanceAnimationProvider = Provider<EnhanceAnimationController>((ref) {
  // P1: BasicEnhanceAnimation, P4: RichEnhanceAnimation
  return BasicEnhanceAnimation();
});

final animationConfigProvider = Provider<EnhanceAnimationConfigTable>((ref) {
  return EnhanceAnimationConfigTable.fromDefaults(); // 또는 JSON에서 로딩
});
```

- `BasicEnhanceAnimation` → `RichEnhanceAnimation` 교체 시 **Provider 한 줄만 변경**
- 연출 설정(서스펜스 시간, 색상 등)은 `EnhanceAnimationConfig`로 데이터화 — 코드 수정 없이 튜닝 가능

---

## 상태관리 (Flutter)

- **라이브러리**: Riverpod 사용
- `GameEngine`을 Provider로 등록, 뷰에서 `ref.watch`/`ref.listen`으로 상태 구독
- 이벤트 스트림은 `StreamProvider`로 래핑

```dart
// game_binding.dart
final gameEngineProvider = Provider<GameEngine>((ref) {
  final repository = ref.watch(storageRepositoryProvider);
  return GameEngine(
    context: GameContext(
      random: SeededRandomProvider(seed: DateTime.now().millisecondsSinceEpoch),
      time: RealTimeProvider(),
    ),
  );
});

final gameEventProvider = StreamProvider<GameEvent>((ref) {
  return ref.watch(gameEngineProvider).events;
});
```

---

## 이벤트 페이로드 정의

모든 이벤트는 `GameEvent`를 상속. 뷰가 UI 업데이트에 필요한 정보를 담는다.

| 이벤트 | 페이로드 |
|--------|----------|
| `EnhanceSuccessEvent` | prevLevel, newLevel, newSwordName, goldSpent |
| `EnhanceFailEvent` | destroyedLevel, destroyedSwordName, fragmentsGained, goldSpent, adProtectionAvailable (일일 잔여 횟수) |
| `SellEvent` | soldLevel, soldSwordName, goldGained |
| `CollectEvent` | collectedLevel, collectedSwordName, isNewCollection, totalCompletion |
| `CollectionCompleteEvent` | - (도감 100% 달성) |
| `FragmentGainEvent` | amount, totalFragments |
| `ExchangeEvent` | itemType, fragmentsSpent, totalFragments |
| `UseItemEvent` | itemType (부적/주문서), remainingCount |
| `MasteryLevelUpEvent` | newLevel, reward (할인율/외형ID/파편보너스) |
| `AdRewardEvent` | adType (보호권/골드/부스터), rewardDetail |
| `GoldChangeEvent` | amount, newTotal, reason (판매/광고/교환/강화비용) |

---

## GameState 구조

세션 중 변하는 상태를 통합. 커맨드의 입력이자 출력.

```dart
class GameState {
  final Sword currentSword;         // 현재 보유 검 (Sword 모델 참조)
  final int currentLevel;           // 현재 강화 단계 (+0 ~ +20)
  final PlayerData playerData;      // 영구 유저 데이터

  // P2 확장 — P1에서는 빈 리스트 / false
  final List<Modifier> activeModifiers;   // 적용 중인 수정자 (부스터, 주문서)
  final bool hasActiveProtection;         // 보호의 부적 적용 여부

  // P2 확장 — 광고 보호권 사후 적용용
  final bool pendingAdProtection;         // 파괴 직후 광고 보호권 사용 대기 상태
}

/// 확률 수정자 인터페이스 (P2에서 구현)
abstract class Modifier {
  double apply(double baseRate);  // 기존 확률에 수정 적용
}
```

---

## PlayerData 복합 구조

P1에서 생성하되, P2/P3 확장을 고려한 구조로 설계.

```dart
class PlayerData {
  // P1 — 핵심
  final int gold;
  final Statistics stats;

  // P2 — 서브 시스템
  final int fragments;
  final MasteryData mastery;        // 장인 숙련도 레벨, 경험치
  final CollectionData collection; // 수집 목록, 중복 횟수
  final Inventory inventory;       // 보유 아이템 (부적, 주문서)
  final AdLimits adLimits;         // 일일 광고 제한 (보호권 잔여 횟수, 마지막 리셋 날짜)

  // P3 — 소셜
  final AchievementData achievements; // 달성 업적
  final TitleData titles;             // 획득 칭호, 장착 칭호
  final String nickname;

  // 메타
  final bool isFirstRun;          // 첫 실행 여부 (초기 200골드 지급 판단)
  final DateTime lastSyncedAt;
}
```

- P1에서 전체 필드를 정의하되, P2/P3 필드는 기본값으로 초기화
- `isFirstRun`으로 첫 실행 시 200골드 지급 판단

## Statistics (통계) 필드

랭킹 산출 및 업적 판정에 사용되는 누적 통계.

```dart
class Statistics {
  final int highestEnhanceLevel;     // 역대 최고 강화 단계
  final int weeklyHighestLevel;      // 이번 주 최고 강화 단계
  final int totalDestroys;           // 총 파괴 횟수
  final int totalEnhanceAttempts;    // 총 강화 시도 횟수
  final int totalSells;              // 총 판매 횟수
  final int totalGoldEarned;         // 총 획득 골드
  final int maxConsecutiveSuccess;   // 연속 성공 최대 기록
  final int maxConsecutiveFail;      // 연속 파괴 최대 기록
  final int currentConsecutiveSuccess; // 현재 연속 성공 (리셋용)
  final int currentConsecutiveFail;    // 현재 연속 파괴 (리셋용)
}
```

## PlayerData 하위 모델 정의

P1에서 클래스만 정의하고 기본값으로 초기화. P2/P3에서 상세 구현.

```dart
/// 장인 숙련도/경험치 (P2에서 상세 구현)
class MasteryData {
  final int level;          // 현재 장인 숙련도 (1~10)
  final int totalAttempts;  // 누적 강화 횟수 (= 경험치)
}

/// 컬렉션 (P2에서 상세 구현)
class CollectionData {
  final Map<int, int> collected;  // {검 레벨: 수집 횟수} (예: {10: 2, 12: 1})
  int get uniqueCount => collected.length;
  int get totalCollectible => 11;  // swords.csv에서 Y인 검 수
  double get completionRate => uniqueCount / totalCollectible;
}

/// 보유 아이템 (P2에서 상세 구현)
class Inventory {
  final int protectionAmulets;    // 보호의 부적 보유량
  final int blessingScrolls;      // 축복의 주문서 보유량
}

/// 광고 일일 제한 (P2에서 상세 구현)
class AdLimits {
  final int adProtectionUsedToday;  // 오늘 사용한 광고 보호권 횟수 (최대 2)
  final DateTime lastResetDate;     // 마지막 일일 리셋 날짜 (자정 기준)
}

/// 업적 (P3에서 상세 구현)
class AchievementData {
  final Set<String> achieved;  // 달성한 업적 ID 집합
}

/// 칭호 (P3에서 상세 구현)
class TitleData {
  final Set<String> earned;     // 획득한 칭호 ID 집합
  final String? equipped;       // 현재 장착 중인 칭호 ID (null = 미장착)
}
```

---

## CSV 파싱 규칙

### 로딩 아키텍처 (Flutter-free 원칙)

`game_core`는 Flutter import 금지이므로, CSV **파일 읽기**는 `app_flutter`가 담당하고, `game_core`는 **문자열 파싱**만 담당.

```dart
// game_core — CSV 문자열을 받아서 파싱만 담당
class SwordDataLoader {
  List<Sword> parse(String csvContent) { ... }
}

// app_flutter — Flutter API로 파일을 읽어서 문자열로 전달
final csvString = await rootBundle.loadString('assets/data/swords.csv');
final swords = SwordDataLoader().parse(csvString);

// 테스트 — 하드코딩된 CSV 문자열을 직접 전달 (파일 I/O 불필요)
final swords = SwordDataLoader().parse('강화,검 이름,...\n+0,나무검,...');
```

이 구조로 `game_core`의 Flutter 의존성 0을 유지하면서 테스트에서도 CSV 데이터를 자유롭게 주입 가능.

### swords.csv
- **인코딩**: UTF-8
- **첫 행**: 헤더 (스킵)
- **+N행의 강화비용**: (N-1)단계 → N단계로 강화하는 데 드는 비용
  - 예: **+1행**(철검, 95%, 5골드) = **+0(나무검) → +1(철검)** 강화에 적용되는 확률과 비용
  - 예: **+10행**(엑스칼리버, 35%, 270골드) = **+9 → +10** 강화에 적용
- **+0행**: 성공률/강화비용/판매가가 `-` → null 또는 0으로 파싱 (나무검은 강화 대상이 아닌 시작점, 강화 비용/확률이 없음)
- **성공률**: CSV에는 % 단위 정수 (예: 95) → 로직에서 0.0~1.0으로 변환 (95 → 0.95)
- **파편 획득량의 정본은 CSV** (`파괴시파편` 컬럼) — 기획서의 구간 테이블은 참고용

### mastery_levels.csv
- **인코딩**: UTF-8
- **첫 행**: 헤더 (스킵)
- **필요경험치**: 해당 레벨 도달에 필요한 **누적** 강화 횟수 (Lv.1은 0)
- **비용할인율**: 0.0~1.0 (0.05 = 5% 할인). 강화비용에 `(1 - costDiscount)` 곱셈 적용, 소수점 이하 버림
- **파편보너스**: 0 또는 1. 파괴 시 CSV 기본 파편 + fragmentBonus

### 에러 처리
- CSV 파일 없음: 앱 크래시 (필수 에셋)
- 파싱 실패 (잘못된 형식): 해당 행 스킵 + 로그 출력
- 없는 레벨 요청: null 반환 (호출부에서 처리)

---

## P1 설계 가이드라인

P1에서 MVP를 만들 때, P2/P3 확장을 위해 지켜야 할 제약 조건.

### enhance_logic 확장 포인트
P1에서 강화 로직을 구현할 때, P2의 부스터/부적/파편/경험치를 **코드 수정 없이 끼울 수 있는 구조**로 만들어야 함.

**확장 방법: EnhanceCommand 내부의 주석 블록 해제**

P1에서 `EnhanceCommand.execute()`는 순수 함수인 Logic들을 순차 호출하여 조합한다. P2/P3 확장 시 주석 처리된 Logic 호출 블록의 주석만 해제하면 됨:

```dart
// EnhanceCommand.execute() 내부 — P2/P3 확장 포인트
// ───────────────────────────────────────────────
// P1 활성: EconomyLogic.spendGold() → EnhanceLogic.roll/handleSuccess/handleFail
//          → GameSessionLogic.resetToWoodenSword()
// P2 해제: MasteryLogic.addExp() → FragmentLogic.giveFragments()
// P3 해제: AchievementLogic.check()
```

- `activeModifiers`: P1에서는 빈 리스트, P2에서 부스터/주문서 추가 (Logic은 이미 처리하는 코드 포함)
- `hasActiveProtection`: P1에서는 항상 false, P2에서 부적 적용 시 true로 전환
- 각 Logic은 순수 함수이므로 조합 순서에 끼워넣기만 하면 됨 — 콜백 연결이나 DI 재설정 불필요

### P1 저장 모드
- P1에서는 `InMemoryRepository` (P0-8에서 생성) 사용
- Hive/Firebase 연동은 P2에서 수행
- P1 태스크에서 자체 로컬 저장 구현 금지

### P1 뷰 — 미구현 화면 처리
- 공방 화면: "준비 중" placeholder 표시
- 업적/칭호 화면: "준비 중" placeholder 표시
- 하단 네비게이션은 3탭 모두 표시하되, 공방/업적은 비활성 느낌

### P1 이벤트 시스템
- `StreamController.broadcast()`로 다중 리스너 지원
- P2에서 동기화 리스너, 업적 체크 리스너를 `events.listen()`으로 추가 가능
- P1에서는 뷰만 구독
