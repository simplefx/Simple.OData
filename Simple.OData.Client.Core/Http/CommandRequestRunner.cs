using System;
using System.Collections;
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
                    var text = response.Content.ReadAsStringAsync().Result;
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
                var text = response.Content.ReadAsStringAsync().Result;
                if (resultRequired)
                {
                    return _responseReader.GetData(text).First();
                }
                else
                {
                    return null;
                }
            }
        }

        public override async Task<int> UpdateEntryAsync(HttpRequest request)
        {
            using (var response = await ExecuteRequestAsync(request))
            {
                // TODO
                return response.IsSuccessStatusCode ? 1 : 0;
            }
        }

        public override async Task<int> DeleteEntryAsync(HttpRequest request)
        {
            using (var response = await ExecuteRequestAsync(request))
            {
                // TODO: check response code
                return response.IsSuccessStatusCode ? 1 : 0;
            }
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request)
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
                    result = _responseReader.GetFunctionResult(response.Content.ReadAsStreamAsync().Result);
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