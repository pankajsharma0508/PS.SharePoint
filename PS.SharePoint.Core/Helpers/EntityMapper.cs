using Microsoft.SharePoint.Client.Taxonomy;
using Microsoft.SharePoint.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PS.SharePoint.Core.Attributes;
using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Interfaces;

namespace PS.SharePoint.Core.Helpers
{
    public class EntityMapper<T> where T : BaseListItem
    {
        private IContextManager ContextManager { get; }

        public EntityMapper(IContextManager contextManager)
        {
            ContextManager = contextManager;
        }

        public void SetEntityPropertyFromListItem(ClientContext clientContext, T entity, ListItem listItem, PropertyInfo prop, ICollection<Action> actions)
        {
            var attribute = prop.GetCustomAttribute<SpColumnAttribute>();
            var listItemValue = listItem[attribute.Name];

            if (listItemValue == null)
            {
                prop.SetValue(entity, null);
                return;
            }

            var propertyType = prop.PropertyType;

            var urlAttribute = attribute as SpColumnUrlAttribute;
            if (urlAttribute != null)
            {
                var urlValue = listItemValue as FieldUrlValue;
                if (urlValue != null)
                    prop.SetValue(entity, urlValue.Url);
                else
                    prop.SetValue(entity, FormatEntityValue(propertyType, listItemValue));
                return;
            }

            var userAttribute = attribute as SpColumnUserAttribute;
            if (userAttribute != null)
            {
                SetEntityPropertyFromUserListItem(entity, prop, userAttribute, clientContext, listItemValue, actions);
                return;
            }

            var taxonomyAttribute = attribute as SpTaxonomyColumnAttribute;
            if (taxonomyAttribute != null)
            {
                SetEntityPropertyFromTaxonomyListItem(entity, prop, taxonomyAttribute, listItemValue);
                return;
            }

            var entityValue = FormatEntityValue(propertyType, listItemValue);
            prop.SetValue(entity, entityValue);
        }

        public void SetListItemFromEntity(List list, ListItem listItem, T entity, IEnumerable<PropertyInfo> props)
        {
            SetItemContentType(listItem, typeof(T));

            AttributeHelper.GetSpProperties(typeof(T));
            foreach (var prop in props)
                SetListItemPropertyFromEntity(list, listItem, entity, prop);

            listItem.Update();
        }

        private static void SetItemContentType(ListItem item, Type entityType)
        {
            var contentTypeId = AttributeHelper.GetContentTypeId(entityType);

            if (string.IsNullOrEmpty(contentTypeId))
                return;

            item["ContentTypeId"] = contentTypeId;
        }

        public object FormatEntityValue(Type fieldType, object value)
        {
            if (fieldType.IsEnum)
            {
                var enumString = Convert.ToString(value);

                var member = fieldType.GetFields().FirstOrDefault(m =>
                {
                    var attr = m.GetCustomAttribute<SpEnumAttribute>();
                    return attr != null && attr.Label == enumString;
                });

                if (member != null)
                    return Enum.Parse(fieldType, member.Name);

                return Enum.IsDefined(fieldType, enumString) ? Enum.Parse(fieldType, enumString) : 0;
            }

            if (fieldType == typeof(string[]))
                return value;

            if (Nullable.GetUnderlyingType(fieldType) != null)
                fieldType = Nullable.GetUnderlyingType(fieldType);

            switch (Type.GetTypeCode(fieldType))
            {
                case TypeCode.String:
                    return Convert.ToString(value);

                case TypeCode.Boolean:
                    return Convert.ToBoolean(value);

                case TypeCode.DateTime:
                    return Convert.ToDateTime(value);

                case TypeCode.Int32:
                    return Convert.ToInt32(value);

                case TypeCode.Int64:
                    return Convert.ToInt64(value);

                default:
                    throw new Exception(string.Format("SharePoint field type is not supported: {0}", fieldType));
            }
        }

        private void SetEntityPropertyFromUserListItem(object entity, PropertyInfo prop, SpColumnUserAttribute userAttribute, ClientContext clientContext, object listItemValue, ICollection<Action> actions)
        {
            if (userAttribute.Multiselect)
            {
                var userValues = listItemValue as IEnumerable<FieldUserValue>;

                if (userValues == null)
                    throw new Exception("The property is supposed to be a multi-select");

                var resolveUsers = userValues.Select(userValue => GetUserFromValue(clientContext, userValue)).ToList();
                actions.Add(() => prop.SetValue(entity, resolveUsers.Select(r => r()).ToArray()));
            }
            else
            {
                var resolveUser = GetUserFromValue(clientContext, listItemValue);
                actions.Add(() => prop.SetValue(entity, resolveUser()));
            }
        }

