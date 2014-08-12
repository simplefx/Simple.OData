using System;
using System.Linq;
using Microsoft.Data.Edm;

namespace Simple.OData.Client
{
    class EdmModelParserV3 : IEdmModelParser
    {
        private IEdmModel _model;

        public EdmModelParserV3(IEdmModel model)
        {
            _model = model;
        }

        public string[] SupportedProtocols
        {
            get
            {
                return new[] { "1.0", "2.0", "3.0" };
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
                return _model.EntityContainers().Select(x =>
                    new EdmEntityContainer()
                    {
                        Namespace = x.Namespace,
                        Name = x.Name,
                    }).ToArray();
            }
        }
    }
}