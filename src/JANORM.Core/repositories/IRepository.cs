namespace JANORM.Core.repositories;

public interface IRepository<T, Tkey>
    where T : class
    where Tkey : notnull
{
    Task<T> Insert(T entity);
    Task<T?> FindById(Tkey id);
    Task<List<T>> FindAll();
    Task<T?> FindOne(Dictionary<string, object> query);
    Task<T> Update(T entity, Tkey id);
    Task Delete(Tkey id);
}
