namespace Roulette.Simulation
{
    public class SessionResult
    {
        public decimal InitialBudget { get; }
        public decimal FinalBalance { get; set; }
        public int TotalSpins { get; set; }
        public bool ReachedTarget { get; set; }
        public List<SpinResult> History { get; } = new List<SpinResult>();

        public SessionResult(decimal initialBudget)
        {
            InitialBudget = initialBudget;
            FinalBalance = initialBudget;
        }
    }
}
