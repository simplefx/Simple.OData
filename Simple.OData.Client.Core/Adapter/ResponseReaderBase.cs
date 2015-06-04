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

        public abstract Task<ODataResponse> GetResponseAsync(HttpResponseMessage responseMessage);

        protected abstract void ConvertEntry(ResponseNode entryNode, object entry);

        protected void StartFeed(Stack<ResponseNode> nodeStack, ODataFeedAnnotations feedAnnotations)
        {
            nodeStack.Push(new ResponseNode
            {
                Feed = new ODataResponse.AnnotatedFeed(),
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

            if (feedNode.Feed.Annotations == null)
            {
                feedNode.Feed.Annotations = feedAnnotations;
            }
            else
            {
                feedNode.Feed.Annotations.Merge(feedAnnotations);
            }
        }

        protected void StartEntry(Stack<ResponseNode> nodeStack)
        {
            nodeStack.Push(new ResponseNode
            {
                Entry = new ODataResponse.AnnotatedEntry()
            });
        }

        protected void EndEntry(Stack<ResponseNode> nodeStack, ref ResponseNode rootNode, object entry)
        {
            var entryNode = nodeStack.Pop();
            ConvertEntry(entryNode, entry);
            if (nodeStack.Any())
            {
                var node = nodeStack.Peek();
                if (node.Feed != null)
                {
                    node.Feed.Entries.Add(entryNode.Entry);
                }
                else
                {
                    node.Entry = entryNode.Entry;
                }
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
                nodeStack.Peek().Entry.Data.Add(linkNode.LinkName, linkValue);
            }
        }
    }
}