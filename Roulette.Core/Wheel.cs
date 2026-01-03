namespace Roulette.Core
{
    public class Wheel : IWheel
    {
        public const int DoubleZero = 37;
        private readonly List<Pocket> _pockets;
        private readonly bool _isAmerican;
        public IReadOnlyList<Pocket> Pockets => _pockets.AsReadOnly();

        public Wheel(bool isAmerican = true)
        {
            _isAmerican = isAmerican;
            _pockets = InitializeWheel(_isAmerican);
        }

        public Pocket Spin()
        {
            int index = Random.Shared.Next(_pockets.Count);
            return _pockets[index];
        }

        public static bool IsRed(int number)
        {
            int[] redNumbers = [1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36];
            return redNumbers.Contains(number);
        }

        private static readonly Dictionary<int, IEnumerable<int>> _dozenCache = [];
        public static IEnumerable<int> Dozen(int dozenIndex)
        {
            if (dozenIndex < 1 || dozenIndex > 3)
            {
                throw new ArgumentException("Dozen index must be 1, 2, or 3.");
            }

            if (_dozenCache.TryGetValue(dozenIndex, out var cached))
            {
                return cached;
            }

            int start = (dozenIndex - 1) * 12 + 1;
            _dozenCache[dozenIndex] = Enumerable.Range(start, 12);
            return _dozenCache[dozenIndex];
        }

        private static readonly Dictionary<int, IEnumerable<int>> _streetCache = [];

        public static bool IsValidStreetStart(int start)
        {
            return start >= 1 && start <= 34 && (start - 1) % 3 == 0;
        }

        public static IEnumerable<int> Street(int streetIndex)
        {
            if (streetIndex < 1 || streetIndex > 12)
            {
                throw new ArgumentException("Street index must be between 1 and 12.");
            }

            if (_streetCache.TryGetValue(streetIndex, out var cached))
            {
                return cached;
            }

            int start = (streetIndex - 1) * 3 + 1;
            _streetCache[streetIndex] = Enumerable.Range(start, 3);
            return _streetCache[streetIndex];
        }

        public static IEnumerable<int> StreetFromNumber(int startNumber)
        {
            if (!IsValidStreetStart(startNumber))
            {
                throw new ArgumentException($"Invalid street start number: {startNumber}. Must be 1, 4, 7...34.");
            }
            int index = (startNumber - 1) / 3 + 1;
            return Street(index);
        }

        public static IEnumerable<int> High()
        {
            return Enumerable.Range(19, 36);
        }

        public static IEnumerable<int> Low()
        {
            return Enumerable.Range(1, 18);
        }

        /// <summary>
        /// Returns a sequence of 6 integers representing the values in the specified line.
        /// </summary>
        /// <param name="lineIndex">The one-based index of the line to retrieve. Must be between 1 and 11.</param>
        /// <returns>An enumerable collection of six consecutive integers corresponding to the specified line.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="lineIndex"/> is less than 1 or greater than 11.</exception>
        public static IEnumerable<int> Line(int lineIndex)
        {
            if (lineIndex < 1 || lineIndex > 11)
            {
                throw new ArgumentException("Line index must be between 1 and 11.");
            }
            int start = (lineIndex - 1) * 3 + 1;
            return Enumerable.Range(start, 6);
        }

        private List<Pocket> InitializeWheel(bool isAmerican)
        {
            var pockets = new List<Pocket>();

            // 0 and 00 (represented optionally as 37)
            pockets.Add(new Pocket(0, Color.Green));
            if (isAmerican)
            {
                pockets.Add(new Pocket(DoubleZero, Color.Green));
            }

            for (int i = 1; i <= 36; i++)
            {
                pockets.Add(new Pocket(i, IsRed(i) ? Color.Red : Color.Black));
            }

            // Standard American order (clockwise starting from 0):
            // 0, 28, 9, 26, 30, 11, 7, 20, 32, 17, 5, 22, 34, 15, 3, 24, 36, 13, 1,
            // 00, 27, 10, 25, 29, 12, 8, 19, 31, 18, 6, 21, 33, 16, 4, 23, 35, 14, 2
            // Payouts don't depend on order, only presence. Order matters optional for 'Neighbors' bets but user didn't request that explicitly.
            // I'll keep the list simple (0, 00, 1..36) unless simulation requires 'neighbors'.
            // For now, simple list is enough for standard strategy.

            return pockets;
        }

        public static string NumberToString(int number)
        {
            return number == DoubleZero ? "00" : number.ToString();
        }

        public static Bet NoBet()
        {
            return new Bet(0, BetType.NoBet, []);
        }
    }
}
