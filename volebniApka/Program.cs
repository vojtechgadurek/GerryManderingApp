// See https://aka.ms/new-console-template for mo


using System.Net;
using System.Xml;
using System.Xml.Linq;

class VotingSystems
{
    //Constanty
    //Počet okrsků je 14886
    
    const int N_OKRSKY = 14886 + 1;

    //Existuje 111 okrsků v zahraničí => Pozor na to
    const int N_OKRSKY_ZAHRA = 111;
    
    //Zahraniční obce mají číslo 999997
    
    const int OBCE_ZAHRA = 999997;
    
    //Počer stran je 22
    const int N_PARTIES = 22;


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

    static string fsuperId(string obec, string okrsek)
    {
        return obec + "-" + okrsek;
    }
    class Okrsek
    {
        public int id;
        public int obec;
        public int okrsek;
        public Tuple<int, int> mapPoint;
        public int[] votesParties = new int[N_PARTIES + 1];
        public string superId;

        public Okrsek(int id, int obec, int okrsek)
        {
            this.id = id;
            this.obec = obec;
            this.okrsek = okrsek;
            this.superId = fsuperId(obec.ToString(), okrsek.ToString());
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

            votingData[id].votesParties[party] = votes;
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

    
    static IDictionary<string, Tuple<int, int>> CreateOkrskyMapData()
    {
        //open map_data.txt file
        // Data are in format
        // Position
        // Okrsek
        // Obec
        // Obec2(optional)
        // Freespace "      "
        const string FREESPACE = "      ";
        IDictionary<string, Tuple<int, int>> mapData = new Dictionary<string, Tuple<int, int>>();

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
            
           

            mapData.Add(superId, positionXY);
            
            
            if (obec2Exists)
            {
                string superId2 = fsuperId(obec2, okrsek);
                mapData.Add(superId2, positionXY);
            }

            counter++;
        }

        mapDataReader.Close();
        
        
    return mapData;
    }

    static Okrsek[] ConnectData()
    {
        Okrsek[] votingData = CreateOkrskyData();
        IDictionary<string, Tuple<int, int>> mapData = CreateOkrskyMapData();
        
        foreach (var okrsek in votingData)
        {
            if (okrsek != null)
            {
                string superId = okrsek.superId;
                if (mapData.ContainsKey(superId))
                {
                    okrsek.mapPoint = mapData[superId];
                }
            }
        }
        
        return votingData;

    }

    static void CheckDataAllHaveLocation(Okrsek[] votingData)
    {
        IList<string> missingLocation = new List<string>();

        foreach (var okrsek in votingData)
            {
                if (okrsek != null)
                {
                    if (okrsek.mapPoint == null)
                    {
                        missingLocation.Add("Error: Okrsek " + okrsek.id + " " + okrsek.obec + " " + okrsek.okrsek + " do not have location");
                    }
                }
            }

        foreach (var missing in missingLocation)
        {
            Console.WriteLine(missing);
        }
    }
    static void CheckDataGood(Okrsek[] votingData)
    {
        CheckDataAllHaveLocation(votingData);
    }
    
    
    public static void Main(string[] args)
    {
        Okrsek[] votingData = ConnectData();
        CheckDataGood(votingData);
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