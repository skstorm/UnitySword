using System.Collections.Generic;

namespace GameCore.Util
{
    public class FakeRandomProvider : IRandomProvider
    {
        private readonly Queue<double> _values = new Queue<double>();
        private double _defaultValue;

        public FakeRandomProvider(double defaultValue = 0.5)
        {
            _defaultValue = defaultValue;
        }

        public double NextDouble()
        {
            return _values.Count > 0 ? _values.Dequeue() : _defaultValue;
        }

        public void SetNext(double value)
        {
            _values.Enqueue(value);
        }

        public void SetDefault(double value)
        {
            _defaultValue = value;
        }
    }
}
