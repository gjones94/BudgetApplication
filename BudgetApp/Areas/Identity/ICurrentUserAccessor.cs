namespace BudgetApp.Areas.Identity
{
    public interface ICurrentUserAccessor
    {
        Guid? UserId { get; }

        string UserName { get; }

        bool IsAuthenticated { get; }
    }
}