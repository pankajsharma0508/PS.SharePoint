using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Attributes
{
    /// <summary>
    /// Used in conjunction with SpTaxonomyColumn, when member is enum, 
    /// to set up correspondence to the terms
    /// </summary>
    public class SpEnumAttribute : Attribute
    {
        public string Label { get; set; }

        public SpEnumAttribute(string label)
        {
            Label = label;
        }
    }
}
