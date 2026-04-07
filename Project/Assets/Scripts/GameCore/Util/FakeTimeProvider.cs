using System;

namespace GameCore.Util
{
    public class FakeTimeProvider : ITimeProvider
    {
        private DateTime _now;

        public FakeTimeProvider(DateTime initial)
        {
            _now = initial;
        }

        public FakeTimeProvider() : this(new DateTime(2026, 1, 1))
        {
        }

        public DateTime Now() => _now;

        public void Advance(TimeSpan duration)
        {
            _now = _now.Add(duration);
        }

        public void SetNow(DateTime value)
        {
            _now = value;
        }
    }
}
