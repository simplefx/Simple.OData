using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public abstract partial class FluentClientBase<T> : IFluentClient<T> 
        where T : class
    {
        protected readonly ODataClient _client;
        internal readonly Session _session;
        protected readonly FluentCommand _parentCommand;
        protected FluentCommand _command;
        protected readonly bool _dynamicResults;

        internal FluentClientBase(ODataClient client, Session session, FluentCommand parentCommand = null, FluentCommand command = null, bool dynamicResults = false)
        {
            _client = client;
            _session = session;
            _parentCommand = parentCommand;
            _command = command;
            _dynamicResults = dynamicResults;
        }

        internal FluentCommand Command
        {
            get
            {
                if (_command != null)
                    return _command;

                lock (this)
                {
                    return _command ?? (_command = CreateCommand());
                }
            }
        }

        protected FluentCommand CreateCommand()
        {
            return new FluentCommand(this.Session, _parentCommand);
        }

        internal Session Session
        {
            get { return _session; }
        }

        public Task<T> ExecuteAsync()
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsync(_command, CancellationToken.None), _command.SelectedColumns);
        }

        public Task<T> ExecuteAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> ExecuteAsEnumerableAsync()
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsEnumerableAsync(_command, CancellationToken.None), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> ExecuteAsEnumerableAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsEnumerableAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        public Task<U> ExecuteAsScalarAsync<U>()
        {
            return _client.ExecuteAsScalarAsync<U>(_command, CancellationToken.None);
        }

        public Task<U> ExecuteAsScalarAsync<U>(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsScalarAsync<U>(_command, cancellationToken);
        }

        public Task<U[]> ExecuteAsArrayAsync<U>()
        {
            return ExecuteAsArrayAsync<U>(CancellationToken.None);
        }

        public Task<string> GetCommandTextAsync()
        {
            return GetCommandTextAsync(CancellationToken.None);
        }

        public Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            return this.Command.GetCommandTextAsync(cancellationToken);
        }

        public Task<U[]> ExecuteAsArrayAsync<U>(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsArrayAsync<U>(_command, cancellationToken);
        }

        protected Task<IEnumerable<T>> RectifyColumnSelectionAsync(Task<IEnumerable<IDictionary<string, object>>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(
                x => RectifyColumnSelection(x.Result, selectedColumns)).ContinueWith(
                y => y.Result == null ? null : y.Result.Select(z => z.ToObject<T>(_dynamicResults)));
        }

        protected Task<T> RectifyColumnSelectionAsync(Task<IDictionary<string, object>> entry, IList<string> selectedColumns)
        {
            return entry.ContinueWith(
                x => RectifyColumnSelection(x.Result, selectedColumns).ToObject<T>(_dynamicResults));
        }

        protected Task<Tuple<IEnumerable<T>, int>> RectifyColumnSelectionAsync(Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(x =>
            {
                var result = x.Result;
                return new Tuple<IEnumerable<T>, int>(
                    RectifyColumnSelection(result.Item1, selectedColumns).Select(y => y.ToObject<T>(_dynamicResults)),
                    result.Item2);
            });
        }

        protected static IEnumerable<IDictionary<string, object>> RectifyColumnSelection(IEnumerable<IDictionary<string, object>> entries, IList<string> selectedColumns)
        {
            return entries == null ? null : entries.Select(x => RectifyColumnSelection(x, selectedColumns));
        }

        protected static IDictionary<string, object> RectifyColumnSelection(IDictionary<string, object> entry, IList<string> selectedColumns)
        {
            if (selectedColumns == null || !selectedColumns.Any())
            {
                return entry;
            }
            else
            {
                return entry.Where(x => selectedColumns.Any(y => x.Key.Homogenize() == y.Homogenize())).ToIDictionary();
            }
        }
    }
}