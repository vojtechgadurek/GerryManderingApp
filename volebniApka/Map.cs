namespace volebniApka;

using System.Drawing;

public class Map
{
    public static void CreateMap(IDictionary<int, Okrsek> votingData, int mapWidth, int mapHeight, string fileLocation)
    {
        Bitmap bitmap = new Bitmap(mapWidth + 1, mapHeight + 1);
        foreach (var okrsek in votingData)
        {
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
}