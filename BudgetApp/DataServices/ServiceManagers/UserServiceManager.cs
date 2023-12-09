using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using BudgetApp.Data;
using BudgetApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MySqlX.XDevAPI.Relational;
using NETCore.MailKit.Core;
using NuGet.Versioning;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace BudgetApp.DataServices.ServiceManagers
{

    public enum UserAction
    {
        Create,
        Read,
        Update,
        Delete,
        ModifyUser
    }

    public class UserServiceManager
    {
        UserManager<User> _userManager;
        ApplicationDbContext _dbContext;
        ICurrentUserAccessor _currentUserAccessor;
        IEmailSender _emailSender;
        ILogger _logger;

        public UserServiceManager(ApplicationDbContext dbContext, UserManager<User> userManager, IEmailSender emailSender, ICurrentUserAccessor userAccessor, ILogger<UserServiceManager> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _currentUserAccessor = userAccessor;
            _emailSender = emailSender;
            _logger = logger;
        }

        #region User
        /// <summary>
        /// Returns the current user ID
        /// </summary>
        /// <returns>Guid or null if not found</returns>
        public Guid? GetCurrentUserId()
        {
            return _currentUserAccessor.UserId;
        }

        /// <summary>
        /// Returns the current user
        /// </summary>
        /// <returns>User or null if not found</returns>
        public User? GetCurrentUser()
        {
            Guid? userId = _currentUserAccessor.UserId;
            User? user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

            return user;
        }

        /// <summary>
        /// Returns the User from the User's Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User</returns>
        public User? GetUserById(Guid userId)
        {
            return _userManager.Users.FirstOrDefault(row => row.Id == userId);
        }

        /// <summary>
        /// Returns the Email of the user from the user's Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>String</returns>
        public string? GetUserEmail(Guid userId)
        {
            return _userManager.Users.Where(u => u.Id == userId).FirstOrDefault()?.Email;
        }

        /// <summary>
        /// Returns the first and last name of a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUserFullName(Guid userId)
        {
            string? name = _userManager.Users.Where(row => row.Id == userId)?.FirstOrDefault()?.FullName;
            return name ?? Areas.Identity.IdentityConstants.UNIDENTIFIED_USER;
        }

        /// <summary>
        /// Obtains a user by the email provided
        /// </summary>
        /// <param name="email"></param>
        /// <returns><see cref="User">User</see> (If found) </returns>
        public async Task<User?> GetUserByEmail(string email)
        {
            User? user = await _userManager.FindByEmailAsync(email);

            return user;
        }

        /// <summary>
        /// Automatically confirms a user email.
        /// </summary>
        /// <remarks>
        /// This is only to be used for the scenario in which 
        /// a user was invited and so their email has already been validated
        /// by the invite link that they used
        /// </remarks>
        /// <param name="user"></param>
        public void ConfirmUser(User user)
        {
            User? userFromDb = _dbContext.Set<User>().Find(user.Id);
            if(userFromDb != null)
            {
                userFromDb.ApprovedByAdmin = true;
                userFromDb.EmailConfirmed = true;
            }
            _dbContext.SaveChanges();
        }
        #endregion

        #region UserToEntity
        /// <summary>
        /// Gets Entity For a User
        /// </summary>
        /// <remarks>
        /// This assumes that there is only one of this type of entity assigned to the user
        /// </remarks>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="userId"></param>
        /// <returns>An Entity of type TEntity specified</returns>
        public TEntity? GetEntityForUser<TEntity>(Guid userId) where TEntity : BaseEntity
        {
            Guid entityId = _dbContext.Set<UserToEntity>().Where(row => row.UserId == userId && row.EntityType == typeof(TEntity).Name).Select(row => row.EntityId).FirstOrDefault();

            TEntity? entityToReturn = _dbContext.Set<TEntity>().FirstOrDefault(row => row.EntityId == entityId);

            return entityToReturn;
        }

        /// <summary>
        /// This links a user to entity (only allows one relationship to this entity type)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityId"></param>
        /// <param name="roleForEntity"></param>
        /// <returns>True if successful, False if not</returns>
        public bool AddUserToEntity(Guid userId, Guid entityId, string entityType, string roleForEntity)
        {
            bool _entityExistsForUser = _dbContext.Set<UserToEntity>().Where(row => row.UserId == userId && row.EntityId == entityId).Any();
            if(_entityExistsForUser)
            {
                return false;
            }
            _dbContext.Set<UserToEntity>().Add(new UserToEntity() { UserId = userId, EntityId = entityId, Role = roleForEntity, EntityType = entityType });
            _dbContext.SaveChanges();
            _logger.LogInformation($"Successfully added user to {entityType} as {roleForEntity}");
            return true;
        }

        /// <summary>
        /// Removes the user to entity relationship if it exists
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool Remove(Guid userId, Guid entityId)
        {
            UserToEntity? entityToRemove = _dbContext.Set<UserToEntity>().FirstOrDefault(row => row.UserId == userId && row.EntityId == entityId);

            if (entityToRemove != null)
            {
                _dbContext.Remove(entityToRemove);
                return true;
            }

            _logger.LogError("Relationship does not exist");
            return false;
        }

        public void RemoveAllUsersForEntity (Guid entityId)
        {
            var usersToEntity = GetAllRelationshipsForEntity(entityId);
            foreach(UserToEntity relationship in usersToEntity)
            {
                _dbContext.Remove(relationship);
            }
        }

        public void RemoveAllEntitiesForUser(Guid userId)
        {
            var userEntities = GetAllRelationshipsForUser(userId);
            if(userEntities.Count > 0)
            {
                _dbContext.RemoveRange(userEntities);
            }
        }

        /// <summary>
        /// Removes a user from an entity and prevents leaving orphan children of the entity without a proper authority
        /// </summary>
        /// <remarks>
        /// Whenever a user has access authority over an entity is deleted, there needs to be a mechanism
        /// in place to make sure that the entity maintains one user account with proper authority if other users
        /// still exist on this entity. Otherwise, you will be left with an entity that has users with no ability
        /// to control the ownership of the entity.
        /// </remarks>
        /// <param name="userId">User needing to be removed</param>
        /// <param name="entityType">Entity needing to be removed from</param>
        /// <param name="authoritativeRole">This is the role that will be assigned to another random user if necessary</param>
        public bool RemoveFromEntitySafely(Guid userId, Guid entityId, string authoritativeRole)
        {
            UserToEntity? userToEntity = GetUserToEntity(userId, entityId);

            if (userToEntity != null)
            {
                var usersToEntity = GetAllRelationshipsForEntity(entityId).Where(row => row.UserId != userId).ToList();

                if(usersToEntity.Count > 0)
                {
                    var usersInNeededRole = usersToEntity.Where(row => row.Role == authoritativeRole).ToList();

                    if(usersInNeededRole.Count == 0)
                    {
                        //Need to assign one of these users to the authoritative role
                        var newEntityUserAuthority = usersToEntity[0];
                        newEntityUserAuthority.Role = authoritativeRole;

                        User? user = GetUserById(newEntityUserAuthority.UserId);
                        if(user != null && user.Email != null)
                        {
                            //Notify random user of their new power
                            string subject = "BudgetApp - Transfer of Responsibility";
                            string message = "<div>We regret to inform you that the current owner of your budget has abandoned you.</div>" +
                                             "<div>Due to no merit of your own, you have been selected as the new owner of the budget.</div>" +
                                             "<hr/>" +
                                             "<div><strong>Remember, with great power comes great financial responsibility.</strong></div>";

                            _emailSender.SendEmailAsync(user.Email, subject, message);
                        }
                    }
                }
                _dbContext.Remove(userToEntity);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }
        

        /// <summary>
        /// Returns a list of all user relationships for an entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public IList<UserToEntity> GetAllRelationshipsForEntity(Guid entityId)
        {
            var userRolesToEntity = _dbContext.Set<UserToEntity>().Where(row => row.EntityId == entityId);

            return userRolesToEntity.ToList();
        }

        public IList<UserToEntity> GetAllRelationshipsForUser(Guid userId)
        {
            var userRolesToEntity = _dbContext.Set<UserToEntity>().Where(row => row.UserId == userId);

            return userRolesToEntity.ToList();
        }

        public IList<User> GetAllUsersForEntity(Guid entityId)
        {
            List<UserToEntity> userToEntities = GetAllRelationshipsForEntity(entityId).ToList();
            List<User> Users = new List<User>();
            if(userToEntities.Count > 0) 
            { 
                foreach(var userEntity in userToEntities)
                {
                    User? user = GetUserById(userEntity.UserId);
                    if(user != null)
                    {
                        Users.Add(user);
                    }
                }
            }

            return Users;
        }

        public UserToEntity? GetUserToEntity(Guid userId, Guid entityId)
        {
            return _dbContext.Set<UserToEntity>().Where(row => row.UserId == userId && row.EntityId == entityId).FirstOrDefault();
        }
        #endregion

        #region Roles
        public string GetUserRoleForEntity(Guid? userId, Guid entityId)
        {
            var role = _dbContext.Set<UserToEntity>().Where(row => row.EntityId == entityId && row.UserId == userId).FirstOrDefault()?.Role;
            return role ?? string.Empty;
        }

        public bool UserIsOwnerOfEntity(Guid userId, Guid entityId)
        {
            UserToEntity? userToEntity = GetUserToEntity(userId, entityId);
            if (userToEntity == null)
            {
                return false;
            }

            return (userToEntity.Role == RoleName.Owner);
        }

        public async Task<bool> IsInRole(Guid userId, string roleName)
        {
            User? user = GetUserById(userId);
            if(user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> DeleteUserAccount(User user)
        {
            IdentityResult result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        #endregion

        #region Permissions
        /// <summary>
        /// Check to make sure that a user is authorized to perform the requested action
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityId"></param>
        /// <param name="action"></param>
        /// <returns>True if authorized, false if not authorized</returns>
        public bool UserIsAuthorizedForAction(Guid userId, Guid entityId, UserAction action)
        {
            UserToEntity? userToEntity = GetUserToEntity(userId, entityId);

            if(userToEntity == null)
            {
                return false;
            }

            if(userToEntity.Role == RoleName.Admin)
            {
                return true;
            }

            switch(action)
            {
                case UserAction.Create:
                case UserAction.Update:
                    return (userToEntity.Role == RoleName.User || userToEntity.Role == RoleName.Owner);
                case UserAction.Delete:
                case UserAction.ModifyUser:
                    return (userToEntity.Role == RoleName.Owner);
                default:
                    return false;
            }
        }


        #endregion

        #region Invitations
        public bool InviteAlreadyExistsForUser(InviteToEntity invite)
        {
            InviteToEntity? existingInviteForUser = _dbContext.Set<InviteToEntity>().Where(row => row.EntityId == invite.EntityId && row.InviterUserId == invite.InviterUserId && row.IsExpired == false).FirstOrDefault();

            if (existingInviteForUser != null)
            {
                if (existingInviteForUser.ExpirationDate <= DateTime.Now)
                {
                    //expire existing invite, and allow another invite to be sent
                    ExpireInvite(existingInviteForUser.Id);
                }
                else
                {
                    return true; // a non-expired valid invite exists for this user already
                }
            }

            return false;
        }

        public InviteToEntity? GetInviteFromToken(string token, out bool isExpired, out bool invalidToken)
        {
            InviteToEntity? inviteToEntity = _dbContext.Set<InviteToEntity>().Where(row => row.Token == token && row.IsExpired == false).FirstOrDefault();

            isExpired = false;
            invalidToken = false;

            if (inviteToEntity != null)
            {
                if (inviteToEntity.ExpirationDate <= DateTime.Now) //not yet marked, need to mark as expired
                {
                    ExpireInvite(inviteToEntity.Id);
                    isExpired = true;
                }
            }
            else //no entity was returned
            {
                invalidToken = true; 
            }

            return (isExpired) ? null : inviteToEntity;
        }

        public InviteToEntity? GetInviteById(Guid InviteId)
        {
            return _dbContext.Set<InviteToEntity>().FirstOrDefault(row => row.Id == InviteId);
        }

        public void AddInvitationForUser(InviteToEntity invite)
        {
            _dbContext.Set<InviteToEntity>().Add(invite);
            _dbContext.SaveChanges();
        }

        public void ExpireInvite(Guid inviteId)
        {
            InviteToEntity? inviteFromDb = _dbContext.Set<InviteToEntity>().FirstOrDefault(row => row.Id == inviteId);
            if(inviteFromDb != null)
            {
                inviteFromDb.IsExpired = true;
            }
            _dbContext.SaveChanges();
        }
        #endregion


        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }
    }
}
