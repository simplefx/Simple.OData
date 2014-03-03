using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class CommandRequestRunner : RequestRunner
    {
        private readonly ResponseReader _responseReader;
        private readonly bool _ignoreResourceNotFoundException;

        public CommandRequestRunner(ISchema schema, ODataClientSettings settings)
        {
            _responseReader = new ResponseReader(schema, settings.IncludeResourceTypeInEntryProperties);
            _ignoreResourceNotFoundException = settings.IgnoreResourceNotFoundException;
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult)
        {
            try
            {
                using (var response = await ExecuteRequestAsync(request))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (!response.IsSuccessStatusCode)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        result = _responseReader.GetData(await response.Content.ReadAsStringAsync(), scalarResult);
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

        public override async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult)
        {
            int totalCount = 0;
            try
            {
                using (var response = await ExecuteRequestAsync(request))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        result = _responseReader.GetData(await response.Content.ReadAsStringAsync(), out totalCount);
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

        public override async Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request)
        {
            try
            {
                using (var response = await ExecuteRequestAsync(request))
                {
                    var text = await response.Content.ReadAsStringAsync();
                    return _responseReader.GetData(text).First();
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

        public override async Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request, bool resultRequired = true)
        {
            using (var response = await ExecuteRequestAsync(request))
            {
                var text = await response.Content.ReadAsStringAsync();
                if (resultRequired && response.StatusCode == HttpStatusCode.Created)
                {
                    return _responseReader.GetData(text).First();
                }
                else
                {
                    return null;
                }
            }
        }

        public override async Task UpdateEntryAsync(HttpRequest request)
        {
            using (await ExecuteRequestAsync(request))
            {
            }
        }

        public override async Task DeleteEntryAsync(HttpRequest request)
        {
            using (await ExecuteRequestAsync(request))
            {
            }
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request)
        {
            using (var response = await ExecuteRequestAsync(request))
            {
                IEnumerable<IDictionary<string, object>> result = null;
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                        result = _responseReader.GetFunctionResult(await response.Content.ReadAsStreamAsync());
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