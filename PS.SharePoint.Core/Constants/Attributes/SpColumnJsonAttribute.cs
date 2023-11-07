using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Attributes
{
    /// <summary>
    /// Sets up correspondence to SP User selector column.
    /// The member is assumed to have (evp) User type.
    /// </summary>
    public class SpColumnJsonAttribute : SpColumnAttribute
    {
        public SpColumnJsonAttribute(string name) : base(name)
        {
        }
    }
}
