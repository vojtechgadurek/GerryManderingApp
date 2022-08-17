using System.Diagnostics.Tracing;

namespace volebniApka;

public class SuperId
{
    public string name;
    public string obec;
    public string okrsek;
    public SuperId(string obec, string okrsek)
    {
        this.obec = obec;
        this.okrsek = okrsek;
        name = obec + "-" + okrsek;
    }
}