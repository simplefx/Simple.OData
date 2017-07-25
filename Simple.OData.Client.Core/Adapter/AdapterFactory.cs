using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class AdapterFactory
    {
        private const string AdapterV3AssemblyName = "Simple.OData.Client.V3.Adapter";
        private const string AdapterV4AssemblyName = "Simple.OData.Client.V4.Adapter";
        private const string AdapterV3TypeName = "Simple.OData.Client.V3.Adapter.ODataAdapter";
        private const string AdapterV4TypeName = "Simple.OData.Client.V4.Adapter.ODataAdapter";

        public async Task<Func<ISession, IODataAdapter>> CreateAdapterAsync(HttpResponseMessage response)
        {
            var protocolVersions = (await GetSupportedProtocolVersionsAsync(response).ConfigureAwait(false)).ToArray();

            foreach (var protocolVersion in protocolVersions)
            {
                var loadAdapter = GetAdapterLoader(protocolVersion, response);
                if (loadAdapter != null)
                    return loadAdapter;
            }
            throw new NotSupportedException(string.Format("OData protocols {0} are not supported", string.Join(",", protocolVersions)));
        }

        public Func<ISession, IODataAdapter> CreateAdapter(string metadataString)
        {
            var protocolVersion = GetMetadataProtocolVersion(metadataString);
            var loadAdapter = GetAdapterLoader(protocolVersion, metadataString);
            if (loadAdapter == null)
                throw new NotSupportedException(string.Format("OData protocol {0} is not supported", protocolVersion));

            return loadAdapter;
        }

        private async Task<IEnumerable<string>> GetSupportedProtocolVersionsAsync(HttpResponseMessage response)
        {
            IEnumerable<string> headerValues;
            if (response.Headers.TryGetValues(HttpLiteral.DataServiceVersion, out headerValues) ||
                response.Headers.TryGetValues(HttpLiteral.ODataVersion, out headerValues))
            {
                return headerValues.SelectMany(x => x.Split(';')).Where(x => x.Length > 0);                
            }
            else
            {
                try
                {
                    var metadataString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var protocolVersion = GetMetadataProtocolVersion(metadataString);
                    return new[] { protocolVersion };
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Unable to identify OData protocol version");
                }
            }
        }

        private Func<ISession, IODataAdapter> GetAdapterLoader(string protocolVersion, object extraInfo)
        {
            if (protocolVersion == ODataProtocolVersion.V1 ||
                protocolVersion == ODataProtocolVersion.V2 ||
                protocolVersion == ODataProtocolVersion.V3)
                return session => LoadAdapter(AdapterV3AssemblyName, AdapterV3TypeName, session, protocolVersion, extraInfo);
            if (protocolVersion == ODataProtocolVersion.V4)
                return session => LoadAdapter(AdapterV4AssemblyName, AdapterV4TypeName, session, protocolVersion, extraInfo);

            return null;
        }

        private IODataAdapter LoadAdapter(string adapterAssemblyName, string adapterTypeName, params object[] ctorParams)
        {
            try
            {
                Assembly assembly = null;
#if PORTABLE
                var assemblyName = new AssemblyName(adapterAssemblyName);
                assembly = Assembly.Load(assemblyName);
#else
                assembly = this.GetType().Assembly;
#endif
                var constructors = assembly.GetType(adapterTypeName).GetDeclaredConstructors();
                var ctor = constructors.Single(x => 
                    x.GetParameters().Count() == ctorParams.Count() &&
                    x.GetParameters().Last().ParameterType == ctorParams.Last().GetType());
                return ctor.Invoke(ctorParams) as IODataAdapter;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(string.Format("Unable to load OData adapter from assembly {0}", adapterAssemblyName), exception);
            }
        }

        private string GetMetadataProtocolVersion(string metadataString)
        {
            using (var reader = XmlReader.Create(new StringReader(metadataString)))
            {
                reader.MoveToContent();

                var protocolVersion = reader.GetAttribute("Version");

                if (protocolVersion == ODataProtocolVersion.V1 ||
                    protocolVersion == ODataProtocolVersion.V2 ||
                    protocolVersion == ODataProtocolVersion.V3)
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            var version = reader.GetAttribute("m:" + HttpLiteral.MaxDataServiceVersion);
                            if (string.IsNullOrEmpty(version))
                                version = reader.GetAttribute("m:" + HttpLiteral.DataServiceVersion);
                            if (!string.IsNullOrEmpty(version) && string.Compare(version, protocolVersion, StringComparison.Ordinal) > 0)
                                protocolVersion = version;

                            break;
                        }
                    }
                }

                return protocolVersion;
            }
        }
    }
}