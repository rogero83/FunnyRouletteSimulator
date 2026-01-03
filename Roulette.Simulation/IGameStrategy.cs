using Roulette.Core;

namespace Roulette.Simulation
{
    public interface IGameStrategy
    {
        string Name { get; }
        /// <summary>
        /// Determine next bets based on history and current balance.
        /// </summary>
        /// <param name="history">List of past spins (latest last).</param>
        /// <param name="currentBalance">Current available chips.</param>
        /// <returns>List of bets to place.</returns>
        IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance);

        /// <summary>
        /// Resets the strategy state for a new session.
        /// </summary>
        void Reset();
    }
}
