using System.Data.Services.Providers;
using System.Data.Services;

namespace ActionProviderImplementation;

public interface IParameterMarshaller
{
	object[] Marshall(DataServiceOperationContext operationContext, ServiceAction serviceAction, object[] parameters);
}
