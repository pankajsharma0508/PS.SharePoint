using Microsoft.SharePoint.Client;
using PS.SharePoint.Core.Attributes;
using PS.SharePoint.Core.Constants;
using PS.SharePoint.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace PS.SharePoint.Core.Helpers
{
    public class PSCamlQueryBuilder
    {
        public static SpQuery PrepareQuery(IEnumerable<PropertyInfo> properties, string queryXml)
        {
            var spQuery = new SpQuery();
            spQuery.Query = new CamlQuery();
            var viewFieldsXml = new XElement(XmlConstants.ViewFields);
            var includes = new List<Expression<Func<ListItemCollection, object>>> { };

            foreach (var attr in properties.Select(prop => prop.GetCustomAttribute<SpColumnAttribute>()))
            {
                var fieldName = attr.Name;
                viewFieldsXml.Add(new XElement(XmlConstants.FieldRef, new XAttribute(XmlConstants.Name, fieldName)));
                includes.Add(items => items.Include(item => item[fieldName]));
            }

            var viewXml = new XElement(XmlConstants.View);

            if (!string.IsNullOrEmpty(queryXml))
                viewXml.Add(XElement.Parse(queryXml));

            viewXml.Add(viewFieldsXml);
            spQuery.Query.ViewXml = viewXml.ToString();
            spQuery.Includes = includes;

            return spQuery;
        }
    }
}
