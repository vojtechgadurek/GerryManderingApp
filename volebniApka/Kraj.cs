namespace volebniApka;

public class Kraj : VotingObject
{
    public int color;
    public IDictionary<int, Okrsek> okrsky;

    public Kraj(int id)
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
        foreach (var party in okrsek.votes.GetStuff())
        {
            this.votes.Add(party.Key, party.Value);
        }
    }

    public void SetMandatesParties(IDictionary<int, int> mandates)
    {
        this.mandates.Set(mandates);
    }
}