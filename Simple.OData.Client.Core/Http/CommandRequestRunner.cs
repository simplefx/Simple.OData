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
        private readonly Session _session;
        private readonly bool _includeResourceTypeInEntryProperties;
        private readonly bool _ignoreResourceNotFoundException;

        public CommandRequestRunner(Session session, ODataClientSettings settings)
        {
            _session = session;
            _includeResourceTypeInEntryProperties = settings.IncludeResourceTypeInEntryProperties;
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
                        var responseReader = _session.Provider.GetResponseReader();
                        var odataResponse = await responseReader.GetResponseAsync(response, _includeResourceTypeInEntryProperties);
                        result = odataResponse.Entries ?? new [] {odataResponse.Entry};
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
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Tuple.Create(Enumerable.Empty<IDictionary<string, object>>(), totalCount);
                    }
                    else
                    {
                        var responseReader = _session.Provider.GetResponseReader();
                        var result = await responseReader.GetResponseAsync(response, _includeResourceTypeInEntryProperties);
                        return Tuple.Create(result.Entries, (int)result.TotalCount.GetValueOrDefault());
                    }
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
                    var responseReader = _session.Provider.GetResponseReader();
                    return (await responseReader.GetResponseAsync(response, _includeResourceTypeInEntryProperties)).Entry;
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
                    var responseReader = _session.Provider.GetResponseReader();
                    return (await responseReader.GetResponseAsync(response, _includeResourceTypeInEntryProperties)).Entry;
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
                    var responseReader = _session.Provider.GetResponseReader();
                    return (await responseReader.GetResponseAsync(response, _includeResourceTypeInEntryProperties)).Entry;
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
                        var responseReader = _session.Provider.GetResponseReader();
                        var odataResponse = await responseReader.GetResponseAsync(response, _includeResourceTypeInEntryProperties);
                        return odataResponse.Entries ?? new [] {odataResponse.Entry};
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