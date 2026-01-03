using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class DozenWaithStrategy : IGameStrategy
    {
        public string Name => $"Dozen With Waith - Play on two dozen with waith";

        private readonly IEnumerable<int> _dozen1;
        private readonly IEnumerable<int> _dozen2;
        private readonly IEnumerable<int> _dozen3;
        private readonly decimal _betAmountPerDozen;

        private int _currentIndex;
        private IEnumerable<Bet> _lastBets = [];

        /// <summary>
        /// Strategy to bet on two dozens simultaneously.
        /// </summary>
        /// <param name="betAmountPerDozen">Amount to place on EACH dozen</param>
        public DozenWaithStrategy(decimal betAmountPerDozen)
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
            if (history == null || history.Count < 3)
            {
                yield return Wheel.NoBet();
            }
            else
            {
                if (_lastBets.Any())
                {
                    var lastSpin = history[history.Count - 1];

                    decimal winnings = PayoutCalculator.CalculateWinnings(_lastBets, lastSpin.Number);

                    if (winnings > 0)
                    {
                        // Last bet won
                        _currentIndex = 0;
                        _lastBets = [];
                    }
                    else
                    {
                        // Last bet lost
                        _currentIndex++;
                        foreach (var bet in _lastBets)
                        {
                            yield return CreateDozenBet(bet.TargetNumbers);
                        }
                    }
                }

                if (!_lastBets.Any())
                {
                    // Place bets on the two dozens that did not hit last time
                    var lastHisotry = history.Skip(history.Count - 3).ToList();
                    var countDozen1 = lastHisotry.Count(x => _dozen1.Contains(x.Number));
                    var countDozen2 = lastHisotry.Count(x => _dozen2.Contains(x.Number));
                    var countDozen3 = lastHisotry.Count(x => _dozen3.Contains(x.Number));

                    if (countDozen1 == 3)
                    {
                        var bet2 = CreateDozenBet(_dozen2);
                        var bet3 = CreateDozenBet(_dozen3);
                        _lastBets = [bet2, bet3];

                        yield return bet2;
                        yield return bet3;
                    }
                    else if (countDozen2 == 3)
                    {
                        var bet1 = CreateDozenBet(_dozen1);
                        var bet3 = CreateDozenBet(_dozen3);
                        _lastBets = [bet1, bet3];

                        yield return bet1;
                        yield return bet3;
                    }
                    else if (countDozen3 == 3)
                    {
                        var bet1 = CreateDozenBet(_dozen1);
                        var bet2 = CreateDozenBet(_dozen2);
                        _lastBets = [bet1, bet2];

                        yield return bet1;
                        yield return bet2;
                    }
                    else
                    {
                        // No bet this round
                        yield return Wheel.NoBet();
                    }
                }
            }
        }

        private Bet CreateDozenBet(IEnumerable<int> targetNumbers)
        {
            return new Bet(_betAmountPerDozen * (_currentIndex + 1),
                BetType.Dozen, targetNumbers);
        }
    }
}
