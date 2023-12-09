using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BudgetApp
{
    public static class HttpExtensions
    {
        public static string BaseUrl(this HttpContext context) 
        {
            return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
        }
    }
}
