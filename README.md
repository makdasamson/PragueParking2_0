Prague Parking 2.0

Det här är min andra inlämning i Prague Parking. Jag har byggt vidare på den första versionen och arbetat med klasser, arv och listor.
I programmet kan man parkera, söka, flytta och hämta ut bilar och MC. Det finns 100 platser som standard. En plats rymmer en bil eller två MC. Kartan visar med färger vilka platser som är lediga, delvis upptagna och fulla. Till kartan används Spectre.Console.

Starta programmet
1.	Hämta hela repot och öppna Parking v2.slnx i Visual Studio. .NET 10 SDK behöver finnas installerat.
2.	Vänta tills NuGet-paketen har hämtats. Första gången behövs internet.
3.	Välj Parking v2 som startprojekt om det inte redan är valt.
4.	Tryck F5 eller Ctrl + F5.

   
Filer och inställningar
Fordonen sparas i parking.json, så de finns kvar när programmet stängs och öppnas igen. Filen innehåller också testfordon. Inställningar och priser finns i settings.json.
Filerna kopieras till Parking v2/bin/Debug/net10.0/ när programmet byggs. Båda ska ha inställningen Copy if newer i Visual Studio.
För att ändra ett pris när programmet körs öppnar man settings.json i den mappen, ändrar priset och sparar. Sedan väljer man 6 – Läs om prislistan i menyn. Antalet platser och storlekar läses när programmet startas igen.
Bil kostar 20 CZK och MC 10 CZK per påbörjad timme. Högst tio minuter är gratis. Om tiden är längre räknas timmarna från ankomsten. När ett fordon hämtas ut visas tiden och priset. Ankomsttiden behålls om fordonet flyttas.

Projekten
Parking v2 är själva programmet.
ParkingData läser och sparar filerna.
ParkingTests innehåller tre tester för prisberäkningen.
För att köra testerna väljer man Test → Run All Tests i Visual Studio.
