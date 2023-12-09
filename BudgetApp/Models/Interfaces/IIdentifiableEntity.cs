namespace BudgetApp.Models.Interfaces
{
    public interface IIdentifiableEntity
    {
        public Guid EntityId { get; set; }

        public string EntityType { get; }
    }
}