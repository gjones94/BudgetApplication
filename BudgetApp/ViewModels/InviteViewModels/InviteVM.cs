using BudgetApp.Models;

namespace BudgetApp.ViewModels.InviteViewModels
{
    public class InviteVM
    {
        public InviteVM()
        {
            InviterName = string.Empty;
        }

        public Guid InviteId { get; set; }

        public string InviterName { get; set; }

        public bool InvitationAccepted { get; set; }
    }
}
