namespace BudgetApp.ViewModels.ErrorViewModels
{
    public class ErrorViewModel
    {
        public Guid Id { get; set; }

        public ErrorViewModel() 
        {
            Id = Guid.NewGuid();
        }

        public ErrorViewModel(string title, string message, string action) : base()
        {
            Title = title;
            Message = message;
            Action = action;
        }

        public string? Title { get; set; }

        public string? Message { get; set; }

        public string? Action { get; set; }
    }
}