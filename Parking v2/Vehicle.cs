using System.Text.Json.Serialization;

[JsonDerivedType(typeof(Car), "CAR")]
[JsonDerivedType(typeof(MC), "MC")]
public class Vehicle
{
    public string RegNumber { get; set; } = "";
    public DateTime ArrivalTime { get; set; } = DateTime.Now;
}