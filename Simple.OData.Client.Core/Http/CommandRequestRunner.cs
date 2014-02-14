using System.Collections.Generic;
using System.Linq;
using System.Net;

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

        public override IEnumerable<IDictionary<string, object>> FindEntries(HttpRequest request, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            totalCount = 0;
            try
            {
                using (var response = ExecuteRequest(request))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (!response.IsSuccessStatusCode)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        var stream = response.Content.ReadAsStreamAsync().Result;
                        if (setTotalCount)
                            result = _responseReader.GetData(stream, out totalCount);
                        else
                            result = _responseReader.GetData(stream, scalarResult);
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

        public override IDictionary<string, object> GetEntry(HttpRequest request)
        {
            try
            {
                using (var response = ExecuteRequest(request))
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

        public override IDictionary<string, object> InsertEntry(HttpRequest request, bool resultRequired = true)
        {
            using (var response = ExecuteRequest(request))
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

        public override int UpdateEntry(HttpRequest request)
        {
            using (var response = ExecuteRequest(request))
            {
                // TODO
                return response.IsSuccessStatusCode ? 1 : 0;
            }
        }

        public override int DeleteEntry(HttpRequest request)
        {
            using (var response = ExecuteRequest(request))
            {
                // TODO: check response code
                return response.IsSuccessStatusCode ? 1 : 0;
            }
        }

        public override IEnumerable<IDictionary<string, object>> ExecuteFunction(HttpRequest request)
        {
            using (var response = ExecuteRequest(request))
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