using System.Data;

namespace Domain.Interfaces.UnitOfWork
{
    /// <summary>
    /// 工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        //是否提交成功
        bool Commit();
    }

    public interface IInterceptorUnitOfWork
    {
        void Begin(IsolationLevel level = IsolationLevel.Unspecified);
        void SaveChanges();
        void Failed();
    }
}
