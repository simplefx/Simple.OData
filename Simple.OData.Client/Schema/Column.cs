using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.OData.Client
{
    public class Column
    {
        private readonly string _actualName;

        public Column(string actualName)
        {
            _actualName = actualName;
        }

        public override string ToString()
        {
            return _actualName;
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
