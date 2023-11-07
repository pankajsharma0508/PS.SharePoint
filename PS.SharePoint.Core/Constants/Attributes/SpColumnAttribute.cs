using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace PS.SharePoint.Core.Attributes
{
    /// <summary>
    /// The member should have simple type (like string, int, enum)
    /// </summary>
    public class SpColumnAttribute : ColumnAttribute
    {
        public SpColumnAttribute(string name) : base(name) { }
    };
}
