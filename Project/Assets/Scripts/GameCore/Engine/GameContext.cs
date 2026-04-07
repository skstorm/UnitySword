using GameCore.Data;
using GameCore.Logic;
using GameCore.Util;

namespace GameCore.Engine;

/// <summary>
/// 게임 실행에 필요한 의존성을 모아둔 컨텍스트.
/// 랜덤, 시간, 검 데이터, 로직 클래스 등을 제공한다.
/// </summary>
public class GameContext
{
    public IRandomProvider Random { get; }
    public ITimeProvider Time { get; }
    public SwordDataTable SwordTable { get; }
    public bool IsAdSystemEnabled { get; }

    /// <summary>강화 판정/성공/실패 처리 로직.</summary>
    internal EnhanceLogic EnhanceLogic { get; }
    /// <summary>골드 수입/지출 처리 로직.</summary>
    internal EconomyLogic EconomyLogic { get; }

    public GameContext(IRandomProvider random, ITimeProvider time, SwordDataTable swordTable, bool isAdSystemEnabled = false)
    {
        Random = random;
        Time = time;
        SwordTable = swordTable;
        IsAdSystemEnabled = isAdSystemEnabled;

        // 로직 인스턴스를 여기서 한 번만 생성 (DI 컨테이너 역할)
        EnhanceLogic = new EnhanceLogic();
        EconomyLogic = new EconomyLogic();
    }
}
