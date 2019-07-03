using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IAdapterFactory
    {
        Task<IODataModelAdapter> CreateModelAdapterAsync(HttpResponseMessage response, ITypeCache typeCache);
        IODataModelAdapter CreateModelAdapter(string metadataString, ITypeCache typeCache);
        Func<ISession, IODataAdapter> CreateAdapterLoader(string metadataString, ITypeCache typeCache);
    }
}