using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class MartingaleStrategy : IGameStrategy
    {
        public string Name => "Martingale (Red)";

        private decimal _baseBet;
        private decimal _currentBet;
        private Bet? _lastBet;

        public MartingaleStrategy(decimal baseBet)
        {
            _baseBet = baseBet;
            Reset();
        }

        public void Reset()
        {
            _currentBet = _baseBet;
            _lastBet = null;
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            // Determine if last bet won or lost
            if (history.Any() && _lastBet != null)
            {
                var lastSpin = history.Last();
                decimal winnings = PayoutCalculator.CalculateWinnings(_lastBet, lastSpin.Number);

                if (winnings > 0)
                {
                    // Won, reset to base
                    _currentBet = _baseBet;
                }
                else
                {
                    // Lost, double bet
                    _currentBet *= 2;
                }
            }

            // Cap at balance
            if (_currentBet > currentBalance)
            {
                _currentBet = currentBalance;
            }

            if (_currentBet <= 0)
            {
                yield break;
            }

            // Bet on Red
            // We need to define the target numbers for Red.
            // Getting them from Wheel or hardcoding? Wheel helper `IsRed` is static but Wheel internal list is private.
            // I'll use Wheel.IsRed to build the set.
            var redNumbers = new HashSet<int>(Enumerable.Range(1, 36).Where(Wheel.IsRed));

            _lastBet = new Bet(_currentBet, BetType.RedBlack, redNumbers);

            yield return _lastBet;
        }
    }
}
