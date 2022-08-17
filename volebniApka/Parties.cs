namespace volebniApka;

public class Parties
{
    public IDictionary<int, Party> stuff = new Dictionary<int, Party>();
    public Parties(string fileLocation)
    {
        StreamReader file = new StreamReader(fileLocation);
        string line;
        
        while ((line = file.ReadLine()) != null)
        {
            string[] parts = line.Split("\t");
            stuff.Add(Int32.Parse(parts[0]),
                new Party(Int32.Parse(parts[0]), parts[1], parts[2], Int32.Parse(parts[3])));
        }

        file.Close();
    }
}