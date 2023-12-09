using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.VariableCostViewModels
{
    public class VariableCostVM
    {
        public VariableCostVM()
        {
            DateIncurred = DateTime.Now;
            Description = string.Empty;
            CategoryName = string.Empty;
            NameOfUser = string.Empty;
        }

        public Guid VariableCostId { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Date Incurred")]
        [DataType(DataType.Date)]
        public DateTime DateIncurred { get; set; }

        public string CategoryName { get; set; }
        
        public string NameOfUser { get; set; }
    }
}
