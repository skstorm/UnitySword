# 검강화 게임 태스크 리스트

## Phase 0: 프로젝트 셋업

- [ ] P0-1. Flutter 프로젝트 생성 (모노레포 구조)
- [ ] P0-2. `game_core` 순수 Dart 패키지 생성 (Flutter 의존성 없음) + barrel file (`game_core.dart`) 작성. **Logic 클래스는 export하지 않음** — models, commands, events, engine, repositories, data, util만 export. 구현계획서 "barrel file" 참조
- [ ] P0-3. `app_flutter` Flutter 앱 생성 (`game_core` 의존)
- [ ] P0-4. 기본 추상화 인터페이스 작성 (`RandomProvider`, `TimeProvider`, `StorageRepository`)
- [ ] P0-5. 커맨드/이벤트 기본 인터페이스 작성 (`Command`(execute + validate), `CommandResult`, `GameEvent`, `CommandRejectedEvent`, `LogicResult`)
- [ ] P0-6. `GameEngine` 뼈대 작성 (dispatch → validate → 실행 → 이벤트 발행). **dispatch 내부에서 `command.validate()` 호출 → 실패 시 `CommandRejectedEvent` 발행 후 종료**. 구현계획서 "GameEngine" 및 "커맨드 유효성 검증" 참조
- [ ] P0-7. `Sword` 모델 작성 + `swords.csv` 파서 (`SwordDataLoader`) 작성 + 테스트. 모델 필드는 구현계획서 "swords.csv 컬럼 → Sword 모델 매핑" 참조. **Sword 모델이 파서의 반환 타입이므로 반드시 함께 작성**
- [ ] P0-8. 테스트 인프라 구축 (`FakeRandomProvider`, `FakeTimeProvider`, `InMemoryRepository`)

---

## Phase 1: 핵심 게임 루프 (MVP)

### 모델

- [ ] P1-1. `Statistics` 모델 (역대 최고 강화, 총 파괴 횟수, 총 강화 시도, 연속 성공/파괴 기록 등). 구현계획서 "Statistics (통계) 필드" 참조
- [ ] P1-2. `PlayerData` 모델 (복합 구조 — gold, stats, fragments, mastery, collection, inventory, adLimits, achievements, titles, nickname, isFirstRun, lastSyncedAt. P2/P3 필드는 기본값 초기화). 구현계획서 "PlayerData 복합 구조" 참조. **Statistics, Inventory 등 하위 모델도 이 태스크에서 정의** (P2/P3 전용 모델은 해당 Phase에서 상세 구현)
- [ ] P1-3. `GameState` 모델 (세션 중 변하는 상태 통합: currentSword, currentLevel, playerData, activeModifiers(P1에서는 빈 리스트), hasActiveProtection(P1에서는 항상 false))

### 로직

- [ ] P1-4. `enhance_logic.dart` — **순수 함수**로 구현 (콜백/사이드이펙트 없음). `getEffectiveRate()`, `roll()`, `handleSuccess()`, `handleFail()` → 각각 `LogicResult(newState, events)` 반환. **P2 확장 포인트 필수**: activeModifiers(부스터/주문서), hasActiveProtection(부적). 구현계획서 "Logic 순수 함수 규칙" 및 "P1 설계 가이드라인" 참조
- [ ] P1-5. `economy_logic.dart` — **순수 함수**로 구현. `canAfford()`, `spendGold()`, `addGold()` → `LogicResult` 반환. 골드 차감 (강화 비용: `SwordDataTable`에서 해당 레벨의 `enhanceCost` 참조), 골드 지급 (판매: `SwordDataTable`에서 해당 레벨의 `sellPrice` 참조). **판매가는 CSV 고정값을 그대로 사용** — 로직에서 계산하지 않음
- [ ] P1-6. `game_session_logic.dart` — `createInitialState`(앱 시작 시 GameState 생성) + `resetToWoodenSword`(파괴/판매/수집 후 리셋). 첫 실행 시 200골드 지급 (`isFirstRun` 판단 포함). 구현계획서 "GameEngine 초기화 흐름" 및 "세션 초기화 규칙" 4케이스 참조

### 커맨드

- [ ] P1-8. `EnhanceCommand` — 강화 시도
- [ ] P1-9. `SellCommand` — 현재 검 판매

### 이벤트

