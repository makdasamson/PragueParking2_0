public class ParkingSettings
{
    public int NumberOfSpots { get; set; } = 100;
    public int SpotSize { get; set; } = 4;
    public int FreeMinutes { get; set; } = 10;

    public List<VehicleSettings> VehicleTypes { get; set; } =
        new List<VehicleSettings>
        {
            new VehicleSettings
            {
                Code = "CAR",
                Name = "Bil",
                Size = 4,
                VehiclesPerSpot = 1,
                HourlyPrice = 20
            },
            new VehicleSettings
            {
                Code = "MC",
                Name = "MC",
                Size = 2,
                VehiclesPerSpot = 2,
                HourlyPrice = 10
            }
        };
}

public class VehicleSettings
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Size { get; set; }
    public int VehiclesPerSpot { get; set; }
    public decimal HourlyPrice { get; set; }
}