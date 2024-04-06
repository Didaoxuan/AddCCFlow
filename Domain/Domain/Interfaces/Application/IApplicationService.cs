using Domain.Interfaces.UnitOfWork;

namespace Domain.Interfaces.Application
{
    public interface IApplicationService
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
