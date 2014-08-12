using System;
using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    class EdmModelParserV4 : IEdmModelParser
    {
        private IEdmModel _model;

        public EdmModelParserV4(IEdmModel model)
        {
            _model = model;
        }

        public string[] SupportedProtocols
        {
            get
            {
                return new[] { "4.0" };
            }
        }

        public EdmEntityType[] EntityTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public EdmComplexType[] ComplexTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public EdmEnumType[] EnumTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public EdmAssociation[] Associations
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public EdmEntityContainer[] EntityContainers
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}