using Domain.Core.Events;

namespace Domain.Events.Student
{
    public class StudentRemovedEvent : Event
    {
        public StudentRemovedEvent(Guid id)
        {
            Id = id;
            AggregateId = id;
        }

        public Guid Id { get; set; }
    }
}