- [ ] P1-10. 이벤트 정의 (`EnhanceSuccessEvent`, `EnhanceFailEvent`, `SellEvent`, `GoldChangeEvent`). broadcast 스트림으로 다중 리스너 지원. 구현계획서 "이벤트 페이로드 정의" 참조

### 테스트

- [ ] P1-11. 강화 성공/실패 단위 테스트 (FakeRandom으로 결정적 테스트)
- [ ] P1-12. 골드 차감/지급 테스트
- [ ] P1-13. 골드 부족 시 강화 불가 테스트
- [ ] P1-14. 판매가 = CSV 고정값 일치 테스트
- [ ] P1-15. 세션 리셋 테스트 (파괴/판매 후 나무검 복귀)
- [ ] P1-15a. 통합 흐름 테스트 — Command → GameEngine.dispatch() → Event 발행 end-to-end 검증. EnhanceCommand로 성공/실패 시나리오, SellCommand로 판매 시나리오를 GameEngine 경유로 실행하여 올바른 Event가 broadcast 스트림에 발행되는지 확인
- [ ] P1-15b. 결정론 테스트 — 같은 시드 + 같은 커맨드 시퀀스를 2회 실행하여 동일한 최종 GameState가 나오는지 검증. 리플레이 구현은 P3이지만, 결정론적 실행 보장은 P1부터 반드시 지켜야 함. `FakeRandomProvider(seed)` + `FakeTimeProvider(fixedTime)` + 동일 커맨드 10~20개 시퀀스를 2회 실행 → 최종 state 동일성 assert
- [ ] P1-15c. 커맨드 유효성 검증 테스트 — 골드 부족 시 EnhanceCommand reject, 나무검 SellCommand reject, pendingAdProtection 상태에서 EnhanceCommand/SellCommand reject, CommandRejectedEvent 발행 확인

### 연출 모듈

- [ ] P1-16. `EnhanceAnimationController` 인터페이스 작성 (playEnhanceAttempt, playSuccess, playDestroy, playSell, playCollect, canSkip). 구현계획서 "연출 모듈화" 참조
- [ ] P1-17. `EnhanceAnimationConfig` 데이터 클래스 + 단계 구간별 기본 프리셋 정의 (서스펜스 시간, 이펙트 색상/크기, 진동 여부 등). 구현계획서 "단계별 연출 설정 데이터" 참조
- [ ] P1-18. `BasicEnhanceAnimation` 구현체 — P1용 최소 연출 (서스펜스 딜레이, 성공 초록 텍스트+스케일, 파괴 빨간 텍스트+페이드아웃). 사운드/진동 없음
- [ ] P1-19. 연출 컨트롤러를 Riverpod Provider로 등록 (`enhanceAnimationProvider`, `animationConfigProvider`). 구현체 교체 시 Provider 한 줄만 변경되는 구조

### 뷰

- [ ] P1-20. `title_view.dart` — 타이틀 화면 (게임 시작 버튼)
- [ ] P1-21. `enhance_view.dart` — 강화 메인 화면 (검 표시, 강화/판매 버튼, 골드, 확률 표시: +14이하 숫자, +15이상 "???"). 이벤트 수신 시 `EnhanceAnimationController`에 위임하여 연출 재생
- [ ] P1-22. 하단 네비게이션 뼈대 (대장간/공방/업적 탭). 공방/업적은 "준비 중" placeholder 표시
- [ ] P1-23. `game_binding.dart` — GameEngine ↔ Flutter 상태관리 연결 (Riverpod). P1에서는 InMemoryRepository 사용. 자체 로컬 저장 구현 금지

---

## Phase 2: 서브 시스템

### 파편 시스템

- [ ] P2-1. `Fragment` 모델 (보유량)
- [ ] P2-2. `fragment_logic.dart` — 파괴 시 파편 지급 (단계 비례)
- [ ] P2-3. `UseItemCommand` — 보호의 부적/축복의 주문서 사용
- [ ] P2-4. `ExchangeCommand` — 파편 → 아이템 교환
- [ ] P2-5. 보호의 부적 적용 로직 (파괴 대신 단계 유지)
- [ ] P2-6. 축복의 주문서 적용 로직 (확률 +5%p)
- [ ] P2-7. 파편 관련 이벤트 (`FragmentGainEvent`, `ExchangeEvent`, `UseItemEvent`)
- [ ] P2-8. 파편 관련 테스트 (지급량, 교환, 아이템 효과)

