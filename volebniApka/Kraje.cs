namespace volebniApka;

using volebniApka;
using System.Drawing;

public class Kraje : VotingObjectGroup
{
    public Kraje(Okrsky okrsky, string mapFile, int mapWidth, int mapHeight)
    {
        Bitmap mapKraje = new Bitmap(mapFile);
        //there must be - 1, or it wouldnt work
        if (mapHeight != mapKraje.Height - 1 || mapWidth != mapKraje.Width - 1)
        {
            throw new Exception(
                $"Dimensions of picture {mapKraje.Height - 1} x {mapKraje.Width - 1} do not match the expected dimensions {mapHeight} x {mapWidth}");
        }

        foreach (var okrsek in okrsky.stuff.Values)
        {
            if (okrsek.status == Status.LOCAL)
            {
                int kraj = mapKraje
                    .GetPixel(okrsek.relativeMapPoint[0], okrsek.relativeMapPoint[1]).R;
                if (!stuff.ContainsKey(kraj))
                {
                    stuff.Add(kraj, new Kraj(kraj));
                }

                ((Kraj) stuff[kraj]).AddOkrsek(okrsek);
            }
        }
    }
}