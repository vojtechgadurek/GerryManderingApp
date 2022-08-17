namespace volebniApka;

using volebniApka;

public class OkrskyPositions
{
    public static IDictionary<string, Location> Create(string mapDataFile)
    {
        /// Will read data from mapDataFile and create a dictionary of okrsky positions
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

            string superId = SuperId.GetSuperId(obec, okrsek);

            string[] position = positionString.Split(' ');

            IList<int> positionXY;

            try
            {
                positionXY =
                    new List<int> {(int) float.Parse(position[0]), (int) float.Parse(position[1])};
            }
            catch (Exception)
            {
                throw new Exception("Error in map_data.txt file, wrong position format " + positionString + " " +
                                    okrsek + " " + obec + " " + obec2 + " counter " + counter);
            }


            mapData.Add(superId, new Location(positionXY, superId));


            if (obec2Exists)
            {
                string superId2 = SuperId.GetSuperId(obec2, okrsek);
                mapData.Add(superId2, new Location(positionXY, superId));
                mapData[superId].AddSuperId(superId2);
            }

            counter++;
        }

        mapDataReader.Close();


        return mapData;
    }

    public static void
        CheckLocationsAllHaveData(IDictionary<string, Location> locations,
            bool verbose) ///Test if to all locations where assingned okrsek
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
}