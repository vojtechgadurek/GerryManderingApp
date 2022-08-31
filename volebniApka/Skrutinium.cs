using System.Dynamic;

namespace volebniApka;

class Skrutinium : VotingObject
{
    public int parties;
    public int kvotaNumber;
    private int kvota;
    private bool _mandateOverflowOk;
    private bool _mandateUnderflowOk;


    public Skrutinium(int mandates, int kvotaNumber, bool mandateOverflowOk, bool mandateUnderflowOk)
    {
        this.maxMandates = mandates;
        this.kvotaNumber = kvotaNumber;
        this._mandateOverflowOk = mandateOverflowOk;
        this._mandateUnderflowOk = mandateUnderflowOk;
    }

    public Skrutinium(int mandates, Counter votes, int kvotaNumber, bool mandateOverflowOk, bool mandateUnderflowOk)
    {
        this.votes = votes;
        this.maxMandates = mandates;
        this.kvotaNumber = kvotaNumber;
        this._mandateOverflowOk = mandateOverflowOk;
        this._mandateUnderflowOk = mandateUnderflowOk;
        CalculateMandates();
    }

    public void AddMaxMandates(int mandates)
    {
        this.maxMandates += mandates;
    }

    public void SetVotes(Counter votes)
    {
        this.votes = votes;
    }

    public void SetVotes(IDictionary<int, int> votes)
    {
        this.votes.Set(votes);
    }

    public void Addvotes(int key, int value)
    {
        votes.Add(key, value);
    }

    private void CalculateKvota()
    {
        kvota = votes.sum / (maxMandates + kvotaNumber);
    }

    private void CalculateMandatesParties()
    {
        foreach (var party in votes.GetStuff())
        {
            int mandatesAdd = party.Value / kvota;
            mandates.Add(party.Key, mandatesAdd);
            leftoverVotes.Add(party.Key, party.Value % kvota);
        }
    }

    public void GiveMandatesFromMost()
    {
        ///It is posssible to give more mandates, than the kraj maximum => that is feature acccoring to the law, but is not allowed and
        /// must be solved. Also we can give less mandatates => thus this also has to be solved. Sometimes we dont want to enforce, these rules. One can
        /// use _mandateOverflowOk and _mandateUnderflowOk to disable these rules.
        IOrderedEnumerable<KeyValuePair<int, int>> ordered;
        int toAdd = maxMandates - mandates.Sum();
        int add = 1;
        if (toAdd > 0 && (!_mandateUnderflowOk))
        {
            ordered = leftoverVotes.GetStuff().OrderByDescending(x => x.Value);
        }
        else if (toAdd < 0 && (!_mandateOverflowOk))
        {
            //Get rid of zero mandates parties
            ordered = leftoverVotes.GetStuff().Where(x => mandates.Get(x.Key) != 0).OrderBy(x => x.Value);

            add = -1;
        }

        else
        {
            return;
        }

        foreach (var party in ordered)
        {
            if (maxMandates - mandates.Sum() == 0)
            {
                break;
            }

            mandates.Add(party.Key, add);
        }
    }

    private void FixNotEqualMandates()
    {
        GiveMandatesFromMost();
    }

    public void CalculateMandates()
    {
        if (maxMandates > 0)
        {
            CalculateKvota();
            CalculateMandatesParties();
            FixNotEqualMandates();
        }
    }
}