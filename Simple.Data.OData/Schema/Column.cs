using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.OData.Schema
{
    public class Column
    {
        private readonly string _actualName;
        private readonly Table _table;

        public Column(string actualName, Table table)
        {
            _actualName = actualName;
            _table = table;
        }

        public string HomogenizedName
        {
            get { return ActualName.Homogenize(); }
        }

        public string ActualName
        {
            get { return _actualName; }
        }
    }
}
