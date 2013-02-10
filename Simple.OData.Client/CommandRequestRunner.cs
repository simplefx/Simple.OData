using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Simple.OData.Client
{
    class CommandRequestRunner : RequestRunner
    {
        private readonly ODataFeedReader _feedReader;
        private readonly bool _ignoreResourceNotFoundException;

        public CommandRequestRunner(ODataClientSettings settings)
        {
            _feedReader = new ODataFeedReader(settings.IncludeResourceTypeInEntryProperties);
            _ignoreResourceNotFoundException = settings.IgnoreResourceNotFoundException;
        }

        public override IEnumerable<IDictionary<string, object>> FindEntries(HttpCommand command, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            totalCount = 0;
            try
            {
                using (var response = TryRequest(command.Request))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        result = Enumerable.Empty<IDictionary<string, object>>();
                    }
                    else
                    {
                        var stream = response.GetResponseStream();
                        if (setTotalCount)
                            result = _feedReader.GetData(stream, out totalCount);
                        else
                            result = _feedReader.GetData(response.GetResponseStream(), scalarResult);
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

        public override IDictionary<string, object> GetEntry(HttpCommand command)
        {
            try
            {
                var text = Request(command.Request);
                return _feedReader.GetData(text).First();
            }
            catch (WebRequestException ex)
            {
                if (_ignoreResourceNotFoundException && IsResourceNotFoundException(ex))
                    return null;
                else
                    throw;
            }
        }

        public override IDictionary<string, object> InsertEntry(HttpCommand command, bool resultRequired)
        {
            var text = Request(command.Request);
            if (resultRequired)
            {
                return _feedReader.GetData(text).First();
            }
            else
            {
                return null;
            }
        }

        public override int UpdateEntry(HttpCommand command)
        {
            using (var response = TryRequest(command.Request))
            {
                // TODO
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        public override int DeleteEntry(HttpCommand command)
        {
            using (var response = TryRequest(command.Request))
            {
                // TODO: check response code
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        public override IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(HttpCommand command)
        {
            using (var response = TryRequest(command.Request))
            {
                IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> result = null;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    result = Enumerable.Empty<IEnumerable<IDictionary<string, object>>>();
                }
                else
                {
                    result = new[] { _feedReader.GetFunctionResult(response.GetResponseStream()) };
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