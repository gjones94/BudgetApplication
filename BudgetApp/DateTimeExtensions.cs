namespace BudgetApp
{
    public static class DateTimeExtensions
    {
        public static DateTime GetMonthBegin(this DateTime date) 
        {
            return new DateTime(date.Year, date.Month, 1);
        }
        public static DateTime GetMonthEnd(this DateTime date) 
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        public static DateTime DateOnly(this DateTime date) 
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }
    }
}
