using Roulette.Core;

namespace Roulette.Simulation
{
    public class SpinResult
    {
        public Pocket WinningPocket { get; }
        public decimal AmountBet { get; }
        public decimal AmountWon { get; }
        public decimal BalanceAfter { get; }
        public IEnumerable<Bet> Bets { get; }

        public SpinResult(Pocket winningPocket, decimal amountBet, decimal amountWon, decimal balanceAfter, IEnumerable<Bet> bets)
        {
            WinningPocket = winningPocket;
            AmountBet = amountBet;
            AmountWon = amountWon;
            BalanceAfter = balanceAfter;
            Bets = bets;
        }

        public override string ToString()
        {
            return $"[{WinningPocket}] \tBet: {AmountBet:C}, \tWon: {AmountWon:C}, \tBal: {BalanceAfter:C} \t{string.Join(",", Bets)}";
        }
    }
}
