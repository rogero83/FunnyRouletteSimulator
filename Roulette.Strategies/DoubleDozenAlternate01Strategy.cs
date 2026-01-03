using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class DoubleDozenAlternate01Strategy : IGameStrategy
    {
        public string Name => $"Double Dozen (alternate dozen, multiply when lost and not bet on first spin)";

        private readonly IEnumerable<int> _dozen1;
        private readonly IEnumerable<int> _dozen2;
        private readonly IEnumerable<int> _dozen3;
        private readonly decimal _betAmountPerDozen;

        private int _currentIndex;
        private IEnumerable<int> _lastBetDozens = [];

        /// <summary>
        /// Strategy to bet on two dozens simultaneously.
        /// </summary>
        /// <param name="betAmountPerDozen">Amount to place on EACH dozen</param>
        public DoubleDozenAlternate01Strategy(decimal betAmountPerDozen)
        {
            _dozen1 = Wheel.Dozen(1);
            _dozen2 = Wheel.Dozen(2);
            _dozen3 = Wheel.Dozen(3);
            _betAmountPerDozen = betAmountPerDozen;

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
                var last = history[history.Count - 1];
                if (_lastBetDozens.Any())
                {
                    if (_lastBetDozens.Contains(last.Number))
                    {
                        // Last bet won
                        _currentIndex = 0;
                    }
                    else
                    {
                        // Last bet lost, Move forward 1 step
                        _currentIndex++;
                    }
                }

                if (_dozen1.Contains(last.Number))
                {
                    _lastBetDozens = _dozen2.Concat(_dozen3);
                    yield return CreateBet(_dozen2);
                    yield return CreateBet(_dozen3);
                }
                else if (_dozen2.Contains(last.Number))
                {
                    _lastBetDozens = _dozen1.Concat(_dozen3);
                    yield return CreateBet(_dozen1);
                    yield return CreateBet(_dozen3);
                }
                else if (_dozen3.Contains(last.Number))
                {
                    _lastBetDozens = _dozen1.Concat(_dozen2);
                    yield return CreateBet(_dozen1);
                    yield return CreateBet(_dozen2);
                }
                else
                {
                    yield return Wheel.NoBet();
                }
            }
        }

        private Bet CreateBet(IEnumerable<int> targetNumbers)
        {
            return new Bet(_betAmountPerDozen * (decimal)Math.Pow(3, _currentIndex),
                BetType.Dozen, targetNumbers);
        }
    }
}
