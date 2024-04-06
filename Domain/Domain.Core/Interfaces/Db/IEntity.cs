namespace Domain.Core.Interfaces.Db
{
    public interface IEntity
    {
    }

    public interface IEntity<TPrimaryKey> : IEntity
    {
        TPrimaryKey Id { get; }
    }
}
