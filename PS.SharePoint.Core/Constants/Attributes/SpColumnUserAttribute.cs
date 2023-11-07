using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Attributes
{
    /// <summary>
    /// Sets up correspondence to SP User selector column.
    /// </summary>
    public class SpColumnUserAttribute : SpColumnAttribute
    {
        public bool Multiselect { get; set; }

        public SpColumnUserAttribute(string name, bool multiselect = false) : base(name)
        {
            Multiselect = multiselect;
        }
    }
}
