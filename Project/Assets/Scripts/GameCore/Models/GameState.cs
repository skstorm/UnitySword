namespace GameCore.Models
{
    public class GameState
    {
        public Sword CurrentSword { get; }
        public int CurrentLevel { get; }
        public PlayerData PlayerData { get; }
        public bool HasActiveProtection { get; }
        public bool PendingAdProtection { get; }

        public GameState(
            Sword currentSword,
            int currentLevel,
            PlayerData playerData,
            bool hasActiveProtection = false,
            bool pendingAdProtection = false)
        {
            CurrentSword = currentSword;
            CurrentLevel = currentLevel;
            PlayerData = playerData;
            HasActiveProtection = hasActiveProtection;
            PendingAdProtection = pendingAdProtection;
        }

        public GameState With(
            Sword currentSword = null,
            int? currentLevel = null,
            PlayerData playerData = null,
            bool? hasActiveProtection = null,
            bool? pendingAdProtection = null)
        {
            return new GameState(
                currentSword ?? CurrentSword,
                currentLevel ?? CurrentLevel,
                playerData ?? PlayerData,
                hasActiveProtection ?? HasActiveProtection,
                pendingAdProtection ?? PendingAdProtection);
        }
    }
}
