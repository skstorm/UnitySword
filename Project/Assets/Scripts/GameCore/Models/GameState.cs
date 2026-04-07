using System.Collections.Generic;

namespace GameCore.Models;

/// <summary>성공률에 영향을 주는 버프 인터페이스. (예: 축복 주문서)</summary>
public interface IModifier
{
    double Apply(double rate);
}

/// <summary>
/// 게임의 전체 상태를 나타내는 불변 객체.
/// 모든 상태 변경은 With()로 새 인스턴스를 생성한다.
/// </summary>
public class GameState
{
    public Sword CurrentSword { get; }
    public int CurrentLevel { get; }
    public PlayerData PlayerData { get; }
    /// <summary>현재 활성화된 강화 버프 목록. 강화 시도 시 소모된다.</summary>
    public List<IModifier> ActiveModifiers { get; }
    /// <summary>보호 부적 활성 여부. true면 다음 실패 시 파괴를 방지.</summary>
    public bool HasActiveProtection { get; }
    /// <summary>광고 보호 대기 상태. Unity 광고 시스템 전용.</summary>
    public bool PendingAdProtection { get; }

    public GameState(
        Sword currentSword,
        int currentLevel,
        PlayerData playerData,
        List<IModifier> activeModifiers = null,
        bool hasActiveProtection = false,
        bool pendingAdProtection = false)
    {
        CurrentSword = currentSword;
        CurrentLevel = currentLevel;
        PlayerData = playerData;
        ActiveModifiers = activeModifiers ?? new List<IModifier>();
        HasActiveProtection = hasActiveProtection;
        PendingAdProtection = pendingAdProtection;
    }

    public GameState With(
        Sword currentSword = null,
        int? currentLevel = null,
        PlayerData playerData = null,
        List<IModifier> activeModifiers = null,
        bool? hasActiveProtection = null,
        bool? pendingAdProtection = null)
    {
        return new GameState(
            currentSword ?? CurrentSword,
            currentLevel ?? CurrentLevel,
            playerData ?? PlayerData,
            activeModifiers ?? ActiveModifiers,
            hasActiveProtection ?? HasActiveProtection,
            pendingAdProtection ?? PendingAdProtection);
    }
}
