using Roulette.Core;

namespace Roulette.Tests
{
    public class PayoutTests
    {
        [Fact]
        public void CalculateWinnings_StraightBet_Returns36x()
        {
            // Arrange
            var bet = new Bet(10m, BetType.Straight, new[] { 17 });

            // Act
            decimal winnings = PayoutCalculator.CalculateWinnings(bet, 17);

            // Assert
            // 10 base + (10 * 35) = 10 + 350 = 360
            Assert.Equal(360m, winnings);
        }

        [Fact]
        public void CalculateWinnings_RedBlack_Returns2x()
        {
            // Arrange
            var bet = new Bet(10m, BetType.RedBlack, new[] { 1, 3, 5 });

            // Act
            decimal winnings = PayoutCalculator.CalculateWinnings(bet, 1); // 1 is in target

            // Assert
            // 10 + (10 * 1) = 20
            Assert.Equal(20m, winnings);
        }

        [Fact]
        public void CalculateWinnings_Loss_ReturnsZero()
        {
            // Arrange
            var bet = new Bet(10m, BetType.Straight, new[] { 17 });

            // Act
            decimal winnings = PayoutCalculator.CalculateWinnings(bet, 20);

            // Assert
            Assert.Equal(0m, winnings);
        }
    }
}
