using System.Security.Policy;

namespace BudgetApp.ViewModels
{
    public class EntityCollectionVM<TItem>
    {
        public EntityCollectionVM() 
        { 
            EntityItems = new List<TItem>();
            EntityType = nameof(TItem);
            ReturnUrl = string.Empty;
        }

        public Guid EntityId { get; set; }

        public string EntityType { get; set; }

        public IList<TItem> EntityItems { get; set; }

        public string ReturnUrl { get; set; }
    }
}
