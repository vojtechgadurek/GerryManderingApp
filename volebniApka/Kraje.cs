namespace volebniApka;

using volebniApka;
using System.Drawing;
public class Kraje
{
    public IDictionary<int, Kraj> stuff =new Dictionary<int, Kraj>();
    public Kraje(Okrsky okrsky, string mapFile)
    {
        Bitmap mapKraje = new Bitmap(mapFile);

        foreach(var okrsek in okrsky.stuff.Values)
        {
            if (okrsek.status == Status.LOCAL)
            {
                int kraj = mapKraje
                    .GetPixel(okrsek.relativeMapPoint[0], okrsek.relativeMapPoint[1]).R;
                if (!stuff.ContainsKey(kraj))
                {
                    stuff.Add(kraj, new Kraj(kraj));
                }
                stuff[kraj].AddOkrsek(okrsek);
            }
        }
    }
}