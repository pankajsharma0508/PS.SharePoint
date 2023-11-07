using PS.SharePoint.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PS.SharePoint.Core.Interfaces
{
    public interface ISharePointRepository<T> where T : BaseListItem
    {
        T Create(T entity);
        void Update(T entity, params Expression<Func<T, object>>[] selectProps);
        IEnumerable<T> Get(string queryXml);
        void Delete(T item, bool recycle);
    }
}
