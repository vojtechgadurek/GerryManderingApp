using System.Dynamic;

namespace volebniApka;

using volebniApka;

public abstract class VotingObject : IVotingObject
{
    public int id;
    public int maxMandates = 0;
    public string name;
    public Counter mandates = new Counter();
    public Counter votes = new Counter();
    public Counter leftoverVotes = new Counter();


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
}

public interface IVotingObject
{
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

    public int GetId();
}