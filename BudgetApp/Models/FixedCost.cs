using BudgetApp.Models.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Models
{
    public class FixedCost : BaseEntity, IArchivableEntity
    {
        public FixedCost()
        {
            EntityId = Guid.NewGuid();
            EntityType = nameof(FixedCost);
            Description = string.Empty;
        }

        public DateTime MonthBegin { get; set; }

        public DateTime MonthEnd { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public virtual Budget Budget { get; set; }

        #region Interfaces
        public bool Archived { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public string? ArchivedBy { get; set; }
        #endregion
    }
}
