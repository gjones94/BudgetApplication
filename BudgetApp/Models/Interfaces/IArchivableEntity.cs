namespace BudgetApp.Models.Interfaces
{
    public interface IArchivableEntity
    {
        public bool Archived { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public string? ArchivedBy { get; set; }
    }
}