### 장인 숙련도

- [ ] P2-9. `Mastery` 모델 (레벨, 경험치). 구현계획서 "mastery_levels.csv 컬럼 → MasteryLevel 모델 매핑" 참조
- [ ] P2-10. `mastery_levels.csv` 파서 (`MasteryDataLoader`) 작성 + 테스트. **`mastery_levels.csv`는 이미 프로젝트 루트에 존재** — 새로 만들지 말고 기존 파일 사용. 구현계획서 "mastery_levels.csv" 파싱 규칙 참조
- [ ] P2-11. `mastery_logic.dart` — 강화 시도 시 경험치 +1, 레벨업 판정
- [ ] P2-12. 레벨 보상 적용 (비용 할인, 파편 +1)
- [ ] P2-13. 장인 숙련도 이벤트 (`MasteryLevelUpEvent`)
- [ ] P2-14. 장인 숙련도 테스트 (경험치 누적, 레벨업, 할인 적용)

### 컬렉션

- [ ] P2-15. `Collection` 모델 (수집 목록, 중복 횟수, 완성도)
- [ ] P2-16. `collection_logic.dart` — 수집 등록, 완성도 계산
- [ ] P2-17. `CollectCommand` — 현재 검 수집 (+10 이상만)
- [ ] P2-18. 컬렉션 이벤트 (`CollectEvent`, `CollectionCompleteEvent`)
- [ ] P2-19. 수집 관련 테스트 (+9 이하 수집 불가, 중복 수집 카운트, 완성도 계산)

### 광고 보상

- [ ] P2-20. `ad_reward_logic.dart` — 보상 타입별 처리 (보호권/골드/부스터)
- [ ] P2-21. `WatchAdCommand` — 광고 시청 커맨드
- [ ] P2-22. 보호권 일일 2회 제한 로직 (TimeProvider 활용)
- [ ] P2-23. 광고 보호권 **사후 적용** 흐름 구현 + `ConfirmDestroyCommand` 작성. 구현계획서 "광고 보호권 사후 적용 커맨드 흐름" 참조. 흐름: EnhanceCommand→파괴→EnhanceFailEvent(adProtectionAvailable)→뷰 팝업→광고 시청 시 WatchAdCommand(protection)로 복구, 거부 시 ConfirmDestroyCommand로 파괴 확정+나무검 리셋. **보호의 부적(사전 적용)과 적용 시점이 다름에 주의**
- [ ] P2-24. 확률 부스터 +5%p 적용 로직. 광고 부스터와 축복의 주문서 **중복 적용 가능 (최대 +10%p)**
- [ ] P2-25. 광고 보상 이벤트 (`AdRewardEvent`)
- [ ] P2-26. 광고 보상 테스트 — 보호권 일일 제한(2회→3회째 거부), 골드 무제한, 부스터 적용, **부스터+주문서 중복 적용 시 +10%p 상한 검증**, 광고 보호권 사후 적용(파괴 후 복구) 테스트

### 뷰

- [ ] P2-27. `workshop_view.dart` — 공방 화면 (장인 숙련도/경험치, 파편 교환소, 컬렉션 도감)
- [ ] P2-28. 파편 교환 UI
- [ ] P2-29. 컬렉션 도감 UI (수집 검 진열, 미수집 실루엣, 완성도, 별 표시)
- [ ] P2-30. 강화 화면에 부스터/보호 부적 사용 UI 추가 + 파괴 시 광고 보호권 팝업 UI (사후 적용)
- [ ] P2-31. `sword_display_widget.dart`, `gold_indicator_widget.dart` — 공용 위젯 분리
- [ ] P2-32. 광고 연동 (`ad_service.dart` — AdMob 리워드 광고)

### 저장 (저장 모드 시스템)

저장 모드 3종: `test`(InMemory) / `local`(Hive) / `server`(Hive+Firebase)
DI 시점에 구현체 교체로 모드 전환 — 코드 수정 없음.
InMemoryRepository는 P0-8에서 생성 완료.

