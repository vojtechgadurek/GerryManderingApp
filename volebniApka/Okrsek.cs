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
                divider = heightEx / height + 1;
            }
            else
            {
                divider = widthEx / width + 1;
            }

            this.relativeMapPoint = new List<int>
            {
                (int) (mapPoint[0] - extremes.minX) / divider,
                (int) height - (mapPoint[1] - extremes.minY) / divider
            };
            if (relativeMapPoint[0] < 0 || relativeMapPoint[1] < 0)
            {
                throw new Exception("Map position is negative");
            }

            if (relativeMapPoint[0] > width + 1 || relativeMapPoint[1] > height + 1)
            {
                throw new Exception("Map position is out of bounds");
            }
        }
    }
}