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


    const int N_OKRSKY = 14886 + 1;

    //Existuje 111 okrsků v zahraničí => Pozor na to META: V součastnosti ignorovány
    const int N_OKRSKY_ZAHRA = 111;

    //Zahraniční obce mají číslo 999997

    const int OBCE_ZAHRA = 999997;

    //Pozice zahraničních okrsků
    const int POZICE_ZAHRA = -1;

    //Pozice nenalezených okrsků
    const int POZICE_NENALEZEN = 1000;

    static HashSet<string> SPECIAL_OKRSKY = new HashSet<string>()
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
    const int N_PARTIES = 22;

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
        SPACIAL,
        NOT_FOUND,
    }
    #endregion
    
    #region Structs
    class Party
    {
        int id;
        string name;
        string leader;
        public bool coalition;
        int n_coalition_parties;
        int mandates = 0;
        public int allvotes = 0;
        public IDictionary<int, int> mandatesInKraj = new Dictionary<int, int>();
        public IDictionary<int, int> votesInKraj = new Dictionary<int, int>();
        public bool isSuccesfull;

        public Party(int id, string name, string leader, int n_coalition_parties, bool coalition)
        {
            this.id = id;
            this.name = name;
            this.leader = leader;
            this.n_coalition_parties = n_coalition_parties;
            this.coalition = coalition;
        }
    }

    class Kraj
    {
        public int id;
        public int color;
        public IList<Okrsek> okrsky;
        public int[] votesParties = new int[N_PARTIES + 1];
        public int[] mandatesParties = new int[N_PARTIES + 1];
        public int sumVotes = 0;
        public int mandates;
        public Kraj (int id)
        {
            this.id = id;
            this.color = id;
            this.okrsky = new List<Okrsek>();
        }
        public void AddOkrsek(Okrsek okrsek)
        {
            this.okrsky.Add(okrsek);
            for(int i = 1; i < N_PARTIES + 1; i++)
            {
                this.votesParties[i] += okrsek.votesParties[i];
                this.sumVotes += okrsek.votesParties[i];
            }
        }
        
        public void AddMandates(int mandates)
        {
            this.mandates = mandates;
        }

        public void AddMandatesParties(int[] mandatesParties)
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
        public int[] votesParties = new int[N_PARTIES + 1];
        public string superId;
        public int krajID;
        public int color;

        private void DetermineStatus()
        {
            if (obec == OBCE_ZAHRA)
            {
                status = Status.ZAHRA;
            }
            else if (SPECIAL_OKRSKY.Contains(superId))
            {
                status = Status.SPACIAL;
            }
        }


        public Okrsek(int id, int obec, int okrsek)
        {
            this.id = id;
            this.obec = obec;
            this.okrsek = okrsek;
            this.superId = fsuperId(obec.ToString(), okrsek.ToString());
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
            votesParties[party] += votes;
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
    }
    
    class Location
    {
        private Tuple<int, int> mapPoint;
        public bool used = false;
        public string superId;
        public string superId2;

        public Location(Tuple<int, int> mapPoint, string superId)
        {
            this.superId = superId;
            this.mapPoint = mapPoint;
            this.superId2 = superId;
        }

        public void AddSuperId(string superId)
        {
            this.superId2 = superId;
        }

        public Tuple<int, int> getLocation()
        {
            if (used)
            {
                Console.WriteLine("Error: Location already used" + superId);
                return mapPoint;
            }

            used = true;
            return mapPoint;
        }
    }
    #endregion

    #region LoadFuncs
    static Party[] LoadParties(string fileLocation)
    {
        StreamReader file = new StreamReader(fileLocation);
        string line;
        Party[] parties = new Party[N_PARTIES + 1];
        while ((line = file.ReadLine()) != null)
        {
            string[] parts = line.Split("\t");
            parties[Int32.Parse(parts[0])] = new Party(Int32.Parse(parts[0]), parts[1], parts[2], Int32.Parse(parts[3]), parts[3] == "1");
        }

        file.Close();

        return parties;
    }
    
    static string fsuperId(string obec, string okrsek)
    {
        return obec + "-" + okrsek;
    }

    static Okrsek[] CreateOkrskyData(string fileLocation)
    {
        FileStream votingDataStream = File.Open(fileLocation, FileMode.Open);
        StreamReader votingDataReader = new StreamReader(votingDataStream);

        int maxId = N_OKRSKY + 1;
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

            if (party > N_PARTIES)
            {
                Console.WriteLine("Chyba: Kód strany je větší než počet stran");
                return null;
            }

            if (votingData[id] == null)
            {
                votingData[id] = new Okrsek(id, obec, okrsek);
            }

            votingData[id].AddVote(party, votes);
        }

        votingDataReader.Close();
        votingDataStream.Close();
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
        const string FREESPACE = "      ";
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
            if (obec2 != FREESPACE)
            {
                obec2Exists = true;
                mapDataReader.ReadLine();
            }

            string superId = fsuperId(obec, okrsek);

            string[] position = positionString.Split(' ');

            Tuple<int, int> positionXY;

            try
            {
                positionXY =
                    new Tuple<int, int>((int) float.Parse(position[0]), (int) float.Parse(position[1]));
            }
            catch (Exception)
            {
                throw new Exception("Error in map_data.txt file, wrong position format " + positionString + " " +
                                    okrsek + " " + obec + " " + obec2 + " counter " + counter);
            }



            mapData.Add(superId, new Location(positionXY, superId));


            if (obec2Exists)
            {
                string superId2 = fsuperId(obec2, okrsek);
                mapData.Add(superId2, new Location(positionXY, superId));
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
                    votingData[i].AddLocation(mapData[superId].getLocation());
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
        int counter_n = 0;
        int counter = 0;
        foreach (var location in locations.Values)
        {
            counter_n++;
            if (!location.used)
            {
                if (verbose)
                {
                    Console.WriteLine("Error: Location " + location.superId + " " + location.superId2 + "  not used");
                }

                counter++;
            }
        }

        Console.WriteLine("Locations not used: " + counter + " of " + counter_n);
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
        for (int i = 1; i < N_OKRSKY; i++)
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

    static void calculateVotes(Okrsek[] votingData)
    {
        //Add all votes to one array
        int[] votesAll = new int[N_PARTIES + 1];
        int max_id = N_OKRSKY;
        for (int i = 0; i < votingData.Length; i++)
        {
            for (int j = 1; j < N_PARTIES + 1; j++)
            {
                votesAll[j] += votingData[i].votesParties[j];
            }
        }

        //Make sum of all votes

        int sumVotes = 0;
        for (int i = 1; i < N_PARTIES + 1; i++)
        {
            sumVotes += votesAll[i];
        }

        Console.WriteLine("Sum of all votes: " + sumVotes);

        //Print all votes
        for (int i = 1; i < N_PARTIES + 1; i++)
        {
            Console.WriteLine("{0}:{1}", i, votesAll[i]);
        }

        int count = 0;
        for (int i = 1; i < votingData.Length; i++)
        {
            if (votingData[i].mapPoint != null)
            {
                count++;
            }
        }

        Console.WriteLine(count);
    }

    static void CreateMap(Okrsek[] votingData, int map_width, int map_height, string fileLocation)
    {
        Bitmap bitmap = new Bitmap(map_width + 1, map_height + 1);

        for (int i = 1; i < N_OKRSKY; i++)
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
        public int[] votes;
        public int mandates;
        public int leftOverMandates;
        public int[] mandatesParties;
        public IDictionary<int, int> leftOverVotes;
        public int kvotaNumber;
        public int kvota;
        public int allVotes;
        public int mandatesUsed;
        private bool mandateOverflowOk;
        private bool mandateUnderflowOk;

        public Skrutinium(int parties, int[] votes, int mandates, int kvotaNumber, bool mandateOverflowOk, bool mandateUnderflowOk)
        {
            this.parties = parties;
            this.votes = votes;
            this.mandates = mandates;
            this.kvotaNumber = kvotaNumber;
            this.mandateOverflowOk = mandateOverflowOk;
            this.mandateUnderflowOk = mandateUnderflowOk;
            CalculateMandates();
        }
        

        private void CalculateKvota()
        {
            allVotes = votes.Sum();
            kvota = allVotes / mandates + kvotaNumber;
        }
        
        private void CalculateMandatesParties()
        {
            mandatesParties = new int[parties];
            leftOverVotes = new Dictionary<int, int>();
            for (int i = 0; i < parties; i++)
            {
                mandatesParties[i] = votes[i] / kvota;
                leftOverVotes[i] = votes[i] % kvota;
            }
        }

        private void FixNotEqualMandates()
        {
            int toAdd = mandates - mandatesUsed;
            if (toAdd > 0 && (!mandateUnderflowOk))
            {
                leftOverVotes.OrderByDescending(x => x.Value);
            }
            else if (toAdd < 0 && (!mandateOverflowOk))
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
            mandatesUsed = mandatesParties.Sum();
            FixNotEqualMandates();
        }
    }
    
    static void CalculateElectionCz2021PS(IDictionary<int, Kraj> kraje, int mandates, int nParties)
    {
        //Number of mandates in each kraj
        if (mandates < 1)
        {
            throw new Exception("Error: Mandates must be greater than 0");
            return;
        }
        
        int[] votesKraje = new int[kraje.Count];
        IDictionary<int,int> KrajeToIdTranslator = new Dictionary<int, int>();
        int id = 0;
        foreach (var kraj  in kraje)
        {
            KrajeToIdTranslator.Add(kraj.Key, id);
            votesKraje[id] = kraj.Value.sumVotes;
            id++;
        }

        Skrutinium DivideMandatesKraje = new Skrutinium(kraje.Count, votesKraje, mandates, 0, false, false);
        foreach (var kraj in kraje)
        {
            kraj.Value.AddMandates(DivideMandatesKraje.mandatesParties[KrajeToIdTranslator[kraj.Key]]);
        }
        
        int leftoverMandates = 0;
        int[] leftoverVotesParties = new int[nParties];
        
        foreach (var kraj in kraje.Values)
        {
            Skrutinium firstSkrutinium = new Skrutinium(nParties, kraj.votesParties, kraj.mandates, 2, false, true);
            kraj.mandatesParties = firstSkrutinium.mandatesParties;
            leftoverMandates += kraj.mandates - kraj.mandatesParties.Sum();
            for(int i = 0; i<nParties; i++)
            {
                leftoverVotesParties[i] += firstSkrutinium.leftOverVotes[i];
            }
        }
        
        Skrutinium secondSkrutinium = new Skrutinium(nParties, leftoverVotesParties, leftoverMandates, 1, false, false);

    }
    
    
    static IDictionary<int, Kraj> CreateKrajData(Okrsek[] votingData)
    {
        Bitmap map_kraje = new Bitmap("map_kraje.bmp");


        IDictionary<int, Kraj> kraje = new Dictionary<int, Kraj>();


        //Tohle není pěkné, předělat pls, ale funguje
        for (int i = 1; i < N_OKRSKY; i++)
        {
            if (votingData[i].status == Status.LOCAL)
            {

                int kraj = map_kraje
                    .GetPixel(votingData[i].relativeMapPoint.Item1, votingData[i].relativeMapPoint.Item2).R;
                if (!kraje.ContainsKey(kraj))
                {
                    kraje.Add(kraj, new Kraj(kraj));
                }

                kraje[kraj].AddOkrsek(votingData[i]);
            }
        }

        return kraje;
    }
    
   
    
    
    public static void Main(string[] args)
    {

        const bool create_new_data = true;
        const bool save_data = true;
        const bool verbose = false;
        const string parties_data_file = "nazvy_stran.txt";
        const string okrsky_data_file = "pst4p.csv";
        
        var parties = LoadParties(parties_data_file);
        
        Okrsek[] votingData;

        if (create_new_data)
        {
            
            IDictionary<string, Location> mapData = CreateOkrskyMapData();
            votingData = CreateOkrskyData(okrsky_data_file);
            ConnectData(votingData, mapData, verbose);
            CheckDataGood(votingData, mapData, verbose);
        }


        int [] extremes = FindExtremes(votingData);
        Console.WriteLine("This are extremes: " + extremes[0] + " " + extremes[1] + " " + extremes[2] + " " + extremes[3]);
        //Find maximum positions of okrseks to draw good map
        
        //Draw map

        const int map_width = 1000;
        const int map_height = 1000;
        for (int i = 1; i < N_OKRSKY; i++)
        {
            
            votingData[i].SetRelativeLocation(map_width, map_height, extremes);
        }
        
        const string map_file = "map.bmp";
        
        CreateMap(votingData, map_width, map_height, map_file);
        
        //Open map of kraje

        IDictionary<int, Kraj> kraje = CreateKrajData(votingData); 
        
        CalculateElectionCz2021PS(kraje, 200, parties.Length);
              

        //Kraje
    }
}