namespace Analytics.Domain.Metrics;

public static class ConversionRateCalculator
{
    public static decimal Calculate(long views, long orders) =>
        views == 0 ? 0 : Math.Round((decimal)orders / views * 100, 2);
}
