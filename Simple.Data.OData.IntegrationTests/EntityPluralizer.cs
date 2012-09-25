using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using Simple.Data.Extensions;

namespace Simple.Data.OData.IntegrationTests
{
    class EntityPluralizer : IPluralizer
    {
        private readonly PluralizationService _pluralizationService =
            PluralizationService.CreateService(new CultureInfo("en-US"));

        public bool IsPlural(string word)
        {
            return _pluralizationService.IsPlural(word);
        }

        public bool IsSingular(string word)
        {
            return _pluralizationService.IsSingular(word);
        }

        public string Pluralize(string word)
        {
            bool upper = (word.IsAllUpperCase());
            word = _pluralizationService.Pluralize(word);
            return upper ? word.ToUpper(_pluralizationService.Culture) : word;
        }

        public string Singularize(string word)
        {
            return _pluralizationService.Singularize(word);
        }
    }
}
