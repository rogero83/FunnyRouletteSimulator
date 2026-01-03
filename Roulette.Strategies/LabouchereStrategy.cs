using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class LabouchereStrategy : IGameStrategy
    {
        public string Name => "Labouchere (Red)";

        // Configuration
        private readonly List<decimal> _initialSequence;

        // State
        private List<decimal> _currentSequence;
        private Bet? _lastBet;
        private decimal _unit; // Multiplier for values in sequence

        public LabouchereStrategy(IEnumerable<decimal> initialSequence, decimal unit = 1m)
        {
            _initialSequence = initialSequence.ToList();
            if (!_initialSequence.Any())
            {
                _initialSequence = [1, 2, 3, 4];
            }
            _unit = unit;

            // Initialize state, like reset
            _currentSequence = new List<decimal>(_initialSequence);
            _lastBet = null;
        }

        public void Reset()
        {
            _currentSequence = new List<decimal>(_initialSequence);
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
                    // Win: Remove First and Last
                    if (_currentSequence.Any())
                    {
                        _currentSequence.RemoveAt(0);
                    }
                    if (_currentSequence.Any())
                    {
                        _currentSequence.RemoveAt(_currentSequence.Count - 1);
                    }
                }
                else
                {
                    // Loss: Append amount bet to end
                    // Amount bet was derived from the sequence, so we add it back (amount / unit to keep sequence abstract or just plain amount?)
                    // Standard Labouchere adds the number that was effectively bet (represented in the sequence).
                    // If we are strictly using the sequence numbers, we should add the sum of what we just bet.

                    // Reconstruct what was bet: Sum of First + Last from PREVIOUS state.
                    // But simpler: just take _lastBet.Amount.
                    // IMPORTANT: Sequence usually represents UNITS. So we divide by unit.
                    decimal lossValue = _lastBet.Amount / _unit;
                    _currentSequence.Add(lossValue);
                }
            }

            // If sequence empty, Reset (Objective completed)
            if (_currentSequence.Count == 0)
            {
                _currentSequence = new List<decimal>(_initialSequence);
            }

            decimal nextBetValue;
            if (_currentSequence.Count == 1)
            {
                nextBetValue = _currentSequence[0];
            }
            else
            {
                nextBetValue = _currentSequence[0] + _currentSequence[_currentSequence.Count - 1];
            }

            decimal betAmount = nextBetValue * _unit;

            if (betAmount > currentBalance)
            {
                betAmount = currentBalance;
            }

            if (betAmount <= 0)
            {
                yield break;
            }

            var redNumbers = new HashSet<int>(Enumerable.Range(1, 36).Where(Wheel.IsRed));
            _lastBet = new Bet(betAmount, BetType.RedBlack, redNumbers);

            yield return _lastBet;
        }
    }
}
