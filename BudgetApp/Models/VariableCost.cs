using BudgetApp.Models.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Models
{
    public class VariableCost : BaseEntity, IArchivableEntity
    {
        public VariableCost()
        {
            EntityId = Guid.NewGuid();
            EntityType = nameof(VariableCost);
            Description = string.Empty;
        }

        [Required(ErrorMessage = "Amount is required")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public Guid UserId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Transaction Date")]
        public DateTime DateIncurred { get; set; }

        public virtual Budget Budget { get; set; }

        public virtual CostCategory Category { get; set; }

        #region Interfaces
        public bool Archived { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public string? ArchivedBy { get; set; }
        #endregion
    }
}
