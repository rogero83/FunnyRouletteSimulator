using Roulette.Core;
using Roulette.Strategies;

namespace Roulette.Tests
{
    public class StrategyProgressionTests
    {
        [Fact]
        public void Roluette30Strategy_PlacesExpectedBets()
        {
            var strategy = new Roluette30Strategy(10m, 5m);
            var bets = strategy.NextBets(new List<Pocket>(), 1000m).ToList();

            Assert.Equal(3, bets.Count);
            Assert.Contains(bets, b => b.Type == BetType.Dozen && b.Amount == 10m && b.TargetNumbers.Intersect(Wheel.Dozen(1)).Count() == 12);
            Assert.Contains(bets, b => b.Type == BetType.Dozen && b.Amount == 10m && b.TargetNumbers.Intersect(Wheel.Dozen(2)).Count() == 12);
            Assert.Contains(bets, b => b.Type == BetType.Line && b.Amount == 5m && b.TargetNumbers.Intersect(Wheel.Line(9)).Count() == 6);
        }

        [Fact]
        public void Roluette36Strategy_PlacesExpectedBets()
        {
            var strategy = new Roluette36Strategy(1m, 10m, 20m);
            var bets = strategy.NextBets(new List<Pocket>(), 1000m).ToList();

            Assert.Equal(3, bets.Count);
            Assert.Contains(bets, b => b.Type == BetType.Dozen && b.Amount == 10m);
            Assert.Contains(bets, b => b.Type == BetType.LowHigh && b.Amount == 20m);
            Assert.Contains(bets, b => b.Type == BetType.Straight && b.Amount == 1m && b.TargetNumbers.Contains(0));
        }

        [Fact]
        public void DozenWaithStrategy_WaitsForThreeHits_ThenBets()
        {
            var strategy = new DozenWaithStrategy(10m);
            var history = new List<Pocket>();

            // 1. First 2 spins - No bet (needs history.Count >= 3)
            history.Add(new Pocket(1, Color.Red));
            history.Add(new Pocket(2, Color.Black));
            var bets1 = strategy.NextBets(history, 1000m).ToList();
            Assert.Single(bets1);
            Assert.Equal(BetType.NoBet, bets1[0].Type);

            // 2. Third spin result: 1 again (making it 3 hits in Dozen 1 if we add another 1)
            // Wait, logic says: skip 3 spins, then check if ANY dozen hit 3 times.
            history.Add(new Pocket(1, Color.Red));
            history.Add(new Pocket(1, Color.Red));
            // History now: [1, 2, 1, 1]. Count = 4.
            // lastHistory = history.Skip(1) = [2, 1, 1]. Not 3 times.

            history.Clear();
            history.Add(new Pocket(1, Color.Red));
            history.Add(new Pocket(1, Color.Red));
            history.Add(new Pocket(1, Color.Red));
            // History: [1, 1, 1]. Count = 3.
            var bets2 = strategy.NextBets(history, 1000m).ToList();

            // Should now bet on Dozen 2 & 3 because Dozen 1 hit 3 times
            Assert.Equal(2, bets2.Count);
            Assert.All(bets2, b => Assert.Equal(10m, b.Amount));
            Assert.Contains(bets2, b => b.TargetNumbers.Intersect(Wheel.Dozen(2)).Any());
            Assert.Contains(bets2, b => b.TargetNumbers.Intersect(Wheel.Dozen(3)).Any());
        }

        [Fact]
        public void DoubleDozenAlternate02Strategy_SwitchesToColorOnLoss()
        {
            var strategy = new DoubleDozenAlternate02Strategy(10m);
            var history = new List<Pocket>();

            // 1. Initial No Bet
            var bets0 = strategy.NextBets(history, 1000m).ToList();
            Assert.Equal(BetType.NoBet, bets0[0].Type);

            // 2. First spin result: 1 (Dozen 1) -> This sets _lastBets to empty but triggers no-bet check.
            // Actually, code says: if history.Count > 0, calculate winnings.
            // If winnings == 0 (which is true if _lastBets is empty), it increments _currentIndex and sets _betColor = true.
            history.Add(new Pocket(1, Color.Red));
            var bets1 = strategy.NextBets(history, 1000m).ToList();

            // After first spin, _lastBets was empty, so winnings = 0.
            // _currentIndex becomes 1. _betColor becomes true.
            Assert.Single(bets1);
            Assert.Equal(BetType.RedBlack, bets1[0].Type);
            Assert.Equal(20m, bets1[0].Amount); // 2 * 10 * sequence[1] (which is 1) = 20
        }

        [Fact]
        public void DoubleDozenAlternate03Strategy_SwitchesToColorOnEmptyLastBets()
        {
            var strategy = new DoubleDozenAlternate03Strategy(10m);
            var history = new List<Pocket>();

            // 1. Initial No Bet
            strategy.NextBets(history, 1000m).ToList();

            // 2. First spin (result Red 1) -> _lastBets empty -> winnings 0 -> switches to color
            history.Add(new Pocket(1, Color.Red));
            var bets = strategy.NextBets(history, 1000m).ToList();

            Assert.Single(bets);
            Assert.Equal(BetType.RedBlack, bets[0].Type);
        }
        [Fact]
        public void DoubleDozenAlternate01Strategy_ProgressionWorks()
        {
            var strategy = new DoubleDozenAlternate01Strategy(10m);
            var history = new List<Pocket>();

            // 1. Initial No Bet
            strategy.NextBets(history, 1000m).ToList();

            // 2. First spin: 1 (Dozen 1)
            history.Add(new Pocket(1, Color.Red));
            var bets1 = strategy.NextBets(history, 1000m).ToList();
            Assert.Equal(2, bets1.Count);
            Assert.All(bets1, b => Assert.Equal(10m, b.Amount)); // 3^0 = 1

            // 3. Loss (Spin 5 - Dozen 1 again) -> Increment index to 1 (3^1 = 3)
            history.Add(new Pocket(5, Color.Red));
            var bets2 = strategy.NextBets(history, 1000m).ToList();
            Assert.All(bets2, b => Assert.Equal(30m, b.Amount));

            // 4. Loss again -> Increment index to 2 (3^2 = 9)
            history.Add(new Pocket(9, Color.Red));
            var bets3 = strategy.NextBets(history, 1000m).ToList();
            Assert.All(bets3, b => Assert.Equal(90m, b.Amount));
        }
    }
}
