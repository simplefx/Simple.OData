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

        public override async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpCommand command, bool scalarResult)
        {
            try
            {
                using (var response = await ExecuteRequestAsync(command.Request))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        result = _responseReader.GetData(response.GetResponseStream(), scalarResult);
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

        public override async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpCommand command, bool scalarResult)
        {
            int totalCount = 0;
            try
            {
                using (var response = await ExecuteRequestAsync(command.Request))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        result = _responseReader.GetData(response.GetResponseStream(), out totalCount);
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

        public override async Task<IDictionary<string, object>> GetEntryAsync(HttpCommand command)
        {
            try
            {
                var text = await ExecuteRequestAndGetResponseAsync(command.Request);
                return _responseReader.GetData(text).First();
            }
            catch (WebRequestException ex)
            {
                if (_ignoreResourceNotFoundException && IsResourceNotFoundException(ex))
                    return null;
                else
                    throw;
            }
        }

        public override async Task<IDictionary<string, object>> InsertEntryAsync(HttpCommand command, bool resultRequired = true)
        {
            var text = await ExecuteRequestAndGetResponseAsync(command.Request);
            if (resultRequired)
            {
                return _responseReader.GetData(text).First();
            }
            else
            {
                return null;
            }
        }

        public override async Task<int> UpdateEntryAsync(HttpCommand command)
        {
            using (var response = await ExecuteRequestAsync(command.Request))
            {
                // TODO
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        public override async Task<int> DeleteEntryAsync(HttpCommand command)
        {
            using (var response = await ExecuteRequestAsync(command.Request))
            {
                // TODO: check response code
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpCommand command)
        {
            using (var response = await ExecuteRequestAsync(command.Request))
            {
                IEnumerable<IDictionary<string, object>> result = null;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    result = Enumerable.Empty<IDictionary<string, object>>();
                }
                else
                {
                    result = _responseReader.GetFunctionResult(response.GetResponseStream());
                }

                return result;
            }
        }

        private bool IsResourceNotFoundException(WebRequestException ex)
        {
            var innerException = ex.InnerException as WebException;
            if (innerException != null)
            {
                var statusCode = (innerException.Response as HttpWebResponse).StatusCode;
                return statusCode == HttpStatusCode.NotFound;
            }
            return false;
        }
    }
}