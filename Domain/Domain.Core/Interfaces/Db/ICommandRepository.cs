using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Interfaces.Db
{
    public interface ICommandRepository<TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>, ITransactionRepository
        where TEntity : class, IEntity<TPrimaryKey>
    {
        int Insert(TEntity entity);

        Task<int> InsertAsync(TEntity entity);

        int Update(TEntity entity);

        Task<int> UpdateAsync(TEntity entity);

        int Delete(TPrimaryKey key);

        Task<int> DeleteAsync(TPrimaryKey key);
    }
}
