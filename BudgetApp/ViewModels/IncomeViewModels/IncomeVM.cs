using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.IncomeViewModels
{
    public class IncomeVM
    {
        public IncomeVM()
        {
            IncomeId = Guid.NewGuid();
            Description = string.Empty;
            Recurring = true;
        }

        public Guid IncomeId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public double Amount { get; set; }

        public string? NameOfUser { get; set; }

        public bool Recurring { get; set; }
    }
}
