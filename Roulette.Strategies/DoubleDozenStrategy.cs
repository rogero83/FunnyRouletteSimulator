using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class DoubleDozenStrategy : IGameStrategy
    {
        public string Name => $"Double Dozen ({_dozen1} & {_dozen2})";

        private readonly int _dozen1;
        private readonly int _dozen2;
        private readonly decimal _betAmountPerDozen;

        /// <summary>
        /// Strategy to bet on two dozens simultaneously.
        /// </summary>
        /// <param name="dozen1">First Dozen (1-3)</param>
        /// <param name="dozen2">Second Dozen (1-3)</param>
        /// <param name="betAmountPerDozen">Amount to place on EACH dozen</param>
        public DoubleDozenStrategy(int dozen1, int dozen2, decimal betAmountPerDozen)
        {
            if (dozen1 < 1 || dozen1 > 3 || dozen2 < 1 || dozen2 > 3)
            {
                throw new ArgumentException("Dozen indices must be 1, 2, or 3.");
            }
            if (dozen1 == dozen2)
            {
                throw new ArgumentException("Dozen indices must be distinct.");
            }

            _dozen1 = dozen1;
            _dozen2 = dozen2;
            _betAmountPerDozen = betAmountPerDozen;
        }

        public void Reset()
        {
            // No state
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            // Need enough balance for BOTH bets
            if (currentBalance < _betAmountPerDozen * 2)
            {
                yield break;
            }

            yield return CreateBet(_dozen1);
            yield return CreateBet(_dozen2);
        }

        private Bet CreateBet(int dozenIndex)
        {
            var targetNumbers = Wheel.Dozen(dozenIndex);
            return new Bet(_betAmountPerDozen, BetType.Dozen, targetNumbers);
        }
    }
}
