using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class FibonacciStrategy : IGameStrategy
    {
        public string Name => "Fibonacci (Black)";

        private readonly decimal _unit;
        private readonly List<int> _sequence;
        private int _currentIndex;
        private Bet? _lastBet;

        public FibonacciStrategy(decimal unit = 1m)
        {
            _unit = unit;
            // Standard Fibonacci sequence: 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144...
            // We can generate it on the fly or pre-calculate. 
            // Let's generate enough for a standard session.
            _sequence = new List<int> { 1, 1 };
            for (int i = 2; i < 30; i++)
            {
                _sequence.Add(_sequence[i - 1] + _sequence[i - 2]);
            }
            Reset();
        }

        public void Reset()
        {
            _currentIndex = 0;
            _lastBet = null;
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            if (history.Any() && _lastBet != null)
            {
                var lastSpin = history.Last();
                decimal winnings = PayoutCalculator.CalculateWinnings(_lastBet, lastSpin.Number);

                if (winnings > 0)
                {
                    // Won: Move back 2 steps
                    _currentIndex -= 2;
                    if (_currentIndex < 0) _currentIndex = 0;
                }
                else
                {
                    // Lost: Move forward 1 step
                    _currentIndex++;
                    // Safety check if we exceed pre-calculated sequence
                    if (_currentIndex >= _sequence.Count)
                    {
                        // Expand sequence
                        _sequence.Add(_sequence[_sequence.Count - 1] + _sequence[_sequence.Count - 2]);
                    }
                }
            }

            decimal betAmount = _sequence[_currentIndex] * _unit;

            if (betAmount > currentBalance)
            {
                betAmount = currentBalance;
            }

            if (betAmount <= 0)
            {
                yield break;
            }

            // Bet on Black (to vary from Red strategies)
            var blackNumbers = new HashSet<int>(Enumerable.Range(1, 36).Where(n => !Wheel.IsRed(n)));
            _lastBet = new Bet(betAmount, BetType.RedBlack, blackNumbers);

            yield return _lastBet;
        }
    }
}
