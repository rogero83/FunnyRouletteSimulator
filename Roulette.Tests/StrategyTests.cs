using Roulette.Core;
using Roulette.Strategies;

namespace Roulette.Tests
{
    public class StrategyTests
    {
        [Fact]
        public void Martingale_Reset_RestoresBaseBet()
        {
            // Arrange
            var strategy = new MartingaleStrategy(10m);
            var history = new List<Pocket> { new Pocket(2, Color.Black) }; // Loss for Red

            // Act: Simulate a loss to increase bet
            var betsBeforeReset = strategy.NextBets(history, 1000m).ToList();
            // Martingale logic: Double on loss. Next bet should be 20.
            // Wait, Martingale implementation tracks _lastBet internally. 
            // In the first call to NextBets, it sees history but _lastBet is null if it's fresh?
            // Let's check MartingaleStrategy implementation.

            // Actually, we must simulate the flow: 
            // 1. Initial Bet
            // 2. Report Result -> NextBets calculates based on RESULT of Last Bet.
        }

        [Fact]
        public void Martingale_DoublesCorrectly_AndResets()
        {
            // 1. First Bet
            var strategy = new MartingaleStrategy(10m);
            var bets1 = strategy.NextBets(new List<Pocket>(), 1000m).ToList();
            Assert.Single(bets1);
            Assert.Equal(10m, bets1[0].Amount);

            // 2. Simulate Loss
            // We need to call NextBets with a history that implies the previous bet lost?
            // The Strategy stores _lastBet. So we just need to provide a history where the latest spin 
            // made _lastBet lose.

            var history = new List<Pocket> { new Pocket(2, Color.Black) }; // Black 2
            var bets2 = strategy.NextBets(history, 990m).ToList();

            Assert.Single(bets2);
            Assert.Equal(20m, bets2[0].Amount); // Doubled

            // 3. Reset
            strategy.Reset();

            // 4. Check Bet is back to base (even if passing same history, though usually history resets too in Simulator)
            // But strategy Reset should clear internal state regarding _lastBet
            var bets3 = strategy.NextBets(new List<Pocket>(), 1000m).ToList();
            Assert.Single(bets3);
            Assert.Equal(10m, bets3[0].Amount);
        }

        [Fact]
        public void DAlembert_IncreasesOnLoss_DecreasesOnWin()
        {
            // Base 10.
            var strategy = new DAlembertStrategy(10m);
            var history = new List<Pocket>();

            // 1. Initial
            var bets1 = strategy.NextBets(history, 1000m).ToList();
            Assert.Equal(10m, bets1[0].Amount);

            // 2. Loss (Bets on Red, result Black)
            history.Add(new Pocket(2, Color.Black));
            var bets2 = strategy.NextBets(history, 990m).ToList();
            Assert.Equal(11m, bets2[0].Amount); // Increased by 1

            // 3. Win (Result Red)
            history.Add(new Pocket(1, Color.Red));
            var bets3 = strategy.NextBets(history, 990m).ToList();
            Assert.Equal(10m, bets3[0].Amount); // Decreased back to Base
        }

        [Fact]
        public void Fibonacci_Progression_Works()
        {
            // Seq: 1, 1, 2, 3, 5
            var strategy = new FibonacciStrategy(1m);
            var history = new List<Pocket>();

            // 1. Bet 1
            var bets1 = strategy.NextBets(history, 1000m).ToList();
            Assert.Equal(1m, bets1[0].Amount);

            // 2. Loss (Bets on Black, result Red)
            history.Add(new Pocket(1, Color.Red));
            var bets2 = strategy.NextBets(history, 999m).ToList();
            Assert.Equal(1m, bets2[0].Amount); // Second '1' in sequence

            // 3. Loss
            history.Add(new Pocket(1, Color.Red));
            var bets3 = strategy.NextBets(history, 998m).ToList();
            Assert.Equal(2m, bets3[0].Amount); // '2'

            // 4. Loss
            history.Add(new Pocket(1, Color.Red));
            var bets4 = strategy.NextBets(history, 996m).ToList();
            Assert.Equal(3m, bets4[0].Amount); // '3'

            // 5. Win (result Black) -> Back 2 steps.
            // Current index was at '3' (index 3: 0,1,2,3).
            // Back 2 => index 1 => '1'.
            history.Add(new Pocket(2, Color.Black)); // Win
            var bets5 = strategy.NextBets(history, 1000m).ToList();
            Assert.Equal(1m, bets5[0].Amount);
        }

        [Fact]
        public void AlternateColor_SwitchesColorOnLoss()
        {
            // Logic: Bets Red initially. Loss -> Switch to Black. 
            // Also Fibonacci progression.
            var strategy = new AlternateColorStrategy(1m);
            var history = new List<Pocket>();

            // 1. Initial (Red, 1)
            var bets1 = strategy.NextBets(history, 1000m).ToList();
            Assert.Single(bets1);
            Assert.Contains(1, bets1[0].TargetNumbers); // Red 1 is in target
            Assert.DoesNotContain(2, bets1[0].TargetNumbers); // Black 2 not in target

            // 2. Loss (Black 2 wins) -> Should switch to Black ?
            // Code: _isRedTurn = !_isRedTurn; on Loss.
            history.Add(new Pocket(2, Color.Black));
            var bets2 = strategy.NextBets(history, 1000m).ToList();

            // Check numbers. Red include 1, Black include 2.
            Assert.Contains(2, bets2[0].TargetNumbers);
            Assert.DoesNotContain(1, bets2[0].TargetNumbers);
        }

