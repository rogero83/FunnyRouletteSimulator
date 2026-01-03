namespace Roulette.Core
{
    public static class PayoutCalculator
    {
        public static decimal GetPayoutRatio(BetType type)
        {
            return type switch
            {
                BetType.Straight => 35m,
                BetType.Split => 17m,
                BetType.Street => 11m,
                BetType.Corner => 8m,
                BetType.FiveNumber => 6m,
                BetType.Line => 5m,
                BetType.Column => 2m,
                BetType.Dozen => 2m,
                BetType.RedBlack => 1m,
                BetType.EvenOdd => 1m,
                BetType.LowHigh => 1m,
                _ => 0m
            };
        }

        public static decimal CalculateWinnings(Bet bet, int winningNumber)
        {
            if (bet.TargetNumbers.Contains(winningNumber))
            {
                // Returns the profit (original bet is also returned by casino, but here we usually calculate profit + stake 
                // OR just profit. Standard is: you get your stake back + payout.
                // If I bet 10 on Red (1:1), I get 20 back (10 profit + 10 stake).
                // Let's return the total amount returned to player.
                decimal payoutRatio = GetPayoutRatio(bet.Type);
                return bet.Amount + (bet.Amount * payoutRatio);
            }
            return 0m;
        }

        public static decimal CalculateWinnings(IEnumerable<Bet> bet, int winningNumber)
        {
            return bet.Sum(b => CalculateWinnings(b, winningNumber));
        }
    }
}
