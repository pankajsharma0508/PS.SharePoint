using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Entities
{
    public class BaseDocument : BaseListItem
    {
        public byte[] Content { get; set; }
    }
}
