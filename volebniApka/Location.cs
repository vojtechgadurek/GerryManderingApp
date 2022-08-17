namespace volebniApka;

public class Location
{
    private IList<int> _mapPoint;
    public bool used = false;
    public string superId;
    public string superId2;

    public Location(IList<int> mapPoint, string superId)
    {
        this.superId = superId;
        this._mapPoint = mapPoint;
        this.superId2 = superId;
    }

    public void AddSuperId(string superId)
    {
        this.superId2 = superId;
    }

    public IList<int> GetLocation()
    {
        if (used)
        {
            Console.WriteLine("Error: Location already used" + superId);
            return _mapPoint;
        }

        used = true;
        return _mapPoint;
    }
}