- [ ] P2-33. `HiveStorageRepository` — 로컬 저장소 구현 (Hive)
- [ ] P2-34. 로컬 저장/로딩 테스트 (골드, 컬렉션, 업적, 장인 숙련도, 파편, 통계)
- [ ] P2-35. `auth_service.dart` — Firebase 익명 로그인
- [ ] P2-36. `FirebaseRepository` — Firestore 원격 저장소 구현
- [ ] P2-37. `SyncedRepository` — Hive(로컬) + Firebase(원격) 동기화 래퍼
  - save() → Hive 즉시 저장 → Firebase 비동기 업로드
  - load() → 앱 시작 시 Firebase pull → Hive 반영 → Hive에서 읽기
  - 동기화 타이밍: 판매/수집/파괴/백그라운드 전환 시
- [ ] P2-38. 동기화 충돌 해결 (최신 타임스탬프 우선)
- [ ] P2-39. 오프라인 처리 (동기화 실패 시 로컬 유지, 복귀 시 재동기화)
- [ ] P2-40. 저장 모드 전환 DI 설정 (`StorageMode` enum → 구현체 팩토리)
- [ ] P2-41. Firebase 동기화 통합 테스트

---

## Phase 3: 소셜 & 라이브

### 업적/칭호

- [ ] P3-1. `Achievement` 모델 (조건, 달성 여부)
- [ ] P3-2. `Title` 모델 (칭호, 장착 여부)
- [ ] P3-3. `achievement_logic.dart` — 달성 조건 판정 (강화/파괴/수집/골드/연속기록)
- [ ] P3-4. `title_logic.dart` — 칭호 부여/장착
- [ ] P3-5. `achievement_view.dart` — 업적/칭호 화면
- [ ] P3-6. 업적/칭호 테스트

### 랭킹

- [ ] P3-7. `ranking_logic.dart` — 랭킹 산출 (최고 강화, 파괴왕)
- [ ] P3-8. `ranking_repository.dart` — Firebase 랭킹 전용 읽기/쓰기 (P2-35 FirebaseRepository 활용)
- [ ] P3-9. 랭킹 화면 UI (공방 화면 또는 별도 접근)
- [ ] P3-10. 랭킹 연출 — 고강화 달성 전체 팝업, 주간 1등 공지, 기록 갱신 푸시

### 리플레이

- [ ] P3-11. `session_recorder.dart` — 커맨드/시드 기록. `SessionRecord`에 `randomSeed`, `startTime`, `initialState`, `commands` 포함. **validate 통과한 커맨드만 기록** (rejected 커맨드 제외). 구현계획서 "리플레이 데이터" 참조
- [ ] P3-12. 리플레이 재생 로직 — `ReplayTimeProvider`(startTime + command.timestamp로 시각 재현) 구현 + SessionRecord → GameEngine 재실행. 재생 순서: initialState로 초기화 → randomSeed로 RandomProvider 초기화 → startTime으로 ReplayTimeProvider 초기화 → commands 순차 dispatch (매 커맨드 전 `replayTimeProvider.syncTo(timestamp)`). 구현계획서 "리플레이 데이터" 참조
- [ ] P3-13. 리플레이 결정론 테스트 (같은 시드+커맨드+startTime = 같은 결과). 시간 의존 로직(광고 보호권 일일 제한, 주간 랭킹 리셋)도 재현되는지 검증

### 푸시/알림

- [ ] P3-14. `push_service.dart` — 푸시 알림 연동
- [ ] P3-15. 기록 갱신 시 알림

---

## Phase 4: 폴리싱

- [ ] P4-1. `RichEnhanceAnimation` 구현체 — 고도화 연출 (서스펜스 어둡게→불꽃 파티클→빛 집중→결과, 단계별 차등 폭발 이펙트, 파괴 시 검 갈라짐 파티클, 사운드/진동 연동). `enhanceAnimationProvider`의 구현체만 교체하여 적용
- [ ] P4-2. 공방 외형 변화 (장인 숙련도 레벨별)
- [ ] P4-3. 확률 표시 고도화 (부스터 적용 시 "??? + 부스터 적용 중" 표시 등)
- [ ] P4-4. FTUE (신규 유저 첫 경험 가이드)
- [ ] P4-5. 설정 화면 (사운드, 진동, 알림, 데이터 초기화)
- [ ] P4-6. 오프라인 모드 처리 (광고 불가 시 안내)
- [ ] P4-7. 밸런스 시뮬레이션 (커맨드 자동 생성으로 기대 수치 검증)
- [ ] P4-8. 앱스토어 배포 준비 (아이콘, 스크린샷, 설명)
