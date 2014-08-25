using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class CommandRequestRunner : RequestRunner
    {
        private Schema _schema;
        //private readonly ResponseReader _responseReader;
        private readonly bool _ignoreResourceNotFoundException;

        public CommandRequestRunner(Schema schema, ODataClientSettings settings)
        {
            _schema = schema;
            //_responseReader = new ResponseReader(schema, settings.IncludeResourceTypeInEntryProperties);
            _ignoreResourceNotFoundException = settings.IgnoreResourceNotFoundException;
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult, CancellationToken cancellationToken)
        {
            try
            {
                using (var response = await ExecuteRequestAsync(request, cancellationToken))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (!response.IsSuccessStatusCode)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        result = await _schema.ProviderMetadata.GetEntriesAsync(response);
                        //result = _responseReader.GetData(await response.Content.ReadAsStringAsync(), scalarResult);
                    }

                    return result;
                }
            }
            catch (WebRequestException ex)
            {
                if (_ignoreResourceNotFoundException && IsResourceNotFoundException(ex))
                    return new[] { (IDictionary<string, object>)null };
                else
                    throw;
            }
        }

        public override async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult, CancellationToken cancellationToken)
        {
            int totalCount = 0;
            try
            {
                using (var response = await ExecuteRequestAsync(request, cancellationToken))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        result = await _schema.ProviderMetadata.GetEntriesAsync(response);
                        //result = _responseReader.GetData(await response.Content.ReadAsStringAsync(), out totalCount);
                    }

                    return Tuple.Create(result, totalCount);
                }
            }
            catch (WebRequestException ex)
            {
                if (_ignoreResourceNotFoundException && IsResourceNotFoundException(ex))
                {
                    return new Tuple<IEnumerable<IDictionary<string, object>>, int>(
                        new[] {(IDictionary<string, object>) null}, 0);
                }
                else
                {
                    throw;
                }
            }
        }

        public override async Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using (var response = await ExecuteRequestAsync(request, cancellationToken))
                {
                    return await _schema.ProviderMetadata.GetEntryAsync(response);
                    //var text = await response.Content.ReadAsStringAsync();
                    //return _responseReader.GetData(text).First();
                }
            }
            catch (WebRequestException ex)
            {
                if (_ignoreResourceNotFoundException && IsResourceNotFoundException(ex))
                    return null;
                else
                    throw;
            }
        }

        public override async Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using (var response = await ExecuteRequestAsync(request, cancellationToken))
            {
                var text = await response.Content.ReadAsStringAsync();
                if (request.ReturnContent && response.StatusCode == HttpStatusCode.Created)
                {
                    return await _schema.ProviderMetadata.GetEntryAsync(response);
                    //return _responseReader.GetData(text).First();
                }
                else
                {
                    return null;
                }
            }
        }

        public override async Task<IDictionary<string, object>> UpdateEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using (var response = await ExecuteRequestAsync(request, cancellationToken))
            {
                var text = await response.Content.ReadAsStringAsync();
                if (request.ReturnContent && response.StatusCode == HttpStatusCode.OK)
                {
                    return await _schema.ProviderMetadata.GetEntryAsync(response);
                    //return _responseReader.GetData(text).First();
                }
                else
                {
                    return null;
                }
            }
        }

        public override async Task DeleteEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using (await ExecuteRequestAsync(request, cancellationToken))
            {
            }
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using (var response = await ExecuteRequestAsync(request, cancellationToken))
            {
                IEnumerable<IDictionary<string, object>> result = null;
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                    return await _schema.ProviderMetadata.GetEntriesAsync(response);
                        //result = _responseReader.GetFunctionResult(await response.Content.ReadAsStreamAsync());
                        break;

                    default:
                        result = Enumerable.Empty<IDictionary<string, object>>();
                        break;
                }
                return result;
            }
        }

        private bool IsResourceNotFoundException(WebRequestException ex)
        {
            return ex.Code == HttpStatusCode.NotFound;
        }
    }
}