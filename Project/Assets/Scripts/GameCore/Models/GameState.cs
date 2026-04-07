using System.Collections.Generic;

namespace GameCore.Models;

public interface IModifier
{
    double Apply(double rate);
}

public class GameState
{
    public Sword CurrentSword { get; }
    public int CurrentLevel { get; }
    public PlayerData PlayerData { get; }
    public List<IModifier> ActiveModifiers { get; }
    public bool HasActiveProtection { get; }
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
