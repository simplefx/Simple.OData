using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple.Data.OData.NorthwindModel
{
    public partial class NorthwindContext : IUpdatable
    {
        object IUpdatable.CreateResource(string containerName, string fullTypeName)
        {
            var entityType = Type.GetType(fullTypeName, true);
            var resource = Activator.CreateInstance(entityType);

            AddEntityToContainer(resource);

            return resource;
        }

        object IUpdatable.GetResource(IQueryable query, string fullTypeName)
        {
            object resource = null;

            foreach (object item in query)
            {
                if (resource != null)
                {
                    throw new DataServiceException("The query must return a single resource");
                }
                resource = item;
            }

            if (resource == null)
                throw new DataServiceException(404, "Resource not found");

            if (fullTypeName != null && resource.GetType().FullName != fullTypeName)
                throw new Exception("Unexpected type for resource");

            return resource;
        }

        object IUpdatable.ResetResource(object resource)
        {
            return resource;
        }

        void IUpdatable.SetValue(object targetResource, string propertyName, object propertyValue)
        {
            var propertyInfo = targetResource.GetType().GetProperty(propertyName);
            propertyInfo.SetValue(targetResource, propertyValue, null);
        }

        object IUpdatable.GetValue(object targetResource, string propertyName)
        {
            var propertyInfo = targetResource.GetType().GetProperty(propertyName);
            return propertyInfo.GetValue(targetResource, null);
        }

        void IUpdatable.SetReference(object targetResource, string propertyName, object propertyValue)
        {
            ((IUpdatable)this).SetValue(targetResource, propertyName, propertyValue);
        }

        void IUpdatable.AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
        {
            var propertyInfo = targetResource.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new Exception("Can't find property");

            var collection = (IList)propertyInfo.GetValue(targetResource, null);
            collection.Add(resourceToBeAdded);
        }

        void IUpdatable.RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
        {
            var propertyInfo = targetResource.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new Exception("Can't find property");
            var collection = (IList)propertyInfo.GetValue(targetResource, null);
            collection.Remove(resourceToBeRemoved);
        }

        void IUpdatable.DeleteResource(object targetResource)
        {
            RemoveEntityFromContainer(targetResource);
        }

        void IUpdatable.SaveChanges()
        {
        }

        object IUpdatable.ResolveResource(object resource)
        {
            return resource;
        }

        void IUpdatable.ClearChanges()
        {
        }

        public void Add(object entity)
        {
            AddEntityToContainer(entity);
        }

        public void Remove(object entity)
        {
            RemoveEntityFromContainer(entity);
        }

        internal void AddEntityToContainer(object entity)
        {
            var containerField = FindContainerField(entity.GetType());
            var method = containerField.FieldType.GetMethod("Add");
            method.Invoke(containerField.GetValue(this), new object[] { entity });
        }

        internal void RemoveEntityFromContainer(object entity)
        {
            var containerField = FindContainerField(entity.GetType());
            var method = containerField.FieldType.GetMethod("Remove");
            method.Invoke(containerField.GetValue(this), new object[] { entity });
        }

        private FieldInfo FindContainerField(Type entityType)
        {
            var containerName = entityType.Name[0].ToString().ToLower() + entityType.Name.Substring(1);
            return this.GetType().GetField(containerName, BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
