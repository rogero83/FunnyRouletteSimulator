using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class DAlembertStrategy : IGameStrategy
    {
        public string Name => "D'Alembert (Red)";

        private readonly decimal _baseBet;
        private readonly decimal _unit;
        private decimal _currentBet;
        private Bet? _lastBet;

        public DAlembertStrategy(decimal baseBet, decimal unit = 1m)
        {
            _baseBet = baseBet;
            _unit = unit;
            Reset();
        }

        public void Reset()
        {
            _currentBet = _baseBet;
            _lastBet = null;
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            // Determine success of last bet
            if (history.Any() && _lastBet != null)
            {
                var lastSpin = history.Last();
                decimal winnings = PayoutCalculator.CalculateWinnings(_lastBet, lastSpin.Number);

                if (winnings > 0)
                {
                    // Won: Decrease bet by one unit
                    _currentBet -= _unit;
                    if (_currentBet < _baseBet) _currentBet = _baseBet;
                }
                else
                {
                    // Lost: Increase bet by one unit
                    _currentBet += _unit;
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

            var redNumbers = new HashSet<int>(Enumerable.Range(1, 36).Where(Wheel.IsRed));
            _lastBet = new Bet(_currentBet, BetType.RedBlack, redNumbers);

            yield return _lastBet;
        }
    }
}
