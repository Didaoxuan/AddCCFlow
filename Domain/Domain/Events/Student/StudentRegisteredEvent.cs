using Domain.Core.Events;

namespace Domain.Events.Student
{
    /// <summary>
    /// Student被添加后引发事件
    /// 继承事件基类标识
    /// </summary>
    public class StudentRegisteredEvent(Guid id, string name, string email, DateTime birthDate, string phone) : Event
    {
        public Guid Id { get; set; } = id;
        public string Name { get; private set; } = name;
        public string Email { get; private set; } = email;
        public DateTime BirthDate { get; private set; } = birthDate;
        public string Phone { get; private set; } = phone;
    }
}
