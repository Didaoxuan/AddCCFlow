using Domain.Interfaces.UnitOfWork;
using Infrastruct.Data.Context;

namespace Infrastruct.Data.UnitOfWork
{
    /// <summary>
    /// 工作单元类
    /// </summary>
    public class UnitOfWork(StudyContext context) : IUnitOfWork
    {
        //数据库上下文
        private readonly StudyContext _context = context;

        //上下文提交
        public bool Commit()
        {
            return _context.SaveChanges() > 0;
        }

        //手动回收
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
