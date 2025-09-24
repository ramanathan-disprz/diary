using backend.Models;
using backend.Requests;

namespace backend.Service;

public interface IEventService
{
    IEnumerable<Event> FindAllByUserIdAndDate(long? userId, DateOnly? date);

    IEnumerable<Event> FindAllByUserIdAndRange(long? userId, DateOnly? start, DateOnly? end);

    Event Fetch(long? userId, long id);

    Event Create(EventRequest request);

    Event Update(long id, EventRequest request);

    void Delete(long userId, long id);
}