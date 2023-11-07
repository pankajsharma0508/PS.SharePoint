using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Interfaces;
using System.IO;

namespace PS.SharePoint.Core.Repository
{
    public class BaseDocumentRepository<T> : BaseRepository<T> where T : BaseDocument, ISharePointDocumentRepository<T>
    {
        public T GetDocumentBlob(string fileRef)
        {
            using (var ms = new MemoryStream())
            {
                byte[] result = null;
                contextManager.ExecuteQuery($"Failed to get binary data for file {fileRef}", clientContext =>
                {
                    var file = clientContext.Web.GetFileByServerRelativeUrl(fileRef);
                    var stream = file.OpenBinaryStream();

                    contextManager.ExecuteQuery(clientContext, $"get content {fileRef}");

                    stream.Value.CopyTo(ms);
                    result = ms.ToArray();
                });

                return (T)new BaseDocument { Content = result };
            }
        }
    }
}
