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

    private IQueryable<Event> QueryByUser(long userId)
    {
        return _context.Events
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.StartTime);
    }

    public IEnumerable<Event> FindAllByUserId(long userId)
    {
        return QueryByUser(userId).ToList();
    }

    public IEnumerable<Event> FindByUserIdAndDate(long userId, DateOnly? date)
    {
        if (!date.HasValue)
        {
            return Enumerable.Empty<Event>();
        }

        return QueryByUser(userId)
            .Where(e => e.EventDate == date)
            .ToList();
    }

    public Event? FindByUserIdAndId(long userId, long id)
    {
        return _context.Events
            .FirstOrDefault(e => e.Id == id && e.UserId == userId);
    }

    public Event FindByUserIdAndIdOrThrow(long userId, long id)
    {
        return FindByUserIdAndId(userId, id) ??
               throw new EntityNotFoundException($"Event with Id {id} for User {userId} was not found.");
    }
}