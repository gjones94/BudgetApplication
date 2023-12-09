using BudgetApp.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Models
{
    public class Income : BaseEntity, IArchivableEntity
    {
        public Income()
        {
            EntityId = Guid.NewGuid();
            EntityType = nameof(Income);
            Description = string.Empty;
            Recurring = true;
        }

        public DateTime MonthBegin { get; set; }

        public DateTime MonthEnd { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public bool Recurring { get; set; }

        public Guid UserId { get; set; } //Loose coupling for user management stuff

        public virtual Budget Budget { get; set; }

        #region Interfaces
        public bool Archived { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public string? ArchivedBy { get; set; }
        #endregion
    }
}
