using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Attributes
{
    /// <summary>
    /// Sets up correspondence between a class and SP List.
    /// </summary>
    public class SpListAttribute : Attribute
    {
        public SpListAttribute(string listTitle, string contentTypeId = null)
        {
            ContentTypeId = contentTypeId;
            ListTitle = listTitle;
        }

        public string ListTitle { get; set; }

        public string ContentTypeId { get; set; }
    };
}
