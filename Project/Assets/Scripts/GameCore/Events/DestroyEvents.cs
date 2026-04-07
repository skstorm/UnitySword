namespace GameCore.Events
{
    public class DestroyConfirmedEvent : GameEvent
    {
        public int DestroyedLevel { get; }
        public string DestroyedSwordName { get; }

        public DestroyConfirmedEvent(int destroyedLevel, string destroyedSwordName)
        {
            DestroyedLevel = destroyedLevel;
            DestroyedSwordName = destroyedSwordName;
        }
    }
}
