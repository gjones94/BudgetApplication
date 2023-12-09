using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

       /// <summary>
       ///  Create Composite Keys Used In DbContext
       /// </summary>
       /// <param name="builder"></param>

        public DbSet<UserToEntity> UserToEntities { get; set; }

        public DbSet<InviteToEntity> InviteToEntities { get; set; }

        public DbSet<Income> Incomes { get; set; }

        public DbSet<FixedCost> FixedCosts { get; set; }

        public DbSet<VariableCost> VariableCosts { get; set; }

        public DbSet<CostCategory> CostCategories { get; set; }

        public DbSet<Budget> Budgets { get; set; }
    }
}