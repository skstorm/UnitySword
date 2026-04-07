namespace GameCore.Data;

/// <summary>게임 전체에서 사용되는 상수 모음. 밸런스 조정 시 이 파일만 수정하면 된다.</summary>
public static class GameConstants
{
    // ── 교환 비용 (파편) ──
    /// <summary>보호 부적 교환에 필요한 파편 수.</summary>
    public const int ProtectionAmuletFragmentCost = 10;
    /// <summary>축복 주문서 교환에 필요한 파편 수.</summary>
    public const int BlessingScrollFragmentCost = 15;
    /// <summary>골드 주머니 교환에 필요한 파편 수.</summary>
    public const int GoldPouchFragmentCost = 5;

    // ── 아이템 효과 ──
    /// <summary>골드 주머니 사용/교환 시 지급되는 골드.</summary>
    public const int GoldPouchGoldAmount = 500;
    /// <summary>축복 주문서의 성공률 보너스 (+5%).</summary>
    public const double BlessingScrollBonus = 0.05;

    // ── 경제 ──
    /// <summary>긴급 지원금 액수.</summary>
    public const int EmergencyFundAmount = 200;
    /// <summary>최초 게임 시작 시 지급되는 골드.</summary>
    public const int InitialGold = 200;

    // ── 수집 ──
    /// <summary>컬렉션 등록이 가능한 최소 강화 레벨.</summary>
    public const int MinCollectionLevel = 10;

    // ── 아이템 이름 (한글) ──
    /// <summary>보호 부적 표시명.</summary>
    public const string ProtectionAmuletName = "보호 부적";
    /// <summary>축복 주문서 표시명.</summary>
    public const string BlessingScrollName = "축복 주문서";
    /// <summary>골드 주머니 표시명.</summary>
    public const string GoldPouchName = "골드 주머니";
}
