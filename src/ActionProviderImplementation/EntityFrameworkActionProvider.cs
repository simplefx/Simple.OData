using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace ActionProviderImplementation
{
    public class EntityFrameworkActionProvider: ActionProvider
    {
		private DbContext _dbContext;

        public EntityFrameworkActionProvider(DbContext dbContext) : base(dbContext, new EntityFrameworkParameterMarshaller()) {
            _dbContext = dbContext;
        }
    }
}
