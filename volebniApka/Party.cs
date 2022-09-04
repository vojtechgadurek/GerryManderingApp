namespace volebniApka;

public class Party : VotingObject
{
    public string name;
    public string leader;
    public int nCoalitionParties;
    public bool isSuccesfull;

    public Party(int id, string name, string leader, int nCoalitionParties)
    {
        this.id = id;
        this.name = name;
        this.leader = leader;
        this.nCoalitionParties = nCoalitionParties;
    }

    public void SetSuccesfullness(bool isSuccesfull)
    {
        this.isSuccesfull = isSuccesfull;
    }

    public bool GetSuccessfullness()
    {
        return isSuccesfull;
    }
}