        [Fact]
        public void Labouchere_Logic_Works()
        {
            // Seq: 1, 2, 3. Unit 1.
            var seq = new List<decimal> { 1, 2, 3 };
            var strategy = new LabouchereStrategy(seq, 1m);
            var history = new List<Pocket>();

            // 1. Initial Bet: 1 + 3 = 4.
            var bets1 = strategy.NextBets(history, 1000m).ToList();
            Assert.Equal(4m, bets1[0].Amount);

            // 2. Win (Red) -> Remove 1 and 3. Left: [2].
            history.Add(new Pocket(1, Color.Red));
            var bets2 = strategy.NextBets(history, 1000m).ToList();
            // Next bet: Only 2 left. Bet 2.
            Assert.Equal(2m, bets2[0].Amount);

            // 3. Loss (Black) -> Add 2 to end. Seq: [2, 2].
            history.Add(new Pocket(2, Color.Black));
            var bets3 = strategy.NextBets(history, 1000m).ToList();
            // Next bet: 2 + 2 = 4.
            Assert.Equal(4m, bets3[0].Amount);

            // 4. Win -> Remove 2 and 2. Seq empty. Reset to [1, 2, 3].
            history.Add(new Pocket(1, Color.Red));
            var bets4 = strategy.NextBets(history, 1000m).ToList();
            // Reset: 1 + 3 = 4.
            Assert.Equal(4m, bets4[0].Amount);
        }

        [Fact]
        public void StreetStrategy_ValidatesInput()
        {
            Assert.Throws<ArgumentException>(() => new StreetStrategy(2, 10m));
            Assert.Throws<ArgumentException>(() => new StreetStrategy(0, 10m));

            var stra = new StreetStrategy(1, 10m);
            Assert.NotNull(stra);
        }

        [Fact]
        public void StreetStrategy_BetsOnCorrectNumbers()
        {
            var strategy = new StreetStrategy(4, 10m);
            var bets = strategy.NextBets(new List<Pocket>(), 100m).ToList();

            Assert.Single(bets);
            var bet = bets[0];
            Assert.Equal(BetType.Street, bet.Type);
            Assert.Equal(10m, bet.Amount);
            Assert.Equal(3, bet.TargetNumbers.Count);
            Assert.Contains(4, bet.TargetNumbers);
            Assert.Contains(5, bet.TargetNumbers);
            Assert.Contains(6, bet.TargetNumbers);
        }

        [Fact]
        public void DozenStrategy_ValidatesInput()
        {
            Assert.Throws<ArgumentException>(() => new DozenStrategy(0, 10m));
            Assert.Throws<ArgumentException>(() => new DozenStrategy(4, 10m));

            var stra = new DozenStrategy(1, 10m);
            Assert.NotNull(stra);
        }

        [Fact]
        public void DozenStrategy_BetsOnCorrectRange()
        {
            var strategy = new DozenStrategy(2, 50m);
            var bets = strategy.NextBets(new List<Pocket>(), 1000m).ToList();

            Assert.Single(bets);
            var bet = bets[0];
            Assert.Equal(BetType.Dozen, bet.Type);
            Assert.Equal(50m, bet.Amount);
            Assert.Equal(12, bet.TargetNumbers.Count);

            Assert.Contains(13, bet.TargetNumbers);
            Assert.Contains(24, bet.TargetNumbers);
        }

        [Fact]
        public void DoubleDozenStrategy_ValidatesInput()
        {
            Assert.Throws<ArgumentException>(() => new DoubleDozenStrategy(1, 1, 10m));
            Assert.Throws<ArgumentException>(() => new DoubleDozenStrategy(0, 1, 10m));

            var stra = new DoubleDozenStrategy(1, 2, 10m);
            Assert.NotNull(stra);
        }

        [Fact]
        public void DoubleDozenStrategy_PlacesTwoBets()
        {
            var strategy = new DoubleDozenStrategy(1, 3, 5m);
            var bets = strategy.NextBets(new List<Pocket>(), 1000m).ToList();

            Assert.Equal(2, bets.Count);
            Assert.All(bets, b => Assert.Equal(BetType.Dozen, b.Type));
            Assert.Contains(bets, b => b.TargetNumbers.Contains(1));
            Assert.Contains(bets, b => b.TargetNumbers.Contains(25));
        }

        [Fact]
        public void LabouchereStrategy_CustomSequence_Works()
        {
            var strategy = new LabouchereStrategy(new[] { 10m, 20m }, 1m);
            var bets = strategy.NextBets(new List<Pocket>(), 1000m).ToList();
            Assert.Equal(30m, bets[0].Amount);
        }
    }
}
