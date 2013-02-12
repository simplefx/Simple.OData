
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public class Column
    {
        private readonly string _actualName;
        private readonly EdmPropertyType _propertyType;
        private readonly bool _isNullable;

        internal Column(string actualName, EdmPropertyType propertyType, bool isNullable)
        {
            _actualName = actualName;
            _propertyType = propertyType;
            _isNullable = isNullable;
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

        public EdmPropertyType PropertyType
        {
            get { return _propertyType; }
        }

        public bool IsNullable
        {
            get { return _isNullable; }
        }
    }
}
