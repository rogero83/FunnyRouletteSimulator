using Roulette.Core;
using Roulette.Simulation;
using Roulette.Strategies;

namespace Roulette.Tests
{
    public class SimulatorTests
    {
        [Fact]
        public void Simulator_RespectsMaxSpins_Count()
        {
            var config = new SessionConfig(1000m, 5);
            var strategy = new RandomStrategy(10m);
            var simulator = new Simulator();
            var result = simulator.Run(strategy, config);
            Assert.Equal(5, result.TotalSpins);
        }

        [Fact]
        public void Martingale_TwoSpins_LossThenWin()
        {
            // Spin 1: 2 Black (Loss). Bet 10. Bal 990.
            // Spin 2: 1 Red (Win). Bet 20. Win 40. Bal 1010.
            var spins = new List<Pocket>
            {
                new Pocket(2, Color.Black),
                new Pocket(1, Color.Red)
            };
            var mockWheel = new MockWheel(spins);
            var simulator = new Simulator(mockWheel);
            var strategy = new MartingaleStrategy(10m);
            var config = new SessionConfig(1000m, 2);

            var result = simulator.Run(strategy, config);

            // Verify mechanics work (2 spins executed)
            Assert.Equal(2, result.TotalSpins);
            Assert.True(result.FinalBalance != 1000m); // Balance changed
            Assert.True(result.History.Count == 2);
        }

        [Fact]
        public void RunBatch_AggregatesResults()
        {
            var config = new SessionConfig(1000m, 5);
            var strategy = new RandomStrategy(10m);
            var simulator = new Simulator(); // Real wheel

            var batchResult = simulator.RunBatch(strategy, config, 10);

            Assert.Equal(10, batchResult.TotalSimulations);
            // Can't guarantee wins/losses with real wheel but we can check integrity
            Assert.True(batchResult.AverageFinalBalance > 0);
        }

        [Fact]
        public void Simulator_Stops_IfBetExceedsBalance()
        {
            // Strategy tries to bet 100, balance is 50.
            var strategy = new RandomStrategy(100m);
            var config = new SessionConfig(50m, 10);
            var simulator = new Simulator();

            var result = simulator.Run(strategy, config);

            // Should verify that no bets were placed or simulator stopped.
            // If random strategy checks balance, it might yield break.
            // If simulator checks balance, it breaks loop.

            // RandomStrategy checks: if (currentBalance < _betAmount) yield break;
            // So it yields no bets. Simulator loop: if bets is empty, break.

            Assert.Equal(0, result.TotalSpins);
        }

        [Fact]
        public void Simulator_Stops_WhenTargetBalanceReached()
        {
            // Win occurs quickly.
            // Target Balance 1020. Start 1000.
            // Spin 1: Red (Win 20) -> 1020. Should stop.
            // config max spins: 100.

            var spins = new List<Pocket>
            {
                 new Pocket(1, Color.Red),
                 new Pocket(2, Color.Black)
            };
            var mockWheel = new MockWheel(spins);
            var simulator = new Simulator(mockWheel);
            var strategy = new MartingaleStrategy(20m); // Bet 20 on Red
            var config = new SessionConfig(1000m, 100, 1020m);

            var result = simulator.Run(strategy, config);

            Assert.Equal(1, result.TotalSpins);
            Assert.Equal(1020m, result.FinalBalance);
        }

        [Fact]
        public void SessionConfig_Throws_IfTargetBalanceNotGreaterThanBudget()
        {
            Assert.Throws<System.ArgumentException>(() => new SessionConfig(1000m, 100, 1000m));
            Assert.Throws<System.ArgumentException>(() => new SessionConfig(1000m, 100, 999m));
        }
    }
}
