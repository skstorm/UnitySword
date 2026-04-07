using System;

namespace GameCore.Util
{
    public class SeededRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SeededRandomProvider(int seed)
        {
            _random = new Random(seed);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}
