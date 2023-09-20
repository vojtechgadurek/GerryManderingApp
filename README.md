#  Dokumentace - Aplikace pro výpočet voleb / Gerrymandering app

## Autor - Vojtěch Gadurek 
## EN - Abstact
This app was created is end of course project in Intro to Programing to.
It allows user to draw his own election district and choose his own election method on 2021 Czech Republic election data.
## Uživatelská příručka 

### Popis
Aplikace umožňuje uživateli zkusit si jaké rozdíly způsobují volební metody na datech z voleb do poslanecké sněmovny v roce 2021. Uživatel má možnost si zvolit ze čtyř  metod a to metody využívané v roce 2021 a staré metody z roku 2017, dále first past the vote, a reverzní 2017, která prve rozděluje stranám a pozdeji krájům. Dále je možné změnit volební klauzule, počet udělovaných mandátů a nakonec i velikost a počet krajů a to i jaké okrsky jsou v nich obsaženy.

### Upozornění na nepřesnosti 

Program není dokonalý. Největší odchylkou od reality je špatný výpočet v D´Hondtové metodě, který neobsahuje krajovou klauzuli.

Dále v datech chybí k dnešku 127 okrsků, která se nezobrazují na mapě. 

Aplikace nepracuje se specialními a zahraničními okrsky, jelikož je není možné umístit do mapy. Jejich Status je Status.ZAHRA a Status.SPECIAL

Dále aplikace nepočítá úspěšné kandidáty, protože s ohledem na možnost upravovat volební kraje, tato funkcionalita postrádá smysl.

Program z důvodu zastaralé knihovny System.Drawing je možné používat jen na operačním systému Windows. 

### Používání programu
Program očekává vyplnění souboru ```settings.txt```  v následujicím formátu.

```
map_file_name = *.bmp - název souboru s mapou kraje
height = N - výška mapy v pixelech
width = N - šířka mapy v pixelech
voting_method = N - na výběr je z let 2017 a 2021
mandates = N - počet udělovaných mandátů
percetage_to_be_successful = N,...,M - procento hlasů, které strana musí získat, 
pro daný počet stran v koalici, jeli více stran v koalici než délka pole platí pro koalici
první hranice
create_map =  Bool - vytvoří se nová mapa
run = Bool - spustí se výpočet
Po vyplnění je možné spustit program.
```

#### Možné vyplnění
```
map_file_name = map_krajeI.bmp
height = 1000
width = 1000
voting_method = 2017
mandates = 200
percetage_to_be_successful = 0
create_map = False
run = True
```

#### Kreslení mapy

Program vždy vygeneruje mapu okrsků v daných rozměrech. Je možno ji najít pod názvem map.btm. Tento obrázek je možné pomocí oblibeného grafického editoru, použít jako podklad pro nakreslení mapy krajů. Doporučen je například program Gimp.

Mapa se kreslí následujicím způsobem: 

Každý kraj musí mít svojí jednu barvu. Jakákoliv jiná barva bude interpretována jako další kraj. Je tedy duležité zajistit, aby grafický editor nepoužíval smoothing. Většinou takto funguje nástroj pencil. Tento způsob umožňuje i nesouvislé kraje. 

Obrázek musí mít zadané rozměry.

### Nahrazení dat

#### Změna okrskových dat

Zdrojem dat je https://www.volby.cz/opendata/opendata.html - a jejich autorem je český statictický úřad.

Ta je možna změnit pomocí nahrání jiného souboru s názvem "pst4p.csv", který musí obsahovat následujicí sloupce ```ID_OKRSKY,TYP_FORM,OPRAVA,CHYBA,OKRES,OBEC,OKRSEK,KC_1,KSTRANA,POC_HLASU.```

#### Změna kandidujicích stran

Zdrojem dat je https://www.seznamzpravy.cz/clanek/volby-2021-kdo-kandiduje-a-koho-volit-173806

To je možné učnit pomocí souboru názvy_stran.txt, je nutné aby první ID byla 1 a dále se zvedala po jedné. Formát dat je následujicí: ```ID\tNázev strany\tJméno republikového lídra\tpočet stran v koalici```

#### Změna pozic okrsků

Zdrojem primárních dat je https://data.gov.cz/datov%C3%A1-sada?iri=https%3A%2F%2Fdata.gov.cz%2Fzdroj%2Fdatov%C3%A9-sady%2F00025712%2F885a03d4d6fe73adda96ba9b822680b7

Tyto data jsou docela mrzačeny pomocí bashových skriptů.

Jejich formát je následujcí

```
pozice_X pozice_Y
okrskové_číslo
číslo_obce
municipální_číslo -Volitelné
      -Mezery
```   

