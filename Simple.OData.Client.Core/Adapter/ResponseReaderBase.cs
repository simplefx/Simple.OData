using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public abstract class ResponseReaderBase : IResponseReader
    {
        protected readonly ISession _session;

        protected ResponseReaderBase(ISession session)
        {
            _session = session;
        }

        public abstract Task<ODataResponse> GetResponseAsync(HttpResponseMessage responseMessage, bool includeResourceTypeInEntryProperties = false);

        protected abstract void ConvertEntry(ResponseNode entryNode, object entry, bool includeResourceTypeInEntryProperties);

        protected void StartFeed(Stack<ResponseNode> nodeStack, ODataFeedAnnotations feedAnnotations)
        {
            nodeStack.Push(new ResponseNode
            {
                Feed = new List<IDictionary<string, object>>(),
                FeedAnnotations = feedAnnotations,
            });
        }

        protected void EndFeed(Stack<ResponseNode> nodeStack, ODataFeedAnnotations feedAnnotations, ref ResponseNode rootNode)
        {
            var feedNode = nodeStack.Pop();
            var entries = feedNode.Feed;
            if (nodeStack.Any())
                nodeStack.Peek().Feed = entries;
            else
                rootNode = feedNode;

            if (feedNode.FeedAnnotations == null)
            {
                feedNode.FeedAnnotations = feedAnnotations;
            }
            else
            {
                feedNode.FeedAnnotations.Merge(feedAnnotations);
            }
        }

        protected void StartEntry(Stack<ResponseNode> nodeStack)
        {
            nodeStack.Push(new ResponseNode
            {
                Entry = new Dictionary<string, object>()
            });
        }

        protected void EndEntry(Stack<ResponseNode> nodeStack, ref ResponseNode rootNode, object entry, bool includeResourceTypeInEntryProperties)
        {
            var entryNode = nodeStack.Pop();
            ConvertEntry(entryNode, entry, includeResourceTypeInEntryProperties);
            if (nodeStack.Any())
            {
                if (nodeStack.Peek().Feed != null)
                    nodeStack.Peek().Feed.Add(entryNode.Entry);
                else
                    nodeStack.Peek().Entry = entryNode.Entry;
            }
            else
            {
                rootNode = entryNode;
            }
        }

        protected void StartNavigationLink(Stack<ResponseNode> nodeStack, string linkName)
        {
            nodeStack.Push(new ResponseNode
            {
                LinkName = linkName,
            });
        }

        protected void EndNavigationLink(Stack<ResponseNode> nodeStack)
        {
            var linkNode = nodeStack.Pop();
            if (linkNode.Value != null)
            {
                var linkValue = linkNode.Value;
                if (linkNode.Value is IDictionary<string, object> && !(linkNode.Value as IDictionary<string, object>).Any())
                    linkValue = null;
                nodeStack.Peek().Entry.Add(linkNode.LinkName, linkValue);
            }
        }
    }
}