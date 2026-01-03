using Roulette.Core;

namespace Roulette.Tests
{
    public class WheelTests
    {
        [Fact]
        public void AmericanWheel_Has38Pockets()
        {
            var wheel = new Wheel();
            Assert.Equal(38, wheel.Pockets.Count);
        }

        [Fact]
        public void AmericanWheel_Has0And00()
        {
            var wheel = new Wheel();
            Assert.Contains(wheel.Pockets, p => p.Number == 0);
            Assert.Contains(wheel.Pockets, p => p.Number == Wheel.DoubleZero);
        }

        [Fact]
        public void IsRed_ReturnsTrueForRedNumbers()
        {
            // 1 is Red, 2 is Black
            Assert.True(Wheel.IsRed(1));
            Assert.False(Wheel.IsRed(2));
            Assert.False(Wheel.IsRed(0)); // Green
        }

        [Fact]
        public void Spin_ReturnsValidPocket()
        {
            var wheel = new Wheel();
            var pocket = wheel.Spin();
            Assert.NotNull(pocket);
            Assert.Contains(pocket, wheel.Pockets);
        }
    }
}
