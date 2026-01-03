using Roulette.Core;
using Roulette.Simulation;
using Roulette.Strategies;

namespace Roulette.ConsoleApp
{
    class Program
    {
        const bool IsAmerican = false;

        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine($"   {(IsAmerican ? "American" : "European")} Roulette Strategy Simulator");
            Console.WriteLine("========================================");

            while (true)
            {
                // 1. Setup Phase (Configure Strategy and Params)
                var sessionSetup = SetupSession();
                if (sessionSetup == null)
                {
                    // User chose to exit or invalid input
                    break;
                }

                // 2. Execution Loop (Run -> Replay -> Run)
                while (true)
                {
                    RunSimulation(sessionSetup.Value.Strategy,
                        sessionSetup.Value.Config,
                        sessionSetup.Value.NumSimulations,
                        IsAmerican);

                    Console.WriteLine("\n[ENTER] to Replay Simulation  |  [N] New Strategy  |  [Any Other Key] Exit");
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Enter)
                    {
                        // Replay: Reset strategy state but keep config
                        // IMPORTANT: For batch runs, Reset is handled inside RunBatch, 
                        // but for single runs or broadly, we should ensure start state is clean.
                        // Since we are re-using the SAME strategy instance, we must reset it manually here if the specific strategy implementation requires it across sessions.
                        sessionSetup.Value.Strategy.Reset();
                        continue;
                    }
                    else if (key.Key == ConsoleKey.N)
                    {
                        // Break inner loop to go back to Setup
                        break;
                    }
                    else
                    {
                        // Exit application
                        return;
                    }
                }
            }
        }

        static (IGameStrategy Strategy, SessionConfig Config, int NumSimulations)? SetupSession()
        {
            // 1. Select Strategy
            IGameStrategy? strategy = SelectStrategy();
            if (strategy == null) return null;

            // 2. Configure Session
            decimal budget = GetDecimalInput("Enter Initial Budget (default 1000): ", 1000m);
            int maxSpins = GetIntInput("Enter Max Spins (default 100): ", 100);
            decimal? targetBalance = GetOptionalDecimalInput($"Enter Target Balance (Leave empty for no limit, must be > {budget}): ", budget);
            int numSimulations = GetIntInput("Enter Number of Simulations (default 1): ", 1);

            SessionConfig config;
            try
            {
                config = new SessionConfig(budget, maxSpins, targetBalance);
                return (strategy, config, numSimulations);
            }
            catch (System.ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        static void RunSimulation(IGameStrategy strategy,
            SessionConfig config,
            int numSimulations,
            bool isAmerican = true)
        {
            var wheel = new Wheel(isAmerican);
            var simulator = new Simulator(wheel); // Uses real wheel

            Console.WriteLine($"\nRunning simulation for {strategy.Name}...");
            Console.WriteLine($"Starting Budget: {config.InitialBudget:C}");
            Console.WriteLine($"Simulations: {numSimulations}");

            if (numSimulations > 1)
            {
                var batchResult = simulator.RunBatch(strategy, config, numSimulations);

                Console.WriteLine("\n--- Batch Simulation Results ---");
                Console.WriteLine($"Total Simulations: {batchResult.TotalSimulations}");
                Console.WriteLine($"Sessions Reached Target: {batchResult.SessionsReachedTarget} ({(double)batchResult.SessionsReachedTarget / batchResult.TotalSimulations:P})");
                Console.WriteLine($"Sessions with Profit (Balance > Initial): {batchResult.SuccessfulSessions} ({(double)batchResult.SuccessfulSessions / batchResult.TotalSimulations:P})");
                Console.WriteLine($"Bankrupt Sessions: {batchResult.BankruptSessions} ({(double)batchResult.BankruptSessions / batchResult.TotalSimulations:P})");
                Console.WriteLine($"Average Final Balance: {batchResult.AverageFinalBalance:C}");
                Console.WriteLine($"Standard Deviation:    {batchResult.StandardDeviation:C}");
                Console.WriteLine($"Best Session Balance: {batchResult.BestSessionBalance:C}");
                Console.WriteLine($"Worst Session Balance: {batchResult.WorstSessionBalance:C}");

                DisplayDistribution(batchResult, config.InitialBudget);
            }
            else
            {
                // Ensure fresh start for single run if replaying on same instance
                strategy.Reset();
                var result = simulator.Run(strategy, config);

                // 4. Results
                Console.WriteLine("\n--- Simulation Results ---");
                Console.WriteLine($"Total Spins: {result.TotalSpins}");
                Console.WriteLine($"Final Balance: {result.FinalBalance:C}");

                decimal profit = result.FinalBalance - result.InitialBudget;
                string profitStr = profit >= 0 ? $"+{profit:C}" : $"{profit:C}";
                Console.ForegroundColor = profit >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"Net Profit/Loss: {profitStr}");
                Console.ResetColor();

                Console.WriteLine("\nSample History (Last 10 spins):");
                int historyCount = result.History.Count;
                int start = Math.Max(0, historyCount - 10);
                for (int i = start; i < historyCount; i++)
                {
                    Console.WriteLine(result.History[i]);
                }
                Console.WriteLine($"Total spin {historyCount}");
            }
        }

        static IGameStrategy? SelectStrategy()
        {
            Console.WriteLine("\nSelect Strategy:");
            Console.WriteLine("1. Martingale (Doubles on loss, bets on Red, Base 10)");
            Console.WriteLine("2. Random Single Number (Bets 10 on random number)");
            Console.WriteLine("3. D'Alembert (Increases by 1 unit on loss, decreases on win, bets on Red)");
            Console.WriteLine("4. Fibonacci (Progresses sequence on loss, back 2 on win, bets on Black)");
            Console.WriteLine("5. Fibonacci alternate (Progresses sequence on loss, back 2 on win, bets on Black)");
            Console.WriteLine("6. Labouchere (Cancel sequence numbers on win, add loss to end, bets on Red)");
            Console.WriteLine("7. Street/Terzina (Bets on 3 consecutive numbers like 1-2-3)");
            Console.WriteLine("8. Dozen/Dozzine (Bets on 1st, 2nd, or 3rd set of 12 numbers)");
            Console.WriteLine("9. Double Dozen (Bets on two dozens e.g. 1st and 2nd)");
            Console.WriteLine("10. Double Dozen alternate 01 (No bet on first spin, on lost use fibonacci)");
            Console.WriteLine("11. Double Dozen alternate 02 (No bet on first spin, on lost use fibonacci on color)");
            Console.WriteLine("12. Double Dozen alternate 03 (No bet on first spin, on lost use fibonacci on color, choose dozen by history)");
            Console.WriteLine("13. Roluette 30 - two Dozen and one Line");
            Console.WriteLine("14. Roluette 36 - on Zero, one Dozen and High (only European)");
            Console.WriteLine("15. Dozen with waith - bet on two Dozen when last 3 number is in other Dozen");
            Console.WriteLine("16. Double Dozen (chooise dozen by history, play only dozen)");
            Console.Write("Choice (1-16): ");
            string choice = Console.ReadLine()!;

            switch (choice.Trim())
            {
                case "1":
                    decimal baseBet = GetDecimalInput("Enter Base Bet (default 10): ", 10m);
                    return new MartingaleStrategy(baseBet);
                case "2":
                    decimal betAmount = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new RandomStrategy(betAmount, IsAmerican);
                case "3":
                    decimal dalembertBase = GetDecimalInput("Enter Base Bet (default 10): ", 10m);
                    decimal dalembertStep = GetDecimalInput("Enter Base Unit step (default 1): ", 1m);
                    return new DAlembertStrategy(dalembertBase, dalembertStep);
                case "4":
                    decimal fibUnit = GetDecimalInput("Enter Unit Amount (default 1): ", 1m);
                    return new FibonacciStrategy(fibUnit);
                case "5":
                    decimal fibAlternateUnit = GetDecimalInput("Enter Unit Amount (default 10): ", 10m);
                    return new AlternateColorStrategy(fibAlternateUnit);
                case "6":
                    string seqInput = GetStringInput("Enter Initial Sequence (space separated, default '1 2 3 4'): ");
                    List<decimal> sequence;
                    if (string.IsNullOrWhiteSpace(seqInput))
                    {
                        sequence = [1, 2, 3, 4];
                    }
                    else
                    {
                        sequence = seqInput.Split([' '], StringSplitOptions.RemoveEmptyEntries)
                                          .Select(s => decimal.TryParse(s, out var d) ? d : 0)
                                          .Where(d => d > 0)
                                          .ToList();
                        if (!sequence.Any()) sequence = new List<decimal> { 1, 2, 3, 4 };
                    }
                    decimal labUnit = GetDecimalInput("Enter Unit Amount (default 1): ", 1m);
                    return new LabouchereStrategy(sequence, labUnit);
                case "7":
                    int startNum = GetIntInput("Enter Street Start Number (1, 4, 7...34): ", 1);
                    if (!Wheel.IsValidStreetStart(startNum))
                    {
                        Console.WriteLine($"Invalid start number {startNum}. Defaulting to 1.");
                        startNum = 1;
                    }
                    decimal streetBet = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new StreetStrategy(startNum, streetBet);
                case "8":
                    int dozenChoice = GetIntInput("Enter Dozen (1, 2, or 3): ", 1);
                    if (dozenChoice < 1 || dozenChoice > 3)
                    {
                        Console.WriteLine("Invalid choice. Defaulting to 1.");
                        dozenChoice = 1;
                    }
                    decimal dozenBet = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new DozenStrategy(dozenChoice, dozenBet);
                case "9":
                    int d1 = GetIntInput("Enter First Dozen (1-3): ", 1);
                    int d2 = GetIntInput("Enter Second Dozen (1-3): ", 2);
                    if (d1 < 1 || d1 > 3 || d2 < 1 || d2 > 3 || d1 == d2)
                    {
                        Console.WriteLine("Invalid or duplicate dozens. Defaulting to 1 and 2.");
                        d1 = 1; d2 = 2;
                    }
                    decimal ddBet = GetDecimalInput("Enter Bet Amount per Dozen (default 10): ", 10m);
                    return new DoubleDozenStrategy(d1, d2, ddBet);
                case "10":
                    decimal doubleDozenAlternateBet = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new DoubleDozenAlternate01Strategy(doubleDozenAlternateBet);
                case "11":
                    decimal doubleDozenAlternate02Bet = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new DoubleDozenAlternate02Strategy(doubleDozenAlternate02Bet);
                case "12":
                    decimal doubleDozenAlternate03Bet = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new DoubleDozenAlternate03Strategy(doubleDozenAlternate03Bet);
                case "13":
                    decimal dozen30Bet = GetDecimalInput("Enter Bet Amount per dozen (default 10): ", 10m);
                    decimal line30Bet = GetDecimalInput("Enter Bet Amount per line (default 5): ", 5m);
                    return new Roluette30Strategy(dozen30Bet, line30Bet);
                case "14":
                    decimal zero36Bet = GetDecimalInput("Enter Bet Amount per Zero (default 1): ", 1m);
                    decimal dozen36Bet = GetDecimalInput("Enter Bet Amount per dozen (default 14): ", 14m);
                    decimal high36Bet = GetDecimalInput("Enter Bet Amount per line (default 22): ", 22m);
                    return new Roluette36Strategy(zero36Bet, dozen36Bet, high36Bet);
                case "15":
                    decimal dozenWithWhaitAlternateBet = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new DozenWaithStrategy(dozenWithWhaitAlternateBet);
                case "16":
                    decimal dozenChangeBet = GetDecimalInput("Enter Bet Amount (default 10): ", 10m);
                    return new DoubleDozenChangeOnWinStrategy(dozenChangeBet);
                default:
                    Console.WriteLine("Invalid choice.");
                    return null;
            }
        }

        static decimal GetDecimalInput(string prompt, decimal defaultValue)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(input)) return defaultValue;
            if (decimal.TryParse(input, out decimal result)) return result;
            return defaultValue;
        }

        static decimal? GetOptionalDecimalInput(string prompt, decimal minThreshold)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (decimal.TryParse(input, out decimal result))
            {
                if (result <= minThreshold)
                {
                    Console.WriteLine($"Warning: Target Balance {result} is not greater than {minThreshold}. Ignoring limit.");
                    return null;
                }
                return result;
            }
            return null;
        }

        static int GetIntInput(string prompt, int defaultValue)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(input)) return defaultValue;
            if (int.TryParse(input, out int result)) return result;
            return defaultValue;
        }

        static string GetStringInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()!;
        }

        static void DisplayDistribution(BatchSessionResult result, decimal initialBudget)
        {
            Console.WriteLine("\n--- Profit/Loss Distribution ---");

            var bins = new[]
            {
                (Label: "Bankrupt (0)", Min: -1.0m, Max: -1.0m), // Special case 0
                (Label: "Large Loss (<-50%)", Min: -1.0m, Max: -0.5000001m),
                (Label: "Small Loss (-50% to 0%)", Min: -0.5m, Max: -0.0000001m),
                (Label: "Break-even (Initial)", Min: 0m, Max: 0m),
                (Label: "Small Profit (0% to +50%)", Min: 0.0000001m, Max: 0.5m),
                (Label: "Large Profit (+50% to +100%)", Min: 0.5000001m, Max: 1.0m),
                (Label: "Jackpot (> +100%)", Min: 1.0000001m, Max: 1000000m)
            };

            int total = result.TotalSimulations;
            foreach (var bin in bins)
            {
                int count;
                if (bin.Label == "Bankrupt (0)")
                {
                    count = result.FinalBalances.Count(b => b <= 0);
                }
                else if (bin.Label == "Break-even (Initial)")
                {
                    count = result.FinalBalances.Count(b => b == initialBudget);
                }
                else
                {
                    count = result.FinalBalances.Count(b =>
                    {
                        decimal profitPct = initialBudget == 0 ? 0 : (b - initialBudget) / initialBudget;
                        return profitPct >= bin.Min && profitPct <= bin.Max;
                    });
                }

                double pct = (double)count / total;
                string bar = new string('*', (int)(pct * 50));
                Console.WriteLine($"{bin.Label,-30} | {count,6} ({pct,7:P1}) {bar}");
            }
        }
    }
}
