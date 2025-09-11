namespace backend.Repository.CrudRepository;

public interface ICrudRepository<T> where T : class
{
    IEnumerable<T> FindAll();

    T? FindById(long id);

    T FindOrThrow(long id);

    T Create(T entity);

    T Update(T entity);

    void Delete(T entity);
}