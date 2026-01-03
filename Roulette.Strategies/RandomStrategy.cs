using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class RandomStrategy : IGameStrategy
    {
        public string Name => "Random Single Number";

        private readonly decimal _betAmount;
        private readonly Random _random;
        private readonly bool _isAmerican;

        public RandomStrategy(decimal betAmount, bool isAmerican = true)
        {
            _betAmount = betAmount;
            _random = new Random();
            _isAmerican = isAmerican;
            //_bet = RandomBet();
        }

        public void Reset()
        {
            // No state to reset
        }

        public IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance)
        {
            if (currentBalance < _betAmount)
            {
                yield break;
            }
            yield return RandomBet();
        }

        private Bet RandomBet()
        {
            // Pick random number 0-36 (or 00)
            int num = _random.Next(0, _isAmerican ? 38 : 37); // 0 to 37 (37 is 00)
            if (num == 37) num = Wheel.DoubleZero;
            return new Bet(_betAmount, BetType.Straight, [num]);
        }
    }
}
