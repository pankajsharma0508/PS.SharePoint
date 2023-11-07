using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Interfaces;

namespace PS.SharePoint.Core.Repository
{
    public class BaseItemRepository<T> : BaseRepository<T> where T : BaseListItem, ISharePointRepository<T>
    {
        public BaseItemRepository() : base()
        {
        }
    }
}
