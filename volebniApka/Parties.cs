namespace volebniApka;

public class Parties:VotingObject {
    public IDictionary<int, Party> stuff = new Dictionary<int, Party>();
    public IDictionary<int, Party> succs;

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
    public void addVotes(int first, int second, int votes)
    {
        stuff[first].votes.Add(second, votes);
        this.votes[second] += votes;
    }
    public void SuccessfulParties( IList<float> percentageNeeded)
    {
        succs = new Dictionary<int, Party>();
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
                party.SetSuccesfullness(false);
            }
            else
            {
                party.SetSuccesfullness(true);
                succs.Add(party.id, party);
            }
        }
    }

}