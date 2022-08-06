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


class VotingSystems
{
    //Najít Porubu => je v datech?
    //Constanty
    //Počet okrsků je 14886

    #region Constants


    const int NOkrsky = 14886 + 1;

    //Existuje 111 okrsků v zahraničí => Pozor na to META: V součastnosti ignorovány
    const int NOkrskyZahra = 111;

    //Zahraniční obce mají číslo 999997

    const int ObceZahra = 999997;

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
    const int NParties = 22;

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

    enum Status
    {
        LOCAL,
        ZAHRA,
        SPECIAL,
        NOT_FOUND,
    }

    #endregion

    #region Structs

    class Party
    {
        public int id;
        public string name;
        public string leader;
        public bool coalition;
        public int nCoalitionParties;
        public int mandates = 0;
        public int allvotes = 0;
        public IDictionary<int, int> mandatesKraj = new Dictionary<int, int>();
        public IDictionary<int, int> votesKraj = new Dictionary<int, int>();
        public IDictionary<int, int> leftoverVotesKraj = new Dictionary<int, int>();
        public bool isSuccesfull;

        public Party(int id, string name, string leader, int nCoalitionParties, bool coalition)
        {
            this.id = id;
            this.name = name;
            this.leader = leader;
            this.nCoalitionParties = nCoalitionParties;
            this.coalition = coalition;
        }
    }

    class Kraj
    {
        public int id;
        public int color;
        public IList<Okrsek> okrsky;
        public int nParties;
        public IDictionary<int, int> votesParties;
        public IDictionary<int, int> mandatesParties;
        public int sumVotes = 0;
        public int mandates;

        public Kraj(int id, int nParties)
        {
            this.id = id;
            this.color = id;
            this.okrsky = new List<Okrsek>();
            this.votesParties = new Dictionary<int, int>();
            this.mandatesParties = new Dictionary<int, int>();
            this.nParties = nParties;
        }

        public void AddOkrsek(Okrsek okrsek)
        {
            this.okrsky.Add(okrsek);
            foreach (var party in okrsek.votesParties)
            {
                if (this.votesParties.ContainsKey(party.Key))
                {
                    this.votesParties[party.Key] += party.Value;
                }
                else
                {
                    this.votesParties.Add(party.Key, party.Value);
                }

                this.sumVotes += party.Value;
            }
        }

        public void AddMandates(int mandates)
        {
            this.mandates = mandates;
        }

        public void AddMandatesParties(IDictionary<int, int> mandatesParties)
        {
            this.mandatesParties = mandatesParties;
        }

    }

    class Okrsek
    {
        public int id;
        public int obec;
        public int okrsek;
        public Status status = Status.NOT_FOUND;
        public Tuple<int, int> mapPoint;
        public Tuple<int, int> relativeMapPoint;
        public IDictionary<int, int> votesParties = new Dictionary<int, int>();
        public string superId;
        public int krajId;
        public int color;
        public int nParties;

        private void DetermineStatus()
        {
            if (obec == ObceZahra)
            {
                status = Status.ZAHRA;
            }
            else if (_specialOkrsky.Contains(superId))
            {
                status = Status.SPECIAL;
            }
        }


        public Okrsek(int id, int obec, int okrsek, int nParties)
        {
            this.id = id;
            this.obec = obec;
            this.okrsek = okrsek;
            this.superId = FsuperId(obec.ToString(), okrsek.ToString());
            this.nParties = nParties;
            DetermineStatus();
        }

