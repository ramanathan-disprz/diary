using backend.Models;
using backend.Repository.CrudRepository;

namespace backend.Repository;

public interface IEventRepository : ICrudRepository<Event>
{
    IEnumerable<Event> FindAllByUserId(long userId);

    IEnumerable<Event> FindByUserIdAndDate(long? userId, DateOnly? date);
    Event? FindByUserIdAndId(long? userId, long id);

    Event FindByUserIdAndIdOrThrow(long? userId, long id);
}