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

    static void CheckDataGood(Okrsky okrsky, IDictionary<string, Location> locations,
        bool verbose)
    {
        okrsky.CheckDataAllHaveLocation(verbose);
        OkrskyPositions.CheckLocationsAllHaveData(locations, verbose);
    }

    #endregion

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
        Map.CreateMap(votingData, mapWidth, mapHeight, mapFile);

        //Open map of kraje

        Kraje kraje = new Kraje(okrsky, mapFile);

        parties.LoadDataFromKraje(kraje);

        Election election;
        if (votingMethod == 2021)
        {
            election = new ElectionCz2021Ps(mandates, parties, kraje, percentageNeeded);
        }
        else if (votingMethod == 2017)
        {
            election = new ElectionCz2017Ps(mandates, parties, kraje, percentageNeeded);
        }
        else
        {
            throw new Exception("Wrong voting method");
        }

        election.RunElection();
        PrintResults(parties);
    }
}