using System.Collections;
using System.Net;

namespace volebniApka;

public abstract class VotingObjectGroup : IEnumerable<IVotingObject>
{
    public IDictionary<int, IVotingObject> stuff = new Dictionary<int, IVotingObject>();

    protected void PushNewStuff(IDictionary<int, IVotingObject> stuff)
    {
        this.stuff = stuff;
    }

    public void SetMaxMandates(Counter mandates)
    {
        foreach (var mandate in mandates.stuff)
        {
            stuff[mandate.Key].SetMaxMandates(mandate.Value);
        }
    }

    public Counter GetVotes()
    {
        Counter votes = new Counter();
        foreach (var votingObject in stuff)
        {
            votes.Add(votingObject.Key, votingObject.Value.SumVotes());
        }

        return votes;
    }


    public void Add(IVotingObject votingObject)
    {
        stuff.Add(votingObject.GetId(), votingObject);
    }

    public void AddOver(string where, int id, Counter counter)
    {
        /// will add over all at same place
        foreach (var votingObject in stuff)
        {
            votingObject.Value.Add(where, id, counter[votingObject.Key]);
        }
    }


    public void AddTo(string where, int id, Counter counter)
    {
        /// will add to specific object all at it
        stuff[id].Add(where, id, counter[id]);
    }

    public void SetOver(string where, int id, Counter counter)
    {
        /// will add over all at same place
        foreach (var votingObject in stuff)
        {
            votingObject.Value.Set(where, id, counter[votingObject.Key]);
        }
    }


    public void SetTo(string where, int id, Counter counter)
    {
        /// will add to specific object all at it
        stuff[id].Set(where, id, counter[id]);
    }

    public int SumMandates()
    {
        return stuff.Sum(x => x.Value.SumMandates());
    }

    public IDictionary<int, IVotingObject> GetStuff()
    {
        return stuff;
    }

    public IEnumerator<IVotingObject> GetEnumerator()
    {
        var enumerator = stuff.Values.GetEnumerator();
        return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(string where, int idX, int idY, int votes)
    {
        stuff[idX].Add(where, idY, votes);
    }
}