using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Providers;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using System.Data.Services;

namespace ActionProviderImplementation
{
    public class EntityFrameworkParameterMarshaller: IParameterMarshaller
    {
        static MethodInfo CastMethodGeneric = typeof(Enumerable).GetMethod("Cast");
        static MethodInfo ToListMethodGeneric = typeof(Enumerable).GetMethod("ToList");

        public object[] Marshall(DataServiceOperationContext operationContext, ServiceAction action, object[] parameters)
        {
            var pvalues = action.Parameters.Zip(parameters, (parameter, parameterValue) => new { Parameter = parameter, Value = parameterValue });
            var marshalled = pvalues.Select(pvalue => GetMarshalledParameter(operationContext, pvalue.Parameter, pvalue.Value)).ToArray();

            return marshalled;
        }
        private object GetMarshalledParameter(DataServiceOperationContext operationContext, ServiceActionParameter serviceActionParameter, object value)
        {
            var parameterKind = serviceActionParameter.ParameterType.ResourceTypeKind;
            
            // Need to Marshall MultiValues i.e. Collection(Primitive) & Collection(ComplexType)
            if (parameterKind == ResourceTypeKind.EntityType)
            {

                // This entity is UNATTACHED. But people are likely to want to edit this...
                IDataServiceUpdateProvider2 updateProvider = operationContext.GetService(typeof(IDataServiceUpdateProvider2)) as IDataServiceUpdateProvider2;
                value = updateProvider.GetResource(value as IQueryable, serviceActionParameter.ParameterType.InstanceType.FullName);
                value = updateProvider.ResolveResource(value); // This will attach the entity.
            }
            else if (parameterKind == ResourceTypeKind.EntityCollection)
            {
                // WCF Data Services constructs an IQueryable that is NoTracking...
                // but that means people can't just edit the entities they pull from the Query.
                var query = value as ObjectQuery;
                query.MergeOption = MergeOption.AppendOnly; 
            }
            else if (parameterKind == ResourceTypeKind.Collection)
            {
                // need to coerce into a List<> for dispatch
                var enumerable = value as IEnumerable;
                // the <T> in List<T> is the Instance type of the ItemType
                var elementType = (serviceActionParameter.ParameterType as CollectionResourceType).ItemType.InstanceType;
                // call IEnumerable.Cast<T>();
                var castMethod = CastMethodGeneric.MakeGenericMethod(elementType);
                object marshalledValue = castMethod.Invoke(null, new[] { enumerable });
                // call IEnumerable<T>.ToList();
                var toListMethod = ToListMethodGeneric.MakeGenericMethod(elementType);
                value = toListMethod.Invoke(null, new[] { marshalledValue });
            }
            return value;
        }
    }
}
