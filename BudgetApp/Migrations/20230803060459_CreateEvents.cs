using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;

#nullable disable

namespace BudgetApp.Migrations
{
    /// <inheritdoc />
    public partial class CreateEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //The following creates events that will automatically roll forward fixed costs and income every first of the month
            StringBuilder sbFixedCostRollForward = new StringBuilder();
            sbFixedCostRollForward.Append("CREATE EVENT RollFixedCosts ON SCHEDULE EVERY 1 MONTH STARTS '2023-08-01 00:05:00' ");
            sbFixedCostRollForward.Append("DO INSERT INTO FixedCosts (EntityId, MonthBegin, MonthEnd, Amount, Description, BudgetEntityId, EntityType, Archived, CreatedDate, CreatedBy, UpdatedDate, UpdatedBy) ");
            sbFixedCostRollForward.Append("SELECT UUID(), CURDATE(), LAST_DAY(CURDATE()), Amount, Description, BudgetEntityId, EntityType, 0, CURDATE(), CURRENT_USER(), CURDATE(), CURRENT_USER() ");
            sbFixedCostRollForward.Append("FROM FixedCosts WHERE MonthEnd = DATE_SUB(CURDATE(), INTERVAL 1 DAY) AND Archived = 0;");

            string query = sbFixedCostRollForward.ToString();
            migrationBuilder.Sql(query);

            StringBuilder sbIncomeRollForward = new StringBuilder();
            sbIncomeRollForward.Append("CREATE EVENT RollIncomes ON SCHEDULE EVERY 1 MONTH STARTS '2023-08-01 00:05:00' ");
            sbIncomeRollForward.Append("DO INSERT INTO Incomes (EntityId, MonthBegin, MonthEnd, Amount, Description, Recurring, EntityType, Archived, CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, BudgetEntityId, UserId) ");
            sbIncomeRollForward.Append("SELECT UUID(), CURDATE(), LAST_DAY(CURDATE()), Amount, Description, Recurring, EntityType, Archived, CURDATE(), CURRENT_USER(), CURDATE(), CURRENT_USER(), BudgetEntityId, UserId ");
            sbIncomeRollForward.Append("FROM Incomes WHERE MonthEnd = DATE_SUB(CURDATE(), INTERVAL 1 DAY) AND Archived = 0 AND Recurring = 1;");

            query = sbIncomeRollForward.ToString();
            migrationBuilder.Sql(query);

            /* 
                (How to view events in MySQL)
                SHOW EVENTS
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP EVENT RollFixedCosts");
            migrationBuilder.Sql("DROP EVENT RollIncomes");
        }
    }
}
