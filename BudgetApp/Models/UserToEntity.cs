using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models.Interfaces;

namespace BudgetApp.Models
{
    public class UserToEntity
    {
        public UserToEntity()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid EntityId { get; set; }

        public string? EntityType { get; set; }

        public string? Role { get; set; }
    }
}
