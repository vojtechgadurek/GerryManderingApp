namespace volebniApka;

public class Extremes
{
    public int maxX;
    public int maxY;
    public int minX;
    public int minY;
    public Extremes(IDictionary<int, Okrsek> okrsky)
    {
        var data = okrsky.Values;
        Func<Okrsek, int, int, int> extremes = (okrsek, value, position) =>
        {
            return okrsek.status != Status.LOCAL ? value : okrsek.mapPoint[position];
        };
        maxX = data.Max(x => extremes(x, Int32.MinValue, 0));
        maxY = data.Max(x => extremes(x, Int32.MinValue, 1));
        minX = data.Min(x => extremes(x, Int32.MaxValue, 0));
        minY = data.Min(x => extremes(x, Int32.MaxValue, 1));
    }
}