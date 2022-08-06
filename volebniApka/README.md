#Dokumentace - Aplikace pro výpočet voleb
##Autor - Vojtěch Gadurek
##Uživatelská příručka
###Popis
Popis
Aplikace umožňuje uživateli zkusit si jaké rozdíly způsobují volební metody na datech z voleb do 
poslanecké sněmovny v roce 2021. A to možností si vybrat z dvou metod a to metody využívané
v roce 2021 a staré metody z roku 2017. Dále je možné změnit volební klauzule, počet
udělovaných mandátů a nakonec i velikost a počet krajů a to i jaké okrsky jsou do v nich obsaženy.
###Upozornění na nepřesnosti
Program není dokonalý. Největší odchylkou od reality je špatný výpočet v D´Hondtové metodě,
který neobsahuje krajouvou klauzuli.

Dále v datech chybí k dnešku 127 okrsků, která se 
nezobrazují na mapě.

Dále aplikace nepočítá úspěšné kandidáty, protože s ohledem na možnost upravovat volební
kraje, tato funkcionalita postrádá smysl.

Dále nefunguje pokud jsou rozměry mapy rozdílné.
###Používání programu
Program očekává vyplnění souboru settings.txt v následujicím formátu.
```
map_file_name = *.bmp - název souboru s mapou kraje
height = N - výška mapy v pixelech
width = N - šířka mapy v pixelech
voting_method = N - na výběr je z let 2017 a 2021
mandates = N - počet udělovaných mandátů
percetage_to_be_successful = N,...,M - procento hlasů, které strana musí získat, 
pro daný počet stran v koalici, jeli více stran v koalici než délka pole platí pro koalici
poslední hranice
 ```
Po vyplnění je možné spustit program.

####Kreslení mapy
Program vždy vygeneruje mapu okresků v daných rozměrech. Je možno ji najít pod názvem map.btm.
Tento obrázek je možné pomocí oblibeného grafického editoru, použít jako podklad pro nakreslení mapy krajů.

Mapa se kreslí následujicím způsobem, každý kraj musí mít svojí jednu barvu. Jakákoliv jiná barva bude
interpretována jako další kraj. Je tedy duležíté zajistit, aby grafický editor nepoužíval smoothing.
Většinou takto funguje nástroj penci.

Obrázek musí mít zadané rozměry.

###Nahrazení dat

####Změna okrskových dat
Zdrojem dat je https://www.volby.cz/opendata/opendata.html - a jejich autorem je český statictický úřad.

Ta je možna změnit pomocí nahrání jiného souboru s názvem "pst4p.csv", který musí
obsahovat následujicí sloupce ID_OKRSKY,TYP_FORM,OPRAVA,CHYBA,OKRES,OBEC,OKRSEK,KC_1,KSTRANA,POC_HLASU.
####Změna kandidujicích stran

Zdrojem dat je https://www.seznamzpravy.cz/clanek/volby-2021-kdo-kandiduje-a-koho-volit-173806

To je možné učnit pomocí souboru názvy_stran.txt, je nutné aby první ID byla 1 a dále
se zvedala po jedné.
Formát dat je následujicí: ID\tNázev strany\tJméno republikového lídra\tpočet stran v koalici.
####Změna pozic okrsků

Zdrojem primárních dat je https://data.gov.cz/datov%C3%A1-sada?iri=https%3A%2F%2Fdata.gov.cz%2Fzdroj%2Fdatov%C3%A9-sady%2F00025712%2F885a03d4d6fe73adda96ba9b822680b7

Tyto data jsou docela mrzačeny pomocí bashových skriptů. TODO

Jejich formát je následujcí
```
pozice_X pozice_Y
okrskové_číslo
číslo_obce
municipální_číslo -Volitelné
      -Mezery
```
##Architektura aplikace
###Popis
Aplikace se dělí na tři části.
####Zpracovánídat
Funkce LoadParties nahraje data o stranách

voting_data obsahuje volební data z jednotlivých okrsků

Data jsou validována.

Dále je zpracováná mapa a dle ní vytvořeny kraje. Každý kraj má unikatní barvu, která
je jeho identifikátorem. Okrsek je přiřazen k nejbližšímu pixelu na mapě a dle jeho barvy, zvolí
svůj kraj.

Do krajů jsou nahrány data z okrsků.

####Výpočet voleb
Podle dané zvolené metody je zvolena metoda. Výsledky jsou uloženy do jednotlivých stran.
####Tisk výsledků
Vysledky jsou printovány z jednotlivých stran.

###Třídy
####Okrsek
####Kraj
####Skrutinum
počítá jednotlivá skrutinia, hlavně v metodě 2021
####Strana
####Lokace
Pozice na mapě jsou ořezány na int. Větší přesnost se zdála nadbytečná.





      