#### Výstup dat

Jako první se výpíše počet okrsků, které nebyly umístěny do mapy
Počet neumístěných hlasů na mapě
Dále počet pozic, které nebyly přiřazeny k okrskům
```  
 -----------------------------------------------------------
Výsledky ve formátu stran "Id\tMan.\tVotes\tSucc.\tName"
 -----------------------------------------------------------
Výsledky ve formátu krajů "Id\tMan.\tVotes\tSucc.\tName"
```  

#### Napsání vlastní volební metody

K tomuto účelu je možné využít třídy Election. Data se nacházejí v objektu kraje, strany v parties. Tyto objekty se chovají jako slovníky(Parties => Party => Votes, Mandates, LeftoverVotes => Counter (KrajId, int)) a obdobně Kraje => Kraj => Votes, Mandates, LeftoverVotes => Counter (ParytId, int) , jež je možno získát pomocí metody Stuff(), ale je možné s nimi pracovat i přímo. Data je potřeba ukládat do stran. 

Očekává se vlastní implemenatace abstraktní funkce RunElection. 

##### Příklad volební metody
```
public class ElectionFirstPastThePost : Election 
{
    //First divide mandates between parties, than bettwen kraje 
    public ElectionFirstPastThePost(int maxMandates, Parties parties, Kraje kraje, float[] percentageNeeded) : base(
        maxMandates,
        parties, kraje, percentageNeeded)
    {
    }

    public override void RunElection() //Je nutná
    {
        MandatesToKraje();
        Parties successfulParties = parties.SuccessfulParties(percentageNeeded, true);
        foreach (var kraj in kraje)
        {
            Counter votes = kraj.Get("votes");
            var max = votes.GetStuff()
                .Max(x => ((Party) parties.Get(x.Key)).GetSuccessfullness() ? x.Value : Int32.MinValue);
            ;
            int key = votes.GetStuff().First(x => (x.Value == max) && ((Party) parties.Get(x.Key)).GetSuccessfullness())
                .Key;
            parties.Add("mandates", key, kraj.GetId(), kraj.GetMaxMandates());
        }
    }
    //Data se ukládají do parties
    //kraje, žádná data z principu neočekávají, hodí se jen pro případné debugování
}
```
   
## Architektura aplikace 

### Popis 

Aplikace se dělí na tři části.

#### Zpracování dat

Funkce LoadParties nahraje data o stranách

voting_data obsahuje volební data z jednotlivých okrsků

Data jsou validována.

Dále je zpracováná mapa a dle ní vytvořeny kraje. Každý kraj má unikatní barvu, která je jeho identifikátorem. Okrsek je přiřazen k nejbližšímu pixelu na mapě a dle jeho barvy, zvolí svůj kraj.

Do krajů jsou nahrány data z okrsků.

#### Výpočet voleb 

Podle dané zvolené metody je zvolena metoda. Výsledky jsou uloženy do jednotlivých stran. 

#### Tisk výsledků
Vysledky jsou printovány z jednotlivých stran.

### Třídy

#### VotingObject a IVotingObject

Je základní objekt, je ukládat počty hlasů, mandátů a pracovat s nimi. Můžeme si všimnout, že skrutinia, kraje i strany jsou si velmi podobné i proto jsou syny tohoto objektu. Interface umužnuje efektivní prácí a větší varialibitu. 

#### VotingObjectGroup

Třída sjednocujicí všechny collections VotingObjektů a umožnujicí efektivní práci s němi.

#### Parties

##### SuccesfullParties(float[] percentageNeeded, bool setSuccesfullnes)
percentageNeeded je procento hlasů, které n-straná strana potřebuje. Pokud je počet stran větší než délka, aplikuje se číslo na 0 indexu.

Flag setSuccesfullnes určuje, zda se bude u stran nastavovat globální úspěšnost



#### Okrsek

#### Counter

Implemenuje dictionary s konstatní sumou

##### Sum()

Vrácí součet všech hodnot v čase 1.

#### Kraj

#### Skrutinum

Počítá jednotlivá skrutinia, hlavně v metodě 2021 

#### Strana 

#### Lokace

Pozice na mapě jsou ořezány na int. Větší přesnost se zdála nadbytečná

### Závěr
Podařilo se mi napsat program, který počítá jednotlivé volební modely pro uživatelem nakreslené volební kraje. Problém spočíval se špatnými opendaty a to především s neexistencí starých okrsků, což vedlo ke ztrátě nižších desítek tisíc hlasů. Další rozšíření je implementace xml readeru pro okrsková data, integrované kreslítko do aplikace. 
