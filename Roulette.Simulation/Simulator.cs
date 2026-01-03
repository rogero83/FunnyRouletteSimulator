using Roulette.Core;

namespace Roulette.Simulation
{
    public class Simulator
    {
        private readonly IWheel _wheel;

        public Simulator(IWheel? wheel = null)
        {
            _wheel = wheel ?? new Wheel();
        }

        public SessionResult Run(IGameStrategy strategy, SessionConfig config)
        {
            var result = new SessionResult(config.InitialBudget);
            var pocketHistory = new List<Pocket>();

            for (int i = 0; i < config.MaxSpins; i++)
            {
                if (result.FinalBalance <= 0)
                {
                    break;
                }

                // get bets from strategy
                // IMPORTANT: Materialize to List to prevent multiple enumerations triggering strategy side-effects (like Martingale doubling)
                var bets = strategy.NextBets(pocketHistory, result.FinalBalance).ToList();

                if (bets == null || !bets.Any())
                {
                    // Strategy stopped playing
                    break;
                }

                decimal totalBetAmount = bets.Sum(b => b.Amount);
                if (totalBetAmount > result.FinalBalance)
                {
                    // Not enough money to place these bets. 
                    // We could try to place partial, but for strict simulation: stop.
                    break;
                }

                // Deduct bets
                result.FinalBalance -= totalBetAmount;

                // Spin
                var winningPocket = _wheel.Spin();
                pocketHistory.Add(winningPocket);
                result.TotalSpins++;

                // Calculate Winnings                
                decimal totalWinnings = PayoutCalculator.CalculateWinnings(bets, winningPocket.Number);

                result.FinalBalance += totalWinnings;

                // Record History
                result.History.Add(new SpinResult(winningPocket, totalBetAmount, totalWinnings, result.FinalBalance, bets));

                // Check Win Limit
                if (config.TargetBalance.HasValue && result.FinalBalance >= config.TargetBalance.Value)
                {
                    result.ReachedTarget = true;
                    break;
                }
            }

            return result;
        }

        public BatchSessionResult RunBatch(IGameStrategy strategy, SessionConfig config, int numberOfSimulations)
        {
            var batchResult = new BatchSessionResult
            {
                TotalSimulations = numberOfSimulations
            };

            decimal totalBalance = 0;

            for (int i = 0; i < numberOfSimulations; i++)
            {
                strategy.Reset();
                var result = Run(strategy, config);

                if (result.FinalBalance > config.InitialBudget)
                {
                    batchResult.SuccessfulSessions++;
                }

                if (result.ReachedTarget)
                {
                    batchResult.SessionsReachedTarget++;
                }

                if (result.FinalBalance <= 0)
                {
                    batchResult.BankruptSessions++;
                }

                if (result.FinalBalance > batchResult.BestSessionBalance)
                {
                    batchResult.BestSessionBalance = result.FinalBalance;
                }

                if (result.FinalBalance < batchResult.WorstSessionBalance)
                {
                    batchResult.WorstSessionBalance = result.FinalBalance;
                }

                totalBalance += result.FinalBalance;
                batchResult.FinalBalances.Add(result.FinalBalance);
            }

            batchResult.AverageFinalBalance = totalBalance / numberOfSimulations;

            double sumOfSquares = batchResult.FinalBalances.Sum(b => (double)((b - batchResult.AverageFinalBalance) * (b - batchResult.AverageFinalBalance)));
            batchResult.StandardDeviation = (decimal)Math.Sqrt(sumOfSquares / numberOfSimulations);

            return batchResult;
        }
    }
}
