using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.Models;
using BudgetApp.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace BudgetApp.DataServices.BaseServices
{
    public class BaseTransactionEntityService<TEntity> : BaseEntityService<TEntity> where TEntity : BaseEntity, IMetadata
    {
        protected readonly ICurrentUserAccessor _currentUserAccessor;

        protected enum EntityTransactionType
        {
            Add,
            Update,
            Remove
        }

        public BaseTransactionEntityService(ApplicationDbContext dbContext, ICurrentUserAccessor currentUserAccessor, ILogger logger) : base(dbContext, logger)
        {
            _dbContext = dbContext;
            _currentUserAccessor = currentUserAccessor;
        }

        public override bool Add(TEntity entityToAdd)
        {
            CaptureEntityTransactionInfo(entityToAdd, EntityTransactionType.Add);
            return base.Add(entityToAdd);
        }

        public override bool Update(TEntity entityToUpdate)
        {
            CaptureEntityTransactionInfo(entityToUpdate, EntityTransactionType.Update);
            bool success = base.Update(entityToUpdate);

            if(success)
            {
                _dbContext.Entry(entityToUpdate).Property(x => x.CreatedBy).IsModified = false;
                _dbContext.Entry(entityToUpdate).Property(x => x.CreatedDate).IsModified = false;
            }

            return success;
        }

        private void CaptureEntityTransactionInfo(TEntity entity, EntityTransactionType type)
        {
            string userName = _currentUserAccessor.UserName;

            switch (type)
            {
                case EntityTransactionType.Add:
                    entity.CreatedBy = userName;
                    entity.CreatedDate = DateTime.Now;
                    entity.UpdatedBy = userName;
                    entity.UpdatedDate = DateTime.Now;
                    break;
                case EntityTransactionType.Update:
                    entity.UpdatedBy = userName;
                    entity.UpdatedDate = DateTime.Now;
                    break;
                default:
                    break;
            }
        }

        public override int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }
    }
}
