using System.Diagnostics.Tracing;
using System.Drawing;

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
        name = GetSuperId(obec, this.okrsek);
    }

    public static string GetSuperId(string obec, string okrsek)
    {
        string name = obec + "-" + okrsek;
        return name;
    }
}