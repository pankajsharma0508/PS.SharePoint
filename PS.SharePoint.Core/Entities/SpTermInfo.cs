using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Entities
{
    internal class SpTermInfo
    {
        public string DefaultLabel;
        public string TermGuid;
        public IDictionary<string, string> CustomProperties { get; set; }
    }
}
