using Roulette.Core;

namespace Roulette.Tests
{
    public class DebugTests
    {
        [Fact]
        public void Test_Martingale_HighBet_Payout()
        {
            // Reproduce the scenario: Bet 640 on Red. Spin 36 (Red).
            decimal betAmount = 640m;
            var redNumbers = new HashSet<int>(Enumerable.Range(1, 36).Where(Wheel.IsRed));
            var bet = new Bet(betAmount, BetType.RedBlack, redNumbers);

            // Assert setup
            Assert.Equal(640m, bet.Amount);
            Assert.Contains(36, bet.TargetNumbers);
            Assert.Equal(BetType.RedBlack, bet.Type);

            // Calculate Winnings
            decimal winnings = PayoutCalculator.CalculateWinnings(bet, 36);

            // Standard payout for Red (1:1): Return Stake + Profit.
            // 640 + 640 = 1280.
            Assert.Equal(1280m, winnings);
        }
    }
}
