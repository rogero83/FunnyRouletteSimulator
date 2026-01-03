namespace Roulette.Core
{
    public class Bet
    {
        public decimal Amount { get; internal set; }
        public BetType Type { get; }
        /// <summary>
        /// The numbers covered by this bet.
        /// </summary>
        public HashSet<int> TargetNumbers { get; }

        public Bet(decimal amount, BetType type, IEnumerable<int> targetNumbers)
        {
            Amount = amount;
            Type = type;
            TargetNumbers = new HashSet<int>(targetNumbers);
        }

        public void ChangeAmount(decimal changeAmount)
        {
            Amount = changeAmount;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case BetType.NoBet:
                    return $"No Bet placed.";
                case BetType.Straight:
                    return $"Bet {Amount:C} on numbers: {string.Join(", ", TargetNumbers.Select(n => Wheel.NumberToString(n)))}";
                case BetType.RedBlack:
                    string color = TargetNumbers.Any(x => Wheel.IsRed(x))
                        ? "Red"
                        : "Black";
                    return $"Bet {Amount:C} on color: {color}";
                case BetType.Dozen:
                    string dozenRange = TargetNumbers switch
                    {
                        var nums when nums.SetEquals(Wheel.Dozen(1)) => "1-12",
                        var nums when nums.SetEquals(Wheel.Dozen(2)) => "13-24",
                        var nums when nums.SetEquals(Wheel.Dozen(3)) => "25-36",
                        _ => "Custom Dozen"
                    };
                    return $"Bet {Amount:C} on dozens {dozenRange}";
                case BetType.Line:
                    return $"Bet {Amount:C} on line covering numbers: {TargetNumbers.First()} {TargetNumbers.Last()}";
                case BetType.Street:
                    return $"Bet {Amount:C} on street covering numbers: {string.Join(", ", TargetNumbers)}";
                case BetType.LowHigh:
                    string range = TargetNumbers.SetEquals(Wheel.High()) ? "High (19-36)" : "Low (1-18)";
                    return $"Bet {Amount:C} on {range}";
                default:
                    return $"Bet {Amount:C} of unknown type.";
            }
        }
    }
}
