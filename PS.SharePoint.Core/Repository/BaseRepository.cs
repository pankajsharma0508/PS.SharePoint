using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Extensions;
using PS.SharePoint.Core.Helpers;
using PS.SharePoint.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity;

namespace PS.SharePoint.Core.Repository
{
    public class BaseRepository<T> : ISharePointRepository<T> where T : BaseListItem
    {
        protected IContextManager contextManager => ServiceConfiguration.GetContainer().Resolve<IContextManager>();
        private EntityMapper<T> entityMapper => ServiceConfiguration.GetContainer().Resolve<EntityMapper<T>>();

        public T Create(T entity)
        {
            var entityType = entity.GetType();
            var listTitle = AttributeHelper.GetListTitle(entityType);
            var postActions = new List<Action>();

            contextManager.ExecuteQuery($"Failed to create new entity in {listTitle}", clientContext =>
            {
                var list = clientContext.Web.Lists.GetByTitle(listTitle);

                var item = list.AddItem(new ListItemCreationInformation());
                if (entity is BaseDocument)  // Check out file after upload to avoid multiple version.
                    item.File.CheckOut();

                var properties = AttributeHelper.GetSpProperties(entityType);

                entityMapper.SetListItemFromEntity(list, item, entity, AttributeHelper.GetEditableSpProperties(entityType));
                clientContext.Load(item);
                if (entity is BaseDocument) { clientContext.Load(item.File); }

                contextManager.ExecuteQuery(clientContext, $"create new item in the {listTitle}");

                entityMapper.SetEntityFromListItem(clientContext, entity, item, properties);


                if (entity is BaseDocument) // Check in file after upload to avoid multiple version.
                    contextManager.CheckInDocument(clientContext, item.File);
            });
            return (T)entity;
        }

        public void Update(T entity, params Expression<Func<T, object>>[] selectProps)
        {
            var entityType = entity.GetType();
            var listTitle = AttributeHelper.GetListTitle(entityType);

            contextManager.ExecuteQuery($"Failed to update item {entity.Id} in {listTitle}", clientContext =>
            {
                var list = clientContext.Web.Lists.GetByTitle(listTitle);
                var item = list.GetItemById(entity.Id);

                var propNames = selectProps.Select(StringHelper.NameOf).ToArray();

                var props = AttributeHelper.GetEditableSpProperties(entityType)
                .Where(p => propNames == null || propNames.Length == 0 || propNames.Contains(p.Name));

                entityMapper.SetListItemFromEntity(list, item, entity, props);
                clientContext.Load(item);

                contextManager.ExecuteQuery(clientContext, $"update item {entity.Id} in {listTitle}");

                entityMapper.SetEntityFromListItem(clientContext, entity, item, props);
            });
        }

        public IEnumerable<T> Get(string queryXml)
        {
            var entityType = typeof(T);
            var listTitle = AttributeHelper.GetListTitle(entityType);

            var result = new List<T>();
            contextManager.ExecuteQuery($"Failed to get items from {listTitle}", clientContext =>
            {
                var list = clientContext.Web.Lists.GetByTitle(listTitle);
                var props = AttributeHelper.GetSpProperties(entityType);
                var spQuery = PSCamlQueryBuilder.PrepareQuery(props, queryXml);

                var listItems = list.GetItems(spQuery.Query);
                clientContext.Load(listItems, spQuery.Includes.ToArray());
                TaxonomyItem taxonomyItem = new TaxonomyItem(clientContext, null);

                //Logger.Debug($"  >Sharepoint: {context.Url} {text}");
                clientContext.ExecuteQuery();

                foreach (var listItem in listItems)
                {
                    var entity = (T)Activator.CreateInstance(entityType);
                    entityMapper.SetEntityFromListItem(clientContext, entity, listItem, props);
                    result.Add(entity);
                }
            });

            return result;
        }

        public void Delete(T item, bool recycle)
        {
            var entityType = typeof(T);
            var listTitle = AttributeHelper.GetListTitle(entityType);
            var itemId = item.Id;

            contextManager.ExecuteQuery($"Failed to delete item id={itemId} from {listTitle}", clientContext =>
            {
                var list = clientContext.Web.Lists.GetByTitle(listTitle);

                var listItem = list.GetItemById(itemId);
                if (recycle)
                    listItem.Recycle();
                else
                    listItem.DeleteObject();

                clientContext.ExecuteQuery();
            });
        }
    }
}
