namespace Roulette.Core
{
    public interface IWheel
    {
        Pocket Spin();
        IReadOnlyList<Pocket> Pockets { get; }
    }
}
