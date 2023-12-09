using System.Security.Claims;

namespace BudgetApp.Areas.Identity
{
    public class CurrentUserAccessor : ICurrentUserAccessor
    {
        private IHttpContextAccessor _httpContextAccessor { get; set; }

        public Guid? UserId
        {
            get
            {
                string? userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdString) == false)
                {
                    if (Guid.TryParse(userIdString, out Guid userId))
                    {
                        return userId;
                    }
                }

                return null;
            }
        }

        public string UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name ?? IdentityConstants.UNIDENTIFIED_USER;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
