using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.FixedCostViewModels
{
    public class FixedCostVM
    {
        public Guid FixedCostId { get; set; }

        [Required( ErrorMessage = "Amount is required")]
        public double Amount { get; set; }

        [Required]
        public string Description { get; set; }

    }
}
