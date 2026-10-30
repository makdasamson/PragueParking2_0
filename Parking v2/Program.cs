using Spectre.Console;
using ParkingData;
using System.IO;
using System.Text.Json;

FileStorage files = new FileStorage();

string datafil = Path.Combine(AppContext.BaseDirectory, "parking.json");
string configfil = Path.Combine(AppContext.BaseDirectory, "settings.json");

ParkingGarage garage;
ParkingSettings settings;

try
{
    if (File.Exists(configfil))
    {
        settings = files.Read<ParkingSettings>(configfil)
            ?? throw new JsonException();
    }
    else
    {
        settings = new ParkingSettings();
        files.Save(configfil, settings);
    }

    if (settings.NumberOfSpots < 1 ||
        settings.SpotSize < 1 ||
        settings.FreeMinutes < 0 ||
        settings.VehicleTypes == null ||
        settings.VehicleTypes.Count == 0)
    {
        throw new JsonException();
    }

    foreach (VehicleSettings typ in settings.VehicleTypes)
    {
        if (typ == null ||
            string.IsNullOrWhiteSpace(typ.Code) ||
            string.IsNullOrWhiteSpace(typ.Name) ||
            typ.Size < 1 ||
            typ.Size > settings.SpotSize ||
            typ.VehiclesPerSpot < 1 ||
            typ.HourlyPrice < 0)
        {
            throw new JsonException();
        }
    }

    if (File.Exists(datafil))
    {
        garage = files.Read<ParkingGarage>(datafil)
            ?? throw new JsonException();
    }
    else
    {
        garage = new ParkingGarage();
    }

    if (garage.Spots == null)
    {
        throw new JsonException();
    }

    // Ta bara bort platser om de är tomma.
    while (garage.Spots.Count > settings.NumberOfSpots)
    {
        ParkingSpot sistaPlats = garage.Spots[garage.Spots.Count - 1];

        if (sistaPlats.Vehicles.Count > 0)
        {
            Console.WriteLine(
                "Antalet platser kan inte minskas. Det står fordon på plats " +
                sistaPlats.Number + ".");
            return;
        }

        garage.Spots.RemoveAt(garage.Spots.Count - 1);
    }

    // Lägg till platser om inställningarna kräver fler.
    while (garage.Spots.Count < settings.NumberOfSpots)
    {
        ParkingSpot plats = new ParkingSpot();
        plats.Number = garage.Spots.Count + 1;
        garage.Spots.Add(plats);
    }
}
catch (JsonException)
{
    Console.WriteLine(
        "Inställningsfilen eller parkeringsfilen innehåller felaktiga uppgifter.");
    return;
}
catch (IOException)
{
    Console.WriteLine("En fil kunde inte läsas eller sparas.");
    return;
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Programmet saknar behörighet att läsa eller spara en fil.");
    return;
}
string menyval;

foreach (ParkingSpot plats in garage.Spots)
{
    plats.Settings = settings;
}

do
{
    Console.WriteLine();
    AnsiConsole.MarkupLine("[bold blue]=== PRAGUE PARKING V2 ===[/]");
    Console.WriteLine("1. Visa parkeringen");
    Console.WriteLine("2. Parkera fordon");
    Console.WriteLine("3. Sök fordon");
    Console.WriteLine("4. Flytta fordon");
    Console.WriteLine("5. Hämta ut fordon");
    Console.WriteLine("6. Läs om prislistan");
    Console.WriteLine("0. Avsluta");
    Console.Write("Välj: ");

    menyval = Console.ReadLine() ?? "";

    switch (menyval)
    {
        case "1":
            garage.VisaParkering();
            break;

        case "2":
            ParkeraFordon	();
            SparaGarage	();
            break;

        case "3":
            SokFordon	();
            break;

        case "4":
            FlyttaFordon	();
            SparaGarage	();
            break;

        case "5":
            HamtaUtFordon	();
            SparaGarage	();
            break;

        case "6":
            LasOmPrislista();
            break;

        case "0":
            Console.WriteLine("Programmet avslutas.");
            break;

        default:
            Console.WriteLine("Ogiltigt menyval. Försök igen.");
            break;
    }
}
while (menyval != "0");

