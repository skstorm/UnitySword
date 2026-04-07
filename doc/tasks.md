# 검강화 게임 태스크 리스트

## Phase 0: 프로젝트 셋업

- [ ] P0-2. `GameCore` 순수 C# 어셈블리 생성 (Unity 의존성 없음) + Assembly Definition (`GameCore.asmdef`) 작성. **Logic 클래스는 `internal`로 선언** — Models, Commands, Events, Engine, Repositories, Data, Util만 public
- [ ] P0-3. `App` Unity 앱 어셈블리 생성 (`App.asmdef`, `GameCore` 참조)
- [ ] P0-4. 기본 추상화 인터페이스 작성 (`IRandomProvider`, `ITimeProvider`, `IStorageRepository`)
- [ ] P0-5. 커맨드/이벤트 기본 인터페이스 작성 (`Command`(execute + validate), `CommandResult`, `GameEvent`, `CommandRejectedEvent`, `LogicResult`)
- [ ] P0-6. `GameEngine` 뼈대 작성 (dispatch → validate → 실행 → 이벤트 발행)
- [ ] P0-7. `Sword` 모델 작성 + `swords.csv` 파서 (`SwordDataLoader`) 작성 + 테스트
- [ ] P0-8. 테스트 인프라 구축 (`FakeRandomProvider`, `FakeTimeProvider`, `InMemoryRepository`)

---

## Phase 1: 코어 게임 루프 (강화 → 판매/파괴 → 나무검 리셋)

### 모델

- [ ] P1-1. `Statistics` 모델 (역대 최고 강화, 총 파괴 횟수, 총 강화 시도, 연속 성공/파괴 기록 등)
- [ ] P1-2. `PlayerData` 모델 (gold, stats, isFirstRun 등. P2/P3 필드는 기본값 초기화)
- [ ] P1-3. `GameState` 모델 (currentSword, currentLevel, playerData, hasActiveProtection, pendingAdProtection)

### 로직

- [ ] P1-4. `EnhanceLogic.cs` — internal 순수 함수. `GetEffectiveRate()`, `Roll()`, `HandleSuccess()`, `HandleFail()`
- [ ] P1-5. `EconomyLogic.cs` — internal 순수 함수. `CanAfford()`, `SpendGold()`, `AddGold()`
- [ ] P1-6. `GameSessionLogic.cs` — `CreateInitialState` + `ResetToWoodenSword`. 첫 실행 시 200골드

### 커맨드

- [ ] P1-8. `EnhanceCommand` — 강화 시도 (validate: 골드 부족, pendingAdProtection)
- [ ] P1-9. `SellCommand` — 현재 검 판매 (validate: 나무검 판매 불가)
- [ ] P1-10. `ConfirmDestroyCommand` — 파괴 확정 + 나무검 리셋 (P1에서는 파괴 시 항상 사용)

### 이벤트

- [ ] P1-11. 이벤트 정의 (`EnhanceSuccessEvent`, `EnhanceFailEvent`, `SellEvent`, `GoldChangeEvent`, `DestroyConfirmedEvent`)

### 테스트

- [ ] P1-12. 강화 성공/실패 단위 테스트 (FakeRandom으로 결정적 테스트)
- [ ] P1-13. 골드 차감/지급 테스트
- [ ] P1-14. 골드 부족 시 강화 불가 테스트
- [ ] P1-15. 판매가 = CSV 고정값 일치 테스트
- [ ] P1-16. 세션 리셋 테스트 (파괴/판매 후 나무검 복귀)
- [ ] P1-17. 통합 흐름 테스트 — Command → GameEngine.dispatch() → Event 발행 e2e
- [ ] P1-18. 결정론 테스트 — 같은 시드 + 같은 커맨드 = 같은 결과
- [ ] P1-19. 커맨드 유효성 검증 테스트 (reject 케이스)

---

## Phase 2: 게임 서브 시스템 (파편 + 수집 + 장인 숙련도)

### 파편

