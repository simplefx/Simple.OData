using System.Data.Entity;

namespace ActionProviderImplementation;

public class EntityFrameworkActionProvider(DbContext dbContext) : ActionProvider(dbContext, new EntityFrameworkParameterMarshaller())
{
}
