using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO.Compression;
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
        parties.SuccessfulParties(percentageNeeded, true);
        Test();
    }

    public void MandatesToKraje() // Tady chci něco jako IVotingObjectDictionary
    {
        Skrutinium divideMandatesKraje = new Skrutinium(maxMandates, kraje.GetVotes(), 0, false, false);
        kraje.SetMaxMandates(divideMandatesKraje.mandates);
    }

    public Counter DeHont(Counter votes, int mandatesToGive)
    {
        Counter mandates = new Counter();
        SortedList<int, int> votesPartiesSorted = new SortedList<int, int>();
        foreach (var party in votes.stuff)
        {
            votesPartiesSorted.Add(party.Value, party.Key);
            mandates.Add(party.Key, 0);
        }

        while (mandatesToGive > 0)
        {
            var party = votesPartiesSorted.Last();
            int key = party.Value;
            int value = party.Key;
            mandates[key]++;
            votesPartiesSorted.Remove(value);
            votesPartiesSorted.Add(votes[key] / (mandates[key] + 1), party.Value);
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

        if (parties.stuff.Count < 1)
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
        var successfulParties = parties.succs;
        ///Divides mandates to kraje to be divied between parties
        MandatesToKraje();

        ///Not all mandates are divided in first skrutinium, thus according to low, not used votes and mandates are sent to second skrutinium
        Skrutinium secondSkrutinium = new Skrutinium(maxMandates, 1, false, false);

        //There we run a skrutinium for all kraje
        foreach (var kraj in kraje.stuff)
        {
            Skrutinium firstSkrutinium = new Skrutinium(kraj.Value.maxMandates, parties.succsVotes(), 2, false, true);

            foreach (var party in successfulParties.stuff)
            {
                party.Value.AddMandates(kraj.Key, firstSkrutinium.mandates[party.Key]);
                party.Value.AddLeftoverVotes(kraj.Key, firstSkrutinium.leftoverVotes[party.Key]);
            }

            secondSkrutinium.AddMaxMandates(firstSkrutinium.maxMandates - firstSkrutinium.mandates.sum);
        }

        //Now we run second skrutinium
        secondSkrutinium.CalculateMandates();
        foreach (var party in successfulParties.stuff)
        {
            secondSkrutinium.AddVotes(party.Key, party.Value.leftoverVotes.sum);
        }

        secondSkrutinium.CalculateMandates();

        //Now we have to issue mandates from second skrutinium to party candidates with most lefover votes


        foreach (var party in successfulParties.stuff)
        {
            Skrutinium thirdSkrutinium = new Skrutinium(secondSkrutinium.GetMandates(party.Key), 0, false, false);
            thirdSkrutinium.SetVotes(party.Value.leftoverVotes);
            thirdSkrutinium.GiveMandatesFromMost();
            party.Value.AddMandates(thirdSkrutinium.mandates);
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

        var successfulParties = parties.succs;

        foreach (var kraj in kraje.stuff)
        {
            Counter mandates = DeHont(kraj.Value.GetVotes(), kraj.Value.SumMandates());
            parties.AddMandates(mandates);
        }
    }
}

public class ElectionCz2017PsRev : Election
{
    //Dividest first mandates between parties and between kraje //In Development
    public ElectionCz2017PsRev(int maxMandates, Parties parties, Kraje kraje, float[] percentageNeeded) : base(
        maxMandates,
        parties, kraje, percentageNeeded)
    {
    }

    public override void RunElection()
    {
        MandatesToKraje();

        var successfulParties = parties.succs;

        foreach (var kraj in kraje.stuff)
        {
            foreach (var party in DeHont(kraj.Value.GetVotes(), kraj.Value.SumMandates()).stuff)
            {
                parties.stuff[party.Key].AddMandates(kraj.Key, party.Value);
            }
        }
    }
}