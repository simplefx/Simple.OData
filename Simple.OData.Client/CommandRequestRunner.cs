using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Simple.OData.Client
{
    class CommandRequestRunner : RequestRunner
    {
        public override IEnumerable<IDictionary<string, object>> FindEntries(HttpCommand command, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            using (var response = TryRequest(command.Request))
            {
                totalCount = 0;
                IEnumerable<IDictionary<string, object>> result = null;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    result = Enumerable.Empty<IDictionary<string, object>>();
                }
                else
                {
                    var stream = response.GetResponseStream();
                    if (setTotalCount)
                        result = ODataFeedReader.GetData(stream, out totalCount);
                    else
                        result = ODataFeedReader.GetData(response.GetResponseStream(), scalarResult);
                }

                return result;
            }
        }

        public override IDictionary<string, object> GetEntry(HttpCommand command)
        {
            var text = Request(command.Request);
            return ODataFeedReader.GetData(text).First();
        }

        public override IDictionary<string, object> InsertEntry(HttpCommand command, bool resultRequired)
        {
            var text = Request(command.Request);
            if (resultRequired)
            {
                return ODataFeedReader.GetData(text).First();
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
                    var stream = response.GetResponseStream();
                    result = new[] { ODataFeedReader.GetData(response.GetResponseStream(), false) };
                }

                return result;
            }
        }
    }
}