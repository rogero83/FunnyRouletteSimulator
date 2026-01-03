namespace Roulette.Core
{
    public enum BetType
    {
        NoBet,          // No bet placed
        Straight,       // Single number
        Split,          // 2 numbers
        Street,         // 3 numbers
        Corner,         // 4 numbers
        FiveNumber,     // 0, 00, 1, 2, 3 (American specific)
        Line,           // 6 numbers
        Column,         // 12 numbers
        Dozen,          // 12 numbers
        RedBlack,       // 18 numbers
        EvenOdd,        // 18 numbers
        LowHigh         // 18 numbers
    }
}
