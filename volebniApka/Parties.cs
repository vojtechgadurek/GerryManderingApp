namespace volebniApka;

public class Parties
{
    public Counter votes = new Counter();

    /// <summary>
    /// It is expected from votes to be unchangeble in nature 
    /// </summary>
    public IDictionary<int, Party> stuff = new Dictionary<int, Party>();

    public Parties succs;

    public Parties()
    {
    }

    public Parties(string fileLocation)
    {
        StreamReader file = new StreamReader(fileLocation);
        string line;

        while ((line = file.ReadLine()) != null)
        {
            string[] parts = line.Split("\t");
            stuff.Add(Int32.Parse(parts[0]),
                new Party(Int32.Parse(parts[0]), parts[1], parts[2], Int32.Parse(parts[3])));
        }

        file.Close();
    }

    public void AddVotes(int first, int second, int votes)
    {
        stuff[first].AddVotes(second, votes);
        this.votes[second] += votes;
    }

    public Parties SuccessfulParties(IList<float> percentageNeeded, bool setSuccefulness)
    {
        succs = new Parties();
        foreach (var party in stuff.Values)
        {
            float percentage = ((float) party.votes.sum * 100 / (float) votes.sum);

            int bracket = party.nCoalitionParties;

            /// The first 0 collum will be applied for all parties over
            if (bracket >= percentageNeeded.Count)
            {
                bracket = 0;
            }

            if (percentage < percentageNeeded[bracket])
            {
                if (setSuccefulness)
                {
                    party.SetSuccesfullness(false);
                }
            }
            else
            {
                if (setSuccefulness)
                {
                    party.SetSuccesfullness(true);
                }

                succs.AddParty(party);
            }
        }

        return succs;
    }

    public Counter succsVotes()
    {
        return succs.votes;
    }

    public void AddParty(Party party)
    {
        stuff.Add(party.id, party);
        votes.Add(party.id, party.votes.sum);
    }

    public void LoadDataFromKraje(Kraje kraje)
    {
        foreach (var kraj in kraje.stuff)
        {
            foreach (var party in stuff)
            {
                AddVotes(party.Key, kraj.Key, kraj.Value.votes[party.Key]);
            }
        }
    }
}