namespace BudgetApp.Models.Interfaces
{
    public interface IMetadata
    {
        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }
    }
}