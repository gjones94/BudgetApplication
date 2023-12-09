using BudgetApp.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models
{
    public class Budget : BaseEntity, IArchivableEntity
    {
        public Budget()
        {
            EntityId = Guid.NewGuid();
            EntityType = nameof(Budget);
        }

        [Display(Name = "Monthly Savings Goal")]
        public double MonthlySavingsGoal { get; set; }

        #region Interface
        public bool Archived { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public string? ArchivedBy { get; set; }
        #endregion

    }
}
