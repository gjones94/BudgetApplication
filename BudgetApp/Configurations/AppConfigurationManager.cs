namespace BudgetApp.Configurations
{
    public static class AppConfigurationManager
    {
        public static void Initialize(IConfiguration configurations)
        {
            #region Credentials
            Credentials.AdminEmail = configurations.GetSection("Admin")["AdminEmail"];
            Credentials.AdminPassword = configurations.GetSection("Admin")["AdminPassword"];
            #endregion

            #region Connections

            #if DEBUG
            Connections.ConnectionString = configurations.GetSection("ConnectionStrings")["MySqlConnectionDev"];
            #else
            Connections.ConnectionString = configurations.GetSection("ConnectionStrings")["MySqlConnection"];
            #endif

            #endregion

            #region Invitations
            IConfigurationSection InvitationSection = configurations.GetSection("Invitations");
            bool success = int.TryParse(InvitationSection["MinutesUntilExpiration"], out int parsedMinutes);
            if (success)
            {
                Invitations.MinutesUntilExpiration = parsedMinutes;
            }
            #endregion

        }

    }
}
