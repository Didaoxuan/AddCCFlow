using Application.EventSourcedNormalizers;
using Application.Interfaces;
using Application.ViewModels;
using AutoMapper;
using Domain.Commands.Student;
using Domain.Core.Bus;
using Domain.Interfaces;
using Infrastruct.Data.Repository.EventSourcing;

namespace Application.Services
{
    /// <summary>
    /// StudentAppService 服务接口实现类,继承 服务接口
    /// 通过 DTO 实现视图模型和领域模型的关系处理
    /// 作为调度者，协调领域层和基础层，
    /// 这里只是做一个面向用户用例的服务接口,不包含业务规则或者知识
    /// </summary>
    public class StudentAppService(
        IStudentRepository StudentRepository,
        IMapper mapper,
        IMediatorHandler bus,
        IEventStoreRepository eventStoreRepository
            ) : IStudentAppService
    {
        public IEnumerable<StudentViewModel> GetAll()
        {
            //第一种写法 Map
            return mapper.Map<IEnumerable<StudentViewModel>>(StudentRepository.GetAll());

            //第二种写法 ProjectTo
            //return (_StudentRepository.GetAll()).ProjectTo<StudentViewModel>(_mapper.ConfigurationProvider);
        }

        public StudentViewModel GetById(Guid id)
        {
            return mapper.Map<StudentViewModel>(StudentRepository.GetById(id));
        }

        public void Register(StudentViewModel StudentViewModel)
        {
            //这里引入领域设计中的写命令 还没有实现
            //请注意这里如果是平时的写法，必须要引入Student领域模型，会造成污染

            //_StudentRepository.Add(_mapper.Map<Student>(StudentViewModel));
            //_StudentRepository.SaveChanges();

            var registerCommand = mapper.Map<RegisterStudentCommand>(StudentViewModel);
            bus.SendCommand(registerCommand);
        }

        public void Update(StudentViewModel StudentViewModel)
        {
            var updateCommand = mapper.Map<UpdateStudentCommand>(StudentViewModel);
            bus.SendCommand(updateCommand);
        }

        public void Remove(Guid id)
        {
            var removeCommand = new RemoveStudentCommand(id);
            bus.SendCommand(removeCommand);
        }

        /// <summary>
        /// 获取某一个聚合id下的所有事件，也就是得到了历史记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<StudentHistoryData> GetAllHistory(Guid id)
        {
            return StudentHistory.ToJavaScriptStudentHistory(eventStoreRepository.All(id));
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}