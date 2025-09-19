using backend.Data;
using backend.Exceptions;
using backend.Models;
using backend.Repository.CrudRepository;

namespace backend.Repository;

public class EventRepository : CrudRepository<Event>, IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public IEnumerable<Event> FindAllByUserIdAndDate(long? userId, DateOnly? date)
    {
        if (!date.HasValue || !userId.HasValue)
        {
            return Enumerable.Empty<Event>();
        }

        return _context.Events
            .Where(e => e.UserId == userId && e.StartDate <= date && e.EndDate >= date)
            .OrderBy(e => e.StartDate)
            .ThenBy(e => e.StartTime)
            .ToList();
    }

    public IEnumerable<Event> FindAllByUserIdAndRange(long? userId, DateOnly? start, DateOnly? end)
    {
        if (!start.HasValue || !end.HasValue || !userId.HasValue)
        {
            return Enumerable.Empty<Event>();
        }

        return _context.Events
            .Where(e => e.UserId == userId && e.StartDate <= end && e.EndDate >= start)
            .OrderBy(e => e.StartDate)
            .ThenBy(e => e.StartTime)
            .ToList();
    }

    public Event? FindByUserIdAndId(long? userId, long id)
    {
        return _context.Events
            .FirstOrDefault(e => e.Id == id && e.UserId == userId);
    }

    public Event FindByUserIdAndIdOrThrow(long? userId, long id)
    {
        return FindByUserIdAndId(userId, id) ??
               throw new EntityNotFoundException($"Event with Id {id} for User {userId} was not found.");
    }
}