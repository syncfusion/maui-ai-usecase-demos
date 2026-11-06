using SmartVehicleCare.Models;

namespace SmartVehicleCare.Helpers;

internal static class VehicleCenterCalculations
{
    internal enum ExpenseBucket
    {
        Day,
        Week,
        Month,
    }

    internal static (int Days, int Kilometres) GetServiceInterval(Vehicle vehicle)
        => vehicle.ServiceInterval switch
        {
            "3 months / 3,000 km" => (90, 3000),
            "6 months / 5,000 km" => (180, 5000),
            "1 year / 10,000 km" => (365, 10000),
            "Every 10,000 km" => (180, 10000),
            "Every 5,000 km" => (180, 5000),
            _ when vehicle.VehicleType == 1 => (180, 5000),
            _ => (180, 10000),
        };

    internal static bool TryParseMileage(string? mileage, out int value)
    {
        var digits = new string((mileage ?? string.Empty).Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out value);
    }

    internal static double ParseCurrency(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        var cleaned = value.Replace("₹", string.Empty)
                           .Replace(",", string.Empty)
                           .Trim();

        return double.TryParse(cleaned, out var parsed) ? parsed : 0;
    }

    internal static double GetNiceAxisInterval(double maximum)
    {
        if (maximum <= 0)
            return 1000;

        var roughInterval = maximum / 4;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughInterval)));
        var normalized = roughInterval / magnitude;
        var niceNormalized = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;

        return niceNormalized * magnitude;
    }

    internal static DateTime ExpenseBucketStart(DateTime date, ExpenseBucket bucket)
        => bucket switch
        {
            ExpenseBucket.Day => date.Date,
            ExpenseBucket.Week => StartOfWeek(date.Date),
            _ => new DateTime(date.Year, date.Month, 1),
        };

    internal static DateTime NextExpenseBucket(DateTime date, ExpenseBucket bucket)
        => bucket switch
        {
            ExpenseBucket.Day => date.AddDays(1),
            ExpenseBucket.Week => date.AddDays(7),
            _ => date.AddMonths(1),
        };

    internal static double HaversineKm(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double earthRadiusKm = 6371;
        var latitudeDelta = (latitude2 - latitude1) * Math.PI / 180;
        var longitudeDelta = (longitude2 - longitude1) * Math.PI / 180;
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
              + Math.Cos(latitude1 * Math.PI / 180) * Math.Cos(latitude2 * Math.PI / 180)
              * Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