- [ ] P2-1. `FragmentLogic.cs` — 파괴 시 파편 지급 (CSV 기반)
- [ ] P2-2. `ExchangeCommand` — 파편 → 아이템 교환 (보호의 부적 30개, 골드 주머니 10개)
- [ ] P2-3. `UseItemCommand` — 보호의 부적 사용 (hasActiveProtection = true)
- [ ] P2-4. 보호의 부적 적용 로직 (파괴 대신 단계 유지)
- [ ] P2-5. 파편 관련 이벤트 (`FragmentGainEvent`, `ExchangeEvent`, `UseItemEvent`)
- [ ] P2-6. 파편 테스트 (지급량, 교환, 부적 효과)

### 수집

- [ ] P2-7. `CollectionLogic.cs` — 수집 등록, 완성도 계산
- [ ] P2-8. `CollectCommand` — 현재 검 수집 (+10 이상만)
- [ ] P2-9. 수집 관련 이벤트 (`CollectEvent`, `CollectionCompleteEvent`)
- [ ] P2-10. 수집 테스트 (+9 이하 불가, 중복 카운트, 완성도)

### 장인 숙련도

- [ ] P2-11. `MasteryLevel` 모델 + `mastery_levels.csv` 파서 (`MasteryDataLoader`)
- [ ] P2-12. `MasteryLogic.cs` — 강화 시도 시 경험치 +1, 레벨업 판정
- [ ] P2-13. 레벨 보상 적용 (비용 할인, 파편 +1)
- [ ] P2-14. 장인 숙련도 이벤트 (`MasteryLevelUpEvent`)
- [ ] P2-15. 장인 숙련도 테스트 (경험치 누적, 레벨업, 할인 적용)

### EnhanceCommand P2 확장

- [ ] P2-16. `EnhanceCommand`에 장인 숙련도 경험치 + 파편 지급 로직 통합

---

## Phase 3: 부가 기능 (광고, 업적, 랭킹, 저장, 리플레이)

### 광고 보상

- [ ] P3-1. `AdRewardLogic.cs` — 보상 타입별 처리 (보호권/골드)
- [ ] P3-2. `WatchAdCommand` — 광고 시청 커맨드
- [ ] P3-3. 보호권 일일 2회 제한 로직 (TimeProvider 활용)
- [ ] P3-4. 광고 보호권 사후 적용 흐름 (파괴 → 팝업 → 복구 or 확정)
- [ ] P3-5. `IAdService` 인터페이스 + `DevAdService` + `RealAdService`
- [ ] P3-6. 광고 보상 테스트

### 업적/칭호

- [ ] P3-7. `Achievement` 모델 + `AchievementLogic.cs`
- [ ] P3-8. `Title` 모델 + `TitleLogic.cs`
- [ ] P3-9. 업적/칭호 테스트

### 랭킹

- [ ] P3-10. `RankingLogic.cs` — 랭킹 산출
- [ ] P3-11. `RankingRepository.cs` — Firebase 랭킹 읽기/쓰기
- [ ] P3-12. 랭킹 연출 이벤트

### 저장/동기화

- [ ] P3-13. `LocalStorageRepository.cs` — JSON 파일 로컬 저장
- [ ] P3-14. `AuthService.cs` — Firebase 익명 로그인
- [ ] P3-15. `FirebaseRepository` + `SyncedRepository` 동기화
- [ ] P3-16. 오프라인 처리 + 충돌 해결
- [ ] P3-17. 저장 모드 전환 DI 설정

### 리플레이

- [ ] P3-18. `SessionRecorder.cs` — 커맨드/시드 기록
- [ ] P3-19. 리플레이 재생 로직 + `ReplayTimeProvider`
- [ ] P3-20. 리플레이 결정론 테스트

---

## Phase 4: 폴리싱

- [ ] P4-1. `RichEnhanceAnimation` 구현체 (풀 연출)
- [ ] P4-2. 사운드/BGM/햅틱 연동
- [ ] P4-3. 공방 외형 변화 (숙련도 레벨별)
- [ ] P4-4. FTUE (신규 유저 가이드)
- [ ] P4-5. 설정 화면
- [ ] P4-6. 밸런스 시뮬레이션
- [ ] P4-7. 앱스토어 배포 준비
