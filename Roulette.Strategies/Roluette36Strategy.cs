using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class Roluette36Strategy : IGameStrategy
    {
        public string Name => $"Roluette 36 - on Zero, one Dozen and High (only European)";

        private readonly IEnumerable<int> _dozen1;
        private readonly IEnumerable<int> _high;
        private readonly decimal _betAmountPerZero;
        private readonly decimal _betAmountPerDozen;
        private readonly decimal _betAmountPerHigh;

        private readonly List<Bet> _bets = [];

        /// <summary>
        /// Strategy to bet on two dozens simultaneously.
        /// </summary>
        /// <param name="betAmountPerZero">Amount to place on Zero</param>
        /// <param name="betAmountPerDozen">Amount to place on dozen</param>
        /// <param name="betAmountPerHigh">Amount to place on High</param>
        public Roluette36Strategy(decimal betAmountPerZero,
            decimal betAmountPerDozen,
            decimal betAmountPerHigh)
        {
            _betAmountPerZero = betAmountPerZero;
            _betAmountPerDozen = betAmountPerDozen;
            _betAmountPerHigh = betAmountPerHigh;

            _dozen1 = Wheel.Dozen(1);
            _high = Wheel.High();

            _bets = [
                new Bet(_betAmountPerDozen, BetType.Dozen, _dozen1),
                new Bet(_betAmountPerHigh, BetType.LowHigh, _high),
                new Bet(_betAmountPerZero, BetType.Straight, [0]),
                ];

            Reset();
        }

        public void Reset()
        {
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            // Need enough balance for ALL bets
            if (currentBalance < _betAmountPerDozen + _betAmountPerHigh + _betAmountPerZero)
            {
                yield break;
            }

            foreach (var bet in _bets)
            {
                yield return bet;
            }
        }
    }
}
