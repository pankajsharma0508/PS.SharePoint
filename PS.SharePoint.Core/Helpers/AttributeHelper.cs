using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PS.SharePoint.Core.Attributes;

namespace PS.SharePoint.Core.Helpers
{
    public class AttributeHelper
    {
        public static string GetContentTypeId(Type entityType) => GetSpTableAttribute(entityType)?.ContentTypeId;

        public static string GetListTitle(Type entityType) => GetSpTableAttribute(entityType)?.ListTitle;

        private static SpListAttribute GetSpTableAttribute(Type entityType)
        {
            var spTableAttribute = entityType.GetCustomAttribute<SpListAttribute>();

            if (spTableAttribute == null)
                throw new ArgumentException(string.Format("SharePoint List Title is not not defined for: {0}", entityType), "entityType");

            return spTableAttribute;
        }

        public static IEnumerable<PropertyInfo> GetSpProperties(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<SpColumnAttribute>() != null);
        }

        public static IEnumerable<PropertyMapping> GetSpColumns(Type type)
        {
            return type.GetProperties()
                .Select(p => new PropertyMapping { PropertyInfo = p, Attribute = p.GetCustomAttribute<SpColumnAttribute>() })
                .Where(x => x.Attribute != null);
        }

        public static IEnumerable<PropertyInfo> GetEditableSpProperties(Type type)
        {
            return GetSpProperties(type)
                .Where(p =>
                {
                    var attr = p.GetCustomAttribute<ReadOnlyAttribute>();
                    return attr == null || !attr.IsReadOnly;
                });
        }
    }


    public class PropertyMapping
    {
        public PropertyInfo PropertyInfo { get; set; }

        public SpColumnAttribute Attribute { get; set; }
    }
}
