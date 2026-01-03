namespace Roulette.Simulation
{
    public class BatchSessionResult
    {
        public int TotalSimulations { get; set; }
        public int SuccessfulSessions { get; set; }
        public int SessionsReachedTarget { get; set; }
        public int BankruptSessions { get; set; }
        public decimal AverageFinalBalance { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal BestSessionBalance { get; set; }
        public decimal WorstSessionBalance { get; set; }
        public List<decimal> FinalBalances { get; } = new List<decimal>();

        public BatchSessionResult()
        {
            BestSessionBalance = decimal.MinValue;
            WorstSessionBalance = decimal.MaxValue;
        }
    }
}