        private void SetEntityPropertyFromTaxonomyListItem(object entity, PropertyInfo prop, SpTaxonomyColumnAttribute taxonomyAttribute, object listItemValue)
        {
            var propertyType = prop.PropertyType;

            if (taxonomyAttribute.Multiselect)
            {
                var taxonomyValueCollection = listItemValue as TaxonomyFieldValueCollection;

                if (taxonomyValueCollection == null)
                    throw new Exception(string.Format("Field {0} is not a multi-select taxonomy field", prop.Name));

                var spval = taxonomyValueCollection.ToList().Select(v => FormatEntityValue(propertyType, v.Label));
                var val = propertyType == typeof(string[]) ? spval.Select(i => i.ToString()).ToArray() : spval.FirstOrDefault();

                prop.SetValue(entity, val);
            }
            else
            {
                var taxonomyValue = listItemValue as TaxonomyFieldValue;
                if (taxonomyValue == null)
                    throw new Exception(string.Format("Field {0} is not a single-select taxonomy field", prop.Name));

                prop.SetValue(entity, FormatEntityValue(propertyType, taxonomyValue.Label));
            }
        }

        public void SetEntityFromListItem(ClientContext clientContext, T entity, ListItem listItem, IEnumerable<PropertyInfo> properties)
        {
            var postActions = new List<Action>();
            foreach (var prop in properties)
            {
                try
                {
                    SetEntityPropertyFromListItem(clientContext, entity, listItem, prop, postActions);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Cannot set property {0} : {1}", prop.Name, ex));
                }
            }

            if (postActions.Count <= 0)
                return;
            clientContext.ExecuteQuery();

            foreach (var action in postActions)
                action();
        }

        private Func<SpUser> GetUserFromValue(ClientContext clientContext, object listItemValue)
        {
            if (listItemValue == null)
                return null;

            var userLookupId = string.Empty;

            try
            {
                var spUserValue = listItemValue as FieldUserValue;
                var regionName = $"{clientContext.Url}/Users";

                if (spUserValue != null)
                {
                    var userCacheKey = $"#{spUserValue.LookupId}";

                    //var cachedUser = CacheHelper.GetData<Entities.SpUser>(userCacheKey, regionName);
                    //if (cachedUser != null)
                    //    return () => cachedUser;

                    userLookupId = spUserValue.LookupId.ToString();

                    var spUser = clientContext.Web.EnsureUser(spUserValue.LookupValue);
                    clientContext.Load(spUser, u => u.Title, u => u.Email, u => u.LoginName);

                    return ResolveSpUser(spUser, userCacheKey, regionName);
                }
                else
                {
                    //TODO: special case for readonly built-in fields created/updated. Maybe to be removed?
                    var stringUserValue = listItemValue as string;
                    if (stringUserValue == null)
                        throw new Exception("The property is supposed to be a single-select user");

                    userLookupId = stringUserValue;

                    //var cachedUser = CacheHelper.GetData<Entities.User>(stringUserValue, regionName);
                    //if (cachedUser != null)
                    //    return () => cachedUser;

                    var spUser = clientContext.Web.EnsureUser(stringUserValue);
                    clientContext.Load(spUser, u => u.Title, u => u.Email, u => u.LoginName);

                    return ResolveSpUser(spUser, stringUserValue, regionName);
                }
            }
            catch (Exception)
            {
                // Logger.Error(ex, $"User not found for following lookupId/UserName:{userLookupId}");
                return null;
            }
        }

        private Func<SpUser> ResolveSpUser(User spUser, string cacheKey, string region)
        {
            return () =>
            {
                var user = new SpUser
                {
                    DisplayName = spUser.Title,
                    EmailId = spUser.Email,
                    UserName = spUser.LoginName
                };

                // CacheHelper.AddData(cacheKey, user, region);
                return user;
            };
        }

        private void SetListItemPropertyFromEntity(List list, ListItem listItem, object entity, PropertyInfo prop)
        {
            var entityValue = prop.GetValue(entity);

            var attribute = prop.GetCustomAttribute<SpColumnAttribute>();

            //if (attribute.SplitData)
            //{
            //    listItem[attribute.Name] = entityValue != null
            //        ? string.Join(",", (string[])entityValue)
            //        : string.Empty;
            //    return;
            //}

            var urlAttribute = attribute as SpColumnUrlAttribute;
            if (urlAttribute != null)
            {
                listItem[attribute.Name] = Convert.ToString(entityValue);
                return;
            }

            //var jsonAttribute = attribute as SpColumnJsonAttribute;
            //if (jsonAttribute != null)
            //{
            //    var serializedObject = JsonConvert.SerializeObject(entityValue);
            //    listItem[attribute.Name] = serializedObject;
            //    return;
            //}

            var userAttribute = attribute as SpColumnUserAttribute;
            if (userAttribute != null)
            {
                listItem[attribute.Name] = FormatListItemUserFromEntityValue(userAttribute, entityValue);
                return;
            }

            var taxonomyAttribute = attribute as SpTaxonomyColumnAttribute;
            if (taxonomyAttribute != null)
            {
                SetListItemTaxonomyFromEntityValue(list, listItem, taxonomyAttribute, prop.PropertyType, entityValue);
                return;
            }

            var listItemValue = FormatListItemValue(prop.PropertyType, entityValue);
            listItem[attribute.Name] = listItemValue;
        }

