namespace volebniApka;

using volebniApka;

public class Okrsky

{
    public IDictionary<int, Okrsek> stuff = new Dictionary<int, Okrsek>();

    public Okrsky(string fileLocation, int ObceZahra, HashSet<string> _specialOkrsky)
    {
        FileStream stream = File.Open(fileLocation, FileMode.Open);
        StreamReader reader = new StreamReader(stream);


        //Načtení dat z CSV soubor
        string line = reader.ReadLine(); //Vynechá hlavičku
        while ((line = reader.ReadLine()) != null)
        {
            string[] data = line.Split(',');
            int id = int.Parse(data[(int) DataNames.ID_OKRSKY]);
            int party = int.Parse(data[(int) DataNames.KSTRANA]);
            int votes = int.Parse(data[(int) DataNames.POC_HLASU]);
            int obec = int.Parse(data[(int) DataNames.OBEC]);
            int okrsek = int.Parse(data[(int) DataNames.OKRSEK]);

            if (!stuff.ContainsKey(id))
            {
                stuff.Add(id, new Okrsek(id, obec, okrsek, ObceZahra, _specialOkrsky));
            }

            stuff[id].votes.Add(party, votes);
        }

        stream.Close();
    }

    public void ConnectData(IDictionary<string, Location> mapData, bool verbose)
    {
        ///Find where okresek are located from a locations
        foreach (var okrsek in stuff.Values)
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
    }

    public void CheckDataAllHaveLocation(bool verbose) /// Test if all okrseks have a location
    {
        IList<string> missingLocation = new List<string>();
        int counter = 0;
        foreach (var okrsek in stuff.Values)
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

        Console.WriteLine("Missing locations: " + counter + " out of " + stuff.Count);
    }
}