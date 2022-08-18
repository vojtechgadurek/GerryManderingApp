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

    public abstract void runElection();

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

    void MandatesToKraje()
    {
        Skrutinium divideMandatesKraje = new Skrutinium(maxMandates, kraje.GetVotes(), 0, false, false);
        kraje.SetMaxMandates(divideMandatesKraje.mandates);
    }

    public override void runElection()
    {
        ///Divides mandates to kraje to be divied between parties
        MandatesToKraje();

        ///Not all mandates are divided in first skrutinium, thus according to low, not used votes and mandates are sent to second skrutinium
        Skrutinium secondSkrutinium = new Skrutinium(maxMandates, 1, false, false);

        //There we run a skrutinium for all kraje
        foreach (var kraj in kraje.stuff)
        {
            Skrutinium firstSkrutinium = new Skrutinium(kraj.Value.mandates.sum, parties.succsVotes(), 2, false, true);

            foreach (var party in parties.succs.stuff)
            {
                party.Value.AddMandates(kraj.Key, firstSkrutinium.mandates[party.Key]);
                party.Value.AddLeftoverVotes(kraj.Key, firstSkrutinium.leftoverVotes[party.Key]);
            }

            secondSkrutinium.AddMaxMandates(firstSkrutinium.maxMandates - firstSkrutinium.mandates.sum);
        }

        //Now we run second skrutinium
        secondSkrutinium.CalculateMandates();
        foreach (var party in parties.succs.stuff)
        {
            secondSkrutinium.AddVotes(party.Key, party.Value.leftoverVotes.sum);
        }

        secondSkrutinium.CalculateMandates();

        //Now we have to issue mandates from second skrutinium to party candidates with most lefover votes


        foreach (var party in parties.succs.stuff)
        {
            Skrutinium thirdSkrutinium = new Skrutinium(secondSkrutinium.GetMandates(party.Key), 0, false, false);
            thirdSkrutinium.SetVotes(party.Value.leftoverVotes);
            thirdSkrutinium.FixNotEqualMandates();
            party.Value.AddMandates(thirdSkrutinium.mandates);
        }
    }
}