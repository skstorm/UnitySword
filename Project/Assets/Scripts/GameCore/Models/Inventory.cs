namespace GameCore.Models
{
    public class Inventory
    {
        public int ProtectionAmulets { get; }

        public Inventory(int protectionAmulets = 0)
        {
            ProtectionAmulets = protectionAmulets;
        }

        public Inventory With(int? protectionAmulets = null)
        {
            return new Inventory(protectionAmulets ?? ProtectionAmulets);
        }
    }
}
