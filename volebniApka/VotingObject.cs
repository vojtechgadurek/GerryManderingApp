
namespace volebniApka;


using volebniApka;

public class VotingObject
{
    public int id;
    public Counter mandates = new Counter();
    public Counter votes = new Counter();
    public Counter leftoverVotes = new Counter();
}
