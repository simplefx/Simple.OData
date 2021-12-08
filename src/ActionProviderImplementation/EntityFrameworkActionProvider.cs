using System.Data.Entity;

namespace ActionProviderImplementation
{
	public class EntityFrameworkActionProvider : ActionProvider
	{
		private readonly DbContext _dbContext;

		public EntityFrameworkActionProvider(DbContext dbContext) : base(dbContext, new EntityFrameworkParameterMarshaller())
		{
			_dbContext = dbContext;
		}
	}
}
