using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using volebniApka;

namespace volebniApka;

public abstract class Election
{
    public int maxMandates;
    public Parties parties;
    public Kraje kraje;
    public float[] percentageNeeded;

    public Election(int maxMandates, Parties parties, Kraje kraje, float[] percentageNeeded)
    {
        this.maxMandates = maxMandates;
        this.parties = parties;
        this.kraje = kraje;
        this.percentageNeeded = percentageNeeded;
        Test();
    }

    public void MandatesToKraje() // Tady chci něco jako IVotingObjectDictionary
    {
        Skrutinium divideMandatesKraje = new Skrutinium(maxMandates, kraje.GetVotes(), 0, false, false);
        kraje.SetMaxMandates(divideMandatesKraje.Get("mandates"));
    }

    public Counter DeHont(Counter votes, int mandatesToGive)
    {
        //I need here a heap like structure to get log(n) search time for the maximum, key => number of votes / by kvota, value => party.id
        Counter mandates = new Counter();
        SortedList<int, IList<int>> votesPartiesSorted = new SortedList<int, IList<int>>();
        foreach (var party in votes.GetStuff())
        {
            //This is not acctually legitimate implementation as not being random
            try
            {
                votesPartiesSorted.Add(party.Value, new List<int>());
            }
            finally
            {
                votesPartiesSorted[party.Value].Add(party.Key);
            }

            mandates.Add(party.Key, 0);
        }

        while (mandatesToGive > 0)
        {
            ///AAAAAAA what I have done here?
            KeyValuePair<int, IList<int>> party = votesPartiesSorted.Last();
            int key = party.Value.Last();
            int value = party.Key;
            mandates[key]++;
            if (party.Value.Count <= 1)
            {
                votesPartiesSorted.Remove(value);
            }
            else
            {
                party.Value.RemoveAt(party.Value.Count - 1);
            }

            try
            {
                votesPartiesSorted.Add(votes[key] / (mandates[key] + 1), new List<int>());
            }
            catch
            {
            }
            finally
            {
                votesPartiesSorted[votes[key] / (mandates[key] + 1)].Add(key);
            }

            mandatesToGive--;
        }

        return mandates;
    }

    public abstract void RunElection();

    public void Test()
    {
        if (maxMandates < 1)
        {
            throw new ArgumentException("Max mandates must be greater than 0");
        }

        if (parties.Count() < 1)
        {
            throw new ArgumentException("There must be at least one party");
        }
    }
}

public class ElectionCz2021Ps : Election
{
    public ElectionCz2021Ps(int maxMandates, Parties parties, Kraje kraje, float[] percentageNeeded) : base(maxMandates,
        parties, kraje, percentageNeeded)
    {
    }

    public override void RunElection()
    {
        var successfulParties = parties.SuccessfulParties(percentageNeeded, true);
        ;
        ///Divides mandates to kraje to be divied between parties
        MandatesToKraje();

        ///Not all mandates are divided in first skrutinium, thus according to low, not used votes and mandates are sent to second skrutinium
        Skrutinium secondSkrutinium = new Skrutinium(maxMandates, 1, false, false);

        //There we run a skrutinium for all kraje
        foreach (Kraj kraj in kraje)
        {
            Skrutinium firstSkrutinium = new Skrutinium(kraj.maxMandates, parties.succsVotes(), 2, false, true);

            foreach (var party in successfulParties)
            {
                party.AddMandates(kraj.GetId(), firstSkrutinium.mandates[party.GetId()]);
                party.AddLeftoverVotes(kraj.GetId(), firstSkrutinium.leftoverVotes[party.GetId()]);
            }
        }

        //Now we run second skrutinium
        foreach (Party party in successfulParties)
        {
            secondSkrutinium.AddVotes(party.GetId(), party.leftoverVotes.Sum());
        }

        secondSkrutinium.SetMaxMandates(maxMandates - parties.SumMandates());

        secondSkrutinium.CalculateMandates();

        //Now we have to issue mandates from second skrutinium to party candidates with most leftover votes


        foreach (Party party in successfulParties)
        {
            Skrutinium thirdSkrutinium = new Skrutinium(secondSkrutinium.GetMandates(party.GetId()), 0, false, false);
            thirdSkrutinium.SetVotes(party.leftoverVotes);
            thirdSkrutinium.GiveMandatesFromMost();
            thirdSkrutinium.CalculateMandates();
            party.AddMandates(thirdSkrutinium.mandates);
        }
    }
}

public class ElectionCz2017Ps : Election
{
    public ElectionCz2017Ps(int maxMandates, Parties parties, Kraje kraje, float[] percentageNeeded) : base(maxMandates,
        parties, kraje, percentageNeeded)
    {
    }

    public override void RunElection()
    {
        MandatesToKraje();

        var successfulParties = parties.SuccessfulParties(percentageNeeded, true);

        foreach (Kraj kraj in kraje)
        {
            Counter votes = new Counter();
            foreach (Party party in successfulParties)
            {
                votes.Add(party.GetId(), party.GetVotes(kraj.GetId()));
            }

            Counter mandates = DeHont(votes, kraj.GetMaxMandates());
            parties.AddOver("mandates", kraj.GetId(), mandates);
        }
    }
}

public class ElectionCz2017PsRev : Election
{
    //First divide mandates between parties, than bettwen kraje 
    public ElectionCz2017PsRev(int maxMandates, Parties parties, Kraje kraje, float[] percentageNeeded) : base(
        maxMandates,
        parties, kraje, percentageNeeded)
    {
    }

    public override void RunElection()
    {
        var successfulParties = parties.SuccessfulParties(percentageNeeded, true);
        Counter partiesVotes = new Counter();
        foreach (var party in successfulParties)
        {
            partiesVotes.Add(party.GetId(), party.SumVotes());
        }

        Counter partiesMandates = DeHont(partiesVotes, maxMandates);

        foreach (var party in successfulParties)
        {
            Skrutinium skrutinium =
                new Skrutinium(partiesMandates.Get(party.GetId()), party.Get("votes"), 0, false, false);
            party.Add("mandates", skrutinium.Get("mandates"));
        }
    }
}

public class ElectionFirstPastThePost : Election
{
    //First divide mandates between parties, than bettwen kraje 
    public ElectionFirstPastThePost(int maxMandates, Parties parties, Kraje kraje, float[] percentageNeeded) : base(
        maxMandates,
        parties, kraje, percentageNeeded)
    {
    }

    public override void RunElection()
    {
        MandatesToKraje();
        foreach (var kraj in kraje)
        {
            Counter votes = kraj.Get("votes");
            var max = votes.GetStuff().Max(x => x.Value);
            int key = votes.GetStuff().First(x => x.Value == max).Key;
            parties.Add("mandates", key, kraj.GetId(), kraj.GetMaxMandates());
        }
    }
}