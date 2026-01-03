namespace Roulette.Core
{
    public class Pocket
    {
        public int Number { get; }
        public Color Color { get; }

        public Pocket(int number, Color color)
        {
            Number = number;
            Color = color;
        }

        public override string ToString()
        {
            return $"[{Number}, {Color}]";
        }
    }
}