string LasRegistreringsnummer()
{
    while (true)
    {
        Console.Write("Registreringsnummer (1–10 tecken): ");
        string regnummer = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        bool giltigt = regnummer.Length >= 1 && regnummer.Length <= 10;

        foreach (char tecken in regnummer)
        {
            if (!char.IsLetterOrDigit(tecken))
            {
                giltigt = false;
            }
        }

        if (giltigt)
        {
            return regnummer;
        }

        Console.WriteLine("Ogiltigt registreringsnummer. Försök igen.");
    }
}

void ParkeraFordon	()
{
    Console.WriteLine("1. Bil");
    Console.WriteLine("2. MC");
    Console.Write("Välj fordonstyp: ");

    string val = Console.ReadLine() ?? "";

    if (val != "1" && val != "2")
    {
        Console.WriteLine("Ogiltig fordonstyp.");
        return;
    }

    string regnummer = LasRegistreringsnummer();

    if (garage.HittaPlatsForFordon(regnummer) != null)
    {
        Console.WriteLine("Registreringsnumret finns redan på parkeringen.");
        return;
    }

    Vehicle fordon;

    if (val == "1")
    {
        fordon = new Car();
    }
    else
    {
        fordon = new MC();
    }

    fordon.RegNumber = regnummer;

    ParkingSpot? plats = garage.HittaLedigPlats(fordon);

    if (plats == null)
    {
        Console.WriteLine("Det finns ingen plats för fordonet.");
        return;
    }

    fordon.ArrivalTime = DateTime.Now;
    plats.Vehicles.Add(fordon);

    Console.WriteLine("Parkera " + regnummer + " på plats " + plats.Number);
}

void SokFordon	()
{
    string regnummer = LasRegistreringsnummer();
    ParkingSpot? plats = garage.HittaPlatsForFordon(regnummer);

    if (plats == null)
    {
        Console.WriteLine("Fordonet finns inte på parkeringen.");
        return;
    }

    Console.WriteLine(regnummer + " står på plats " + plats.Number);
}

void FlyttaFordon	()
{
    string regnummer = LasRegistreringsnummer();
    ParkingSpot? gammalPlats = garage.HittaPlatsForFordon(regnummer);

    if (gammalPlats == null)
    {
        Console.WriteLine("Fordonet finns inte på parkeringen.");
        return;
    }

    Console.WriteLine("Fordonet står på plats " + gammalPlats.Number);

    int platsnummer;

    while (true)
    {
        Console.Write("Ange ny plats (1–" + garage.Spots.Count + "): ");
        string inmatning = Console.ReadLine() ?? "";

        if (int.TryParse(inmatning, out platsnummer) &&
            platsnummer >= 1 && platsnummer <= garage.Spots.Count)
        {
            break;
        }

        Console.WriteLine("Ogiltigt platsnummer. Försök igen.");
    }

    ParkingSpot nyPlats = garage.Spots[platsnummer - 1];

    if (nyPlats == gammalPlats)
    {
        Console.WriteLine("Fordonet står redan på den platsen.");
        return;
    }

    Vehicle? fordonAttFlytta = null;

    foreach (Vehicle fordon in gammalPlats.Vehicles)
    {
        if (fordon.RegNumber == regnummer)
        {
            fordonAttFlytta = fordon;
            break;
        }
    }

    if (fordonAttFlytta == null)
    {
        Console.WriteLine("Fordonet kunde inte hittas.");
        return;
    }

    if (!nyPlats.KanParkera(fordonAttFlytta))
    {
        Console.WriteLine("Fordonet får inte plats där. Ingen flytt gjordes.");
        return;
    }

    gammalPlats.Vehicles.Remove(fordonAttFlytta);
    nyPlats.Vehicles.Add(fordonAttFlytta);

    Console.WriteLine(
        "Flytta " + regnummer +
        " från plats " + gammalPlats.Number +
        " till plats " + nyPlats.Number);
}

