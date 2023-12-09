using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BudgetApp
{
    public static class GeneralExtensions
    {
        public static string GetCurrencyFormat(this double currency)
        {
            return string.Format("$ {0:N2}", currency);
        }
    }
}