        public bool AddLocation(Tuple<int, int> mapPoint)
        {
            if (this.status == Status.NOT_FOUND)
            {
                this.mapPoint = mapPoint;
                this.status = Status.LOCAL;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddVote(int party, int votes)
        {
            if (votesParties.ContainsKey(party))
            {
                votesParties[party] += votes;
            }
            else
            {
                votesParties.Add(party, votes);
            }
        }

        public void SetRelativeLocation(int width, int height, int[] extremes)
        {
            if (this.status == Status.LOCAL)
            {
                int widthEx = extremes[1] - extremes[0];
                int heightEx = extremes[3] - extremes[2];

                int divider;

                if (heightEx / height < widthEx / width)
                {
                    divider = heightEx;
                }
                else
                {
                    divider = widthEx;
                }

                this.relativeMapPoint = new Tuple<int, int>(
                    width - (mapPoint.Item1 - extremes[0]) * width / divider,
                    (mapPoint.Item2 - extremes[2]) * height / divider);
            }
        }

        public void checkAllDeclared()
        {
            for (int i = 1; i < nParties + 1; i++)
            {
                if (!votesParties.ContainsKey(i))
                {
                    votesParties.Add(i, 0);
                }
            }
        }
    }

    class Location
    {
        private Tuple<int, int> _mapPoint;
        public bool used = false;
        public string superId;
        public string superId2;

        public Location(Tuple<int, int> mapPoint, string superId)
        {
            this.superId = superId;
            this._mapPoint = mapPoint;
            this.superId2 = superId;
        }

        public void AddSuperId(string superId)
        {
            this.superId2 = superId;
        }

        public Tuple<int, int> GetLocation()
        {
            if (used)
            {
                Console.WriteLine("Error: Location already used" + superId);
                return _mapPoint;
            }

            used = true;
            return _mapPoint;
        }
    }

    #endregion

    #region LoadFuncs

    static IDictionary<int, Party> LoadParties(string fileLocation)
    {
        StreamReader file = new StreamReader(fileLocation);
        string line;
        IDictionary<int, Party> parties = new Dictionary<int, Party>();
        while ((line = file.ReadLine()) != null)
        {
            string[] parts = line.Split("\t");
            parties.Add(Int32.Parse(parts[0]),
                new Party(Int32.Parse(parts[0]), parts[1], parts[2], Int32.Parse(parts[3]), parts[3] == "1"));
        }

        file.Close();

        return parties;
    }

    static string FsuperId(string obec, string okrsek)
    {
        return obec + "-" + okrsek;
    }

    static Okrsek[] CreateOkrskyData(string fileLocation)
    {
        FileStream votingDataStream = File.Open(fileLocation, FileMode.Open);
        StreamReader votingDataReader = new StreamReader(votingDataStream);

        int maxId = NOkrsky + 1;
        Okrsek[] votingData = new Okrsek[maxId];


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

            if (id > maxId)
            {
                Console.WriteLine("Chyba: ID okrseku je větší než počet okrsků");
                return null;
            }

            if (party > NParties)
            {
                Console.WriteLine("Chyba: Kód strany je větší než počet stran");
                return null;
            }

            if (votingData[id] == null)
            {
                votingData[id] = new Okrsek(id, obec, okrsek, NParties);
            }

            votingData[id].AddVote(party, votes);
        }

        votingDataReader.Close();
        votingDataStream.Close();

        for (int i = 1; i < NOkrsky; i++)
        {
            votingData[i].checkAllDeclared();
        }

        return votingData;
    }

    static IDictionary<string, int> CreateTranslatorSuperIdTo(Okrsek[] votingData)
    {
        IDictionary<string, int> translator = new Dictionary<string, int>();
        for (int i = 1; i < votingData.Length; i++)
        {
            if (votingData[i] != null)
            {
                translator.Add(votingData[i].superId, i);
            }
        }

        return translator;
    }


    static IDictionary<string, Location> CreateOkrskyMapData()
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

        FileStream mapDataStream = File.Open("map_data.txt", FileMode.Open);
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

            Tuple<int, int> positionXy;

