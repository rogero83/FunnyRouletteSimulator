using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class DozenStrategy : IGameStrategy
    {
        public string Name => $"Dozen Bet ({_dozenIndex}st/nd/rd Dozen)";

        private readonly int _dozenIndex;
        private readonly decimal _betAmount;

        /// <summary>
        /// Strategies for Dozen bets.
        /// </summary>
        /// <param name="dozenIndex">1 (1-12), 2 (13-24), or 3 (25-36)</param>
        /// <param name="betAmount">Amount to bet</param>
        public DozenStrategy(int dozenIndex, decimal betAmount)
        {
            if (dozenIndex < 1 || dozenIndex > 3)
            {
                throw new ArgumentException("Dozen index must be 1, 2, or 3.");
            }
            _dozenIndex = dozenIndex;
            _betAmount = betAmount;
        }

        public void Reset()
        {
            // No state
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            if (currentBalance < _betAmount)
            {
                yield break;
            }

            // Calculate range:
            // Index 1: 1-12
            // Index 2: 13-24
            // Index 3: 25-36
            var targetNumbers = Wheel.Dozen(_dozenIndex);

            yield return new Bet(_betAmount, BetType.Dozen, targetNumbers);
        }
    }
}
