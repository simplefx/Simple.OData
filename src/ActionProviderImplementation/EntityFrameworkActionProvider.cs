using System.Data.Entity;

namespace ActionProviderImplementation
{
	public class EntityFrameworkActionProvider : ActionProvider
	{
		public EntityFrameworkActionProvider(DbContext dbContext) : base(dbContext, new EntityFrameworkParameterMarshaller())
		{
		}
	}
}
