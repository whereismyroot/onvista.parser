using System.Collections.Generic;

namespace Onvista.Parser.Data
{
    public interface IRepository<T>
    {
        T GetEntity(int id);

        ICollection<T> GetEntities(string where);

        void Insert(T entity);
    }
}
