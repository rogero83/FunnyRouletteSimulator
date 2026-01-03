namespace Roulette.Simulation
{
    public class SessionConfig
    {
        public decimal InitialBudget { get; }
        public int MaxSpins { get; }
        public decimal? TargetBalance { get; }

        public SessionConfig(decimal initialBudget, int maxSpins = 1000, decimal? targetBalance = null)
        {
            if (targetBalance.HasValue && targetBalance.Value <= initialBudget)
            {
                throw new System.ArgumentException("Target Balance must be greater than Initial Budget.");
            }

            InitialBudget = initialBudget;
            MaxSpins = maxSpins;
            TargetBalance = targetBalance;
        }
    }
}
