using PS.SharePoint.Core.Entities;
using System.Collections.Generic;

namespace PS.SharePoint.Core.Interfaces
{
    public interface ISharePointDocumentRepository<T> where T : BaseDocument
    {
        T Create(T entity);
        void Update(T entity, string[] propNames);
        IEnumerable<T> Get(string queryXml);
        void Delete(T item, bool recycle);
        T GetDocumentBlob(string fileRef);
    }
}
