using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.OData.Schema
{
    public class Association
    {
        private readonly string _actualName;
        private readonly string _referenceTableName;

        public Association(string actualName, string referenceTableName)
        {
            _actualName = actualName;
            _referenceTableName = referenceTableName;
        }

        public override string ToString()
        {
            return _actualName;
        }

        public string HomogenizedActualName
        {
            get { return ActualName.Homogenize(); }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public string HomogenizedReferenceTableName
        {
            get { return ReferenceTableName.Homogenize(); }
        }

        public string ReferenceTableName
        {
            get { return _referenceTableName; }
        }
    }
}
