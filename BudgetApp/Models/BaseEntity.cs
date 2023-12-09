using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models
{
    public abstract class BaseEntity : IIdentifiableEntity, IMetadata
    {
        [Key]
        public Guid EntityId { get; set; }

        public string EntityType { get; set; } = nameof(BaseEntity);

        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
