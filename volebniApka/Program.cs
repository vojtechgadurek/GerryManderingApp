// See https://aka.ms/new-console-template for mo

using System;
using System.Collections;
using System.Net;
using System.Net.Mime;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using volebniApka;


class VolebniApka
{
    //Najít Porubu => je v datech?
    //Constanty
    //Počet okrsků je 14886

    #region Constants

    const int NOkrsky = 14886 + 1;

    //Existuje 111 okrsků v zahraničí => Pozor na to META: V součastnosti ignorovány
    const int NOkrskyZahra = 111;

    //Zahraniční obce mají číslo 999997

    public const int ObceZahra = 999997;

    static HashSet<string> _specialOkrsky = new HashSet<string>()
    {
        "500054-1000",
        "532053-0",
        "544256-0",
        "554774-8000",
        "554961-0",
        "567892-1000",
        "556904-0",
        "569810-0",
        "574716-1000",
        "586846-0",
        "550990-27000",
        "500496-0",
        "585068-0",
        "546224-15000"
    };

    // META: V součastnosti ignorovány


    //Počer stran je 22

    //Covid okrsky

    #endregion

    #region Enums

    #endregion

    #region Structs

    #endregion

    #region LoadFuncs

    public static string FsuperId(string obec, string okrsek)
    {
        return obec + "-" + okrsek;
    }

    static void CheckDataGood(Okrsky okrsky, IDictionary<string, Location> locations,
        bool verbose)
    {
        okrsky.CheckDataAllHaveLocation(verbose);
        OkrskyPositions.CheckLocationsAllHaveData(locations, verbose);
    }

    #endregion

    static void CreateMap(IDictionary<int, Okrsek> votingData, int mapWidth, int mapHeight, string fileLocation)
    {
        Bitmap bitmap = new Bitmap(mapWidth + 1, mapHeight + 1);
        foreach (var okrsek in votingData)
        {
            if (okrsek.Value.status == Status.LOCAL)
            {
                bitmap.SetPixel(okrsek.Value.relativeMapPoint[0], okrsek.Value.relativeMapPoint[1],
                    //Constant for color 
                    Color.FromArgb(255, 2, 229));
            }
        }

        //Draw bitmap
        bitmap.Save(fileLocation);
    }

    static void MoveDataBetweenKrajeAndParties(Parties parties, IDictionary<int, Kraj> kraje)
    {
        foreach (var kraj in kraje)
        {
            foreach (var party in parties.stuff)
            {
                parties.addVotes(party.Key, kraj.Key, kraj.Value.votes[party.Key]);
            }
        }
    }

    static IDictionary<int, int> DeHont(IDictionary<int, int> votesParties, int mandates)
    {
        IDictionary<int, int> mandatesParties = new Dictionary<int, int>();
        SortedList<int, int> votesPartiesSorted = new SortedList<int, int>();
        foreach (var party in votesParties)
        {
            votesPartiesSorted.Add(party.Value, party.Key);
            mandatesParties.Add(party.Key, 0);
        }

        while (mandates > 0)
        {
            var party = votesPartiesSorted.Last();
            int key = party.Value;
            int value = party.Key;
            mandatesParties[key]++;
            votesPartiesSorted.Remove(value);
            votesPartiesSorted.Add(votesParties[key] / (mandatesParties[key] + 1), party.Value);
            mandates--;
        }

        return mandatesParties;
    }

    static void CalculateElectionCz2017Ps(IDictionary<int, Kraj> kraje, int mandates, Parties parties,
        float[] percentageNeeded)
    {
        //Number of mandates in each kraj
        if (mandates < 1)
        {
            throw new Exception("Error: Mandates must be greater than 0");
            return;
        }

        IDictionary<int, int> votesKraje = new Dictionary<int, int>();
        foreach (var kraj in kraje)
        {
            votesKraje.Add(kraj.Key, kraj.Value.votes.sum);
        }

        MandatesToKraje(kraje, votesKraje, mandates);

        parties.SuccessfulParties(percentageNeeded);
        IDictionary<int, Party> successfulParties = parties.succs;

        foreach (var kraj in kraje)
        {
            IDictionary<int, int> votesParties = new Dictionary<int, int>();
            foreach (var party in successfulParties)
            {
                votesParties.Add(party.Key, kraj.Value.votes[party.Key]);
            }

            IDictionary<int, int> mandatesParties = DeHont(votesParties, kraj.Value.mandates.sum);
            foreach (var party in mandatesParties)
            {
                successfulParties[party.Key].mandates.Add(kraj.Key, party.Value);
            }
        }
    }

    static void MandatesToKraje(IDictionary<int, Kraj> kraje, IDictionary<int, int> votesKraje, int mandates)
    {
        Skrutinium divideMandatesKraje = new Skrutinium(mandates, votesKraje, 0, false, false);
        foreach (var kraj in kraje)
        {
            kraj.Value.mandates.Add(kraj.Key, divideMandatesKraje.mandates[kraj.Key]);
        }
    }

