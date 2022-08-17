
namespace volebniApka;


using volebniApka;

public class VotingObject
{
    public int id;
    public Counter mandates = new Counter();
    public Counter votes = new Counter();
    public Counter leftoverVotes = new Counter();
}


class Kraj:VotingObject
{
    public int color;
    public IDictionary<int,Okrsek> okrsky;
    public int nParties;
    public int maxMandates;

    public Kraj(int id, int nParties)
    {
        this.id = id;
        this.color = id;
        this.okrsky = new Dictionary<int, Okrsek>();
    }

    public void AddOkrsek(Okrsek okrsek)
    {
        if (okrsky.ContainsKey(okrsek.id))
        {
            throw new Exception("Okrsek " + okrsek.id + " already exists in Kraj " + id);
        }
        this.okrsky.Add(okrsek.id, okrsek);
        foreach (var party in okrsek.votes.stuff)
        {
            this.votes.Add(party.Key, party.Value);
        }
    }

    public void SetMandatesParties(IDictionary<int, int> mandates)
    {
        this.mandates.Set(mandates);
    }
}