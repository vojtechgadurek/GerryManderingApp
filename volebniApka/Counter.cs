using System.Collections;
using System.Diagnostics.Tracing;
using Microsoft.VisualBasic.CompilerServices;

namespace volebniApka;

public class Counter : IEnumerable<int>
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
            sum -= stuff[key];
            stuff[key] = value;
            sum += value;
        }
        else
        {
            stuff.Add(key, value);
            sum += value;
        }
    }

    public void Set(IDictionary<int, int> stuff)
    {
        this.stuff = stuff;
        sum = stuff.Sum(x => x.Value);
    }

    public void Set(Counter counter)
    {
        this.stuff = counter.stuff;
        sum = counter.sum;
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

    static public Counter operator +(Counter a, Counter b)
    {
        foreach (var data in b.stuff)
        {
            a.Add(data.Key, data.Value);
        }

        return a;
    }

    public IEnumerator<int> GetEnumerator()
    {
        return stuff.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}