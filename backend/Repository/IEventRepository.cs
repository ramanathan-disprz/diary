using backend.Models;
using backend.Repository.CrudRepository;

namespace backend.Repository;

public interface IEventRepository : ICrudRepository<Event>
{
    IEnumerable<Event> FindAllByUserIdAndDate(long? userId, DateOnly? date);
    IEnumerable<Event> FindAllByUserIdAndRange(long? userId, DateOnly? start, DateOnly? end);
    Event? FindByUserIdAndId(long? userId, long id);

    Event FindByUserIdAndIdOrThrow(long? userId, long id);
}