    static void CalculateElectionCz2021Ps(IDictionary<int, Kraj> kraje, int mandates, Parties parties,
        float[] percentageNeeded)
    {
        //Number of mandates in each kraj
        if (mandates < 1)
        {
            throw new Exception("Error: Mandates must be greater than 0");
            return;
        }

        IDictionary<int, int> votesKraje = new Dictionary<int, int>();
        foreach (var kraj in kraje)
        {
            votesKraje.Add(kraj.Key, kraj.Value.votes.sum);
        }

        MandatesToKraje(kraje, votesKraje, mandates);

        parties.SuccessfulParties(percentageNeeded);
        IDictionary<int, Party> successfulParties = parties.succs;
        int leftoverMandates = 0;

        foreach (var kraj in kraje)
        {
            IDictionary<int, int> votesKrajSuccessfulParties = new Dictionary<int, int>();
            foreach (var party in successfulParties)
            {
                votesKrajSuccessfulParties.Add(party.Key, kraj.Value.votes[party.Key]);
            }

            Skrutinium firstSkrutinium =
                new Skrutinium(kraj.Value.mandates.sum, votesKrajSuccessfulParties, 2, false, true);
            foreach (var party in successfulParties)
            {
                party.Value.mandates.Add(kraj.Key, firstSkrutinium.mandates[party.Key]);
                party.Value.leftoverVotes.Add(kraj.Key, firstSkrutinium.leftoverVotes[party.Key]);
            }

            leftoverMandates += firstSkrutinium.maxMandates - firstSkrutinium.mandates.sum;
        }

        Skrutinium secondSkrutinium = new Skrutinium(leftoverMandates, 1, false, false);

        foreach (var party in successfulParties)
        {
            secondSkrutinium.votes.Add(party.Key, party.Value.leftoverVotes.sum);
        }

        secondSkrutinium.CalculateMandates();

        foreach (var party in successfulParties)
        {
            int partyLeftOverMandates = secondSkrutinium.mandates.Get(party.Key);
            if (partyLeftOverMandates > 0)
            {
                var leftoverVotesSorted = party.Value.leftoverVotes.stuff.OrderByDescending(x => x.Value);
                foreach (var leftoverParty in leftoverVotesSorted)
                {
                    if (partyLeftOverMandates <= 0)
                    {
                        break;
                    }

                    party.Value.mandates.Add(leftoverParty.Key, 1);
                    partyLeftOverMandates--;
                }
            }
        }
    }

    static void PrintResults(Parties parties)
    {
        Console.WriteLine(
            "-----------------------------------------------------------------------------------------------------------------------");
        Console.WriteLine("Mandates isused: " + parties.stuff.Values.Sum(x => x.mandates.sum));
        Console.WriteLine("Id\tMan.\tVotes\tSucc.\tName");
        foreach (var party in parties.stuff)
        {
            Console.WriteLine(
                $"{party.Value.id}\t{party.Value.mandates.sum}\t{party.Value.votes.sum}\t{party.Value.isSuccesfull}\t{party.Value.name}");
        }

        Console.WriteLine(
            "-----------------------------------------------------------------------------------------------------------------------");
    }


    static string ReadConfigLine(StreamReader streamReader, string test)
    {
        string line = streamReader.ReadLine();
        if (line.StartsWith(test))
        {
            return line.Split(" = ")[1];
        }
        else
        {
            throw new Exception("wrong format of config file " + line);
        }
    }

    public static void Main(string[] args)
    {
        DateTime start = DateTime.Now;
        StreamReader configFile = new StreamReader("settings.txt");
        string mapKrajeFile = ReadConfigLine(configFile, "map_file_name");
        int mapHeight = Int32.Parse(ReadConfigLine(configFile, "height"));
        int mapWidth = Int32.Parse(ReadConfigLine(configFile, "width"));
        int votingMethod = Int32.Parse(ReadConfigLine(configFile, "voting_method"));
        const int nParties = 22;
        const bool createNewData = true;
        const bool saveData = true;
        const bool verbose = false;
        const string partiesDataFile = "nazvy_stran.txt";
        const string okrskyDataFile = "pst4p.csv";
        int mandates = Int32.Parse(ReadConfigLine(configFile, "mandates"));
        const string mapDataFile = "map_data.txt";
        const string mapFile = "map.bmp";
        //float[] percentageNeeded = new float[] {20};
        float[] percentageNeeded = ReadConfigLine(configFile, "percetage_to_be_successful").Split(",")
            .Select(x => float.Parse(x)).ToArray();
        //float[] percentageNeeded = new float[] {5, 5, 8, 11};
        //float[] percentageNeeded = new flota[] {5, 5, 10, 15}
        configFile.Close();

        Parties parties = new Parties(partiesDataFile);

        IDictionary<int, Okrsek> votingData;
        Okrsky okrsky = new Okrsky(okrskyDataFile, ObceZahra, _specialOkrsky);

        if (createNewData)
        {
            IDictionary<string, Location> mapData = OkrskyPositions.Create(mapDataFile);
            votingData = okrsky.stuff;
            okrsky.ConnectData(mapData, verbose);
            CheckDataGood(okrsky, mapData, verbose);
        }

        //Find maximum positions of okrseks to draw good map
        ;

        // maxX, minX, maxY, minY
        Extremes mapExtremes = new Extremes(votingData);
        //Console.WriteLine("This are extremes: " + extremes[0] + " " + extremes[1] + " " + extremes[2] + " " + extremes[3]);
        //Draw map

        //Dodělat testovaní, že obrázky splnují rozměry.
        //Volbu generovat nové
        //Nefunguje pro rozdílné poměry => je potřeba to opravit //Opraveno 

        foreach (var okrsek in votingData.Values)
        {
            okrsek.SetRelativeLocation(mapWidth, mapHeight, mapExtremes);
        }

        //Tohle bych rád měl samostatně
        CreateMap(votingData, mapWidth, mapHeight, mapFile);

        //Open map of kraje

        Kraje krajeX = new Kraje(okrsky, mapFile);
        IDictionary<int, Kraj> kraje = krajeX.stuff;

        MoveDataBetweenKrajeAndParties(parties, kraje);

        if (votingMethod == 2021)
        {
            CalculateElectionCz2021Ps(kraje, mandates, parties, percentageNeeded);
        }
        else if (votingMethod == 2017)
        {
            CalculateElectionCz2017Ps(kraje, mandates, parties, percentageNeeded);
        }
        else
        {
            throw new Exception("Wrong voting method");
        }

        PrintResults(parties);
    }
}