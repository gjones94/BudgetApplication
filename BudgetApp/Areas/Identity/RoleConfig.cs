using BudgetApp.Areas.Identity.Models;

namespace BudgetApp.Areas.Identity
{
    public static class RoleId
    {
        public const string Admin = "D85E2BFA-A9B4-4D15-BE5F-CF7D9A7CB714";
        public const string Owner = "3E925C06-C229-48A3-BC2B-7FB09E815AFD";
        public const string User = "F5957B92-9AE3-4A90-BD12-3C9C85EA2203";
    }

    public static class RoleName
    {
        public const string Admin = "Admin";
        public const string Owner = "Owner";
        public const string User = "User";
    }

    public static class RoleDescription
    {
        public const string Admin = "Ability to administer user access and has access to all objects";
        public const string Owner = "Ability to administer user access to objects that the user owns";
        public const string User = "Ability to access objects associated to user";
    }

    public static class RoleProvider
    {
        public static List<Role> GetAllRoles()
        {
            List<Role> roles = new List<Role>()
            {
                new Role() { Id = new Guid(RoleId.Admin), Name = RoleName.Admin, NormalizedName = RoleName.Admin.Normalize(), Description = RoleDescription.Admin },
                new Role() { Id = new Guid(RoleId.Owner), Name = RoleName.Owner, NormalizedName = RoleName.Owner.Normalize(), Description = RoleDescription.Owner },
                new Role() { Id = new Guid(RoleId.User), Name = RoleName.User, NormalizedName = RoleName.User.Normalize(), Description = RoleDescription.User },
            };

            return roles;
        }

        public static List<string> GetNonAdminRoles()
        {
            return GetAllRoles().Where(row => row.Name != RoleName.Admin).Select(row => row.Name).ToList();
        }
    }
}
