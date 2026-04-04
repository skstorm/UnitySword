# 인수인계 문서

## 프로젝트
- **이름**: 검강화 게임 (캐주얼 강화 시뮬레이션)
- **위치**: `C:\Work\Sword\Sword`
- **기술 스택**: Flutter + 순수 Dart (game_core)
- **Flutter SDK**: `C:\dev\flutter` (3.29.3, PATH 등록 완료)

## 프로젝트 구조
```
Sword/
├── packages/game_core/     ← 순수 Dart 패키지 (Flutter 의존성 0)
│   ├── lib/
│   │   ├── game_core.dart  ★ barrel file (Logic 비공개)
│   │   ├── models/         Sword, GameState, PlayerData, Mastery 등
│   │   ├── logic/          enhance, economy, game_session (비공개)
│   │   ├── commands/       EnhanceCommand, SellCommand
│   │   ├── events/         EnhanceEvent, EconomyEvent
│   │   ├── engine/         GameEngine, SessionRecorder
│   │   ├── repositories/   StorageRepository, RankingRepository
│   │   ├── data/           SwordDataLoader, MasteryDataLoader
│   │   └── util/           RandomProvider, TimeProvider
│   └── test/               92 tests passing
├── app_flutter/            ← Flutter 앱
│   ├── lib/
│   │   ├── views/          title, enhance, main_shell
│   │   ├── animations/     controller, config, basic impl
│   │   ├── bindings/       game_binding (Riverpod)
│   │   └── repositories/   InMemoryRepository
│   └── assets/data/        swords.csv, mastery_levels.csv
└── doc/
    ├── plan.md             기획서
    ├── implementation-guide.md  구현계획서
    ├── tasks.md            태스크리스트
    ├── ideas.md            추가 아이디어
    ├── swords.csv          검 데이터
    ├── mastery_levels.csv  숙련도 데이터
    ├── WorkLog/            작업일지
    └── HANDOFF.md          이 파일
```

## 참조 문서 (반드시 읽을 것)
| 문서 | 경로 | 용도 |
|------|------|------|
| 기획서 | `doc/plan.md` | 게임 전체 설계, 시스템 사양, 밸런스 방향 |
| 구현계획서 | `doc/implementation-guide.md` | 아키텍처, 코드 규칙, 인터페이스 상세, P1~P4 확장 가이드 |
| 태스크리스트 | `doc/tasks.md` | Phase별 태스크 정의, 체크리스트 |
| 추가 아이디어 | `doc/ideas.md` | 미채택 아이디어 (구현 시 기획서에 반영 필요) |
| 검 데이터 | `doc/swords.csv` | 21단계 검 밸런스 (원본, app_flutter/assets/data에 복사본) |
| 숙련도 데이터 | `doc/mastery_levels.csv` | 장인 숙련도 10레벨 테이블 (원본) |
| 작업일지 | `doc/WorkLog/` | 일자별 작업 기록, 트러블슈팅 이력 |

## 진행 상황
| Phase | 상태 | 비고 |
|-------|------|------|
| Phase 0: 프로젝트 셋업 | ✅ 완료 | 미커밋 |
| Phase 1: 핵심 게임 루프 (MVP) | ✅ 완료 | 미커밋, 92 tests passing |
| Phase 2: 서브 시스템 | ❌ 미착수 | 파편, 숙련도, 컬렉션, 광고, 저장 |
| Phase 3: 소셜 & 라이브 | ❌ 미착수 | 업적, 칭호, 랭킹, 리플레이 |
| Phase 4: 폴리싱 | ❌ 미착수 | |

## 주의사항
- **커밋 미완료**: Phase 0 + Phase 1 작업이 아직 커밋되지 않음
- **Visual Studio 미설치**: `flutter run -d windows` 불가 → `flutter run -d chrome`으로 실행
- **Phase 완료 검증 필수**: 매 Phase 완료 시 코드/구현계획서/태스크리스트 3중 대조 검증
- **barrel file 규칙**: Logic 클래스는 절대 export하지 않음
- **Logic 순수 함수 규칙**: 사이드이펙트 없음, 다른 Logic 호출 금지, Command가 조합 담당

## 다음 작업
1. Phase 0 + Phase 1 커밋 (사용자 확인 후)
2. Phase 2 진행 (tasks.md P2-1 ~ P2-41)
   - 파편 시스템 (P2-1~8)
   - 장인 숙련도 (P2-9~14)
   - 컬렉션 (P2-15~19)
   - 광고 보상 (P2-20~26)
   - 뷰 (P2-27~32)
   - 저장 모드 시스템 (P2-33~41)

## 테스트 실행
```bash
export PATH="/c/dev/flutter/bin:$PATH"
cd /c/Work/Sword/Sword/packages/game_core && dart test
cd /c/Work/Sword/Sword/app_flutter && flutter analyze
```
