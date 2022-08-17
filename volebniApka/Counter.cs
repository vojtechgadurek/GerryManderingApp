namespace volebniApka;

public class Counter
{
    public int sum = 0;
    public IDictionary<int, int> stuff = new Dictionary<int, int>();

    public int this[int key]
    {
        get => Get(key);
        set => SetValue(key, value);
    }

    public void SetValue(int key, int value)
    {
        if (stuff.ContainsKey(key))
        {
            stuff[key] = value;
        }
        else
        {
            stuff.Add(key, value);
        }
    } 
    public void Set(IDictionary<int, int> stuff)
    {
        this.stuff = stuff;
        sum = stuff.Sum(x => x.Value);
    }
    public void Add(int id, int number)
    {
        sum += number;
        if (stuff.ContainsKey(id))
        {
            stuff[id] += number;
        }
        else
        {
            stuff.Add(id, number);
        }
    }

    public int Get(int id)
    {
        if (stuff.ContainsKey(id))
        {
            return stuff[id];
        }
        else
        {
            return 0;
        }
    }

    public int Sum()
    {
        return sum;
    }
}