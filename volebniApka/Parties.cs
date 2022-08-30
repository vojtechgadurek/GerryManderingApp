using System.Dynamic;

namespace volebniApka;

public class Parties : VotingObjectGroup
{
    public Counter votes = new Counter();

    /// <summary>
    /// It is expected from votes to be unchangeable in nature 
    /// </summary>
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
        foreach (Party party in stuff.Values)
        {
            float percentage = ((float) party.votes.Sum() * 100 / (float) votes.Sum());

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
        this.Add(party);
        votes.Add(party.id, party.votes.Sum());
    }

    public void LoadDataFromKraje(Kraje kraje)
    {
        foreach (Kraj kraj in kraje)
        {
            AddOver("votes", kraj.id, kraj.votes);
            votes += kraj.Get("votes");
        }
    }
}