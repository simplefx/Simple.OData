using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;
using Simple.OData.Schema;

namespace Simple.Data.OData
{
    using Simple.OData;
    using Simple.Data.OData.Helpers;

    public class Inserter
    {
        private ProviderHelper _providerHelper;
        private ExpressionFormatter _expressionFormatter;

        public Inserter(ProviderHelper providerHelper, ExpressionFormatter expressionFormatter)
        {
            _providerHelper = providerHelper;
            _expressionFormatter = expressionFormatter;
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            var table = DatabaseSchema.Get(_providerHelper).FindTable(tableName);

            CheckInsertablePropertiesAreAvailable(table, data);

            var entry = DataServicesHelper.CreateDataElement(data);
            var request = _providerHelper.CreateTableRequest(tableName, RestVerbs.POST, entry.ToString());

            var text = new RequestRunner().Request(request);
            if (resultRequired)
            {
                return DataServicesHelper.GetData(text).First();
            }
            else
            {
                return null;
            }
        }

        private void CheckInsertablePropertiesAreAvailable(Table table, IEnumerable<KeyValuePair<string, object>> data)
        {
            data = data.Where(kvp => table.HasColumn(kvp.Key));

            if (data.Count() == 0)
            {
                throw new SimpleDataException("No properties were found which could be mapped to the database.");
            }
        }
    }
}