void HamtaUtFordon	()
{
    string regnummer = LasRegistreringsnummer();
    ParkingSpot? plats = garage.HittaPlatsForFordon(regnummer);

    if (plats == null)
    {
        Console.WriteLine("Fordonet finns inte på parkeringen.");
        return;
    }

    Vehicle? fordonAttHamta = null;

    foreach (Vehicle fordon in plats.Vehicles)
    {
        if (fordon.RegNumber == regnummer)
        {
            fordonAttHamta = fordon;
            break;
        }
    }

    if (fordonAttHamta == null)
    {
        Console.WriteLine("Fordonet kunde inte hittas.");
        return;
    }

    string kod;

    if (fordonAttHamta is Car)
    {
        kod = "CAR";
    }
    else
    {
        kod = "MC";
    }

    VehicleSettings? fordonsinställning = null;

    foreach (VehicleSettings typ in settings.VehicleTypes)
    {
        if (typ.Code == kod)
        {
            fordonsinställning = typ;
            break;
        }
    }

    if (fordonsinställning == null)
    {
        Console.WriteLine("Priset för fordonstypen saknas. Fordonet är kvar.");
        return;
    }

    TimeSpan parkeringstid = DateTime.Now - fordonAttHamta.ArrivalTime;

    if (parkeringstid < TimeSpan.Zero)
    {
        Console.WriteLine("Ankomsttiden ligger i framtiden. Kontrollera tiden.");
        return;
    }

    PriceCalculator calculator = new PriceCalculator();

    decimal pris = calculator.CalculatePrice(
        parkeringstid,
        settings.FreeMinutes,
        fordonsinställning.HourlyPrice);

    Console.WriteLine(
        "Tid parkerad: " + parkeringstid.Days + " dygn, " +
        parkeringstid.Hours + " timmar, " +
        parkeringstid.Minutes + " minuter och " +
        parkeringstid.Seconds + " sekunder.");

    Console.WriteLine("Att betala: " + pris + " CZK");

    plats.Vehicles.Remove(fordonAttHamta);

    Console.WriteLine(
        "Hämta ut " + regnummer + " från plats " + plats.Number +
        ". Fordonet har tagits bort ur systemet.");
}
void SparaGarage	()
{
    try
    {
        files.Save(datafil, garage);
    }
    catch (IOException)
    {
        Console.WriteLine(
            "Ändringen kunde inte sparas. Programmet avslutas för att undvika fler osparade ändringar.");
        menyval = "0";
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine(
            "Programmet saknar behörighet att spara. Ändringen är inte sparad.");
        menyval = "0";
    }
}
void LasOmPrislista()
{
    try
    {
        ParkingSettings? nya = files.Read<ParkingSettings>(configfil);

        if (nya == null ||
            nya.FreeMinutes < 0 ||
            nya.VehicleTypes == null)
        {
            throw new JsonException();
        }

        List<decimal> priser = new List<decimal>();

        foreach (VehicleSettings nuvarandeTyp in settings.VehicleTypes)
        {
            VehicleSettings? nyTyp = null;

            foreach (VehicleSettings typ in nya.VehicleTypes)
            {
                if (typ == null)
                {
                    throw new JsonException();
                }

                if (typ.Code == nuvarandeTyp.Code)
                {
                    if (nyTyp != null)
                    {
                        throw new JsonException();
                    }

                    nyTyp = typ;
                }
            }

            if (nyTyp == null || nyTyp.HourlyPrice < 0)
            {
                throw new JsonException();
            }

            priser.Add(nyTyp.HourlyPrice);
        }

        // Uppdatera först när alla priser är kontrollerade.
        for (int i = 0; i < settings.VehicleTypes.Count; i++)
        {
            settings.VehicleTypes[i].HourlyPrice = priser[i];
        }

        settings.FreeMinutes = nya.FreeMinutes;

        Console.WriteLine("Prislistan har lästs in på nytt.");

        foreach (VehicleSettings typ in settings.VehicleTypes)
        {
            Console.WriteLine(
                typ.Name + ": " + typ.HourlyPrice +
                " CZK per påbörjad timme.");
        }

        Console.WriteLine(
            "Gratis parkering i " + settings.FreeMinutes + " minuter.");
    }
    catch (JsonException)
    {
        Console.WriteLine(
            "Prislistan innehåller felaktiga uppgifter. Tidigare priser används.");
    }
    catch (IOException)
    {
        Console.WriteLine(
            "Prislistan kunde inte läsas. Tidigare priser används.");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine(
            "Behörighet saknas för att läsa prislistan. Tidigare priser används.");
    }
}

