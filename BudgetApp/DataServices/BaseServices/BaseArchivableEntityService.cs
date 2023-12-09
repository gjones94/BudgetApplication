using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.Models;
using BudgetApp.Models.Interfaces;

namespace BudgetApp.DataServices.BaseServices
{
    public class BaseArchivableEntityService<TEntity> : BaseTransactionEntityService<TEntity> where TEntity : BaseEntity, IArchivableEntity
    {
        /// <summary>
        /// Entity Service that archives entities instead of deleting them from the database
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="currentUserAccessor"></param>
        /// <param name="logger"></param>
        public BaseArchivableEntityService(ApplicationDbContext dbContext, ICurrentUserAccessor currentUserAccessor, ILogger logger) : base(dbContext, currentUserAccessor, logger)
        {

        }

        /// <summary>
        /// Obtains all unarchived entities of type <typeparamref name="TEntity"/>
        /// </summary>
        /// <returns></returns>
        public override IQueryable<TEntity> GetAll()
        {
            return base.GetAll().Where(row => row.Archived == false);
        }

        
        /// <summary>
        /// Obtains all entities (including archived entities) of type <typeparamref name="TEntity"/>
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> GetAllWithArchived()
        {
            return GetAll();
        }

        /// <summary>
        /// Archives an entity
        /// </summary>
        /// <param name="entityToArchive"></param>
        /// <returns></returns>
        public bool Archive(TEntity entityToArchive)
        {
            if (EntityExists(entityToArchive))
            {
                entityToArchive.Archived = true;
                CaptureArchiveInfo(entityToArchive);
                _dbContext.Set<TEntity>().Update(entityToArchive);
                return true;
            }

            return false;
        }

        private void CaptureArchiveInfo(TEntity entityToRemove)
        {
            string userName = _currentUserAccessor.UserName;
            entityToRemove.ArchiveDate = DateTime.Now;
            entityToRemove.ArchivedBy = userName;
        }
    }
}
