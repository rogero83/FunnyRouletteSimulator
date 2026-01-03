using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class DoubleDozenAlternate03Strategy : IGameStrategy
    {
        public string Name => $"Double Dozen (alternate dozen, fibonacci on color when lost and choise dozen by history)";

        private readonly IEnumerable<int> _dozen1;
        private readonly IEnumerable<int> _dozen2;
        private readonly IEnumerable<int> _dozen3;
        private readonly decimal _betAmountPerDozen;
        private readonly List<int> _sequence;

        private int _currentIndex;
        private IEnumerable<Bet> _lastBets = [];
        private bool _betColor = false;
        private bool _betToRed = true;

        /// <summary>
        /// Strategy to bet on two dozens simultaneously.
        /// </summary>
        /// <param name="betAmountPerDozen">Amount to place on EACH dozen</param>
        public DoubleDozenAlternate03Strategy(decimal betAmountPerDozen)
        {
            _dozen1 = Wheel.Dozen(1);
            _dozen2 = Wheel.Dozen(2);
            _dozen3 = Wheel.Dozen(3);
            _betAmountPerDozen = betAmountPerDozen;

            _sequence = [1, 1];
            for (int i = 2; i < 30; i++)
            {
                _sequence.Add(_sequence[i - 1] + _sequence[i - 2]);
            }
            Reset();
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            // Need enough balance for BOTH bets
            if (currentBalance < _betAmountPerDozen * 2)
            {
                yield break;
            }

            // On first spin, do not bet
            if (history == null || history.Count == 0)
            {
                yield return Wheel.NoBet();
            }
            else
            {
                var lastSpin = history[history.Count - 1];

                decimal winnings = PayoutCalculator.CalculateWinnings(_lastBets, lastSpin.Number);

                if (winnings > 0)
                {
                    // Last bet won, continue with base bet
                    _currentIndex = 0;
                    _betColor = false;
                }
                else
                {
                    // Last bet lost, bet on color next round
                    _currentIndex++;
                    // Safety check if we exceed pre-calculated sequence
                    if (_currentIndex >= _sequence.Count)
                    {
                        // Expand sequence
                        _sequence.Add(_sequence[_sequence.Count - 1] + _sequence[_sequence.Count - 2]);
                    }

                    if (!_betColor)
                    {
                        _betColor = true;
                        _betToRed = !(lastSpin.Color == Color.Red);
                    }
                }


                if (_betColor)
                {
                    decimal colorBetAmount = 2 * _betAmountPerDozen * _sequence[_currentIndex];
                    var numbers = _betToRed
                        ? new HashSet<int>(Enumerable.Range(1, 36).Where(n => Wheel.IsRed(n)))
                        : new HashSet<int>(Enumerable.Range(1, 36).Where(n => !Wheel.IsRed(n)));

                    var betColor = new Bet(colorBetAmount, BetType.RedBlack, numbers);
                    _lastBets = [betColor];
                    yield return betColor;
                    yield break;
                }

                // Place bets on the two dozens that did not hit last time
                var lastHisotry = history.Count() >= 10
                    ? history.Skip(history.Count - 10).ToList()
                    : history.ToList();
                var countDozen1 = lastHisotry.Count(x => _dozen1.Contains(x.Number));
                var countDozen2 = lastHisotry.Count(x => _dozen2.Contains(x.Number));
                var countDozen3 = lastHisotry.Count(x => _dozen3.Contains(x.Number));

                if (countDozen1 > countDozen2 && countDozen1 > countDozen3)
                {
                    var bet2 = CreateDozenBet(_dozen2);
                    var bet3 = CreateDozenBet(_dozen3);
                    _lastBets = [bet2, bet3];

                    yield return bet2;
                    yield return bet3;
                }
                else if (countDozen2 > countDozen1 && countDozen2 > countDozen3)
                {
                    var bet1 = CreateDozenBet(_dozen1);
                    var bet3 = CreateDozenBet(_dozen3);
                    _lastBets = [bet1, bet3];

                    yield return bet1;
                    yield return bet3;
                }
                else
                {
                    var bet1 = CreateDozenBet(_dozen1);
                    var bet2 = CreateDozenBet(_dozen2);
                    _lastBets = [bet1, bet2];

                    yield return bet1;
                    yield return bet2;
                }
            }
        }

        private Bet CreateDozenBet(IEnumerable<int> targetNumbers)
        {
            return new Bet(_betAmountPerDozen, BetType.Dozen, targetNumbers);
        }
    }
}
