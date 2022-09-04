using System.Collections;

namespace volebniApka;

using volebniApka;

public class Okrsek
{
    public int id;
    public int obec;
    public int okrsek;
    public Status status = Status.NOT_FOUND;
    public IList<int> mapPoint;
    public IList<int> relativeMapPoint;
    public Counter votes = new Counter();
    public SuperId superId;

    public Okrsek(int id, int obec, int okrsek, int obceZahra, HashSet<string> specialOkrsky)
    {
        this.id = id;
        this.obec = obec;
        this.okrsek = okrsek;
        this.superId = new SuperId(obec.ToString(), okrsek.ToString());
        DetermineStatus(obceZahra, specialOkrsky);
    }

    private void DetermineStatus(int obceZahra, HashSet<string> specialOkrsky)
    {
        if (obec == obceZahra)
        {
            status = Status.ZAHRA;
        }
        else if (specialOkrsky.Contains(superId.name))
        {
            status = Status.SPECIAL;
        }
    }

    public bool AddLocation(IList<int> mapPoint)
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

    public void SetRelativeLocation(int width, int height, Extremes extremes)
    {
        if (this.status == Status.LOCAL)
        {
            int widthEx = extremes.width;
            int heightEx = extremes.height;

            int divider;

            if (heightEx / height > widthEx / width)
            {
                divider = heightEx;
            }
            else
            {
                divider = widthEx;
            }

            this.relativeMapPoint = new List<int>
            {
                (mapPoint[0] - extremes.minX) * width / divider,
                height - (mapPoint[1] - extremes.minY) * height / divider
            };
            if (relativeMapPoint[0] < 0 || relativeMapPoint[1] < 0)
            {
                throw new Exception("Map position is negative");
            }

            if (relativeMapPoint[0] > width || relativeMapPoint[1] > height)
            {
                throw new Exception("Map position is out of bounds");
            }
        }
    }
}