using Microsoft.OData;

namespace Simple.OData.Client.V4.Adapter
{
    static class ODataExtensions
    {
        public static ODataMessageReaderSettings ToReaderSettings(this ISession session)
        {
            return session.Settings.ToReaderSettings();
        }

        public static ODataMessageReaderSettings ToReaderSettings(this ODataClientSettings settings)
        {
            var readerSettings = new ODataMessageReaderSettings();
            // TODO ODataLib7
            if (settings.IgnoreUnmappedProperties)
                readerSettings.Validations &= ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;
            readerSettings.MessageQuotas.MaxReceivedMessageSize = int.MaxValue;
            readerSettings.ShouldIncludeAnnotation = x => settings.IncludeAnnotationsInResults;
            return readerSettings;
        }
    }
}