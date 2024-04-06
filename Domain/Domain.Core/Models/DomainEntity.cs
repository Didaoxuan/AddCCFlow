using Domain.Core.Interfaces.Domain;

namespace Domain.Core.Models
{
    /// <summary>
    /// 定义领域实体基类
    /// </summary>
    public abstract class DomainEntity<TPrimaryKey> : Entity<TPrimaryKey>, IDomainEntity<TPrimaryKey>
    {

    }
}