            try
            {
                positionXy =
                    new Tuple<int, int>((int) float.Parse(position[0]), (int) float.Parse(position[1]));
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

    static Okrsek[] ConnectData(Okrsek[] votingData, IDictionary<string, Location> mapData, bool verbose)
    {
        //Connect data
        for (int i = 1; i < votingData.Length; i++)
        {
            if (votingData[i] != null)
            {
                string superId = votingData[i].superId;
                if (mapData.ContainsKey(superId))
                {
                    votingData[i].AddLocation(mapData[superId].GetLocation());
                }
                else
                {
                    if (verbose)
                    {
                        Console.WriteLine("Error: Location not found " + superId);
                    }

                    votingData[i].status = Status.NOT_FOUND;
                }
            }
        }

        return votingData;
    }

    static void CheckDataAllHaveLocation(Okrsek[] votingData, bool verbose)
    {
        IList<string> missingLocation = new List<string>();
        int counter = 0;
        foreach (var okrsek in votingData)
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

        Console.WriteLine("Missing locations: " + counter + " out of " + votingData.Length);
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

    static void CheckDataGood(Okrsek[] votingData, IDictionary<string, Location> locations, bool verbose)
    {
        CheckDataAllHaveLocation(votingData, verbose);
        CheckLocationsAllHaveData(locations, verbose);
    }

    #endregion

    static int[] FindExtremes(Okrsek[] votingData)
    {
        int maxX = votingData[1].mapPoint.Item1;
        int minX = votingData[1].mapPoint.Item1;
        int maxY = votingData[1].mapPoint.Item2;
        int minY = votingData[1].mapPoint.Item2;
        for (int i = 1; i < NOkrsky; i++)
        {
            if (votingData[i].status == Status.LOCAL)
            {
                if (votingData[i].mapPoint.Item1 > maxX)
                {
                    maxX = votingData[i].mapPoint.Item1;
                }

                if (votingData[i].mapPoint.Item1 < minX)
                {
                    minX = votingData[i].mapPoint.Item1;
                }

                if (votingData[i].mapPoint.Item2 > maxY)
                {
                    maxY = votingData[i].mapPoint.Item2;
                }

                if (votingData[i].mapPoint.Item2 < minY)
                {
                    minY = votingData[i].mapPoint.Item2;
                }
            }
        }

        int[] extremes = new int[4] {maxX, minX, maxY, minY};
        return extremes;
    }

    static void CreateMap(Okrsek[] votingData, int mapWidth, int mapHeight, string fileLocation)
    {
        Bitmap bitmap = new Bitmap(mapWidth + 1, mapHeight + 1);

        for (int i = 1; i < NOkrsky; i++)
        {
            if (votingData[i].status == Status.LOCAL)
            {
                bitmap.SetPixel(votingData[i].relativeMapPoint.Item1, votingData[i].relativeMapPoint.Item2,
                    Color.FromArgb(255, 2, 229));
            }
        }

        //Draw bitmap
        bitmap.Save(fileLocation);
    }


    class Skrutinium
    {
        public int parties;
        public IDictionary<int, int> votes;
        public int mandates;
        public int leftOverMandates;
        public IDictionary<int, int> mandatesParties;
        public IDictionary<int, int> leftOverVotes;
        public int kvotaNumber;
        public int kvota;
        public int allVotes;
        public int mandatesUsed;
        private bool _mandateOverflowOk;
        private bool _mandateUnderflowOk;

        public Skrutinium(IDictionary<int, int> votes, int mandates, int kvotaNumber, bool mandateOverflowOk,
            bool mandateUnderflowOk)
        {
            this.votes = votes;
            this.mandates = mandates;
            this.kvotaNumber = kvotaNumber;
            this._mandateOverflowOk = mandateOverflowOk;
            this._mandateUnderflowOk = mandateUnderflowOk;
            CalculateMandates();
        }


        private void CalculateKvota()
        {
            allVotes = votes.Values.Sum();
            kvota = allVotes / mandates + kvotaNumber;
        }

        private void CalculateMandatesParties()
        {
            mandatesParties = new Dictionary<int, int>();
            leftOverVotes = new Dictionary<int, int>();
            foreach (var party in votes)
            {
                mandatesParties.Add(party.Key, party.Value / kvota);
                leftOverVotes.Add(party.Key, party.Value % kvota);

            }
        }

        private void FixNotEqualMandates()
        {
            int toAdd = mandates - mandatesUsed;
            if (toAdd > 0 && (!_mandateUnderflowOk))
            {
                leftOverVotes.OrderByDescending(x => x.Value);
            }
            else if (toAdd < 0 && (!_mandateOverflowOk))
            {
                leftOverVotes.OrderBy(x => x.Value);
                toAdd = -toAdd;
            }

            else
            {
                return;
            }

            foreach (var party in leftOverVotes)
            {
                if (toAdd <= 0)
                {
                    break;
                }

                mandatesParties[party.Key]++;
                toAdd--;
            }
        }

        public void CalculateMandates()
        {
            CalculateKvota();
            CalculateMandatesParties();
            mandatesUsed = mandatesParties.Values.Sum();
            FixNotEqualMandates();
        }
    }

    static IDictionary<int, Party> SuccessfulParties(IDictionary<int, Party> parties, IList<float> percentageNeeded,
        int allVotes)
    {
        IDictionary<int, Party> successfulParties = new Dictionary<int, Party>();
        foreach (var party in parties.Values)
        {
            float percentage = ((float) party.allvotes * 100 / (float) allVotes);

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

    static void MoveDataBetweenKrajeAndParties(IDictionary<int, Party> parties, IDictionary<int, Kraj> kraje)
    {
        foreach (var kraj in kraje)
        {

            foreach (var party in parties)
            {
                party.Value.votesKraj.Add(kraj.Key, kraj.Value.votesParties[party.Key]);
            }
        }

        foreach (var party in parties)
        {
            party.Value.allvotes = party.Value.votesKraj.Values.Sum();
        }
    }

    static void CalculateElectionCz2017Ps(IDictionary<int, Kraj> kraje, int mandates, IDictionary<int, Party> parties,
        float[] percentageNeeded)
    {

    }

    static void CalculateElectionCz2021Ps(IDictionary<int, Kraj> kraje, int mandates, IDictionary<int, Party> parties,
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
            votesKraje.Add(kraj.Key, kraj.Value.sumVotes);
        }

        Skrutinium divideMandatesKraje = new Skrutinium(votesKraje, mandates, 0, false, false);
        foreach (var kraj in kraje)
        {
            kraj.Value.AddMandates(divideMandatesKraje.mandatesParties[kraj.Key]);
        }

        IDictionary<int, Party> successfulParties =
            SuccessfulParties(parties, percentageNeeded, votesKraje.Values.Sum());

        int leftoverMandates = 0;


        foreach (var kraj in kraje)
        {
            IDictionary<int, int> votesKrajSuccessfulParties = new Dictionary<int, int>();
            foreach (var party in successfulParties)
            {
                votesKrajSuccessfulParties.Add(party.Key, kraj.Value.votesParties[party.Key]);
            }

            Skrutinium firstSkrutinium =
                new Skrutinium(votesKrajSuccessfulParties, kraj.Value.mandates, 2, false, true);
            foreach (var party in successfulParties)
            {
                party.Value.mandatesKraj.Add(kraj.Key, firstSkrutinium.mandatesParties[party.Key]);
                party.Value.leftoverVotesKraj.Add(kraj.Key, firstSkrutinium.leftOverVotes[party.Key]);
            }

            leftoverMandates += firstSkrutinium.mandates - firstSkrutinium.mandatesParties.Values.Sum();
        }

        IDictionary<int, int> leftoverVotesParties = new Dictionary<int, int>();

        foreach (var party in successfulParties)
        {
            leftoverVotesParties.Add(party.Key, party.Value.leftoverVotesKraj.Values.Sum());
        }


        Skrutinium secondSkrutinium = new Skrutinium(leftoverVotesParties, leftoverMandates, 1, false, false);
        foreach (var party in successfulParties)
        {
            if (secondSkrutinium.mandatesParties[party.Key] > 0)
            {
                var leftoverVotesSorted = party.Value.leftoverVotesKraj.OrderByDescending(x => x.Value);
                foreach (var leftoverParty in leftoverVotesSorted)
                {
                    if (secondSkrutinium.mandatesParties[party.Key] <= 0)
                    {
                        break;
                    }

                    party.Value.mandatesKraj[leftoverParty.Key]++;
                    secondSkrutinium.mandatesParties[party.Key]--;
                }
            }

            party.Value.mandates = party.Value.mandatesKraj.Values.Sum();
        }

    }

    static void PrintResults(IDictionary<int, Party> parties)
    {
        Console.WriteLine(
            "-----------------------------------------------------------------------------------------------------------------------");
        Console.WriteLine("Mandates isused: " + parties.Values.Sum(x => x.mandates));
        Console.WriteLine("Id\tMan.\tVotes\tSucc.\tName");
        foreach (var party in parties)
        {
            Console.WriteLine(
                $"{party.Value.id}\t{party.Value.mandates}\t{party.Value.allvotes}\t{party.Value.isSuccesfull}\t{party.Value.name}");
        }

        Console.WriteLine(
            "-----------------------------------------------------------------------------------------------------------------------");
    }


    static IDictionary<int, Kraj> CreateKrajData(Okrsek[] votingData, int nParties)
    {
        Bitmap mapKraje = new Bitmap("map_kraje.bmp");


        IDictionary<int, Kraj> kraje = new Dictionary<int, Kraj>();


        //Tohle není pěkné, předělat pls, ale funguje
        for (int i = 1; i < NOkrsky; i++)
        {
            if (votingData[i].status == Status.LOCAL)
            {

                int kraj = mapKraje
                    .GetPixel(votingData[i].relativeMapPoint.Item1, votingData[i].relativeMapPoint.Item2).R;
                if (!kraje.ContainsKey(kraj))
                {
                    kraje.Add(kraj, new Kraj(kraj, nParties));
                }

                kraje[kraj].AddOkrsek(votingData[i]);
            }
        }

        return kraje;
    }
    
    public static void Main(string[] args)
    {

        const bool createNewData = true;
        const bool saveData = true;
        const bool verbose = false;
        const string partiesDataFile = "nazvy_stran.txt";
        const string okrskyDataFile = "pst4p.csv";
        const int mandates = 200;
        float[] percentageNeeded = new float[] {20};
        //float[] percentageNeeded = new float[] {5, 5, 8, 11};
        //float[] percentageNeeded = new flota[] {5, 5, 10, 15}


        IDictionary<int, Party> parties = LoadParties(partiesDataFile);

        Okrsek[] votingData;

        if (createNewData)
        {
            IDictionary<string, Location> mapData = CreateOkrskyMapData();
            votingData = CreateOkrskyData(okrskyDataFile);
            ConnectData(votingData, mapData, verbose);
            CheckDataGood(votingData, mapData, verbose);
        }

        //Find maximum positions of okrseks to draw good map

        int[] extremes = FindExtremes(votingData);
        //Console.WriteLine("This are extremes: " + extremes[0] + " " + extremes[1] + " " + extremes[2] + " " + extremes[3]);


        //Draw map

        const int mapWidth = 1000;
        const int mapHeight = 1000;
        for (int i = 1; i < NOkrsky; i++)
        {

            votingData[i].SetRelativeLocation(mapWidth, mapHeight, extremes);
        }

        const string mapFile = "map.bmp";

        CreateMap(votingData, mapWidth, mapHeight, mapFile);

        //Open map of kraje

        IDictionary<int, Kraj> kraje = CreateKrajData(votingData, NParties);

        MoveDataBetweenKrajeAndParties(parties, kraje);

        CalculateElectionCz2021Ps(kraje, mandates, parties, percentageNeeded);

        PrintResults(parties);

        //Kraje
    }
}
    