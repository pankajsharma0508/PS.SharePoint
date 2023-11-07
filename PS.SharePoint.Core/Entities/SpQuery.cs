using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PS.SharePoint.Core.Entities
{
    public class SpQuery
    {
        public SpQuery()
        {
            Query = new CamlQuery();
        }

        public CamlQuery Query { get; set; }

        public List<Expression<Func<ListItemCollection, object>>> Includes { get; set; }
    }
}
