using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.Models;
using BudgetApp.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace BudgetApp.DataServices.BaseServices
{
    public class BaseEntityService<TEntity> where TEntity : BaseEntity
    {
        protected ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        /// <summary>
        /// Generic service that provides basic CRUD operations for all entities
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        public BaseEntityService(ApplicationDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an object and it's related entities (if included) from the database.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>An entity of type <typeparamref name="TEntity"/> from the provided Id if it exists</returns>
        public virtual TEntity? GetById(Guid Id, params Expression<Func<TEntity, object>>[] relatedPropertiesToLoad) 
        {
            IQueryable<TEntity> query = GetAll().Where(row => row.EntityId == Id).AsQueryable();

            foreach(var property in relatedPropertiesToLoad)
            {
                //eager load any properties that the user specified
                query = query.Include(property);
            }

            TEntity? entity = query.FirstOrDefault();

            return entity;
        }

        /// <summary>
        /// Provides all entities of type <typeparamref name="TEntity"/> from the context
        /// </summary>
        /// <returns><see cref="IQueryable<typeparamref name="TEntity"/>"/></returns>
        public virtual IQueryable<TEntity> GetAll()
        {
            return _dbContext.Set<TEntity>();
        }

        /// <summary>
        /// Adds an entity if it does not exist
        /// </summary>
        /// <param name="entityToAdd"></param>
        /// <returns><see cref="Boolean"/> indicating success or failure</returns>
        public virtual bool Add(TEntity entityToAdd)
        {
            bool entityExists = EntityExists(entityToAdd);

            if (entityExists == false)
            {
                _dbContext.Add(entityToAdd);
                return true;
            }

            _logger.LogError($"Failed to add entity. Entity {entityToAdd} already exists.");
            return false;
        }

        /// <summary>
        /// Updates an entity if it exists
        /// </summary>
        /// <param name="entityToUpdate"></param>
        /// <returns><see cref="Boolean"/> indicating success or failure</returns>
        public virtual bool Update(TEntity entityToUpdate)
        {
            bool entityExists = EntityExists(entityToUpdate);

            if (entityExists)
            {
                _dbContext.Set<TEntity>().Update(entityToUpdate);
                return true;
            }

            _logger.LogError($"Failed to update entity. Entity {entityToUpdate} does not exist.");
            return false;
        }

        /// <summary>
        /// Permanently removes an entity from the database
        /// </summary>
        /// <param name="entityToRemove"></param>
        public virtual void Remove(TEntity entityToRemove)
        {
            _dbContext.Remove(entityToRemove);
        }

        /// <summary>
        /// Saves changes in the context change tracker
        /// </summary>
        /// <returns></returns>
        public virtual int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }

        /// <summary>
        /// Checks to see if an entity with the provided Id exists
        /// </summary>
        /// <param name="entity"></param>
        /// <returns><see cref="Boolean"/> of whether entity exists or not</returns>
        public bool EntityExists(TEntity entity)
        {
            return _dbContext.Set<TEntity>().Any(row => row.EntityId == entity.EntityId);
        }
    }
}
