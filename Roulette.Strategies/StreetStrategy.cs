using Roulette.Core;
using Roulette.Simulation;

namespace Roulette.Strategies
{
    public class StreetStrategy : IGameStrategy
    {
        public string Name => $"Street Bet (Start: {_streetStart})";

        private readonly int _streetStart;
        private readonly decimal _betAmount;

        public StreetStrategy(int streetStart, decimal betAmount)
        {
            if (!Wheel.IsValidStreetStart(streetStart))
            {
                throw new ArgumentException($"Invalid street start number: {streetStart}. Must be 1, 4, 7...34.");
            }
            _streetStart = streetStart;
            _betAmount = betAmount;
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

            var targetNumbers = Wheel.StreetFromNumber(_streetStart);
            yield return new Bet(_betAmount, BetType.Street, targetNumbers);
        }
    }
}
