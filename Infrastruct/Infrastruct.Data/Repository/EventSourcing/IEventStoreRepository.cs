using Domain.Core.Events;

namespace Infrastruct.Data.Repository.EventSourcing
{
    /// <summary>
    /// 事件存储仓储接口
    /// 继承IDisposable ，可手动回收
    /// </summary>
    public interface IEventStoreRepository : IDisposable
    {
        void Store(StoredEvent theEvent);
        IList<StoredEvent> All(Guid aggregateId);
    }
}
