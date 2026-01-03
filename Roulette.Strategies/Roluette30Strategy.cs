using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class Roluette30Strategy : IGameStrategy
    {
        public string Name => $"Roluette 30 - two Dozen and one Line";

        private readonly IEnumerable<int> _dozen1;
        private readonly IEnumerable<int> _dozen2;
        private readonly IEnumerable<int> _line1;
        private readonly decimal _betAmountPerDozen;
        private readonly decimal _betAmountPerLine;

        private readonly List<Bet> _bets = [];

        /// <summary>
        /// Strategy to bet on two dozens simultaneously.
        /// </summary>
        /// <param name="betAmountPerDozen">Amount to place on EACH dozen</param>
        /// <param name="betAmountPerLine">Amount to place on EACH line</param>
        public Roluette30Strategy(decimal betAmountPerDozen, decimal betAmountPerLine)
        {
            _betAmountPerDozen = betAmountPerDozen;
            _betAmountPerLine = betAmountPerLine;

            _dozen1 = Wheel.Dozen(1);
            _dozen2 = Wheel.Dozen(2);
            _line1 = Wheel.Line(9);

            _bets = [
                new Bet(_betAmountPerDozen, BetType.Dozen, _dozen1),
                new Bet(_betAmountPerDozen, BetType.Dozen, _dozen2),
                new Bet(_betAmountPerLine, BetType.Line, _line1),
                ];

            Reset();
        }

        public void Reset()
        {
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            // Need enough balance for ALL bets
            if (currentBalance < _betAmountPerDozen * 2 + _betAmountPerLine)
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