public class Car : Vehicle
{
}

public class MC : Vehicle
{
}

public class ParkingSpot
{
    public int Number { get; set; }

    public int? Size { get; set; }

    public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ParkingSettings Settings { get; set; } = new ParkingSettings();

    public bool KanParkera(Vehicle fordon)
    {
        string kod = fordon is Car ? "CAR" : "MC";

        VehicleSettings? installning = null;

        foreach (VehicleSettings typ in Settings.VehicleTypes)
        {
            if (typ.Code == kod)
            {
                installning = typ;
                break;
            }
        }

        if (installning == null)
        {
            return false;
        }

        foreach (Vehicle parkeratFordon in Vehicles)
        {
            if (parkeratFordon.GetType() != fordon.GetType())
            {
                return false;
            }
        }

        int platsstorlek = Size ?? Settings.SpotSize;

        return Vehicles.Count < installning.VehiclesPerSpot &&
               Vehicles.Count < platsstorlek / installning.Size;
    }
}
public class ParkingGarage
{
    public List<ParkingSpot> Spots { get; set; } = new List<ParkingSpot>();

    public ParkingSpot? HittaLedigPlats(Vehicle fordon)
    {
        foreach (ParkingSpot plats in Spots)
        {
            if (plats.KanParkera(fordon))
            {
                return plats;
            }
        }

        return null;
    }

    public ParkingSpot? HittaPlatsForFordon(string regnummer)
    {
        foreach (ParkingSpot plats in Spots)
        {
            foreach (Vehicle fordon in plats.Vehicles)
            {
                if (fordon.RegNumber == regnummer)
                {
                    return plats;
                }
            }
        }

        return null;
    }
    public void VisaParkering()
    {
        Table karta = new Table();
        karta.RoundedBorder();
        karta.HideHeaders();

        for (int kolumn = 0; kolumn < 10; kolumn++)
        {
            karta.AddColumn("");
        }

        int lediga = 0;

        for (int start = 0; start < Spots.Count; start += 10)
        {
            string[] rad = new string[10];

            for (int kolumn = 0; kolumn < 10; kolumn++)
            {
                int index = start + kolumn;
                rad[kolumn] = "";

                if (index >= Spots.Count)
                {
                    continue;
                }

                ParkingSpot plats = Spots[index];
                string status;
                string farg;

                if (plats.Vehicles.Count == 0)
                {
                    status = "LEDIG";
                    farg = "green";
                    lediga++;
                }
                else if (plats.KanParkera(new Car()) ||
         plats.KanParkera(new MC()))
                {
                    status = "DELVIS";
                    farg = "yellow";
                }
                else
                {
                    status = "FULL";
                    farg = "red";
                }

                rad[kolumn] =
                    "[" + farg + "]" +
                    plats.Number + " " + status +
                    "[/]";
            }

            karta.AddRow(rad);
        }

        AnsiConsole.Write(karta);

        AnsiConsole.MarkupLine(
            "[green]Lediga platser: " + lediga +
            " av " + Spots.Count + "[/]");

        AnsiConsole.MarkupLine(
            "[green]Grön = tom[/]  " +
            "[yellow]Gul = delvis upptagen[/]  " +
            "[red]Röd = full[/]");

        Console.WriteLine();
        Console.WriteLine("Parkerade fordon:");

        foreach (ParkingSpot plats in Spots)
        {
            foreach (Vehicle fordon in plats.Vehicles)
            {
                string typ = fordon is Car ? "Bil" : "MC";

                Console.WriteLine(
                    "Plats " + plats.Number + ": " +
                    typ + " " + fordon.RegNumber);
            }
        }
    }
}