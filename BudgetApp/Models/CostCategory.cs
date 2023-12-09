using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BudgetApp.Models.Interfaces;

namespace BudgetApp.Models
{
    public class CostCategory : BaseEntity, IArchivableEntity
    {
        public CostCategory()
        {
            EntityId = Guid.NewGuid();
            EntityType = nameof(CostCategory);
        }

        [Display(Name = "Category Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Amount Budgeted")]
        [Required]
        public double BudgetedAmount { get; set; }

        public virtual Budget Budget { get; set; }

        #region Interfaces 
        public bool Archived { get; set; }
        public DateTime? ArchiveDate { get; set; }
        public string? ArchivedBy { get; set; }
        #endregion
    }
}
