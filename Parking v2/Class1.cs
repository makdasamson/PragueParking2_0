public class PriceCalculator
{
    public decimal CalculatePrice(
        TimeSpan parkingTime,
        int freeMinutes,
        decimal hourlyPrice)
    {
        if (parkingTime < TimeSpan.Zero ||
            freeMinutes < 0 ||
            hourlyPrice < 0)
        {
            throw new ArgumentException("Ogiltiga värden för prisberäkningen.");
        }

        if (parkingTime.TotalMinutes <= freeMinutes)
        {
            return 0;
        }

        decimal hours = (decimal)Math.Ceiling(parkingTime.TotalHours);

        return hours * hourlyPrice;
    }
}