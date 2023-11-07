using PS.SharePoint.Core.Attributes;
using System.ComponentModel;

namespace PS.SharePoint.Core.Entities
{
    public class BaseListItem
    {
        [SpColumn("ID")]
        [ReadOnly(true)]
        public int Id { get; set; }
    }
}
