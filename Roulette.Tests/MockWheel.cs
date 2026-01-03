using Roulette.Core;

namespace Roulette.Tests
{
    public class MockWheel : IWheel
    {
        private readonly Queue<Pocket> _predefinedSpins;

        public IReadOnlyList<Pocket> Pockets => new List<Pocket>(); // Not needed for current tests

        public MockWheel(IEnumerable<Pocket> spins)
        {
            _predefinedSpins = new Queue<Pocket>(spins);
        }

        public Pocket Spin()
        {
            if (_predefinedSpins.Count > 0)
                return _predefinedSpins.Dequeue();

            // Default fallback if runs out
            return new Pocket(0, Color.Green);
        }
    }
}
