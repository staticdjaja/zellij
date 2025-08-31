namespace zellij.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AddBusinessDays(this DateTime date, int minDays, int maxDays)
        {
            var businessDaysToAdd = new Random().Next(minDays, maxDays + 1);
            var result = date;
            var daysAdded = 0;

            while (daysAdded < businessDaysToAdd)
            {
                result = result.AddDays(1);
                if (result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday)
                {
                    daysAdded++;
                }
            }

            return result;
        }
    }
}