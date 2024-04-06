using Domain.Core.Interfaces.Db;

namespace Domain.Core.Interfaces.Domain
{
    /// <summary>
    /// 聚合根接口定义
    /// </summary>
    /// <typeparam name="TPrimaryKey"></typeparam>
    public interface IAggregateRoot<TPrimaryKey> : IEntity<TPrimaryKey>
    {
    }
}
