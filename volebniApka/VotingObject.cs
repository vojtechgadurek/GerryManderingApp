using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;

namespace volebniApka;

using volebniApka;

public abstract class VotingObject : IVotingObject
{
    protected int id;
    public int maxMandates = 0;
    public string name;
    public Counter mandates = new Counter();
    public Counter votes = new Counter();
    public Counter leftoverVotes = new Counter();
    public IDictionary<string, Counter> counters = new Dictionary<string, Counter>();

    public VotingObject()
    {
        counters.Add("votes", votes);
        counters.Add("leftoverVotes", leftoverVotes);
        counters.Add("mandates", mandates);
    }

    private Counter GetCounterByName(string where)
    {
        return counters[where];
    }

    public void Add(string where, Counter counter)
    {
        Counter data = GetCounterByName(where);
        data += counter;
    }

    public void Set(string where, Counter counter)
    {
        Counter data = GetCounterByName(where);
        data.Set(counter);
    }

    public void Add(string where, int id, int value)
    {
        Counter data = GetCounterByName(where);
        data[id] += value;
    }

    public int Sum(string where)
    {
        Counter data = GetCounterByName(where);
        return data.Sum();
    }

    public void Set(string where, int id, int value)
    {
        Counter data = GetCounterByName(where);
        data[id] = value;
    }

    public int GetId()
    {
        return id;
    }

    public void SetMaxMandates(int maxMandates)
    {
        this.maxMandates = maxMandates;
    }

    public Counter GetVotes()
    {
        return votes;
    }

    public int GetVotes(int krajId)
    {
        return votes.Get(krajId);
    }

    public void AddVotes(int krajId, int votes)
    {
        this.votes.Add(krajId, votes);
    }

    public void SetVotes(IDictionary<int, int> stuff)
    {
        this.votes.Set(stuff);
    }

    public int SumVotes()
    {
        return votes.Sum();
    }

    public int GetMandates(int krajId)
    {
        return mandates.Get(krajId);
    }

    public void AddMandates(int krajId, int mandates)
    {
        this.mandates.Add(krajId, mandates);
    }

    public void AddMandates(Counter mandates)
    {
        this.mandates += mandates;
    }

    public void SetMandates(IDictionary<int, int> stuff)
    {
        this.mandates.Set(stuff);
    }

    public int SumMandates()
    {
        return mandates.Sum();
    }

    public int GetLeftoverVotes(int krajId)
    {
        return this.leftoverVotes.Get(krajId);
    }

    public void AddLeftoverVotes(int krajId, int votes)
    {
        this.leftoverVotes.Add(krajId, votes);
    }

    public void SetLeftoverVotes(IDictionary<int, int> stuff)
    {
        this.leftoverVotes.Set(stuff);
    }

    public int SumLeftoverVotes()
    {
        return this.leftoverVotes.Sum();
    }

    public int GetMaxMandates()
    {
        return maxMandates;
    }

    public Counter Get(string where)
    {
        return GetCounterByName(where);
    }
}

public interface IVotingObject
{
    public void Set(string where, Counter counter);
    public void Set(string where, int id, int value);
    public void Add(string where, Counter counter);
    public void Add(string where, int id, int value);
    public int GetVotes(int krajId);
    public void SetVotes(IDictionary<int, int> stuff);
    public void AddVotes(int krajId, int votes);
    public int SumVotes();
    public int GetMandates(int krajId);
    public void AddMandates(int krajId, int mandates);
    public void SetMandates(IDictionary<int, int> stuff);
    public int SumMandates();
    public int GetLeftoverVotes(int krajId);
    public void AddLeftoverVotes(int krajId, int votes);
    public void SetLeftoverVotes(IDictionary<int, int> stuff);
    public int SumLeftoverVotes();
    public void SetMaxMandates(int maxMandates);
    public int GetMaxMandates();
    public int GetId();
    public Counter Get(string where);
    public int Sum(string where);
}