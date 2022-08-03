﻿// See https://aka.ms/new-console-template for mo


using System.Net;
using System.Xml;
using System.Xml.Linq;

class VotingSystems
{
    //Najít Porubu => je v datech?
    //Constanty
    //Počet okrsků je 14886
    
    const int N_OKRSKY = 14886 + 1;

    //Existuje 111 okrsků v zahraničí => Pozor na to
    const int N_OKRSKY_ZAHRA = 111;

    //Zahraniční obce mají číslo 999997
    
    const int OBCE_ZAHRA = 999997;
    
    //Pozice zahraničních okrsků
    const int POZICE_ZAHRA = -1;
    
    //Pozice nenalezených okrsků
    const int POZICE_NENALEZEN = 1000;

    static HashSet<string> SPECIAL_OKRSKY = new HashSet<string>() { "500054-1000", 
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
            
        

    //Počer stran je 22
    const int N_PARTIES = 22;
    
    //Covid okrsky
    
    


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
    
    enum Status {
        LOCAL,
        ZAHRA,
        SPACIAL,
        NOT_FOUND,
    }
    
    
    static string fsuperId(string obec, string okrsek)
    {
        return obec + "-" + okrsek;
    }
    class Okrsek
    {
        public int id;
        public int obec;
        public int okrsek;
        public Status status = Status.NOT_FOUND;
        public Tuple<int, int> mapPoint;
        public int[] votesParties = new int[N_PARTIES + 1];
        public string superId;

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
        
    }

    static Okrsek[] CreateOkrskyData()
    {
        FileStream votingDataStream = File.Open("pst4p.csv", FileMode.Open);
        StreamReader votingDataReader = new StreamReader(votingDataStream);

        int maxId = N_OKRSKY + 1; 
        Okrsek[] votingData = new Okrsek[maxId];


        //Načtení dat z CSV souboru

        string line = votingDataReader.ReadLine(); //Vynechá hlavičku
        while ((line = votingDataReader.ReadLine()) != null)
        {
            string[] data = line.Split(',');
            int id = int.Parse(data[(int)DataNames.ID_OKRSKY]);
            int party = int.Parse(data[(int)DataNames.KSTRANA]);
            int votes = int.Parse(data[(int)DataNames.POC_HLASU]);
            int obec = int.Parse(data[(int)DataNames.OBEC]);
            int okrsek = int.Parse(data[(int)DataNames.OKRSEK]);

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
                    new Tuple<int, int>((int)float.Parse(position[0]), (int)float.Parse(position[1]));
            }
            catch (Exception)
            {
                throw new Exception("Error in map_data.txt file, wrong position format " + positionString + " " + okrsek + " " + obec + " " + obec2 + " counter " + counter);
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

    static Okrsek[] ConnectData(Okrsek[] votingData,IDictionary<string, Location> mapData, bool verbose)
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
        foreach (var okrsek in votingData) {
            if (okrsek != null)
            {
                if (okrsek.status == Status.NOT_FOUND)
                {
                    counter++;
                    missingLocation.Add("Error: Okrsek " + okrsek.id + " " + okrsek.obec + " " + okrsek.okrsek + " do not have location");
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
                if(verbose){Console.WriteLine("Error: Location " + location.superId + " " + location.superId2 + "  not used");}
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
    
    
    public static void Main(string[] args)
    {
        IDictionary<string, Location> mapData = CreateOkrskyMapData();
        Okrsek[] votingData = CreateOkrskyData();
        
        const bool verbose = false;
        ConnectData(votingData, mapData, verbose);
        CheckDataGood(votingData, mapData, verbose);
        
        
        /*
        //Add all votes to one array
        int[] votesAll = new int[N_PARTIES + 1];
        int max_id = N_OKRSKY;
        for(int i = 1; i < max_id; i++)
        {
            for(int j = 1; j < N_PARTIES + 1; j++)
            {
                votesAll[j] += votingData[i].votesParties[j];
            }
        }
        
        //Make sum of all votes
        
        int sumVotes = 0;
        for(int i = 1; i < N_PARTIES + 1; i++)
        {
            sumVotes += votesAll[i];
        }
        
        Console.WriteLine("Sum of all votes: " + sumVotes);

        //Print all votes
        for(int i = 1; i < N_PARTIES + 1; i++)
        {
            Console.WriteLine("{0}:{1}", i, votesAll[i]);
        }
        
        int count = 0; 
        for (int i = 1; i < N_OKRSKY ; i++)
        {
            if (votingData[i].mapPoint != null)
            {
                count++;
            }
        }
        Console.WriteLine(count);
        */
        
            //Find maximum positions of okrseks to draw good map
        int maxX = votingData[1].mapPoint.Item1;
        int minX = votingData[1].mapPoint.Item1;
        int maxY = votingData[1].mapPoint.Item2;
        int minY = votingData[1].mapPoint.Item2;
        for (int i = 1; i < N_OKRSKY + 1; i++)
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
        Console.WriteLine("Max X: " + maxX);
        Console.WriteLine("Min X: " + minX);
        Console.WriteLine("Max Y: " + maxY);
        Console.WriteLine("Min Y: " + minY);


        //Kraje
    }
}