        private object FormatListItemUserFromEntityValue(SpColumnUserAttribute userAttribute, object entityValue)
        {
            if (entityValue == null)
                return null;

            if (userAttribute.Multiselect)
            {
                var entityUsers = entityValue as IEnumerable<SpUser>;

                if (entityUsers == null)
                    throw new ArgumentException("Internal error: The property is expected to be a multi-select user.");

                return entityUsers.Select(u => FieldUserValue.FromUser(u.UserName)).ToArray();
            }
            else
            {
                var entityUser = entityValue as SpUser;

                if (entityUser == null)
                    throw new ArgumentException("Internal error: The property is expected to be a single-select user.");

                return FieldUserValue.FromUser(entityUser.UserName);
            }
        }

        private void SetListItemTaxonomyFromEntityValue(List list, ListItem listItem, SpTaxonomyColumnAttribute taxonomyAttribute, Type propertyType, object value)
        {
            var field = list.Fields.GetByInternalNameOrTitle(taxonomyAttribute.Name);
            var txField = list.Context.CastTo<TaxonomyField>(field);

            if (taxonomyAttribute.Multiselect)
            {
                var values = new List<string>();
                var arraStrings = value as IEnumerable ?? new List<string>();
                foreach (var sc in arraStrings)
                {
                    var label = Convert.ToString(FormatListItemValue(propertyType, sc));
                    var term = GetTermByLabel(taxonomyAttribute.TermSet, label);

                    if (term != null)
                        values.Add(string.Format("-1;#{0}|{1}", term.DefaultLabel, term.TermGuid));
                }
                var termValueString = string.Join(";#", values);
                var coll = new TaxonomyFieldValueCollection(list.Context, termValueString, txField);
                txField.SetFieldValueByValueCollection(listItem, coll);
                listItem.Update();
            }
            else
            {
                var label = Convert.ToString(FormatListItemValue(propertyType, value));

                if (string.IsNullOrEmpty(label))
                {
                    txField.ValidateSetValue(listItem, null);
                    return;
                }

                var term = GetTermByLabel(taxonomyAttribute.TermSet, label);
                if (term == null)
                    return; //TODO: throw new Exception("Term not found")???

                var termValue = new TaxonomyFieldValue
                {
                    Label = term.DefaultLabel,
                    TermGuid = term.TermGuid,
                    WssId = -1
                };
                txField.SetFieldValueByValue(listItem, termValue);
            }
        }

        protected object FormatListItemValue(Type fieldType, object value)
        {
            if (value == null)
                return null;

            if (fieldType.IsEnum)
            {
                foreach (var enumValue in Enum.GetValues(fieldType))
                {
                    var m = fieldType.GetField(enumValue.ToString());
                    if (Convert.ToInt32(m.GetRawConstantValue()) == Convert.ToInt32(value))
                    {
                        var attr = m.GetCustomAttribute<SpEnumAttribute>();
                        return attr != null ? attr.Label : m.Name;
                    }
                }

                return Enum.GetName(fieldType, value);
            }
            return value;
        }

        private SpTermInfo GetTermByLabel(string termSetName, string termLabel)
        {
            var cacheRegion = $"TermSet-{termSetName}";

            //var cachedTermInfo = CacheHelper.GetData<TermInfo>(termLabel, cacheRegion);
            //if (cachedTermInfo != null)
            //    return cachedTermInfo;

            SpTermInfo result = null;
            ContextManager.ExecuteQuery($"Unable to resolve termLabel '{termLabel}' into a term, using the term set '{termSetName}'", clientContext =>
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termSets = taxonomySession.GetTermSetsByName(termSetName, 1033);
                var termSet = termSets.GetByName(termSetName);

                var terms = termSet.GetTerms(new LabelMatchInformation(clientContext)
                {
                    TermLabel = termLabel,
                    TrimUnavailable = true
                });

                clientContext.Load(terms);
                clientContext.ExecuteQuery();

                var results = terms.ToList().Select(t => new SpTermInfo
                {
                    DefaultLabel = t.Name,
                    TermGuid = t.Id.ToString(),
                    CustomProperties = t.CustomProperties
                }).ToList();

                var filtered = results.Count > 1
                    ? results.Where(r => r.DefaultLabel == termLabel)
                    : results;

                result = filtered.Single();
                //.Single($"GetTermByLabel: cannot resolve term label '{termLabel}' to a term name using term set '{termSetName}'. Please check configuration.");
            });

            //if (result != null)
            //    CacheHelper.AddData(termLabel, result, cacheRegion);

            return result;
        }

    }
}
