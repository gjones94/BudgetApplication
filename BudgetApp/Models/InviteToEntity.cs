using BudgetApp.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models
{
    public class InviteToEntity
    {

        public InviteToEntity()
        {
            Id = Guid.NewGuid();
            InvitedUserEmail = string.Empty;
            EntityType = string.Empty;
            Token = string.Empty;
        }

        public Guid Id { get; private set; }

        public Guid InviterUserId { get; set; }

        [Display(Name = "Email Of User")]
        [Required(ErrorMessage = "Email is required")]
        public string InvitedUserEmail { get; set; }

        public string Token { get; set; }

        public Guid EntityId { get; set; }

        public string EntityType { get; set; }

        public DateTime ExpirationDate { get; set; }

        public bool IsExpired { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
