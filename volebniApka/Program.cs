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

    enum DataNames
    {
        ID_OKRSKY,
        TYP_FORM,
        OPRAVA,
        CHYBA,
        OKRES,
        OBEC,
        OKRSEK,
        KC_1,
        KSTRANA,
        POC_HLASU,
    }

    #endregion

    #region Structs

    #endregion

    #region LoadFuncs

    public static string FsuperId(string obec, string okrsek)
    {
        return obec + "-" + okrsek;
    }

    static IDictionary<int, Okrsek> CreateOkrskyData(string fileLocation, int nParties)
    {
        FileStream votingDataStream = File.Open(fileLocation, FileMode.Open);
        StreamReader votingDataReader = new StreamReader(votingDataStream);
        IDictionary<int, Okrsek> votingData = new Dictionary<int, Okrsek>();


        //Načtení dat z CSV souboru

        string line = votingDataReader.ReadLine(); //Vynechá hlavičku
        while ((line = votingDataReader.ReadLine()) != null)
        {
            string[] data = line.Split(',');
            int id = int.Parse(data[(int) DataNames.ID_OKRSKY]);
            int party = int.Parse(data[(int) DataNames.KSTRANA]);
            int votes = int.Parse(data[(int) DataNames.POC_HLASU]);
            int obec = int.Parse(data[(int) DataNames.OBEC]);
            int okrsek = int.Parse(data[(int) DataNames.OKRSEK]);

            if (!votingData.ContainsKey(id))
            {
                votingData.Add(id, new Okrsek(id, obec, okrsek, ObceZahra, _specialOkrsky));
            }

            votingData[id].votes.Add(party, votes);
        }

        votingDataReader.Close();
        votingDataStream.Close();
        
        return votingData;
    }

    static IDictionary<string, Location> CreateOkrskyMapData(string mapDataFile)
    {
        //open map_data.txt file
        // Data are in format
        // Position
        // Okrsek
        // Obec
        // Obec2(optional)
        // Freespace "      "
        const string freespace = "      ";
        IDictionary<string, Location> mapData = new Dictionary<string, Location>();

        FileStream mapDataStream = File.Open(mapDataFile, FileMode.Open);
        StreamReader mapDataReader = new StreamReader(mapDataStream);

        string positionString;
        int counter = 0;
        while ((positionString = mapDataReader.ReadLine()) != null)
        {
            string okrsek = mapDataReader.ReadLine();
            string obec = mapDataReader.ReadLine();
            string obec2 = mapDataReader.ReadLine();

            bool obec2Exists = false; //optional
            if (obec2 != freespace)
            {
                obec2Exists = true;
                mapDataReader.ReadLine();
            }

            string superId = FsuperId(obec, okrsek);

            string[] position = positionString.Split(' ');

            IList<int> positionXy;

            try
            {
                positionXy =
                    new List<int>{(int) float.Parse(position[0]), (int) float.Parse(position[1])};
            }
            catch (Exception)
            {
                throw new Exception("Error in map_data.txt file, wrong position format " + positionString + " " +
                                    okrsek + " " + obec + " " + obec2 + " counter " + counter);
            }



            mapData.Add(superId, new Location(positionXy, superId));


            if (obec2Exists)
            {
                string superId2 = FsuperId(obec2, okrsek);
                mapData.Add(superId2, new Location(positionXy, superId));
                mapData[superId].AddSuperId(superId2);
            }

            counter++;
        }

        mapDataReader.Close();


        return mapData;
    }

    static IDictionary<int, Okrsek> ConnectData(IDictionary<int, Okrsek> votingData,
        IDictionary<string, Location> mapData, bool verbose)
    {
        //Connect data
        foreach (var okrsek in votingData.Values)
        {
            if (okrsek != null)
            {
                string superId = okrsek.superId.name;
                if (mapData.ContainsKey(superId))
                {
                    okrsek.AddLocation(mapData[superId].GetLocation());
                }
                else
                {
                    if (verbose)
                    {
                        Console.WriteLine("Error: Location not found " + superId);
                    }

                    okrsek.status = Status.NOT_FOUND;
                }
            }
        }

        return votingData;
    }

    static void CheckDataAllHaveLocation(IDictionary<int, Okrsek> votingData, bool verbose)
    {
        IList<string> missingLocation = new List<string>();
        int counter = 0;
        foreach (var okrsek in votingData.Values)
        {
            if (okrsek != null)
            {
                if (okrsek.status == Status.NOT_FOUND)
                {
                    counter++;
                    missingLocation.Add("Error: Okrsek " + okrsek.id + " " + okrsek.obec + " " + okrsek.okrsek +
                                        " do not have location");
                }
            }
        }

        if (verbose)
        {
            foreach (var missing in missingLocation)
            {
                Console.WriteLine(missing);
            }
        }

        Console.WriteLine("Missing locations: " + counter + " out of " + votingData.Count);
    }

    static void CheckLocationsAllHaveData(IDictionary<string, Location> locations, bool verbose)
    {
        int counterN = 0;
        int counter = 0;
        foreach (var location in locations.Values)
        {
            counterN++;
            if (!location.used)
            {
                if (verbose)
                {
                    Console.WriteLine("Error: Location " + location.superId + " " + location.superId2 + "  not used");
                }

                counter++;
            }
        }

        Console.WriteLine("Locations not used: " + counter + " of " + counterN);
    }

    static void CheckDataGood(IDictionary<int, Okrsek> votingData, IDictionary<string, Location> locations,
        bool verbose)
    {
        CheckDataAllHaveLocation(votingData, verbose);
        CheckLocationsAllHaveData(locations, verbose);
    }

    #endregion

    static void CreateMap(IDictionary<int, Okrsek> votingData, int mapWidth, int mapHeight, string fileLocation)
    {
        Bitmap bitmap = new Bitmap(mapWidth + 1, mapHeight + 1);
        foreach (var okrsek in votingData){
            
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

    static IDictionary<int, Party> SuccessfulParties(IDictionary<int, Party> parties, IList<float> percentageNeeded,
        int allVotes)
    {
        IDictionary<int, Party> successfulParties = new Dictionary<int, Party>();
        foreach (var party in parties.Values)
        {
            float percentage = ((float) party.votes.sum * 100 / (float) allVotes);

            int bracket = party.nCoalitionParties;
            if (bracket >= percentageNeeded.Count)
            {
                bracket = percentageNeeded.Count - 1;
            }

            if (percentage < percentageNeeded[bracket])
            {
                party.isSuccesfull = false;
            }
            else
            {
                party.isSuccesfull = true;
                successfulParties.Add(party.id, party);
            }
        }

        return successfulParties;
    }

    static void MoveDataBetweenKrajeAndParties(Parties parties, IDictionary<int, Kraj> kraje)
    {
        foreach (var kraj in kraje)
        {

            foreach (var party in parties.stuff)
            {
                party.Value.votes.Add(kraj.Key, kraj.Value.votes[party.Key]);
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

        IDictionary<int, Party> successfulParties =
            SuccessfulParties(parties.stuff, percentageNeeded, /*Do I really need this */votesKraje.Values.Sum());

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
        Skrutinium divideMandatesKraje = new Skrutinium( mandates, votesKraje, 0, false, false);
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

        IDictionary<int, Party> successfulParties =
            SuccessfulParties(parties.stuff, percentageNeeded, votesKraje.Values.Sum());

        int leftoverMandates = 0;


        foreach (var kraj in kraje)
        {
            IDictionary<int, int> votesKrajSuccessfulParties = new Dictionary<int, int>();
            foreach (var party in successfulParties)
            {
                votesKrajSuccessfulParties.Add(party.Key, kraj.Value.votes.stuff[party.Key]);
            }

            Skrutinium firstSkrutinium =
                new Skrutinium( kraj.Value.mandates.sum, votesKrajSuccessfulParties, 2, false, true);
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
            if ( partyLeftOverMandates > 0)
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


    static IDictionary<int, Kraj> CreateKrajData(IDictionary<int, Okrsek> votingData, int nParties, string mapFile)
    {
        Bitmap mapKraje = new Bitmap(mapFile);


        IDictionary<int, Kraj> kraje = new Dictionary<int, Kraj>();


        for (int i = 1; i < NOkrsky; i++)
        {
            if (votingData[i].status == Status.LOCAL)
            {

                int kraj = mapKraje
                    .GetPixel(votingData[i].relativeMapPoint[0], votingData[i].relativeMapPoint[1]).R;
                if (!kraje.ContainsKey(kraj))
                {
                    kraje.Add(kraj, new Kraj(kraj, nParties));
                }

                kraje[kraj].AddOkrsek(votingData[i]);
            }
        }

        return kraje;
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

        IDictionary<int,Okrsek> votingData;

        if (createNewData)
        {
            IDictionary<string, Location> mapData = CreateOkrskyMapData(mapDataFile);
            votingData = CreateOkrskyData(okrskyDataFile, nParties);
            ConnectData(votingData, mapData, verbose);
            CheckDataGood(votingData, mapData, verbose);
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

        IDictionary<int, Kraj> kraje = CreateKrajData(votingData, nParties, mapKrajeFile);

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
    