using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class RequestRunner
    {
        private readonly ISession _session;
        private readonly bool _isBatch;
        private readonly bool _includeResourceTypeInEntryProperties;
        private readonly bool _ignoreResourceNotFoundException;

        private const string PreferHeaderName = "Prefer";
        private const string PreferenceAppliedHeaderName = "Preference-Applied";
        private const string ReturnContentHeaderValue = "return-content";
        private const string ReturnNoContentHeaderValue = "return-no-content";

        public Action<HttpRequestMessage> BeforeRequest { get; set; }
        public Action<HttpResponseMessage> AfterResponse { get; set; }

        public RequestRunner(ISession session, ODataClientSettings settings, bool isBatch = false)
        {
            _session = session;
            _isBatch = isBatch;

            _includeResourceTypeInEntryProperties = settings.IncludeResourceTypeInEntryProperties;
            _ignoreResourceNotFoundException = settings.IgnoreResourceNotFoundException;
        }

        public async Task<HttpResponseMessage> ExecuteRequestAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var clientHandler = new HttpClientHandler();

                // Perform this test to prevent failure to access Credentials/PreAuthenticate properties on SL5
                if (request.Credentials != null)
                {
                    clientHandler.Credentials = request.Credentials;
                    if (clientHandler.SupportsPreAuthenticate())
                        clientHandler.PreAuthenticate = true;
                }

                using (var httpClient = new HttpClient(clientHandler))
                {
                    if (request.Accept != null)
                    {
                        foreach (var accept in request.Accept)
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
                        }
                    }

                    if (request.CheckOptimisticConcurrency &&
                        (request.Method == RestVerbs.PUT || request.Method == RestVerbs.MERGE || request.Method == RestVerbs.DELETE))
                    {
                        httpClient.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                    }

                    if (this.BeforeRequest != null)
                        this.BeforeRequest(request.RequestMessage);

                    request.RequestMessage.Headers.Add(
                        PreferHeaderName,
                        request.ReturnContent ? ReturnContentHeaderValue : ReturnNoContentHeaderValue);

                    var responseMessage = await httpClient.SendAsync(request.RequestMessage, cancellationToken);

                    if (this.AfterResponse != null)
                        this.AfterResponse(responseMessage);

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        throw new WebRequestException(responseMessage.ReasonPhrase, responseMessage.StatusCode);
                    }

                    return responseMessage;
                }
            }
            catch (WebException ex)
            {
                throw WebRequestException.CreateFromWebException(ex);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is WebException)
                {
                    throw WebRequestException.CreateFromWebException(ex.InnerException as WebException);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(ODataRequest request, bool scalarResult, CancellationToken cancellationToken)
        {
            if (_isBatch)
                return await Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));

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
                        result = odataResponse.Entries ?? new[] { odataResponse.Entry };
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

        public async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(ODataRequest request, bool scalarResult, CancellationToken cancellationToken)
        {
            if (_isBatch)
                return await Utils.GetTaskFromResult(default(Tuple<IEnumerable<IDictionary<string, object>>, int>));

            try
            {
                using (var response = await ExecuteRequestAsync(request, cancellationToken))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Tuple.Create(Enumerable.Empty<IDictionary<string, object>>(), 0);
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
                        new[] { (IDictionary<string, object>)null }, 0);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (_isBatch)
                return await Utils.GetTaskFromResult(default(IDictionary<string, object>));

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

        public async Task<IDictionary<string, object>> InsertEntryAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (_isBatch)
                return await Utils.GetTaskFromResult(request.EntryData);

            using (var response = await ExecuteRequestAsync(request, cancellationToken))
            {
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

        public async Task<IDictionary<string, object>> UpdateEntryAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (_isBatch)
                return await Utils.GetTaskFromResult(request.EntryData);

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

        public async Task DeleteEntryAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (_isBatch)
                return;

            using (await ExecuteRequestAsync(request, cancellationToken))
            {
            }
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (_isBatch)
                return await Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));

            using (var response = await ExecuteRequestAsync(request, cancellationToken))
            {
                IEnumerable<IDictionary<string, object>> result = null;
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                        var responseReader = _session.Provider.GetResponseReader();
                        var odataResponse = await responseReader.GetResponseAsync(response, _includeResourceTypeInEntryProperties);
                        return odataResponse.Entries ?? new[] { odataResponse.Entry };
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
